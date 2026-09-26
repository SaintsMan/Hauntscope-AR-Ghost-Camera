using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    // One perk on offer at a shift break: tap to take it.
    public sealed class ShiftPerkCardView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _description;

        public event Action Clicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetPerk(Sprite icon, string perkName, string description)
        {
            _icon.sprite = icon;
            _icon.enabled = icon != null;
            _name.text = perkName;
            _description.text = description;
        }

        private void Awake()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            Clicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _button = GetComponent<Button>();
            _icon = transform.Find("Icon")?.GetComponent<Image>();
            _name = transform.Find("Name")?.GetComponent<TMP_Text>();
            _description = transform.Find("Description")?.GetComponent<TMP_Text>();
        }
#endif
    }
}
