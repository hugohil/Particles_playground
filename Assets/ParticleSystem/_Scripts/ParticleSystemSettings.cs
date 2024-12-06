using UnityEngine;

namespace ParticleSystem {
    [CreateAssetMenu(fileName = "ParticleSystemSettings", menuName = "ParticleSystem/Settings")]
    public class ParticleSystemSettings : ScriptableObject {
        public ParticleCount maxParticles = ParticleCount._1024;
        public Vector3 gridSize = new Vector3(8, 8, 8);
        public float particleSize = 0.2f;
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