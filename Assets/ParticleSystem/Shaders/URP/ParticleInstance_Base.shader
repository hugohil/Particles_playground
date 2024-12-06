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
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #include "Assets/ParticleSystem/Shaders/Utils/ParticlesUtils.hlsl"

            StructuredBuffer<ParticleData> particles;
            uint debugItem;
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
                uint instanceID : SV_InstanceID;
            };

            Varyings Vert(Attributes input) {
                Varyings output;

                ParticleData particle = particles[input.instanceID];

                float3 pos = mul(particle.mat, input.positionOS);
                pos = TransformObjectToWorld(pos);

                output.positionCS = TransformWorldToHClip(pos);

                output.viewDir = normalize(_WorldSpaceCameraPos - pos);

                float3 transformedNormal = mul(particle.mat, input.normal);
                output.normal = normalize(TransformObjectToWorldNormal(transformedNormal));

                output.instanceID = input.instanceID;

                return output;
            }

            float4 Frag(Varyings input) : SV_Target {
                ParticleData particle = particles[input.instanceID];

                float3 viewDir = input.viewDir;
                float3 normalWS = input.normal;

                float shade = dot(normalWS, viewDir);
                shade += abs(particle.force);

                /*
                if (input.instanceID == debugItem) {
                    _BaseColor = float4(0,0,1,1) * 50.0;
                }
                */

                return _BaseColor * shade;
            }
            ENDHLSL
        }
    }
}
