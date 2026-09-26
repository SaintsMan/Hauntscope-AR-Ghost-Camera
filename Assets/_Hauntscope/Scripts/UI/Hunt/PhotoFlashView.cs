using DG.Tweening;
using Hauntscope.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    public sealed class PhotoFlashView : MonoBehaviour
    {
        [SerializeField] private Image _flash;
        [SerializeField, Min(0.05f)] private float _duration = 0.45f;

        public void Play()
        {
            _flash.DOKill();
            _flash.enabled = true;
            var color = _flash.color;
            color.a = 1f;
            _flash.color = color;
            _flash.DOFade(0f, _duration).SetEase(Ease.OutQuad).OnComplete(() => _flash.enabled = false).Ui(gameObject);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _flash = transform.Find("Flash")?.GetComponent<Image>();
        }
#endif
    }
}
