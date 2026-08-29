Shader "ColorGateRunner/FlickerEmblemDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _DissolveProgress ("Dissolve Progress", Range(0, 1)) = 0
        _Incoming ("Incoming Layer", Range(0, 1)) = 0
        [HDR] _EdgeColor ("Edge Color", Color) = (0, 1, 1, 1)
        _EdgeWidth ("Edge Width", Range(0.005, 0.2)) = 0.075
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+40"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "CanUseSpriteAtlas" = "True"
        }

        Pass
        {
            Name "FlickerEmblemDissolve"
            Blend SrcAlpha OneMinusSrcAlpha
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
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _DissolveProgress;
                half _Incoming;
                half4 _EdgeColor;
                half _EdgeWidth;
            CBUFFER_END

            float Hash21(float2 samplePosition)
            {
                samplePosition = frac(
                    samplePosition * float2(123.34, 456.21));
                samplePosition += dot(
                    samplePosition,
                    samplePosition + 45.32);
                return frac(samplePosition.x * samplePosition.y);
            }

            float ValueNoise(float2 samplePosition)
            {
                float2 cell = floor(samplePosition);
                float2 local = frac(samplePosition);
                local = local * local * (3.0 - (2.0 * local));
                float bottom = lerp(
                    Hash21(cell),
                    Hash21(cell + float2(1.0, 0.0)),
                    local.x);
                float top = lerp(
                    Hash21(cell + float2(0.0, 1.0)),
                    Hash21(cell + float2(1.0, 1.0)),
                    local.x);
                return lerp(bottom, top, local.y);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color * _Color;
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 sprite = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    input.uv) * input.color;
                float coarse = ValueNoise(input.uv * 5.0);
                float detail = ValueNoise((input.uv * 13.0) + 4.73);
                float field = saturate(
                    (coarse * 0.56) +
                    (detail * 0.20) +
                    (input.uv.x * 0.24));
                float threshold = lerp(-0.12, 1.12, _DissolveProgress);
                float outgoing = smoothstep(
                    threshold - 0.035,
                    threshold + 0.035,
                    field);
                float mask = lerp(outgoing, 1.0 - outgoing, _Incoming);
                float edge = 1.0 - smoothstep(
                    0.0,
                    _EdgeWidth,
                    abs(field - threshold));
                edge *= step(0.001, _DissolveProgress) *
                    step(_DissolveProgress, 0.999);
                half baseAlpha = sprite.a;
                half alpha = baseAlpha * max(mask, edge * 0.82);
                clip(alpha - 0.002h);
                half3 color = lerp(
                    sprite.rgb,
                    _EdgeColor.rgb * 2.8h,
                    saturate(edge));
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
