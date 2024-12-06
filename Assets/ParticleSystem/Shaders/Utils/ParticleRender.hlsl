#ifndef ParticleRender
#define ParticleRender

#include "Assets/ParticleSystem/Shaders/Utils/ParticlesUtils.hlsl"

/*
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
-> gets included by the generated shader
 */

StructuredBuffer<ParticleData> particles;
uint instanceID;

void Vert_float(uint instanceID, float3 position, float3 normal, out float3 v, out float3 n) {
    ParticleData particle = particles[instanceID];

    float3 pos = mul(particle.mat, position);
    pos = TransformObjectToWorld(pos);
    v = TransformWorldToHClip(pos);

    float3 transformedNormal = mul(particle.mat, normal);
    n = normalize(TransformObjectToWorldNormal(transformedNormal));
}

#endif