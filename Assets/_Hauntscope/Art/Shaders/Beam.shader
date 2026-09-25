Shader "Hauntscope/Beam"
{
    // Capture beam for LineRenderer (Texture Mode: Stretch): uv.x runs along the beam, uv.y across it.
    // Two tileable noise layers scroll towards the ghost at different speeds, so energy visibly pours down the beam.
    Properties
    {
        _MainTex ("Noise (tileable along U)", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 8)) = 2
        _Tiling ("Tiling Along Beam", Float) = 3
        _ScrollSpeed ("Scroll Speed", Float) = 4
        _Softness ("Edge Softness", Range(0.05, 1)) = 0.35
        _CoreWhiten ("Core Whiten", Range(0, 1)) = 0.6
        _Pulse ("Pulse Frequency", Float) = 18
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+20"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Beam"
            Tags { "LightMode" = "UniversalForward" }

            // Additive like every other effect: light only, never darkens the AR camera feed.
            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half _Intensity;
                float _Tiling;
                float _ScrollSpeed;
                half _Softness;
                half _CoreWhiten;
                float _Pulse;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                half across = abs(uv.y * 2.0h - 1.0h);
                half profile = exp(-(across * across) / (_Softness * _Softness));

                float time = _Time.y * _ScrollSpeed;
                half slow = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(uv.x * _Tiling - time, uv.y * 0.35 + 0.1)).r;
                half fast = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(uv.x * _Tiling * 2.3 - time * 1.9, uv.y * 0.35 + 0.55)).r;
                half pulse = 0.85h + 0.15h * sin(uv.x * 40.0 - _Time.y * _Pulse);
                half energy = profile * (0.45h + 1.1h * slow * fast) * pulse;

                // Fades in just past the lens and flares slightly at the impact end.
                half ends = smoothstep(0.0h, 0.12h, uv.x) * (1.0h + 0.6h * smoothstep(0.85h, 1.0h, uv.x));

                half3 color = lerp(input.color.rgb, 1.0h, saturate(profile * profile * _CoreWhiten));
                return half4(color * energy * ends * input.color.a * _Intensity, 0.0h);
            }
            ENDHLSL
        }
    }
}
