Shader "ColorGateRunner/NeonNightSky"
{
    Properties
    {
        _ZenithColor ("Zenith Color", Color) = (0.02, 0.024, 0.05, 1)
        _HorizonColor ("Horizon Color", Color) = (0.07, 0.06, 0.15, 1)
        _GlowColor ("Horizon Glow", Color) = (0.32, 0.06, 0.3, 1)
        _GroundColor ("Ground Color", Color) = (0.02, 0.024, 0.05, 1)
        _FarSkylineColor ("Far Skyline", Color) = (0.06, 0.055, 0.12, 1)
        _NearSkylineColor ("Near Skyline", Color) = (0.025, 0.028, 0.06, 1)
        _WindowColor ("Window Color", Color) = (0.78, 0.95, 1, 1)
        _AccentColor ("Accent Color", Color) = (1, 0.17, 0.84, 1)
        _GlowWidth ("Glow Width", Range(0.01, 0.4)) = 0.09
        _SkylineHeight ("Skyline Height", Range(0.02, 0.3)) = 0.11
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "PreviewType" = "Skybox"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "NeonNightSky"
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _ZenithColor;
                half4 _HorizonColor;
                half4 _GlowColor;
                half4 _GroundColor;
                half4 _FarSkylineColor;
                half4 _NearSkylineColor;
                half4 _WindowColor;
                half4 _AccentColor;
                half _GlowWidth;
                half _SkylineHeight;
            CBUFFER_END

            float Hash(float value)
            {
                return frac(sin(value * 127.1) * 43758.5453);
            }

            float Hash2(float2 value)
            {
                return frac(sin(dot(value, float2(127.1, 311.7))) * 43758.5453);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 direction = normalize(input.direction);
                float height = direction.y;
                float azimuth = atan2(direction.x, direction.z) *
                    0.15915494 + 0.5;

                half3 color = lerp(
                    _HorizonColor.rgb,
                    _ZenithColor.rgb,
                    (half)smoothstep(0.0, 0.55, height));
                color += _GlowColor.rgb *
                    (half)exp(-abs(height - 0.015) / _GlowWidth);

                // Two deterministic skyline layers wrap the horizon so the
                // city stays world-fixed while the chase camera turns.
                float farCell = floor(azimuth * 220.0);
                float farTop = _SkylineHeight *
                    (0.22 + Hash(farCell) * 0.38);
                float nearCell = floor(azimuth * 96.0);
                float nearGap = step(0.18, Hash(nearCell + 41.0));
                float nearTop = _SkylineHeight *
                    (0.35 + Hash(nearCell + 7.0) * 0.65) * nearGap;
                float edge = max(fwidth(height), 0.0005);
                half farMask = (half)(1.0 -
                    smoothstep(farTop - edge, farTop + edge, height));
                half nearMask = (half)(1.0 -
                    smoothstep(nearTop - edge, nearTop + edge, height));
                color = lerp(color, _FarSkylineColor.rgb, farMask);
                color = lerp(color, _NearSkylineColor.rgb, nearMask);

                float2 windowGrid = float2(azimuth * 1536.0, height * 260.0);
                float2 windowCell = floor(windowGrid);
                float2 windowLocal = frac(windowGrid);
                float windowShape = step(0.25, windowLocal.x) *
                    step(windowLocal.x, 0.75) *
                    step(0.3, windowLocal.y) *
                    step(windowLocal.y, 0.7);
                float windowSeed = Hash2(windowCell);
                half windowLit = (half)(step(0.9, windowSeed) * windowShape) *
                    nearMask * (half)step(0.004, height);
                half3 windowColor = lerp(
                    _WindowColor.rgb,
                    _AccentColor.rgb,
                    (half)step(0.975, windowSeed));
                color += windowColor * windowLit * 0.55h;

                half below = (half)smoothstep(0.0, -0.08, height);
                color = lerp(color, _GroundColor.rgb, below);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
