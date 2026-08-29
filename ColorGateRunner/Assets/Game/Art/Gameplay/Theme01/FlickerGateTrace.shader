Shader "ColorGateRunner/FlickerGateTrace"
{
    Properties
    {
        [HDR] _TraceColor ("Trace Color", Color) = (0, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+30"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "FlickerGateTrace"
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _TraceColor;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half pulse = 1.1h +
                    (sin(_Time.y * 38.0h) * 0.08h);
                return half4(
                    _TraceColor.rgb * pulse * 3.2h,
                    _TraceColor.a);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
