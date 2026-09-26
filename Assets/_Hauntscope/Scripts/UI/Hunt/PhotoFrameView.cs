using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    // The camcorder frame burnt into a photo. It replaces the HUD for exactly the frame that is captured, so the
    // picture carries the case file and stars instead of buttons.
    public sealed class PhotoFrameView : MonoBehaviour
    {
        [SerializeField] private Canvas _hud;
        [SerializeField] private GameObject _frame;
        [SerializeField] private TMP_Text _caseLabel;
        [SerializeField] private TMP_Text _dateLabel;
        [SerializeField] private StarRow _stars;

        public void Show(string caseText, string dateText, int stars)
        {
            _caseLabel.text = caseText;
            _dateLabel.text = dateText;
            _stars.SetStars(stars);
            _hud.enabled = false;
            _frame.SetActive(true);
        }

        public void Hide()
        {
            _frame.SetActive(false);
            _hud.enabled = true;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            var frame = transform.Find("Frame");
            if (frame == null)
                return;

            _frame = frame.gameObject;
            _caseLabel = frame.Find("Case")?.GetComponent<TMP_Text>();
            _dateLabel = frame.Find("Date")?.GetComponent<TMP_Text>();
            _stars = frame.GetComponentInChildren<StarRow>(true);
        }
#endif
    }
}
