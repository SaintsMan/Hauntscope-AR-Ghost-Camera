using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    public sealed class TutorialView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _counterLabel;
        [SerializeField] private TMP_Text _hintLabel;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetCounter(string text)
        {
            _counterLabel.text = text;
        }

        public void SetHint(string text)
        {
            _hintLabel.text = text;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _counterLabel = Find("Card/Counter");
            _hintLabel = Find("Card/Hint");
        }

        private TMP_Text Find(string path)
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<TMP_Text>() : null;
        }
#endif
    }
}
