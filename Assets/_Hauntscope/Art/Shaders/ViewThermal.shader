// Thermal camera over the whole viewfinder (GDD 5.33.3). There is no heat sensor, so the picture's brightness stands in
// for temperature, read through a coarse sensor grid and mapped to the classic iron palette — lit things glow hot, dark
// corners cool to violet, but never down to the coldest band: that is kept for ghosts. The ghost's own thermal pass
// draws it in a marker colour (saturated blue); here that marker becomes an ice-white shape with a cold halo.
Shader "Hauntscope/ViewThermal"
{
    Properties
    {
        _Gain ("Gain", Range(0.5, 4)) = 1.7
        _Lift ("Lift", Range(0, 0.3)) = 0.03
        _RoomMin ("Room Minimum Heat", Range(0, 0.6)) = 0.22
        _Pixel ("Sensor Cell (px)", Range(1, 12)) = 5
        _Noise ("Noise", Range(0, 0.2)) = 0.035
        _Vignette ("Vignette", Range(0, 2)) = 0.55
        _MarkerThreshold ("Cold Marker Threshold", Range(0, 1)) = 0.5
        _MarkerSoftness ("Cold Marker Softness", Range(0.01, 1)) = 0.3
        _IceDeep ("Ice Deep", Color) = (0.05, 0.35, 0.95, 1)
        _Ice ("Ice", Color) = (0.7, 0.98, 1, 1)
        _Halo ("Cold Halo", Range(0, 1)) = 0.55
        _HaloCells ("Cold Halo Reach (cells)", Range(1, 8)) = 3
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
            Name "Thermal"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            half _Gain;
            half _Lift;
            half _RoomMin;
            half _Pixel;
            half _Noise;
            half _Vignette;
            half _MarkerThreshold;
            half _MarkerSoftness;
            half4 _IceDeep;
            half4 _Ice;
            half _Halo;
            half _HaloCells;

            float Hash(float2 p)
            {
                p = frac(p * float2(443.897, 441.423));
                p += dot(p, p.yx + 19.19);
                return frac((p.x + p.y) * p.x);
            }

            // Iron palette: black-violet, violet, crimson, orange, yellow-white.
            half3 Iron(half t)
            {
                const half3 c0 = half3(0.02, 0.01, 0.10);
                const half3 c1 = half3(0.30, 0.04, 0.55);
                const half3 c2 = half3(0.82, 0.08, 0.34);
                const half3 c3 = half3(1.00, 0.46, 0.05);
                const half3 c4 = half3(1.00, 0.96, 0.62);
                half3 color = lerp(c0, c1, saturate(t * 4.0));
                color = lerp(color, c2, saturate(t * 4.0 - 1.0));
                color = lerp(color, c3, saturate(t * 4.0 - 2.0));
                return lerp(color, c4, saturate(t * 4.0 - 3.0));
            }

            half ColdMarker(half3 color)
            {
                half blueness = color.b - max(color.r, color.g);
                return saturate((blueness - _MarkerThreshold) / _MarkerSoftness);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 cell = _Pixel / _ScreenParams.xy;
                float2 uv = (floor(input.texcoord / cell) + 0.5) * cell;
                half3 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;

                half luma = dot(color, half3(0.299, 0.587, 0.114));
                half heat = lerp(_RoomMin, 1.0, saturate(luma * _Gain + _Lift));
                heat = saturate(heat + (Hash(floor(input.texcoord / cell) + floor(_Time.y * 12.0)) - 0.5) * _Noise);
                half3 result = Iron(heat);

                // The ghost: its core ice-white, its edge a deep cold blue, and the cold bleeding into the room around it.
                half cold = ColdMarker(color);
                float2 reach = cell * _HaloCells;
                half halo = max(max(ColdMarker(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(reach.x, 0)).rgb),
                                    ColdMarker(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(reach.x, 0)).rgb)),
                                max(ColdMarker(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, reach.y)).rgb),
                                    ColdMarker(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(0, reach.y)).rgb)));
                result = lerp(result, _IceDeep.rgb * 0.8, halo * _Halo * (1.0 - cold));
                half3 ice = lerp(_IceDeep.rgb, _Ice.rgb, cold * cold);
                result = lerp(result, ice, smoothstep(0.0, 0.35, cold));

                float2 centered = input.texcoord - 0.5;
                centered.x *= _ScreenParams.x / _ScreenParams.y;
                result *= saturate(1.0 - dot(centered, centered) * _Vignette);
                return half4(result, 1.0);
            }
            ENDHLSL
        }
    }
}
