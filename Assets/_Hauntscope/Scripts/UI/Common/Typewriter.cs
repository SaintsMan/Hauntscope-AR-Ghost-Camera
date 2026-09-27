using System;
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

        // Every time the whole text is out: typed to the end, completed early or shown at once.
        public event Action Finished;

        public bool IsTyping => _typing != null;

        public void Play()
        {
            _typing?.Kill();
            // A tween on an inactive object would be killed at once and leave the text hidden.
            if (!isActiveAndEnabled)
            {
                Finish();
                return;
            }

            _text.ForceMeshUpdate();
            var count = _text.textInfo.characterCount;
            _text.maxVisibleCharacters = 0;
            _typing = DOTween.To(() => _text.maxVisibleCharacters, value => _text.maxVisibleCharacters = value, count,
                    count / _charactersPerSecond)
                .SetDelay(_delay)
                .SetEase(Ease.Linear)
                .OnComplete(Finish)
                .Ui(gameObject);
        }

        // A tap on a text that is still typing shows the rest at once.
        public void Complete()
        {
            if (_typing == null)
                return;

            _typing.Kill();
            Finish();
        }

        private void OnDisable()
        {
            _typing = null;
            ShowAll();
        }

        private void Finish()
        {
            _typing = null;
            ShowAll();
            Finished?.Invoke();
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
