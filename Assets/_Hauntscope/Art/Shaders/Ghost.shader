Shader "Hauntscope/Ghost"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.31, 0.96, 0.9, 0.22)
        _RimColor ("Rim Color", Color) = (0.31, 0.96, 0.9, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.5
        _RimIntensity ("Rim Intensity", Range(0, 4)) = 1.6
        _NoiseScale ("Smoke Scale", Float) = 3
        _NoiseSpeed ("Smoke Scroll Speed", Vector) = (0, 0.35, 0.1, 0)
        _NoiseStrength ("Smoke Strength", Range(0, 1)) = 0.55
        _WobbleAmplitude ("Wobble Amplitude", Float) = 0.02
        _WobbleFrequency ("Wobble Frequency", Float) = 2.5
        _Reveal ("Reveal", Range(0, 1)) = 1
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (1, 1, 1, 1)
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0.001, 0.3)) = 0.08
        _DepthReveal ("Depth Write From Reveal", Range(0, 1)) = 0.05
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

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _RimColor;
            half _RimPower;
            half _RimIntensity;
            float _NoiseScale;
            float4 _NoiseSpeed;
            half _NoiseStrength;
            float _WobbleAmplitude;
            float _WobbleFrequency;
            half _Reveal;
            half _Dissolve;
            half4 _DissolveEdgeColor;
            half _DissolveEdgeWidth;
            half _DepthReveal;
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
            float3 noisePosition : TEXCOORD2;
        };

        float Hash(float3 p)
        {
            p = frac(p * 0.3183099 + 0.1);
            p *= 17.0;
            return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
        }

        float ValueNoise(float3 x)
        {
            float3 i = floor(x);
            float3 f = frac(x);
            f = f * f * (3.0 - 2.0 * f);

            return lerp(
                lerp(lerp(Hash(i + float3(0, 0, 0)), Hash(i + float3(1, 0, 0)), f.x),
                     lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x),
                     lerp(Hash(i + float3(0, 1, 1)), Hash(i + float3(1, 1, 1)), f.x), f.y),
                f.z);
        }

        float Fbm(float3 p)
        {
            float value = 0.0;
            float amplitude = 0.5;
            for (int octave = 0; octave < 3; octave++)
            {
                value += amplitude * ValueNoise(p);
                p *= 2.03;
                amplitude *= 0.5;
            }

            return value / 0.875;
        }

        // Both passes must place every vertex identically, or the depth pass would hide slivers of the body.
        Varyings Vert(Attributes input)
        {
            float3 position = input.positionOS.xyz;
            float wobble = sin(_Time.y * _WobbleFrequency + position.y * TWO_PI) * _WobbleAmplitude;
            position += input.normalOS * wobble;

            VertexPositionInputs positions = GetVertexPositionInputs(position);
            Varyings output;
            output.positionCS = positions.positionCS;
            output.positionWS = positions.positionWS;
            output.normalWS = TransformObjectToWorldNormal(input.normalOS);
            // Object space keeps the smoke attached to the ghost while it moves.
            output.noisePosition = input.positionOS.xyz * _NoiseScale;
            return output;
        }

        float DissolveNoise(Varyings input)
        {
            return Fbm(input.noisePosition * 1.7 + 11.3);
        }
        ENDHLSL

        // Depth first, so the body pass below shades only the surface nearest to the camera. Without it every layer
        // of a translucent mesh blends on top of the others: the back wall and folds show through the front as
        // bright overlapping triangles. Both sides are drawn: under the hem the inside of the sheet is the nearest
        // surface, like under a real sheet.
        Pass
        {
            Name "GhostDepth"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            ZWrite On
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragDepth

            half4 FragDepth(Varyings input) : SV_Target
            {
                clip(DissolveNoise(input) - _Dissolve);
                // A hidden ghost must not cut holes into the beam and effects drawn after it.
                clip(_Reveal - _DepthReveal);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Ghost"
            Tags { "LightMode" = "UniversalForward" }

            // Premultiplied alpha: the rim adds light on top of the translucent body, so it glows without Bloom.
            Blend One OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            half4 Frag(Varyings input, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
            {
                float dissolveNoise = DissolveNoise(input);
                clip(dissolveNoise - _Dissolve);

                // The inside of the sheet faces the camera too: lit like the outside, it glows only at its edges.
                float3 normal = normalize(input.normalWS);
                normal = IS_FRONT_VFACE(face, normal, -normal);
                float3 viewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half fresnel = pow(1.0h - saturate(dot(normal, viewDirection)), _RimPower);
                float smoke = Fbm(input.noisePosition + _Time.y * _NoiseSpeed.xyz);

                half edge = (1.0h - smoothstep(0.0h, _DissolveEdgeWidth, dissolveNoise - _Dissolve)) * step(0.001h, _Dissolve);
                half body = _BaseColor.a * lerp(1.0h, smoke, _NoiseStrength);
                half rim = fresnel * _RimIntensity;

                half3 color = _BaseColor.rgb * body + _RimColor.rgb * rim + _DissolveEdgeColor.rgb * edge;
                half alpha = saturate(body + rim * _RimColor.a + edge);
                return half4(color * _Reveal, alpha * _Reveal);
            }
            ENDHLSL
        }
    }
}
