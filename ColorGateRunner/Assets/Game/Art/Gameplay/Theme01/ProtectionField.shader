Shader "ColorGateRunner/ProtectionField"
{
    Properties
    {
        _FieldColor ("Field Color", Color) = (0, 0.72, 1, 1)
        _FieldPulse ("Field Pulse", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+20"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ProtectionField"
            Blend SrcAlpha One
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _FieldColor;
                half _FieldPulse;
            CBUFFER_END

            float HexDistance(float2 point)
            {
                point = abs(point);
                return max(
                    dot(point, normalize(float2(1.0, 1.7320508))),
                    point.x);
            }

            float HexEdge(float2 point)
            {
                float2 cell = float2(1.0, 1.7320508);
                float2 halfCell = cell * 0.5;
                float2 first = fmod(point, cell) - halfCell;
                float2 second = fmod(point - halfCell, cell) - halfCell;
                float2 local = dot(first, first) < dot(second, second)
                    ? first
                    : second;
                float interior = 0.5 - HexDistance(local);
                return 1.0 - smoothstep(0.018, 0.065, interior);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float wave = sin(
                    (input.uv.x * 37.0) +
                    (input.uv.y * 21.0) +
                    (_Time.y * 2.4));
                float3 displaced = input.positionOS.xyz +
                    (input.normalOS * wave * 0.008);
                VertexPositionInputs positions =
                    GetVertexPositionInputs(displaced);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 viewDirection = SafeNormalize(
                    GetWorldSpaceViewDir(input.positionWS));
                half fresnel = pow(
                    1.0h - saturate(dot(normalize(input.normalWS), viewDirection)),
                    2.35h);
                float2 grid = input.uv * float2(22.0, 11.0);
                half edge = (half)HexEdge(grid);
                half scan = 0.72h +
                    (sin((input.uv.y * 65.0) - (_Time.y * 5.0)) * 0.08h);
                half strength = saturate(
                    (edge * 0.62h) + (fresnel * 0.92h) + 0.035h);
                half intensity = _FieldPulse * scan *
                    (1.15h + (edge * 3.2h) + (fresnel * 2.6h));
                return half4(
                    _FieldColor.rgb * intensity,
                    strength * _FieldColor.a * 0.72h);
            }
            ENDHLSL
        }
    }
}
