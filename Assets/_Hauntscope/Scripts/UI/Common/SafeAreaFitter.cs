using UnityEngine;

namespace Hauntscope.UI.Common
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _appliedArea;
        private Vector2Int _appliedScreen;

        private void OnEnable()
        {
            Apply();
        }

        // Called when the canvas is resized (rotation, split screen), so no per-frame polling is needed.
        private void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        private void Apply()
        {
            if (_rect == null)
                _rect = (RectTransform)transform;

            var area = Screen.safeArea;
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (area == _appliedArea && screen == _appliedScreen || screen.x <= 0 || screen.y <= 0)
                return;

            _appliedArea = area;
            _appliedScreen = screen;
            _rect.anchorMin = new Vector2(area.xMin / screen.x, area.yMin / screen.y);
            _rect.anchorMax = new Vector2(area.xMax / screen.x, area.yMax / screen.y);
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
