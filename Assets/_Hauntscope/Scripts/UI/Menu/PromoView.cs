using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // The promo code card: the code field, the Bureau's line under it (a hint, "checking…", an error or the gift)
    // and two buttons. After a gift the field hides and the main button closes the card.
    public sealed class PromoView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _input;
        [SerializeField] private TMP_Text _placeholder;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private Button _activateButton;
        [SerializeField] private TMP_Text _activateLabel;
        [SerializeField] private CanvasGroup _activateGroup;
        [SerializeField] private Button _closeButton;
        [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.45f;

        public event Action ActivateClicked;

        public event Action CloseClicked;

        public event Action<string> CodeChanged;

        public string Code => _input.text;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetCode(string code)
        {
            _input.SetTextWithoutNotify(code);
        }

        public void SetPlaceholder(string text)
        {
            _placeholder.text = text;
        }

        public void SetBody(string text)
        {
            _body.text = text;
        }

        public void SetInputVisible(bool visible)
        {
            _input.gameObject.SetActive(visible);
        }

        public void SetActivate(string label, bool interactable)
        {
            _activateLabel.text = label;
            _activateButton.interactable = interactable;
            _activateGroup.alpha = interactable ? 1f : _disabledAlpha;
        }

        public void SetCloseVisible(bool visible)
        {
            _closeButton.gameObject.SetActive(visible);
        }

        public void SetInputInteractable(bool interactable)
        {
            _input.interactable = interactable;
        }

        private void Awake()
        {
            _activateButton.onClick.AddListener(OnActivateClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
            _input.onValueChanged.AddListener(OnCodeChanged);
            _input.onSubmit.AddListener(OnSubmitted);
        }

        private void OnDestroy()
        {
            _activateButton.onClick.RemoveListener(OnActivateClicked);
            _closeButton.onClick.RemoveListener(OnCloseClicked);
            _input.onValueChanged.RemoveListener(OnCodeChanged);
            _input.onSubmit.RemoveListener(OnSubmitted);
        }

        private void OnActivateClicked()
        {
            ActivateClicked?.Invoke();
        }

        private void OnCloseClicked()
        {
            CloseClicked?.Invoke();
        }

        private void OnCodeChanged(string code)
        {
            CodeChanged?.Invoke(code);
        }

        private void OnSubmitted(string code)
        {
            if (_activateButton.interactable)
                ActivateClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _input = GetComponentInChildren<TMP_InputField>(true);
            var placeholder = transform.Find("Card/CodeField/Text Area/Placeholder");
            _placeholder = placeholder != null ? placeholder.GetComponent<TMP_Text>() : null;
            var body = transform.Find("Card/Body");
            _body = body != null ? body.GetComponent<TMP_Text>() : null;
            var activate = transform.Find("Card/ActivateButton");
            _activateButton = activate != null ? activate.GetComponent<Button>() : null;
            _activateGroup = activate != null ? activate.GetComponent<CanvasGroup>() : null;
            var activateLabel = transform.Find("Card/ActivateButton/Label");
            _activateLabel = activateLabel != null ? activateLabel.GetComponent<TMP_Text>() : null;
            var close = transform.Find("Card/CloseButton");
            _closeButton = close != null ? close.GetComponent<Button>() : null;
        }
#endif
    }
}
