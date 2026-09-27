// A mark of ectoplasm on the floor (GDD 5.33.4), drawn only while the UV beam shows it. It is painted in a saturated
// marker green that the UV filter turns into fluorescence, and breathes a little, like something still wet.
Shader "Hauntscope/UvDecal"
{
    Properties
    {
        _MainTex ("Mark (alpha)", 2D) = "white" {}
        _Color ("Marker Colour", Color) = (0.08, 1, 0.3, 1)
        _Alpha ("Alpha", Range(0, 1)) = 1
        _Pulse ("Pulse", Range(0, 1)) = 0.18
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent-10"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "UvDecal"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            Offset -1, -1

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _Alpha;
                half _Pulse;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half mark = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                half pulse = 1.0h - _Pulse * (0.5h + 0.5h * sin(_Time.y * 3.0 + input.positionWS.x * 4.0 + input.positionWS.z * 3.0));
                return half4(_Color.rgb, mark * _Alpha * pulse);
            }
            ENDHLSL
        }
    }
}
