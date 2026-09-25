using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.Editor
{
    // Tutorial gestures are looping clips generated as data: the HUD shows a finger dragging, swiping or holding and
    // a pulse around the control to use, without any animation code running in the game.
    public static class TutorialCueGenerator
    {
        public const string Folder = "Assets/_Hauntscope/Art/Animations/Tutorial";
        public const string RippleChild = "Ripple";

        private const float PulseLength = 1.2f;
        private const float PulseScale = 1.18f;
        private const float GestureLength = 1.6f;
        private const float HoldLength = 1.8f;
        private const float DragDistance = 200f;
        private const float SwipeDistance = 130f;
        private const float PressedScale = 0.8f;

        [MenuItem("Hauntscope/Build Tutorial Cues")]
        public static void BuildAll()
        {
            EnsureFolders();
            Save("CuePulse", BuildPulse());
            Save("TouchDrag", BuildGesture("m_AnchoredPosition.y", 0f, DragDistance));
            Save("TouchSwipe", BuildGesture("m_AnchoredPosition.x", SwipeDistance, -SwipeDistance));
            Save("TouchHold", BuildHold());
            AssetDatabase.SaveAssets();
        }

        public static RuntimeAnimatorController Controller(string name)
        {
            return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{Folder}/{name}.controller");
        }

        private static AnimationClip BuildPulse()
        {
            var clip = new AnimationClip();
            var scale = Curve((0f, 1f), (PulseLength, PulseScale));
            clip.SetCurve(string.Empty, typeof(Transform), "m_LocalScale.x", scale);
            clip.SetCurve(string.Empty, typeof(Transform), "m_LocalScale.y", scale);
            clip.SetCurve(string.Empty, typeof(Image), "m_Color.a", Curve((0f, 0.95f), (PulseLength * 0.25f, 0.8f), (PulseLength, 0f)));
            return clip;
        }

        // A fingertip appears, travels, lifts off; the ripple marks the moment it touches the glass.
        private static AnimationClip BuildGesture(string axis, float from, float to)
        {
            var clip = new AnimationClip();
            clip.SetCurve(string.Empty, typeof(RectTransform), axis,
                Curve((0f, from), (0.25f, from), (1.05f, to), (GestureLength, to)));
            clip.SetCurve(string.Empty, typeof(Image), "m_Color.a",
                Curve((0f, 0f), (0.2f, 1f), (1.1f, 1f), (1.35f, 0f), (GestureLength, 0f)));
            AddRipple(clip, 0.2f, 0.7f);
            return clip;
        }

        private static AnimationClip BuildHold()
        {
            var clip = new AnimationClip();
            var scale = Curve((0f, 1f), (0.3f, 1f), (0.45f, PressedScale), (1.4f, PressedScale), (1.55f, 1f), (HoldLength, 1f));
            clip.SetCurve(string.Empty, typeof(Transform), "m_LocalScale.x", scale);
            clip.SetCurve(string.Empty, typeof(Transform), "m_LocalScale.y", scale);
            clip.SetCurve(string.Empty, typeof(Image), "m_Color.a",
                Curve((0f, 0f), (0.2f, 1f), (1.5f, 1f), (HoldLength, 0f)));
            AddRipple(clip, 0.45f, 1.25f);
            return clip;
        }

        private static void AddRipple(AnimationClip clip, float start, float end)
        {
            var scale = Curve((0f, 0.6f), (start, 0.6f), (end, 1.7f), (GestureLength, 1.7f));
            clip.SetCurve(RippleChild, typeof(Transform), "m_LocalScale.x", scale);
            clip.SetCurve(RippleChild, typeof(Transform), "m_LocalScale.y", scale);
            clip.SetCurve(RippleChild, typeof(Image), "m_Color.a", Curve((0f, 0f), (start, 0.8f), (end, 0f), (GestureLength, 0f)));
        }

        private static AnimationCurve Curve(params (float time, float value)[] keys)
        {
            var curve = new AnimationCurve();
            foreach (var key in keys)
                curve.AddKey(key.time, key.value);

            for (var i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }

            return curve;
        }

        private static void Save(string name, AnimationClip built)
        {
            var clipPath = $"{Folder}/{name}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            EditorUtility.CopySerialized(built, clip);
            clip.name = name;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            Object.DestroyImmediate(built);

            var controllerPath = $"{Folder}/{name}.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) == null)
                AnimatorController.CreateAnimatorControllerAtPathWithClip(controllerPath, clip);
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Hauntscope/Art/Animations"))
                AssetDatabase.CreateFolder("Assets/_Hauntscope/Art", "Animations");
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/_Hauntscope/Art/Animations", "Tutorial");
        }
    }
}
