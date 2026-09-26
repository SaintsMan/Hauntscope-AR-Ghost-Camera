using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    // The break between two rounds of a night shift: three perks to choose from, the state the next round starts in,
    // and the perks already carried.
    public sealed class ShiftBreakView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private ShiftPerkCardView[] _cards;
        [SerializeField] private GameObject _chosenRow;
        [SerializeField] private Image[] _chosenIcons;
        [SerializeField] private RectTransform _card;
        [SerializeField] private float _compactHeight = 1160f;
        [SerializeField] private float _fullHeight = 1280f;

        public event Action<int> PerkClicked;

        public int CardCount => _cards.Length;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetTitle(string title, string status)
        {
            _title.text = title;
            _status.text = status;
        }

        public void SetCard(int index, Sprite icon, string perkName, string description)
        {
            _cards[index].SetVisible(true);
            _cards[index].SetPerk(icon, perkName, description);
        }

        public void HideCard(int index)
        {
            _cards[index].SetVisible(false);
        }

        // The first break has nothing carried yet, so the card drops the row instead of leaving a gap under the perks.
        public void SetChosen(IReadOnlyList<Sprite> icons)
        {
            var any = icons.Count > 0;
            _chosenRow.SetActive(any);
            _card.sizeDelta = new Vector2(_card.sizeDelta.x, any ? _fullHeight : _compactHeight);
            for (var i = 0; i < _chosenIcons.Length; i++)
            {
                var shown = i < icons.Count;
                _chosenIcons[i].gameObject.SetActive(shown);
                if (shown)
                    _chosenIcons[i].sprite = icons[i];
            }
        }

        private Action[] _cardHandlers;

        private void Awake()
        {
            _cardHandlers = new Action[_cards.Length];
            for (var i = 0; i < _cards.Length; i++)
            {
                var index = i;
                _cardHandlers[i] = () => PerkClicked?.Invoke(index);
                _cards[i].Clicked += _cardHandlers[i];
            }
        }

        private void OnDestroy()
        {
            if (_cardHandlers == null)
                return;

            for (var i = 0; i < _cards.Length; i++)
                _cards[i].Clicked -= _cardHandlers[i];
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _card = transform.Find("Card") as RectTransform;
            _title = transform.Find("Card/Title")?.GetComponent<TMP_Text>();
            _status = transform.Find("Card/Status")?.GetComponent<TMP_Text>();
            _cards = GetComponentsInChildren<ShiftPerkCardView>(true);
            var chosen = transform.Find("Card/Chosen");
            _chosenRow = chosen != null ? chosen.gameObject : null;
            var icons = transform.Find("Card/Chosen/Icons");
            if (icons != null)
            {
                _chosenIcons = new Image[icons.childCount];
                for (var i = 0; i < icons.childCount; i++)
                    _chosenIcons[i] = icons.GetChild(i).GetComponent<Image>();
            }
        }
#endif
    }
}
