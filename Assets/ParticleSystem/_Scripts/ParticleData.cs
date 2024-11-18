using UnityEngine;

namespace ParticleSystem {
    public struct ParticleData {
        public Matrix4x4 mat;
        public Vector3 anchor;

        public static int Size() {
            int matrix = sizeof(float) * 4 * 4;
            int vec3 = sizeof(float) * 3; 
            return matrix + vec3;
        }
    }

    [CreateAssetMenu(fileName = "ParticleSystemSettings", menuName = "ParticleSystem/Settings")]
    public class ParticleSystemSettings : ScriptableObject {
        public int maxParticles = 500;
        public Vector3 bounds = new Vector3(10, 10, 10);
        public float particleSize = 1f;
        public Mesh particleMesh;
        public Material particleMaterial;
    }
}