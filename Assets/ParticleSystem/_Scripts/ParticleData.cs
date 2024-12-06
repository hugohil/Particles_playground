using UnityEngine;

namespace ParticleSystem {
    public struct ParticleData {
        public Vector3 base_position;
        public Vector3 position;
        public Vector3 velocity;

        public Vector3 rotation;
        public Vector3 angular_velocity;

        public Vector3 scale;

        public float force;

        public Matrix4x4 mat;

        public static int Size() {
            int matrix = sizeof(float) * 4 * 4;
            int vec3 = sizeof(float) * 3;
            int f = sizeof(float);
            int i = sizeof(int);
            return (vec3 * 6) + f + matrix;
        }
    }
}