using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Common
{
    // Three rating stars: earned ones in gold, missed ones as dim outlines, so a 1-star photo still shows what is possible.
    // On a shared photo the missed ones are hidden: a picture people post should never read as "failed".
    public sealed class StarRow : MonoBehaviour
    {
        [SerializeField] private Image[] _stars;
        [SerializeField] private Sprite _earned;
        [SerializeField] private Sprite _missed;
        [SerializeField] private Color _earnedColor = new Color(1f, 0.82f, 0.4f, 1f);
        [SerializeField] private Color _missedColor = new Color(0.49f, 0.55f, 0.6f, 0.6f);
        [SerializeField] private bool _hideMissed;

        public void SetStars(int stars, bool animate = false)
        {
            for (var i = 0; i < _stars.Length; i++)
            {
                var star = _stars[i];
                var earned = i < stars;
                star.gameObject.SetActive(earned || !_hideMissed);
                star.sprite = earned ? _earned : _missed;
                star.color = earned ? _earnedColor : _missedColor;
                star.transform.DOKill();
                star.transform.localScale = Vector3.one;
                if (animate && earned && isActiveAndEnabled)
                {
                    star.transform.localScale = Vector3.zero;
                    star.transform.DOScale(1f, 0.3f).SetDelay(0.12f * i).SetEase(Ease.OutBack).Ui(gameObject);
                }
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _stars = GetComponentsInChildren<Image>(true);
        }
#endif
    }
}
