using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    // AGENT TIP: a strip under the top of the frame that drops in with one line of advice and fades out again.
    public sealed class FieldTipView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private TMP_Text _body;
        [SerializeField, Min(0.05f)] private float _inDuration = 0.35f;
        [SerializeField, Min(0.05f)] private float _outDuration = 0.25f;
        [SerializeField] private float _dropDistance = 60f;

        private Vector2 _restPosition;
        private bool _captured;
        private bool _shown;

        public void Show(string text)
        {
            Capture();
            _body.text = text;
            if (_shown)
                return;

            _shown = true;
            gameObject.SetActive(true);
            _group.DOKill();
            _panel.DOKill();
            _group.alpha = 0f;
            _panel.anchoredPosition = _restPosition + new Vector2(0f, _dropDistance);
            _group.DOFade(1f, _inDuration).Ui(gameObject);
            _panel.DOAnchorPos(_restPosition, _inDuration).SetEase(Ease.OutBack).Ui(gameObject);
        }

        public void Hide()
        {
            Capture();
            if (!_shown)
            {
                gameObject.SetActive(false);
                return;
            }

            _shown = false;
            _group.DOKill();
            _group.DOFade(0f, _outDuration).Ui(gameObject).OnComplete(Deactivate);
        }

        private void Deactivate()
        {
            if (!_shown)
                gameObject.SetActive(false);
        }

        private void Capture()
        {
            if (_captured)
                return;

            _captured = true;
            _restPosition = _panel.anchoredPosition;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _group = GetComponent<CanvasGroup>();
            _panel = transform.Find("Panel") as RectTransform;
            _body = transform.Find("Panel/Body")?.GetComponent<TMP_Text>();
        }
#endif
    }
}
