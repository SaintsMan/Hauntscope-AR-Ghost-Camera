// UV flashlight over the whole viewfinder (GDD 5.33.4): the room is lit only by a violet beam — a pool of light in the
// middle, near darkness around it — and pale things fluoresce faintly blue. Ectoplasm decals are drawn in a marker green
// that this filter turns into bright fluorescence, so the trail glows on its own in the beam.
Shader "Hauntscope/ViewUv"
{
    Properties
    {
        _Tint ("UV Tint", Color) = (0.42, 0.18, 1, 1)
        _Dark ("Outside the Beam", Range(0, 1)) = 0.14
        _Lit ("Inside the Beam", Range(0, 2)) = 1.1
        _SpotRadius ("Beam Radius (screen heights)", Range(0.05, 0.7)) = 0.3
        _SpotSoft ("Beam Edge", Range(0.005, 0.3)) = 0.07
        _Fluor ("Fluorescence", Color) = (0.72, 0.78, 1, 1)
        _FluorStrength ("Fluorescence Strength", Range(0, 2)) = 0.5
        _Glow ("Ectoplasm Glow", Color) = (0.5, 1, 0.78, 1)
        _GlowGain ("Ectoplasm Gain", Range(0.5, 3)) = 1.4
        _MarkerThreshold ("Marker Threshold", Range(0, 1)) = 0.3
        _MarkerSoftness ("Marker Softness", Range(0.01, 1)) = 0.3
        _Grain ("Grain", Range(0, 0.3)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Name "Uv"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            half4 _Tint;
            half _Dark;
            half _Lit;
            half _SpotRadius;
            half _SpotSoft;
            half4 _Fluor;
            half _FluorStrength;
            half4 _Glow;
            half _GlowGain;
            half _MarkerThreshold;
            half _MarkerSoftness;
            half _Grain;

            float Hash(float2 p)
            {
                p = frac(p * float2(443.897, 441.423));
                p += dot(p, p.yx + 19.19);
                return frac((p.x + p.y) * p.x);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                half3 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;
                half luma = dot(color, half3(0.299, 0.587, 0.114));

                float2 centered = uv - 0.5;
                centered.x *= _ScreenParams.x / _ScreenParams.y;
                half spot = 1.0 - smoothstep(_SpotRadius - _SpotSoft, _SpotRadius + _SpotSoft, length(centered));
                half light = lerp(_Dark, _Lit, spot);

                half3 room = _Tint.rgb * luma * light;
                room += _Fluor.rgb * pow(luma, 3.0) * _FluorStrength * spot;

                half marker = saturate((color.g - max(color.r, color.b) - _MarkerThreshold) / _MarkerSoftness);
                half3 glow = _Glow.rgb * _GlowGain;
                half3 result = lerp(room, glow, marker) + glow * marker * 0.25;

                result += (Hash(floor(uv * _ScreenParams.xy * 0.5) + floor(_Time.y * 24.0)) - 0.5) * _Grain;
                return half4(saturate(result), 1.0);
            }
            ENDHLSL
        }
    }
}
