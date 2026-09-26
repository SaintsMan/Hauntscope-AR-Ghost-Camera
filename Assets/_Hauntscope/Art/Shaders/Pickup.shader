Shader "Hauntscope/Pickup"
{
    Properties
    {
        _Color ("Color", Color) = (0.24, 1, 0.43, 1)
        _Fill ("Body Fill", Range(0, 1)) = 0.18
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.2
        _RimIntensity ("Rim Intensity", Range(0, 4)) = 1.8
        _Whiten ("Core Whiten", Range(0, 1)) = 0.25
        _ScanDensity ("Scanline Density", Float) = 90
        _ScanSpeed ("Scanline Speed", Float) = 1.4
        _ScanStrength ("Scanline Strength", Range(0, 1)) = 0.35
        _SpinSpeed ("Spin Speed", Float) = 0.9
        _BobAmplitude ("Bob Amplitude", Float) = 0.025
        _BobFrequency ("Bob Frequency", Float) = 0.6
        _Flicker ("Flicker", Range(0, 1)) = 0.12
        _Visibility ("Visibility", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Pickup"
            Tags { "LightMode" = "UniversalForward" }

            // Premultiplied like the ghosts: a hologram adds light over the camera feed and stays readable without Bloom.
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Fill;
                half _RimPower;
                half _RimIntensity;
                half _Whiten;
                float _ScanDensity;
                float _ScanSpeed;
                half _ScanStrength;
                float _SpinSpeed;
                float _BobAmplitude;
                float _BobFrequency;
                half _Flicker;
                half _Visibility;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float phase : TEXCOORD2;
            };

            float2 Rotate(float2 v, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float2(v.x * c - v.y * s, v.x * s + v.y * c);
            }

            // Spin and bob live in the vertex stage, so a pickup animates with no per-frame script.
            Varyings Vert(Attributes input)
            {
                float3 origin = float3(UNITY_MATRIX_M._m03, UNITY_MATRIX_M._m13, UNITY_MATRIX_M._m23);
                float phase = frac(sin(dot(origin.xz, float2(12.9898, 78.233))) * 43758.5453) * TWO_PI;
                float angle = _Time.y * _SpinSpeed + phase;

                float3 position = input.positionOS.xyz;
                position.xz = Rotate(position.xz, angle);
                position.y += sin(_Time.y * _BobFrequency * TWO_PI + phase) * _BobAmplitude;
                float3 normal = input.normalOS;
                normal.xz = Rotate(normal.xz, angle);

                VertexPositionInputs positions = GetVertexPositionInputs(position);
                Varyings output;
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(normal);
                output.phase = phase;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);
                float3 viewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half fresnel = pow(1.0h - saturate(dot(normal, viewDirection)), _RimPower);

                half scan = 1.0h - _ScanStrength * step(0.5h, frac(input.positionWS.y * _ScanDensity - _Time.y * _ScanSpeed));
                half flicker = 1.0h - _Flicker * step(0.93h, frac(sin(floor(_Time.y * 24.0 + input.phase) * 91.7) * 43758.5));

                half body = _Fill;
                half rim = fresnel * _RimIntensity;
                half3 tint = lerp(_Color.rgb, half3(1.0h, 1.0h, 1.0h), _Whiten * saturate(body * 2.0h));
                half3 color = (tint * body + _Color.rgb * rim) * scan * flicker;
                half alpha = saturate(body + rim) * scan;
                return half4(color * _Visibility, alpha * _Visibility * _Color.a);
            }
            ENDHLSL
        }
    }
}
