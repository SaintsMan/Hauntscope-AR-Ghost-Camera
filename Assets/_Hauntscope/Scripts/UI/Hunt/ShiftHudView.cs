using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    // The shift round and its reward multiplier under the battery: SHIFT 2/3 · ×1.5.
    public sealed class ShiftHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        public void SetVisible(bool visible)
        {
            _label.gameObject.SetActive(visible);
        }

        public void SetText(string text)
        {
            _label.text = text;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _label = GetComponentInChildren<TMP_Text>(true);
        }
#endif
    }
}
