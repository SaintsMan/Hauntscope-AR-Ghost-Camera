using Hauntscope.Gameplay.Config;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.Editor
{
    // The splash is a camcorder powering on: a CRT line opens into the picture, the focus brackets lock in,
    // the logo tears in through VHS tracking noise, and on exit the picture collapses like an old TV switching off.
    // Clips are generated as data for the Animator on the SplashView root, so the game runs no animation code.
    public static class SplashAnimationGenerator
    {
        public const string Folder = "Assets/_Hauntscope/Art/Animations/Splash";
        public const string OutroTrigger = "Outro";

        public const string ScreenPath = "Screen";
        public const string CrtPath = "Crt";
        public const string GlowPath = "Screen/Glow";
        public const string GhostPath = "Screen/GhostAnchor/Ghost";
        public const string LogoPath = "Screen/Logo";
        public const string TitlePath = "Screen/Logo/Title";
        public const string SplitWarmPath = "Screen/Logo/SplitWarm";
        public const string SplitColdPath = "Screen/Logo/SplitCold";
        public const string SubtitlePath = "Screen/Subtitle";
        public const string BandPath = "Screen/BandTrack/Band";
        public const string BracketsPath = "Screen/SafeArea/Brackets";
        public const string TopBarPath = "Screen/SafeArea/TopBar";
        public const string DotPath = "Screen/SafeArea/TopBar/Status/Dot";
        public const string BootPath = "Screen/SafeArea/Boot";

        public static readonly string[] BracketCorners = { "TopLeft", "TopRight", "BottomLeft", "BottomRight" };

        public const float BandTravel = 1400f;
        public const float BandAlpha = 0.1f;
        public const float GhostAlpha = 0.14f;
        public const float GlowAlpha = 0.1f;
        public const float SplitAlpha = 0.5f;
        public const float SplitRest = 4f;

        private const float IdleLength = 3.2f;
        private const float CrtLine = 0.006f;
        private const float BracketTravel = 90f;
        private const float GhostDrift = 16f;

        [MenuItem("Hauntscope/Build Splash Animation")]
        public static void BuildFromConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(HauntscopeAssetBuilder.GameConfigPath);
            Build(config.Boot);
        }

        public static void Build(BootConfig config)
        {
            EnsureFolder();
            var intro = Save("SplashIntro", BuildIntro(config.IntroDuration), false);
            var idle = Save("SplashIdle", BuildIdle(), true);
            var outro = Save("SplashOutro", BuildOutro(config.OutroDuration), false);
            BuildController(intro, idle, outro);
            AssetDatabase.SaveAssets();
        }

        public static RuntimeAnimatorController Controller()
        {
            return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{Folder}/Splash.controller");
        }

        private static AnimationClip BuildIntro(float length)
        {
            var clip = new AnimationClip();

            // Power-on: a bright line draws across, opens vertically into the picture and fades.
            Scale(clip, CrtPath, Curve((0f, 0f), (0.08f, 1f)), Curve((0f, CrtLine), (0.1f, CrtLine), (0.2f, 1f)));
            ImageAlpha(clip, CrtPath, Curve((0f, 1f), (0.1f, 1f), (0.15f, 0.4f), (0.3f, 0f), (length, 0f)));
            GroupAlpha(clip, ScreenPath, Step((0f, 0f), (0.1f, 1f), (0.15f, 0.35f), (0.19f, 1f)));

            GroupAlpha(clip, BracketsPath, Curve((0f, 0f), (0.2f, 0f), (0.32f, 1f)));
            for (var i = 0; i < BracketCorners.Length; i++)
            {
                var sx = i % 2 == 0 ? -1f : 1f;
                var sy = i < 2 ? 1f : -1f;
                var path = $"{BracketsPath}/{BracketCorners[i]}/Bracket";
                Position(clip, path,
                    Curve((0f, sx * BracketTravel), (0.2f, sx * BracketTravel), (0.46f, -sx * 8f), (0.58f, 0f)),
                    Curve((0f, sy * BracketTravel), (0.2f, sy * BracketTravel), (0.46f, -sy * 8f), (0.58f, 0f)));
            }

            // The logo tears in: hard flicker, colour channels split apart and settle.
            GroupAlpha(clip, LogoPath, Step((0f, 0f), (0.3f, 1f), (0.34f, 0.15f), (0.38f, 1f), (0.44f, 0f), (0.47f, 1f)));
            PositionX(clip, TitlePath, Step((0f, 0f), (0.3f, 14f), (0.35f, -9f), (0.41f, 0f), (0.49f, 6f), (0.54f, 0f)));
            PositionX(clip, SplitWarmPath, Step((0f, -40f), (0.38f, -22f), (0.47f, 18f), (0.56f, -10f), (0.7f, -SplitRest)));
            PositionX(clip, SplitColdPath, Step((0f, 40f), (0.38f, 22f), (0.47f, -18f), (0.56f, 10f), (0.7f, SplitRest)));

            GroupAlpha(clip, SubtitlePath, Curve((0f, 0f), (0.55f, 0f), (0.75f, 1f)));
            GroupAlpha(clip, TopBarPath, Step((0f, 0f), (0.5f, 1f), (0.55f, 0f), (0.6f, 1f)));
            GroupAlpha(clip, BootPath, Curve((0f, 0f), (0.7f, 0f), (0.9f, 1f)));

            ImageAlpha(clip, GhostPath, Curve((0f, 0f), (0.4f, 0f), (0.62f, GhostAlpha * 1.6f), (0.66f, 0.02f), (length, GhostAlpha)));
            PositionY(clip, GhostPath, Curve((0f, -30f), (length, 0f)));
            ImageAlpha(clip, GlowPath, Curve((0f, 0f), (0.3f, 0f), (0.8f, GlowAlpha)));

            PositionY(clip, BandPath, Curve((0f, BandTravel), (0.1f, BandTravel), (0.7f, -BandTravel), (length, -BandTravel)));
            ImageAlpha(clip, BandPath, Curve((0f, BandAlpha * 2.5f), (length, BandAlpha * 2.5f)));
            return clip;
        }

        private static AnimationClip BuildIdle()
        {
            var clip = new AnimationClip();

            PositionY(clip, GhostPath, Curve((0f, 0f), (0.8f, GhostDrift), (1.6f, 0f), (2.4f, -GhostDrift), (IdleLength, 0f)));
            ImageAlpha(clip, GhostPath, Curve((0f, GhostAlpha), (1.9f, GhostAlpha), (1.93f, GhostAlpha * 2.3f),
                (1.97f, GhostAlpha * 0.4f), (2.02f, GhostAlpha * 1.5f), (2.1f, GhostAlpha), (IdleLength, GhostAlpha)));
            ImageAlpha(clip, GlowPath, Curve((0f, GlowAlpha * 0.8f), (IdleLength * 0.5f, GlowAlpha * 1.2f), (IdleLength, GlowAlpha * 0.8f)));

            PositionY(clip, BandPath, Linear((0f, BandTravel), (IdleLength, -BandTravel)));
            ImageAlpha(clip, BandPath, Curve((0f, BandAlpha), (IdleLength, BandAlpha)));

            // One tracking glitch per loop, timed with the ghost's flicker.
            PositionX(clip, TitlePath, Step((0f, 0f), (2.3f, 10f), (2.35f, -6f), (2.4f, 0f)));
            PositionX(clip, SplitWarmPath, Step((0f, -SplitRest), (2.3f, -26f), (2.36f, 14f), (2.44f, -SplitRest)));
            PositionX(clip, SplitColdPath, Step((0f, SplitRest), (2.3f, 26f), (2.36f, -14f), (2.44f, SplitRest)));

            ImageAlpha(clip, DotPath, Step((0f, 1f), (0.5f, 0f), (1f, 1f), (1.5f, 0f), (2f, 1f), (2.5f, 0f), (3f, 1f), (IdleLength, 1f)));
            return clip;
        }

        private static AnimationClip BuildOutro(float length)
        {
            var clip = new AnimationClip();
            var collapse = length * 0.47f;
            var shrink = length * 0.76f;

            PositionX(clip, TitlePath, Step((0f, 14f), (0.04f, -10f), (0.08f, 0f)));
            PositionX(clip, SplitWarmPath, Step((0f, -30f), (0.05f, 20f), (0.09f, -SplitRest)));
            PositionX(clip, SplitColdPath, Step((0f, 30f), (0.05f, -20f), (0.09f, SplitRest)));

            // Power-off: the picture squeezes into a glowing line, the line into a dot, the dot fades out.
            Scale(clip, ScreenPath, Curve((0f, 1f), (collapse, 1f), (shrink, 0f)), Curve((0f, 1f), (0.08f, 1f), (collapse, CrtLine)));
            Scale(clip, CrtPath, Curve((0f, 1f), (collapse, 1f), (shrink, 0.02f)), Curve((0f, 1f), (0.08f, 1f), (collapse, CrtLine)));
            ImageAlpha(clip, CrtPath, Curve((0f, 0f), (0.08f, 0f), (collapse * 0.6f, 0.2f), (collapse, 1f), (shrink, 1f), (length, 0f)));
            return clip;
        }

        private static void BuildController(AnimationClip intro, AnimationClip idle, AnimationClip outro)
        {
            // Rebuilt in place: recreating the asset would change its GUID and break the Splash scene's Animator.
            var path = $"{Folder}/Splash.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path)
                ?? AnimatorController.CreateAnimatorControllerAtPath(path);
            foreach (var parameter in controller.parameters)
                controller.RemoveParameter(parameter);
            controller.AddParameter(OutroTrigger, AnimatorControllerParameterType.Trigger);

            var machine = controller.layers[0].stateMachine;
            foreach (var child in machine.states)
                machine.RemoveState(child.state);
            foreach (var transition in machine.anyStateTransitions)
                machine.RemoveAnyStateTransition(transition);
            var introState = machine.AddState("Intro");
            introState.motion = intro;
            var idleState = machine.AddState("Idle");
            idleState.motion = idle;
            var outroState = machine.AddState("Outro");
            outroState.motion = outro;
            machine.defaultState = introState;

            var toIdle = introState.AddTransition(idleState);
            toIdle.hasExitTime = true;
            toIdle.exitTime = 1f;
            toIdle.duration = 0f;

            foreach (var from in new[] { introState, idleState })
            {
                var toOutro = from.AddTransition(outroState);
                toOutro.hasExitTime = false;
                toOutro.duration = 0f;
                toOutro.AddCondition(AnimatorConditionMode.If, 0f, OutroTrigger);
            }

            EditorUtility.SetDirty(controller);
        }

        private static void Scale(AnimationClip clip, string path, AnimationCurve x, AnimationCurve y)
        {
            clip.SetCurve(path, typeof(RectTransform), "m_LocalScale.x", x);
            clip.SetCurve(path, typeof(RectTransform), "m_LocalScale.y", y);
        }

        private static void Position(AnimationClip clip, string path, AnimationCurve x, AnimationCurve y)
        {
            PositionX(clip, path, x);
            PositionY(clip, path, y);
        }

        private static void PositionX(AnimationClip clip, string path, AnimationCurve curve)
        {
            clip.SetCurve(path, typeof(RectTransform), "m_AnchoredPosition.x", curve);
        }

        private static void PositionY(AnimationClip clip, string path, AnimationCurve curve)
        {
            clip.SetCurve(path, typeof(RectTransform), "m_AnchoredPosition.y", curve);
        }

        private static void ImageAlpha(AnimationClip clip, string path, AnimationCurve curve)
        {
            clip.SetCurve(path, typeof(Image), "m_Color.a", curve);
        }

        private static void GroupAlpha(AnimationClip clip, string path, AnimationCurve curve)
        {
            clip.SetCurve(path, typeof(CanvasGroup), "m_Alpha", curve);
        }

        private static AnimationCurve Curve(params (float time, float value)[] keys)
        {
            return Build(keys, AnimationUtility.TangentMode.ClampedAuto);
        }

        private static AnimationCurve Linear(params (float time, float value)[] keys)
        {
            return Build(keys, AnimationUtility.TangentMode.Linear);
        }

        // Glitches jump between values: a smooth curve would read as motion, not as a broken signal.
        private static AnimationCurve Step(params (float time, float value)[] keys)
        {
            return Build(keys, AnimationUtility.TangentMode.Constant);
        }

        private static AnimationCurve Build((float time, float value)[] keys, AnimationUtility.TangentMode mode)
        {
            var curve = new AnimationCurve();
            foreach (var key in keys)
                curve.AddKey(key.time, key.value);

            for (var i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, mode);
                AnimationUtility.SetKeyRightTangentMode(curve, i, mode);
            }

            return curve;
        }

        private static AnimationClip Save(string name, AnimationClip built, bool loop)
        {
            var path = $"{Folder}/{name}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            EditorUtility.CopySerialized(built, clip);
            clip.name = name;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            Object.DestroyImmediate(built);
            return clip;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Hauntscope/Art/Animations"))
                AssetDatabase.CreateFolder("Assets/_Hauntscope/Art", "Animations");
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/_Hauntscope/Art/Animations", "Splash");
        }
    }
}
