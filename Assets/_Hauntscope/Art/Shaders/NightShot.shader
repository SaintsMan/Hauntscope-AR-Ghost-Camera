Shader "Hauntscope/NightShot"
{
    Properties
    {
        _BaseColor ("Albedo", Color) = (0.5, 0.5, 0.5, 1)
        _BaseMap ("Albedo Map", 2D) = "white" {}
        _EmissionStrength ("Emission", Range(0, 4)) = 0
        _EmissionFlicker ("Emission Flicker", Float) = 1
        _Ambient ("Ambient", Range(0, 0.5)) = 0.03
        _Exposure ("Exposure", Range(0.1, 8)) = 3.4
        _NightTint ("Night Tint", Color) = (0.902, 0.929, 0.953, 1)
        _ShadowTint ("Shadow Tint", Color) = (0.043, 0.059, 0.078, 1)
        _DarkStart ("Darkness Start", Float) = 2.5
        _DarkEnd ("Darkness End", Float) = 11
        _GrimeScale ("Grime Scale", Float) = 1.7
        _Grime ("Grime", Range(0, 1)) = 0.55
        _Streaks ("Wall Streaks", Range(0, 1)) = 0.45
        _FloorY ("Floor Height", Float) = 0
        _CeilingY ("Ceiling Height", Float) = 2.58
        _ContactDarkness ("Contact Darkness", Range(0, 1)) = 0.7
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "NightShot"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _EmissionStrength;
                half _EmissionFlicker;
                half _Ambient;
                half _Exposure;
                half4 _NightTint;
                half4 _ShadowTint;
                float _DarkStart;
                float _DarkEnd;
                float _GrimeScale;
                half _Grime;
                half _Streaks;
                float _FloorY;
                float _CeilingY;
                half _ContactDarkness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            float Hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float Noise(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(lerp(Hash(i), Hash(i + float3(1, 0, 0)), f.x),
                         lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                    lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x),
                         lerp(Hash(i + float3(0, 1, 1)), Hash(i + float3(1, 1, 1)), f.x), f.y), f.z);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half Grime(float3 positionWS, float3 normalWS)
            {
                // Stains at two scales plus vertical water streaks on walls: the kit's flat colours read as
                // decades of neglect instead of clean plastic.
                float3 p = positionWS * _GrimeScale;
                half stains = Noise(p) * 0.65 + Noise(p * 3.7) * 0.35;
                half wall = 1.0h - saturate(abs(normalWS.y) * 2.0h);
                half streaks = Noise(float3(p.x * 9.0, p.y * 0.35, p.z * 9.0));
                half dirt = lerp(1.0h, 0.35h + stains, _Grime);
                return saturate(dirt * lerp(1.0h, 0.55h + streaks * 0.6h, _Streaks * wall));
            }

            half Contact(float3 positionWS, float3 normalWS)
            {
                // Fake ambient occlusion by height: vertical faces darken where they meet the floor and ceiling.
                half vertical = 1.0h - saturate(abs(normalWS.y));
                half floorShade = smoothstep(0.0, 0.45, positionWS.y - _FloorY);
                half ceilingShade = smoothstep(0.0, 0.6, _CeilingY - positionWS.y);
                return lerp(1.0h, floorShade * ceilingShade, _ContactDarkness * vertical);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                albedo *= Grime(input.positionWS, normalWS) * Contact(input.positionWS, normalWS);

                // Only the brightness of each light matters: the camcorder's night mode records in monochrome.
                half light = _Ambient;
            #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                for (uint i = 0u; i < count; ++i)
                {
                    Light additional = GetAdditionalLight(i, input.positionWS);
                    half diffuse = saturate(dot(normalWS, additional.direction));
                    light += Luminance(additional.color) * additional.distanceAttenuation * diffuse;
                }
            #endif

                half3 color = albedo * (light * _Exposure + _EmissionStrength * _EmissionFlicker);
                half value = Luminance(color);
                // Soft shoulder like a sensor at high gain: bright spots bloom into white instead of clipping flat.
                value = saturate(2.0h * value / (1.0h + value));
                // The curve above is perceptual; squaring brings it back to linear so the sRGB output doesn't lift
                // the shadows and flatten the illuminator's falloff.
                value *= value;

                // The illuminator only reaches a few metres; beyond that the footage sinks into the noise floor.
                float distanceToCamera = distance(_WorldSpaceCameraPos, input.positionWS);
                half darkness = saturate((distanceToCamera - _DarkStart) / max(_DarkEnd - _DarkStart, 0.001));
                value *= 1.0h - darkness * darkness;

                return half4(lerp(_ShadowTint.rgb, _NightTint.rgb, value), 1.0h);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            float4 DepthVert(Attributes input) : SV_POSITION
            {
                return TransformObjectToHClip(input.positionOS.xyz);
            }

            half DepthFrag() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
