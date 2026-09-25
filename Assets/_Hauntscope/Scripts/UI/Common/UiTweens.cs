using DG.Tweening;
using UnityEngine;

namespace Hauntscope.UI.Common
{
    // Shared timings and the staggered reveal used by screens and popups, so every panel in the game moves alike.
    public static class UiTweens
    {
        public const float ItemDuration = 0.28f;
        public const float ItemScale = 0.9f;

        // UI motion ignores Time.timeScale and dies with the object, so a panel closed mid-tween never leaks a tween.
        public static T Ui<T>(this T tween, GameObject owner) where T : Tween
        {
            return tween.SetUpdate(true).SetLink(owner, LinkBehaviour.KillOnDisable);
        }

        public static void Stagger(GameObject owner, CanvasGroup[] items, float delay, float step)
        {
            if (items == null)
                return;

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null)
                    continue;

                var rect = item.transform;
                item.alpha = 0f;
                rect.localScale = Vector3.one * ItemScale;
                var at = delay + step * i;
                item.DOFade(1f, ItemDuration).SetDelay(at).SetEase(Ease.OutQuad).Ui(owner);
                rect.DOScale(1f, ItemDuration).SetDelay(at).SetEase(Ease.OutBack).Ui(owner);
            }
        }

        public static void Settle(CanvasGroup[] items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                if (item == null)
                    continue;
                item.alpha = 1f;
                item.transform.localScale = Vector3.one;
            }
        }
    }
}
