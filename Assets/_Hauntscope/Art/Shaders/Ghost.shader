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
        [Header(Living Body)]
        _BodyBottom ("Body Bottom (object Y)", Float) = -0.5
        _BodyTop ("Body Top (object Y)", Float) = 0.5
        _BreathAmplitude ("Breath Amplitude", Float) = 0.008
        _BreathSpeed ("Breath Speed", Float) = 1.3
        _HemFlutter ("Hem Flutter", Float) = 0.03
        _HemHeight ("Hem Height (0..1)", Range(0.01, 1)) = 0.35
        _HemFrequency ("Hem Frequency", Float) = 3.5
        _SwayAmplitude ("Sway Amplitude", Float) = 0.03
        _SwaySpeed ("Sway Speed", Float) = 0.7
        _Agitation ("Agitation", Range(0, 1)) = 0
        _AgitationAmplitude ("Agitation Amplitude", Float) = 0.012
        _Stretch ("Stretch (object space)", Vector) = (0, 0, 0, 0)
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
            float _BodyBottom;
            float _BodyTop;
            float _BreathAmplitude;
            float _BreathSpeed;
            float _HemFlutter;
            float _HemHeight;
            float _HemFrequency;
            float _SwayAmplitude;
            float _SwaySpeed;
            float _Agitation;
            float _AgitationAmplitude;
            float4 _Stretch;
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

        // The body is alive: it breathes, the hem ripples like cloth in a draught, the head sways, it trembles when
        // tense and trails behind itself when it moves fast. Everything but the breath and the wobble moves points by
        // their position alone, so eyes built into the same mesh stay exactly on the face.
        float3 Animate(float3 position, float3 normal)
        {
            float time = _Time.y;
            float height = saturate((position.y - _BodyBottom) / max(0.001, _BodyTop - _BodyBottom));

            float wobble = sin(time * _WobbleFrequency + position.y * TWO_PI) * _WobbleAmplitude;
            float breath = sin(time * _BreathSpeed * TWO_PI * 0.25) * _BreathAmplitude * smoothstep(0.15, 0.7, height);
            position += normal * (wobble + breath);

            // The hem: strongest at the very bottom, gone above _HemHeight; two travelling waves around the body.
            float hem = pow(saturate(1.0 - height / _HemHeight), 1.6);
            float angle = atan2(position.z, position.x);
            float ripple = sin(angle * 5.0 + time * _HemFrequency) * 0.6 + sin(angle * 3.0 - time * _HemFrequency * 0.7 + 1.3) * 0.4;
            position.xz += normalize(position.xz + 1e-4) * ripple * _HemFlutter * hem;
            position.y += ripple * _HemFlutter * 0.5 * hem;

            // The upper body sways a little, the way something hanging in the air would.
            float sway = height * height;
            position.x += sin(time * _SwaySpeed * TWO_PI * 0.5) * _SwayAmplitude * sway;
            position.z += sin(time * _SwaySpeed * TWO_PI * 0.37 + 0.8) * _SwayAmplitude * 0.6 * sway;

            // Tension: a fast, fine shiver over the whole body.
            float3 shiver = float3(sin(time * 41.0 + position.y * 23.0), sin(time * 37.0 + position.x * 19.0), sin(time * 43.0 + position.z * 29.0));
            position += shiver * _AgitationAmplitude * _Agitation;

            // Moving fast, the lower body lags behind: _Stretch is the lag at the hem, in object space.
            position += _Stretch.xyz * pow(1.0 - height, 1.5);
            return position;
        }

        // Both passes must place every vertex identically, or the depth pass would hide slivers of the body.
        Varyings Vert(Attributes input)
        {
            float3 position = Animate(input.positionOS.xyz, input.normalOS);

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
