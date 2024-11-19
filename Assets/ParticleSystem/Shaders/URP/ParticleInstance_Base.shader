Shader "Custom/InstanceParticle"
{
    Properties
    {
        _BaseColor("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "InstanceParticle"

            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #include "../Utils/ParticlesUtils.hlsl"

            StructuredBuffer<ParticleData> _ParticleBuffer;
            float4 _BaseColor;

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 viewDir : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            Varyings Vert(Attributes input) {
                Varyings output;

                ParticleData particle = _ParticleBuffer[input.instanceID];

                float3 pos = mul(particle.mat, input.positionOS);
                pos = TransformObjectToWorld(pos);

                output.positionCS = TransformWorldToHClip(pos);

                output.viewDir = normalize(_WorldSpaceCameraPos - pos);

                float3 transformedNormal = mul(particle.mat, input.normal);
                output.normal = normalize(TransformObjectToWorldNormal(transformedNormal));

                return output;
            }

            float4 Frag(Varyings input) : SV_Target {
                float3 viewDir = input.viewDir;
                float3 normalWS = input.normal;

                float shade = dot(normalWS, viewDir);

                return _BaseColor * shade;
            }
            ENDHLSL
        }
    }
}
