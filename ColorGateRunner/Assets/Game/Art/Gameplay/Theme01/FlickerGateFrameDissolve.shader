Shader "ColorGateRunner/FlickerGateFrameDissolve"
{
    Properties
    {
        _CurrentColor ("Current Color", Color) = (0, 1, 1, 1)
        _NextColor ("Next Color", Color) = (1, 0, 0, 1)
        [HDR] _CurrentEmission ("Current Emission", Color) = (0, 4, 4, 1)
        [HDR] _NextEmission ("Next Emission", Color) = (4, 0, 0, 1)
        _RevealProgress ("Reveal Progress", Range(0, 1)) = 0
        _Axis ("Axis", Range(0, 1)) = 1
        _AxisMin ("Axis Minimum", Float) = 0
        _AxisRange ("Axis Range", Float) = 1
        _AxisDirection ("Axis Direction", Float) = 1
        _EdgeWidth ("Edge Width", Range(0.005, 0.2)) = 0.065
        _NoiseAmount ("Noise Amount", Range(0, 0.25)) = 0.09
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
            Name "FlickerGateFrameDissolve"
            Cull Back
            ZWrite On

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
                float3 positionOS : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _CurrentColor;
                half4 _NextColor;
                half4 _CurrentEmission;
                half4 _NextEmission;
                half _RevealProgress;
                half _Axis;
                float _AxisMin;
                float _AxisRange;
                half _AxisDirection;
                half _EdgeWidth;
                half _NoiseAmount;
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
                float2 localPosition = frac(samplePosition);
                localPosition = localPosition * localPosition *
                    (3.0 - (2.0 * localPosition));
                float bottom = lerp(
                    Hash21(cell),
                    Hash21(cell + float2(1.0, 0.0)),
                    localPosition.x);
                float top = lerp(
                    Hash21(cell + float2(0.0, 1.0)),
                    Hash21(cell + float2(1.0, 1.0)),
                    localPosition.x);
                return lerp(bottom, top, localPosition.y);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(
                    input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float axisPosition = lerp(
                    input.positionOS.x,
                    input.positionOS.y,
                    _Axis);
                float coordinate = saturate(
                    (axisPosition - _AxisMin) /
                    max(_AxisRange, 0.0001));
                coordinate = _AxisDirection < 0.0h
                    ? 1.0 - coordinate
                    : coordinate;

                float noise = ValueNoise(
                    (input.positionOS.xy * 7.0) +
                    (input.positionOS.zy * 3.7));
                float field = coordinate +
                    ((noise - 0.5) * _NoiseAmount);
                float nextMask = smoothstep(
                    field - _EdgeWidth,
                    field + _EdgeWidth,
                    _RevealProgress);
                float edge = 1.0 - smoothstep(
                    0.0,
                    _EdgeWidth,
                    abs(_RevealProgress - field));
                edge *= step(0.001, _RevealProgress) *
                    step(_RevealProgress, 0.999);

                half3 baseColor = lerp(
                    _CurrentColor.rgb,
                    _NextColor.rgb,
                    nextMask);
                half3 emission = lerp(
                    _CurrentEmission.rgb,
                    _NextEmission.rgb,
                    nextMask);
                half3 edgeColor = lerp(
                    _CurrentEmission.rgb,
                    _NextEmission.rgb,
                    saturate(_RevealProgress));
                half3 finalColor = baseColor + emission +
                    (edgeColor * edge * 0.42h);
                return half4(finalColor, 1.0h);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
