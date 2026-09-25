using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.Editor
{
    // Gesture cues are looping clips generated as data: the tutorial shows a finger dragging, swiping or holding and
    // a pulse around the control to use, the AR scan shows a phone sweeping over the floor, and no animation code
    // runs in the game.
    public static class TutorialCueGenerator
    {
        public const string Folder = "Assets/_Hauntscope/Art/Animations/Tutorial";
        public const string RippleChild = "Ripple";

        // Scan sweep layout, in cue units from the cue centre: the phone near the bottom, the floor patch it aims at
        // further away, plane dots lighting up where the scan passes.
        public const string PhonePath = "Phone";
        public const string ConePath = "Cone";
        public const string FootprintPath = "Footprint";
        public const string RingPath = "Footprint/Ring";
        public const string DotsPath = "Dots";
        public const float PhoneY = -150f;
        public const float LensOffsetY = 48f;
        public const float FootprintY = 70f;
        public static readonly Vector2[] Dots =
        {
            new Vector2(-135f, 55f), new Vector2(-95f, 95f), new Vector2(-55f, 40f), new Vector2(-15f, 105f),
            new Vector2(25f, 50f), new Vector2(65f, 90f), new Vector2(105f, 45f), new Vector2(135f, 85f)
        };

        private const float PulseLength = 1.2f;
        private const float PulseScale = 1.18f;
        private const float GestureLength = 1.6f;
        private const float HoldLength = 1.8f;
        private const float DragDistance = 200f;
        private const float SwipeDistance = 130f;
        private const float PressedScale = 0.8f;
        private const float SweepLength = 3.4f;
        private const float SweepStart = 0.25f;
        private const float SweepTime = 1.3f;
        private const float SweepHold = 0.3f;
        private const float PhoneTravel = 110f;
        public const float FootprintTravel = 135f;
        private const float PhoneTilt = 6f;
        private const float PhoneBob = 6f;
        private const float DotRest = 0.6f;
        private const float PulseStep = 0.85f;

        [MenuItem("Hauntscope/Build Tutorial Cues")]
        public static void BuildAll()
        {
            EnsureFolders();
            Save("CuePulse", BuildPulse());
            Save("TouchDrag", BuildGesture("m_AnchoredPosition.y", 0f, DragDistance));
            Save("TouchSwipe", BuildGesture("m_AnchoredPosition.x", SwipeDistance, -SwipeDistance));
            Save("TouchHold", BuildHold());
            Save("ScanSweep", BuildScanSweep());
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

        // The phone sweeps left to right and back; its camera cone swings to the floor patch it aims at, and each plane
        // dot flashes on as the patch passes over it and fades before the loop starts again.
        private static AnimationClip BuildScanSweep()
        {
            var clip = new AnimationClip();
            var back = SweepStart + SweepTime + SweepHold;
            var end = back + SweepTime;

            clip.SetCurve(PhonePath, typeof(RectTransform), "m_AnchoredPosition.x", Sweep(PhoneTravel, 0f));
            clip.SetCurve(PhonePath, typeof(RectTransform), "m_AnchoredPosition.y", Swing(PhoneBob, PhoneY));
            clip.SetCurve(PhonePath, typeof(Transform), "localEulerAnglesRaw.z", Swing(-PhoneTilt, 0f));

            // The cone leaves the camera bar and points at the patch, which travels further than the phone.
            var reach = FootprintY - PhoneY - LensOffsetY;
            var swing = Mathf.Atan2(FootprintTravel - PhoneTravel, reach) * Mathf.Rad2Deg;
            clip.SetCurve(ConePath, typeof(RectTransform), "m_AnchoredPosition.x", Sweep(PhoneTravel, 0f));
            clip.SetCurve(ConePath, typeof(RectTransform), "m_AnchoredPosition.y", Swing(PhoneBob, PhoneY + LensOffsetY));
            clip.SetCurve(ConePath, typeof(Transform), "localEulerAnglesRaw.z", Sweep(-swing, 0f));
            clip.SetCurve(ConePath, typeof(Image), "m_Color.a", Pulse(0.7f, 1f));

            clip.SetCurve(FootprintPath, typeof(RectTransform), "m_AnchoredPosition.x", Sweep(FootprintTravel, 0f));
            var ring = Pulse(0.92f, 1.08f);
            clip.SetCurve(RingPath, typeof(Transform), "m_LocalScale.x", ring);
            clip.SetCurve(RingPath, typeof(Transform), "m_LocalScale.y", ring);

            for (var i = 0; i < Dots.Length; i++)
            {
                var lit = SweepStart + SweepTime * InverseSmoothStep((Dots[i].x / FootprintTravel + 1f) * 0.5f);
                var path = $"{DotsPath}/Dot{i}";
                clip.SetCurve(path, typeof(Image), "m_Color.a", Curve((0f, 0f), (lit - 0.02f, 0f), (lit + 0.06f, 1f),
                    (lit + 0.45f, DotRest), (end - 0.1f, DotRest), (end + 0.15f, 0f), (SweepLength, 0f)));
                var pop = Curve((0f, 0.3f), (lit - 0.02f, 0.3f), (lit + 0.1f, 1.5f), (lit + 0.3f, 1f), (SweepLength, 1f));
                clip.SetCurve(path, typeof(Transform), "m_LocalScale.x", pop);
                clip.SetCurve(path, typeof(Transform), "m_LocalScale.y", pop);
            }

            return clip;
        }

        // Left, across to the right, hold, back to the left: the eased keys make it a smoothstep between the ends.
        private static AnimationCurve Sweep(float travel, float offset)
        {
            var back = SweepStart + SweepTime + SweepHold;
            return Curve((0f, offset - travel), (SweepStart, offset - travel), (SweepStart + SweepTime, offset + travel),
                (back, offset + travel), (back + SweepTime, offset - travel), (SweepLength, offset - travel));
        }

        // Peaks halfway through each sweep, with the sign of the direction of travel.
        private static AnimationCurve Swing(float amount, float offset)
        {
            var back = SweepStart + SweepTime + SweepHold;
            return Curve((0f, offset), (SweepStart, offset), (SweepStart + SweepTime * 0.5f, offset + amount),
                (SweepStart + SweepTime, offset), (back, offset), (back + SweepTime * 0.5f, offset - amount),
                (back + SweepTime, offset), (SweepLength, offset));
        }

        private static AnimationCurve Pulse(float low, float high)
        {
            var curve = new AnimationCurve();
            var steps = Mathf.RoundToInt(SweepLength / PulseStep);
            for (var i = 0; i <= steps; i++)
                curve.AddKey(SweepLength * i / steps, i % 2 == 0 ? low : high);
            return Smooth(curve);
        }

        // Inverse of the ease between two flat keys (a smoothstep), so a dot lights exactly when the patch reaches it.
        private static float InverseSmoothStep(float value)
        {
            return 0.5f - Mathf.Sin(Mathf.Asin(1f - 2f * Mathf.Clamp01(value)) / 3f);
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

            return Smooth(curve);
        }

        private static AnimationCurve Smooth(AnimationCurve curve)
        {
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
