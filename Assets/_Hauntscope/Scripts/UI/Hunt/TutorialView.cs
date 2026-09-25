using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    public sealed class TutorialView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _counterLabel;
        [SerializeField] private TMP_Text _hintLabel;
        [SerializeField] private Typewriter _hintTypewriter;

        // The gesture cues live next to the controls they point at (the buttons, the EMF meter), not in this card.
        [SerializeField] private GameObject _walkCue;
        [SerializeField] private GameObject _emfCue;
        [SerializeField] private GameObject _lensCue;
        [SerializeField] private GameObject _beamCue;

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
            _hintTypewriter.Play();
        }

        public void SetWalkCue(bool visible)
        {
            _walkCue.SetActive(visible);
        }

        public void SetEmfCue(bool visible)
        {
            _emfCue.SetActive(visible);
        }

        public void SetLensCue(bool visible)
        {
            _lensCue.SetActive(visible);
        }

        public void SetBeamCue(bool visible)
        {
            _beamCue.SetActive(visible);
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
