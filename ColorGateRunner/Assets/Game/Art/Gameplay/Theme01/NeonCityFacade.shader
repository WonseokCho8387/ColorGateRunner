Shader "ColorGateRunner/NeonCityFacade"
{
    Properties
    {
        _BaseColor ("Facade Color", Color) = (0.014, 0.018, 0.034, 1)
        _WindowColor ("Window Color", Color) = (0.78, 0.95, 1, 1)
        _CyanWindowColor ("Cyan Window Color", Color) = (0, 0.72, 0.85, 1)
        _AccentColor ("Accent Window Color", Color) = (1, 0.17, 0.84, 1)
        _WindowIntensity ("Window Intensity", Range(0, 4)) = 1.15
        _LitRatio ("Lit Ratio", Range(0, 1)) = 0.3
        _CellSize ("Cell Size", Vector) = (0.7, 0.9, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _WindowColor;
            half4 _CyanWindowColor;
            half4 _AccentColor;
            half _WindowIntensity;
            half _LitRatio;
            float4 _CellSize;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "NeonCityFacade"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 facadePosition : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float4 seedAndFog : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Hash2(float2 value)
            {
                return frac(sin(dot(value, float2(127.1, 311.7))) * 43758.5453);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float4x4 objectToWorld = GetObjectToWorldMatrix();
                // Windows use the world-aligned offset from the tower pivot so
                // recycled towers keep square, upright cells without
                // depending on mesh UVs or the imported object axes.
                float4 positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionCS = positionCS;
                output.facadePosition =
                    mul((float3x3)objectToWorld, input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 origin = objectToWorld._m03_m13_m23;
                output.seedAndFog = float4(
                    floor(origin.x * 0.25),
                    floor(origin.z * 0.25),
                    0.0,
                    ComputeFogFactor(positionCS.z));
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                Light mainLight = GetMainLight();
                half3 lighting = SampleSH(normalWS) +
                    mainLight.color *
                    saturate(dot(normalWS, mainLight.direction)) * 0.35h;
                half3 color = _BaseColor.rgb * lighting;

                float side = 1.0 - step(0.5, abs(normalWS.y));
                float horizontal = abs(normalWS.x) > abs(normalWS.z)
                    ? input.facadePosition.z
                    : input.facadePosition.x;
                float2 grid = float2(horizontal, input.facadePosition.y) /
                    max(_CellSize.xy, 0.05);
                float2 cell = floor(grid);
                float2 local = frac(grid);
                float shape = step(0.2, local.x) * step(local.x, 0.8) *
                    step(0.24, local.y) * step(local.y, 0.76);
                float2 seed = input.seedAndFog.xy +
                    float2(sign(normalWS.x) * 3.0, sign(normalWS.z) * 5.0);
                float litRoll = Hash2(cell + seed * 17.0);
                float colorRoll = Hash2(cell.yx + seed * 29.0);
                half lit = (half)(side * shape *
                    step(1.0 - _LitRatio, litRoll));
                half3 windowColor = colorRoll < 0.12
                    ? _AccentColor.rgb
                    : (colorRoll < 0.3
                        ? _CyanWindowColor.rgb
                        : _WindowColor.rgb * 0.8h);
                color += windowColor * lit * _WindowIntensity;
                color = MixFog(color, input.seedAndFog.w);
                return half4(color, 1.0h);
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
            #pragma target 3.0
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            DepthVaryings DepthVert(DepthAttributes input)
            {
                DepthVaryings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half DepthFrag(DepthVaryings input) : SV_Target
            {
                return input.positionCS.z;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
