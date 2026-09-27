using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Common
{
    // Buttons in one row only read as a set at one text size. Each label is autosized to its own box, then all take
    // the smallest size, so the longest word of the current language sets the size for the row.
    public sealed class TextSizeGroup : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] _texts;

        private float[] _maxSizes;
        private string[] _shown;
        private bool _dirty;

        private void Awake()
        {
            _maxSizes = new float[_texts.Length];
            _shown = new string[_texts.Length];
            for (var i = 0; i < _texts.Length; i++)
                _maxSizes[i] = _texts[i].enableAutoSizing ? _texts[i].fontSizeMax : _texts[i].fontSize;
        }

        private void OnEnable()
        {
            _dirty = true;
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        // The event also fires for every mesh rebuild; only a new string needs a new size.
        private void OnTextChanged(Object changed)
        {
            for (var i = 0; i < _texts.Length; i++)
            {
                if (_texts[i] == changed && _texts[i].text != _shown[i])
                    _dirty = true;
            }
        }

        private void LateUpdate()
        {
            if (!_dirty)
                return;

            _dirty = false;
            var size = float.MaxValue;
            for (var i = 0; i < _texts.Length; i++)
            {
                var text = _texts[i];
                if (!text.gameObject.activeInHierarchy)
                    continue;
                text.enableAutoSizing = true;
                text.fontSizeMax = _maxSizes[i];
                text.ForceMeshUpdate();
                size = Mathf.Min(size, text.fontSize);
            }

            for (var i = 0; i < _texts.Length; i++)
            {
                var text = _texts[i];
                _shown[i] = text.text;
                if (!text.gameObject.activeInHierarchy || size == float.MaxValue)
                    continue;
                text.enableAutoSizing = false;
                text.fontSize = size;
            }
        }
    }
}
