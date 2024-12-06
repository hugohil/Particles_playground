#define PI 3.1415926538
#define HALF_PI 1.57079632679489661923

struct ParticleData {
    float3 base_position;
    float3 position;
    float3 velocity;

    float3 rotation;
    float3 angular_rotation;

    float3 scale;

    float4x4 mat;

    float force;
};

struct CellOffset {
    uint startIndex;
    uint count;
};

float4x4 GetRotationMatrix(float3 velocity) {
    float3 forward = length(velocity) > 0.0001f ? normalize(velocity) : float3(0, 0, 1);
    float3 up = float3(0, 1, 0);
    float3 right = abs(dot(forward, up)) > 0.99f ?  float3(1, 0, 0) : normalize(cross(up, forward));

    up = normalize(cross(right, forward));
    right = normalize(cross(up, forward));

    return float4x4(
        right.x, right.y, right.z, 0.0,
        up.x, up.y, up.z, 0.0,
        forward.x, forward.y, forward.z, 0.0,
        0.0, 0.0, 0.0, 1.0
    );
}

float4x4 EulerToRotationMatrix(float3 eulerAngles) {
    float3 angles = radians(eulerAngles);
    
    float3 s = sin(angles);
    float3 c = cos(angles);
    
    return float4x4(
        c.y * c.z, -c.y * s.z, s.y, 0,
        c.z * s.x * s.y + c.x * s.z, c.x * c.z - s.x * s.y * s.z, -c.y * s.x, 0,
        s.x * s.z - c.x * c.z * s.y, c.z * s.x + c.x * s.y * s.z,  c.x * c.y, 0,
        0, 0, 0, 1
    );
}

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

float4x4 GetScaleMatrix(float3 scale) {
    return float4x4(
        scale.x, 0.0, 0.0, 0.0,
        0.0, scale.y, 0.0, 0.0,
        0.0, 0.0, scale.z, 0.0,
        0.0, 0.0, 0.0, 1.0
    );
}
