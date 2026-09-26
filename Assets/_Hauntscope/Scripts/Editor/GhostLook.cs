using UnityEngine;

namespace Hauntscope.Editor
{
    // How one ghost's body and eyes are drawn (GDD 5.32): translucency, rim, smoke and how its body moves. The values
    // go into that ghost's own materials, so the shared shader stays one.
    internal sealed class GhostLook
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int RimColorId = Shader.PropertyToID("_RimColor");

        public float BaseAlpha { get; private set; } = 0.22f;

        public float RimPower { get; private set; } = 2.5f;

        public float RimIntensity { get; private set; } = 1.6f;

        public float NoiseStrength { get; private set; } = 0.55f;

        public float NoiseScale { get; private set; } = 3f;

        public float Wobble { get; private set; } = 0.02f;

        public float Breath { get; private set; } = 0.008f;

        public float BreathSpeed { get; private set; } = 1.3f;

        public float HemFlutter { get; private set; } = 0.03f;

        public float HemHeight { get; private set; } = 0.35f;

        public float HemFrequency { get; private set; } = 3.5f;

        public float Sway { get; private set; } = 0.03f;

        public float SwaySpeed { get; private set; } = 0.7f;

        public float Agitation { get; private set; } = 0.012f;

        public Color EyeColor { get; private set; } = new Color(1f, 1f, 1f, 0.95f);

        // Normally the body is tinted with the rim colour; a set body colour stays as it is (the negative's dark body).
        public Color? BodyColor { get; private set; }

        public GhostLook Body(float baseAlpha, float rimPower, float rimIntensity, float noiseStrength = 0.55f, float noiseScale = 3f)
        {
            BaseAlpha = baseAlpha;
            RimPower = rimPower;
            RimIntensity = rimIntensity;
            NoiseStrength = noiseStrength;
            NoiseScale = noiseScale;
            return this;
        }

        public GhostLook Hem(float flutter, float height, float frequency)
        {
            HemFlutter = flutter;
            HemHeight = height;
            HemFrequency = frequency;
            return this;
        }

        public GhostLook Motion(float breath, float breathSpeed, float sway, float swaySpeed, float wobble = 0.02f, float agitation = 0.012f)
        {
            Breath = breath;
            BreathSpeed = breathSpeed;
            Sway = sway;
            SwaySpeed = swaySpeed;
            Wobble = wobble;
            Agitation = agitation;
            return this;
        }

        public GhostLook Eyes(Color color)
        {
            EyeColor = color;
            return this;
        }

        public GhostLook Colored(Color body)
        {
            BodyColor = body;
            return this;
        }

        public void ApplyBody(Material material, Color rim, Bounds body)
        {
            var tint = BodyColor ?? rim;
            material.SetColor(BaseColorId, new Color(tint.r, tint.g, tint.b, BodyColor.HasValue ? BodyColor.Value.a : BaseAlpha));
            material.SetColor(RimColorId, rim);
            material.SetFloat("_RimPower", RimPower);
            material.SetFloat("_RimIntensity", RimIntensity);
            material.SetFloat("_NoiseStrength", NoiseStrength);
            material.SetFloat("_NoiseScale", NoiseScale);
            material.SetFloat("_WobbleAmplitude", Wobble);
            material.SetFloat("_Reveal", 0f);
            ApplyMotion(material, body);
        }

        // The eyes share the body's motion so they stay on the face; they neither wobble nor glow at the rim.
        public void ApplyEyes(Material material, Bounds body)
        {
            material.SetColor(BaseColorId, EyeColor);
            material.SetColor(RimColorId, EyeColor);
            material.SetFloat("_RimIntensity", 0f);
            material.SetFloat("_NoiseStrength", 0f);
            material.SetFloat("_WobbleAmplitude", 0f);
            material.SetFloat("_Reveal", 0f);
            ApplyMotion(material, body);
            material.SetFloat("_BreathAmplitude", 0f);
        }

        private void ApplyMotion(Material material, Bounds body)
        {
            material.SetFloat("_BodyBottom", body.min.y);
            material.SetFloat("_BodyTop", body.max.y);
            material.SetFloat("_BreathAmplitude", Breath);
            material.SetFloat("_BreathSpeed", BreathSpeed);
            material.SetFloat("_HemFlutter", HemFlutter);
            material.SetFloat("_HemHeight", HemHeight);
            material.SetFloat("_HemFrequency", HemFrequency);
            material.SetFloat("_SwayAmplitude", Sway);
            material.SetFloat("_SwaySpeed", SwaySpeed);
            material.SetFloat("_AgitationAmplitude", Agitation);
        }
    }
}
