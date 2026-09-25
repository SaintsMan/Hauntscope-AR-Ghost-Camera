using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Common
{
    // An overlay above every scene that switches the picture off and on like the splash does: the last frame tears,
    // squeezes into a glowing line, the line into a dot; the next scene opens from a line with a flash. It lives for
    // the whole session and swallows taps while the screen is covered.
    public sealed class ScreenTransitionView : MonoBehaviour
    {
        private static readonly float[] Tear = { 1f, -0.7f, 0f };

        [SerializeField] private Canvas _canvas;
        [SerializeField] private GraphicRaycaster _raycaster;
        [SerializeField] private Graphic _backdrop;
        [SerializeField] private RawImage _snapshot;
        [SerializeField] private Graphic _crt;
        [SerializeField, Min(0.05f)] private float _offDuration = 0.55f;
        [SerializeField, Min(0.05f)] private float _onDuration = 0.32f;
        [SerializeField, Range(0.001f, 0.05f)] private float _lineThickness = 0.006f;
        [SerializeField, Range(0.001f, 0.1f)] private float _dotWidth = 0.02f;
        [SerializeField, Min(0f)] private float _tearOffset = 14f;
        [SerializeField, Min(0.01f)] private float _tearStep = 0.04f;
        [SerializeField, Min(0)] private int _settleFrames = 3;

        private Texture2D _frame;

        public bool IsCovered { get; private set; }

        public async UniTask CoverAsync(CancellationToken cancellationToken)
        {
            if (IsCovered)
                return;

            IsCovered = true;
            SetShown(true);
            await CaptureFrameAsync(cancellationToken);
            SetAlpha(_backdrop, 1f);
            SetAlpha(_crt, 0f);
            _crt.rectTransform.localScale = Vector3.one;

            var picture = _snapshot.rectTransform;
            picture.localScale = Vector3.one;
            var squeeze = _offDuration * 0.15f;
            var collapse = _offDuration * 0.47f;
            var shrink = _offDuration * 0.76f;

            var sequence = DOTween.Sequence();
            for (var i = 0; i < Tear.Length; i++)
            {
                var x = Tear[i] * _tearOffset;
                sequence.InsertCallback(_tearStep * i, () => picture.anchoredPosition = new Vector2(x, 0f));
            }

            sequence.Insert(squeeze, picture.DOScaleY(_lineThickness, collapse - squeeze).SetEase(Ease.InCubic));
            sequence.Insert(squeeze, _crt.rectTransform.DOScaleY(_lineThickness, collapse - squeeze).SetEase(Ease.InCubic));
            sequence.Insert(squeeze, _crt.DOFade(1f, collapse - squeeze).SetEase(Ease.InQuad));
            sequence.Insert(collapse, picture.DOScaleX(0f, shrink - collapse).SetEase(Ease.InQuad));
            sequence.Insert(collapse, _crt.rectTransform.DOScaleX(_dotWidth, shrink - collapse).SetEase(Ease.InQuad));
            sequence.Insert(shrink, _crt.DOFade(0f, _offDuration - shrink).SetEase(Ease.InQuad));
            await PlayAsync(sequence, cancellationToken);

            ReleaseFrame();
        }

        public void CoverImmediately()
        {
            IsCovered = true;
            SetShown(true);
            ReleaseFrame();
            SetAlpha(_backdrop, 1f);
            SetAlpha(_crt, 0f);
        }

        public async UniTask RevealAsync(CancellationToken cancellationToken)
        {
            if (!IsCovered)
                return;

            ReleaseFrame();
            SetAlpha(_backdrop, 1f);
            SetAlpha(_crt, 0f);

            // The frames right after a scene activates are long hitches; a tween started there would jump straight to
            // its end, so the power-on waits until the new scene is running smoothly.
            await UniTask.DelayFrame(_settleFrames, PlayerLoopTiming.Update, cancellationToken);
            SetAlpha(_crt, 1f);
            _crt.rectTransform.localScale = new Vector3(0f, _lineThickness, 1f);

            var draw = _onDuration * 0.27f;
            var open = _onDuration * 0.33f;
            var catchUp = _onDuration * 0.5f;
            var settle = _onDuration * 0.63f;

            var sequence = DOTween.Sequence();
            sequence.Insert(0f, _crt.rectTransform.DOScaleX(1f, draw).SetEase(Ease.OutQuad));
            sequence.Insert(open, _crt.rectTransform.DOScaleY(1f, open).SetEase(Ease.OutExpo));
            sequence.Insert(open, _crt.DOFade(0.4f, catchUp - open));
            sequence.Insert(catchUp, _crt.DOFade(0f, _onDuration - catchUp).SetEase(Ease.OutQuad));
            // The picture catches with one flicker, like an old tube warming up.
            sequence.InsertCallback(open, () => SetAlpha(_backdrop, 0f));
            sequence.InsertCallback(catchUp, () => SetAlpha(_backdrop, 0.65f));
            sequence.InsertCallback(settle, () => SetAlpha(_backdrop, 0f));
            await PlayAsync(sequence, cancellationToken);

            IsCovered = false;
            SetShown(false);
        }

        private void Awake()
        {
            SetShown(false);
        }

        private void OnDestroy()
        {
            ReleaseFrame();
        }

        // The frame is grabbed before the overlay draws anything, so the picture that collapses is the one on screen.
        private async UniTask CaptureFrameAsync(CancellationToken cancellationToken)
        {
            SetAlpha(_backdrop, 0f);
            SetAlpha(_crt, 0f);
            _snapshot.enabled = false;
            await UniTask.WaitForEndOfFrame(this, cancellationToken);

            ReleaseFrame();
            _frame = ScreenCapture.CaptureScreenshotAsTexture();
            _snapshot.texture = _frame;
            _snapshot.enabled = _frame != null;
            _snapshot.rectTransform.anchoredPosition = Vector2.zero;
        }

        private async UniTask PlayAsync(Sequence sequence, CancellationToken cancellationToken)
        {
            var completion = new UniTaskCompletionSource();
            sequence.OnKill(() => completion.TrySetResult()).Ui(gameObject);
            using (cancellationToken.Register(() => sequence.Kill()))
                await completion.Task;
        }

        private void SetShown(bool shown)
        {
            _canvas.enabled = shown;
            _raycaster.enabled = shown;
        }

        private void ReleaseFrame()
        {
            _snapshot.texture = null;
            _snapshot.enabled = false;
            if (_frame == null)
                return;

            Destroy(_frame);
            _frame = null;
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            var color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _canvas = GetComponent<Canvas>();
            _raycaster = GetComponent<GraphicRaycaster>();
            _backdrop = Find<Graphic>("Backdrop");
            _snapshot = Find<RawImage>("Snapshot");
            _crt = Find<Graphic>("Crt");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
