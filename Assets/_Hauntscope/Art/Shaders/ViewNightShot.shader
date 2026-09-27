// Night vision over the whole viewfinder (GDD 5.33.2): the picture's luminance is amplified like an image
// intensifier, mapped to phosphor green, with sensor grain, faint scanlines, a round tube vignette and highlights that
// bloom white. Drawn by UrpViewFilter as a full-screen blit; the HUD canvas stays on top untouched.
Shader "Hauntscope/ViewNightShot"
{
    Properties
    {
        _Phosphor ("Phosphor", Color) = (0.55, 1, 0.55, 1)
        _Shadow ("Shadow", Color) = (0.01, 0.06, 0.02, 1)
        _Gain ("Gain", Range(0.5, 6)) = 2.8
        _Lift ("Lift", Range(0, 0.3)) = 0.05
        _Gamma ("Gamma", Range(0.3, 2)) = 0.75
        _Grain ("Grain", Range(0, 0.5)) = 0.14
        _GrainSize ("Grain Size (px)", Range(1, 6)) = 2
        _Scanlines ("Scanlines", Range(0, 0.5)) = 0.07
        _Vignette ("Tube Vignette", Range(0, 3)) = 1.3
        _Bloom ("Highlight Bloom", Range(0, 1)) = 0.4
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
            Name "NightShot"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            half4 _Phosphor;
            half4 _Shadow;
            half _Gain;
            half _Lift;
            half _Gamma;
            half _Grain;
            half _GrainSize;
            half _Scanlines;
            half _Vignette;
            half _Bloom;

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
                half amplified = pow(saturate(luma * _Gain + _Lift), _Gamma);

                // Grain changes every frame and is coarser than a pixel, like an intensifier tube's speckle.
                float2 cell = floor(uv * _ScreenParams.xy / _GrainSize);
                float grain = Hash(cell + floor(_Time.y * 30.0) * 17.31) - 0.5;
                amplified = saturate(amplified + grain * _Grain);

                half scan = 1.0 - _Scanlines * (0.5 + 0.5 * sin(uv.y * _ScreenParams.y * 1.4));
                float2 centered = uv - 0.5;
                centered.x *= _ScreenParams.x / _ScreenParams.y;
                half vignette = saturate(1.0 - dot(centered, centered) * _Vignette);

                half3 result = lerp(_Shadow.rgb, _Phosphor.rgb, amplified) * scan * vignette;
                result += smoothstep(0.75, 1.0, amplified) * _Bloom * vignette;
                return half4(result, 1.0);
            }
            ENDHLSL
        }
    }
}
