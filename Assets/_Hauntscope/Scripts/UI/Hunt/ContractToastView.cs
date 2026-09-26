using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    // Under the result card: an order this hunt completed, and that its pay waits in the menu.
    public sealed class ContractToastView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private TMP_Text _hint;
        [SerializeField, Min(0.05f)] private float _popDuration = 0.4f;
        [SerializeField, Min(0f)] private float _popDelay = 0.6f;

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show(string title, string body, string hint)
        {
            _title.text = title;
            _body.text = body;
            _hint.text = hint;
            var wasShown = gameObject.activeSelf;
            gameObject.SetActive(true);
            if (wasShown)
                return;

            transform.DOKill();
            transform.localScale = Vector3.one * 0.6f;
            transform.DOScale(1f, _popDuration).SetDelay(_popDelay).SetEase(Ease.OutBack).Ui(gameObject);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _title = transform.Find("Title")?.GetComponent<TMP_Text>();
            _body = transform.Find("Body")?.GetComponent<TMP_Text>();
            _hint = transform.Find("Hint")?.GetComponent<TMP_Text>();
        }
#endif
    }
}
