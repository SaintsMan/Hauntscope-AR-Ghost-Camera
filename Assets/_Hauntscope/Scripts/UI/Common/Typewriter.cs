using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Common
{
    // Reveals a text character by character like a terminal printout. Views call Play() after setting the text;
    // the full text is already laid out, so wrapping never jumps while it types.
    [RequireComponent(typeof(TMP_Text))]
    public sealed class Typewriter : MonoBehaviour
    {
        private const int AllCharacters = 99999;

        [SerializeField] private TMP_Text _text;
        [SerializeField, Min(1f)] private float _charactersPerSecond = 70f;
        [SerializeField, Min(0f)] private float _delay = 0.15f;

        private Tween _typing;

        public void Play()
        {
            _typing?.Kill();
            // A tween on an inactive object would be killed at once and leave the text hidden.
            if (!isActiveAndEnabled)
            {
                ShowAll();
                return;
            }

            _text.ForceMeshUpdate();
            var count = _text.textInfo.characterCount;
            _text.maxVisibleCharacters = 0;
            _typing = DOTween.To(() => _text.maxVisibleCharacters, value => _text.maxVisibleCharacters = value, count,
                    count / _charactersPerSecond)
                .SetDelay(_delay)
                .SetEase(Ease.Linear)
                .OnComplete(ShowAll)
                .Ui(gameObject);
        }

        private void OnDisable()
        {
            _typing = null;
            ShowAll();
        }

        private void ShowAll()
        {
            _text.maxVisibleCharacters = AllCharacters;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _text = GetComponent<TMP_Text>();
        }
#endif
    }
}
