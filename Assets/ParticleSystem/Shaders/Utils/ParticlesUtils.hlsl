#define PI 3.1415926538

struct ParticleData {
    float4x4 mat;
    float3 anchor;
};

float4x4 identity = float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1);

float3 GetTranslationFromTransform(float4x4 transform) {
    return float3(transform[0][3], transform[1][3], transform[2][3]);
}

float4x4 GetTranslationMatrix(float3 position) {
    return float4x4(
        1, 0, 0, position.x,
        0, 1, 0, position.y,
        0, 0, 1, position.z,
        0, 0, 0, 1
    );
}

float3 UP = float3(0, 1, 0);
float3 RIGHT = float3(1, 0, 0);
float3 FORWARD = float3(0, 0, 1);

float4x4 ComputeRotationMatrix(float3 angles) {
    float sx = sin(angles.x);
    float cx = cos(angles.x);

    float sy = sin(angles.y);
    float cy = cos(angles.y);

    float sz = sin(angles.z);
    float cz = cos(angles.z);

    return float4x4(
        cy * cz, -cy * sz, sy, 0,
        cx * sz + sx * sy * cz, cx * cz - sx * sy * sz, -sx * cy, 0,
        sx * sz - cx * sy * cz, sx * cz + cx * sy * sz, cx * cy, 0,
        0, 0, 0, 1
    );
}
