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

struct GridCell {
    int particle_count;
    int start_index;
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

float3 GetEuler(float4x4 rotationMatrix) {
    return float3(
        atan2(rotationMatrix[1][2], rotationMatrix[2][2]),
        atan2(-rotationMatrix[0][2], length(float3(rotationMatrix[0][0], rotationMatrix[0][1], rotationMatrix[0][2]))),
        atan2(rotationMatrix[0][1], rotationMatrix[0][0])
    );
}

float3 GetAngularVelocity(float3 vel, float3 pos) {
    return cross(pos, vel) / dot(pos, pos);
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

float3x3 NormalizeMatrix(float3x3 mat) {
    mat[0] = normalize(mat[0]);
    mat[1] = normalize(mat[1] - dot(mat[0], mat[1]) * mat[0]);
    mat[2] = cross(mat[0], mat[1]);
    return mat;
}

