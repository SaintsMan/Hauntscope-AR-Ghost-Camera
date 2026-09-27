using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    // Over the result card: a tape of Vale's this hunt brought to the surface, and where to hear it. It sits higher when
    // the night shift's summary line already takes the space above the card.
    public sealed class TapeToastView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private TMP_Text _hint;
        [SerializeField] private RectTransform _reel;
        [SerializeField] private GameObject _summary;
        [SerializeField] private float _offset = 30f;
        [SerializeField] private float _offsetOverSummary = 150f;
        [SerializeField, Min(0.05f)] private float _popDuration = 0.4f;
        [SerializeField, Min(0f)] private float _popDelay = 0.9f;
        [SerializeField, Min(0.1f)] private float _reelTurnTime = 1.1f;

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show(string title, string body, string hint)
        {
            _title.text = title;
            _body.text = body;
            _hint.text = hint;
            var rect = (RectTransform)transform;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, _summary.activeSelf ? _offsetOverSummary : _offset);
            var wasShown = gameObject.activeSelf;
            gameObject.SetActive(true);
            if (wasShown)
                return;

            transform.DOKill();
            transform.localScale = Vector3.one * 0.6f;
            transform.DOScale(1f, _popDuration).SetDelay(_popDelay).SetEase(Ease.OutBack).Ui(gameObject);
            _reel.localRotation = Quaternion.identity;
            _reel.DOLocalRotate(new Vector3(0f, 0f, -360f), _reelTurnTime, RotateMode.FastBeyond360).SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental).Ui(gameObject);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _title = transform.Find("Title")?.GetComponent<TMP_Text>();
            _body = transform.Find("Body")?.GetComponent<TMP_Text>();
            _hint = transform.Find("Hint")?.GetComponent<TMP_Text>();
            _reel = transform.Find("Reel") as RectTransform;
            var summary = transform.parent != null ? transform.parent.Find("ShiftSummary") : null;
            _summary = summary != null ? summary.gameObject : null;
        }
#endif
    }
}
