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

    [CreateAssetMenu(fileName = "ParticleSystemSettings", menuName = "ParticleSystem/Settings")]
    public class ParticleSystemSettings : ScriptableObject {
        public ParticleCount maxParticles = ParticleCount._512;
        public Vector3 bounds = new Vector3(50, 50, 50);
        public Vector3 collisionGridSize = new Vector3(10, 10, 10);
        public float particleSize = 1f;
        public float radius = 1f;
        public Mesh particleMesh;
        public Material particleMaterial;
    }
    
    public enum ParticleCount {
        _16 = 16,
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192,
        _16384 = 16384,
        _32768 = 32768,
        _65536 = 65536,
        _131072 = 131072,
        _262144 = 262144,
        _524288 = 524288,
        _1048576 = 1048576,
        _2097152 = 2097152
    }
}