using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    // Hunt supplies on the viewfinder: the spare battery button under the charge meter and the boosters taken along.
    public sealed class SupplyHudView : MonoBehaviour
    {
        [SerializeField] private Button _spareButton;
        [SerializeField] private CanvasGroup _spareGroup;
        [SerializeField] private TMP_Text _spareCount;
        [SerializeField] private RectTransform _spareIcon;
        [SerializeField] private Image[] _boosterIcons;
        [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.4f;

        public event Action SpareClicked;

        public void SetSpare(bool visible, bool usable, string count)
        {
            _spareButton.gameObject.SetActive(visible);
            _spareButton.interactable = usable;
            _spareGroup.alpha = usable ? 1f : _disabledAlpha;
            _spareCount.text = count;
        }

        public void PlaySpareUsed()
        {
            _spareIcon.DOKill(true);
            _spareIcon.DOPunchScale(Vector3.one * 0.4f, 0.45f, 6).Ui(gameObject);
        }

        public void SetBoosters(Sprite[] icons, Color[] colors)
        {
            for (var i = 0; i < _boosterIcons.Length; i++)
            {
                var visible = i < icons.Length;
                _boosterIcons[i].gameObject.SetActive(visible);
                if (!visible)
                    continue;

                _boosterIcons[i].sprite = icons[i];
                _boosterIcons[i].color = colors[i];
            }
        }

        private void Awake()
        {
            _spareButton.onClick.AddListener(OnSpareClicked);
        }

        private void OnDestroy()
        {
            _spareButton.onClick.RemoveListener(OnSpareClicked);
        }

        private void OnSpareClicked()
        {
            SpareClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _spareButton = Find<Button>("Spare");
            _spareGroup = Find<CanvasGroup>("Spare");
            _spareCount = Find<TMP_Text>("Spare/Row/Count");
            _spareIcon = Find<RectTransform>("Spare/Row/Icon");
            var boosters = transform.Find("Boosters");
            if (boosters != null)
                _boosterIcons = boosters.GetComponentsInChildren<Image>(true);
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
