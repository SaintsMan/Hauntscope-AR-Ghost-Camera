using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    // The prank photo screen over the camera picture: what to do next, the chosen ghost's name, the strip of caught
    // ghosts, PLACE / MOVE and the shutter, over a gesture pad that only listens once the ghost is placed.
    public sealed class PrankPhotoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _hintLabel;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private RectTransform _slotsRoot;
        [SerializeField] private PrankSlotView _slotTemplate;
        [SerializeField] private Button _placeButton;
        [SerializeField] private TMP_Text _placeLabel;
        [SerializeField] private Button _shutterButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private CanvasGroup _shutterGroup;
        [SerializeField] private PrankGesturePad _pad;
        [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.4f;

        private readonly List<PrankSlotView> _slots = new List<PrankSlotView>();
        private readonly List<Action> _slotHandlers = new List<Action>();

        public event Action<int> SlotClicked;

        public event Action PlaceClicked;

        public event Action ShutterClicked;

        public event Action ExitClicked;

        public event Action<Vector2> Dragged
        {
            add => _pad.Dragged += value;
            remove => _pad.Dragged -= value;
        }

        public event Action<float> Pinched
        {
            add => _pad.Pinched += value;
            remove => _pad.Pinched -= value;
        }

        public event Action<float> Twisted
        {
            add => _pad.Twisted += value;
            remove => _pad.Twisted -= value;
        }

        public int SlotCount => _slots.Count;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void AddSlot(Sprite icon, Color rim)
        {
            var slot = Instantiate(_slotTemplate, _slotsRoot);
            slot.gameObject.SetActive(true);
            slot.Set(icon, rim);
            var index = _slots.Count;
            Action handler = () => SlotClicked?.Invoke(index);
            slot.Clicked += handler;
            _slots.Add(slot);
            _slotHandlers.Add(handler);
        }

        public void SetSelected(int index)
        {
            for (var i = 0; i < _slots.Count; i++)
                _slots[i].SetSelected(i == index);
        }

        public void SetHint(string text)
        {
            _hintLabel.text = text;
        }

        public void SetName(string text)
        {
            _nameLabel.text = text;
        }

        public void SetPlaceLabel(string text)
        {
            _placeLabel.text = text;
        }

        // The pad takes the screen only once the ghost is placed; before that a drag still turns the virtual camera.
        public void SetPadActive(bool active)
        {
            _pad.gameObject.SetActive(active);
        }

        public void SetShutterEnabled(bool enabled)
        {
            _shutterButton.interactable = enabled;
            _shutterGroup.alpha = enabled ? 1f : _disabledAlpha;
        }

        private void Awake()
        {
            _slotTemplate.gameObject.SetActive(false);
            _placeButton.onClick.AddListener(OnPlaceClicked);
            _shutterButton.onClick.AddListener(OnShutterClicked);
            _exitButton.onClick.AddListener(OnExitClicked);
        }

        private void OnDestroy()
        {
            _placeButton.onClick.RemoveListener(OnPlaceClicked);
            _shutterButton.onClick.RemoveListener(OnShutterClicked);
            _exitButton.onClick.RemoveListener(OnExitClicked);
            for (var i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] != null)
                    _slots[i].Clicked -= _slotHandlers[i];
            }
        }

        private void OnPlaceClicked()
        {
            PlaceClicked?.Invoke();
        }

        private void OnShutterClicked()
        {
            ShutterClicked?.Invoke();
        }

        private void OnExitClicked()
        {
            ExitClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _hintLabel = transform.Find("Hint")?.GetComponent<TMP_Text>();
            _nameLabel = transform.Find("Name")?.GetComponent<TMP_Text>();
            _slotsRoot = transform.Find("Strip/Viewport/Slots") as RectTransform;
            _slotTemplate = transform.Find("Strip/Viewport/Slots/Slot")?.GetComponent<PrankSlotView>();
            _placeButton = transform.Find("PlaceButton")?.GetComponent<Button>();
            _placeLabel = transform.Find("PlaceButton/Label")?.GetComponent<TMP_Text>();
            _shutterButton = transform.Find("Shutter")?.GetComponent<Button>();
            _shutterGroup = transform.Find("Shutter")?.GetComponent<CanvasGroup>();
            _exitButton = transform.Find("ExitButton")?.GetComponent<Button>();
            _pad = transform.Find("Pad")?.GetComponent<PrankGesturePad>();
        }
#endif
    }
}
