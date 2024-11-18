using Unity.Mathematics.Geometry;
using UnityEngine;

namespace ParticleSystem  {
    public class ParticleManager : MonoBehaviour {
        [SerializeField] private ParticleSystemSettings settings;

        private ComputeBuffer _particleBuffer;
        private ComputeBuffer _indirectArgsBuffer;
        private uint[] _indirectArgs = new uint[5];

        [SerializeField] private ComputeShader movementShader;

        private int _moveKernelID;

        // shader property IDs for the compute shaders
        private static readonly int ParticlesID = Shader.PropertyToID("_Particles");
        private static readonly int ParticleCountID = Shader.PropertyToID("_ParticleCount");
        private static readonly int DeltaTimeID = Shader.PropertyToID("_DeltaTime");
        private static readonly int TimeID = Shader.PropertyToID("_Time");

        // shader property IDs for the HDRP instance shader
        private static readonly int ParticleBufferID = Shader.PropertyToID("_ParticleBuffer");

        private Bounds _bounds;

        private void Start() {
            InitializeBuffers();
            InitializeParticles();
            SetupComputeShaders();
            SetupBounds();
            
            Debug.Log($"Starting Particle System with {settings.maxParticles} particles.");
        }

        private void InitializeBuffers() {
            _particleBuffer = new ComputeBuffer(settings.maxParticles, ParticleData.Size());

            _indirectArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            _indirectArgs = new uint[5] {
                settings.particleMesh.GetIndexCount(0),
                (uint)settings.maxParticles,
                0, 0, 0
            };
            _indirectArgsBuffer.SetData(_indirectArgs);
        }
        
        private void InitializeParticles()  {
            ParticleData[] particles = new ParticleData[settings.maxParticles];


            for (int i = 0; i < settings.maxParticles; i++) {
                Vector3 position = new Vector3(
                    Random.Range(-settings.bounds.x * 0.5f, settings.bounds.x * 0.5f),
                    Random.Range(-settings.bounds.y * 0.5f, settings.bounds.y * 0.5f),
                    Random.Range(-settings.bounds.z * 0.5f, settings.bounds.z * 0.5f)
                );
                Quaternion rotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
                Vector3 scale = Vector3.one * (settings.particleSize * Random.Range(0.5f, 1.5f));

                particles[i] = new ParticleData  {
                    mat = Matrix4x4.TRS(position, rotation, scale),
                    anchor = position
                };
            }

            _particleBuffer.SetData(particles);
        }

        private void SetupComputeShaders() {
            _moveKernelID = movementShader.FindKernel("CSMovement");

            movementShader.SetBuffer(_moveKernelID, ParticlesID, _particleBuffer);
        }
        
        private void SetupBounds()  {
            _bounds = new Bounds(
                transform.position,
                settings.bounds * 10f
            );
        }
        
        private void Update() {
            UpdateBounds();
            UpdateComputeShaderParameters();
            DispatchComputeShaders();
            RenderParticles();
        }

        private void UpdateBounds() {
            if (_bounds.center != transform.position) {
                _bounds.center = transform.position;
            }

            if (_bounds.size != settings.bounds) {
                _bounds.size = settings.bounds * 10f;
            }
        }

        private void UpdateComputeShaderParameters() {
            movementShader.SetInt(ParticleCountID, settings.maxParticles);
            movementShader.SetFloat(DeltaTimeID, Time.deltaTime);
            movementShader.SetFloat(TimeID, Time.time);
            //movementShader.SetFloat(WaveFrequencyID, settings.waveFrequency);
            //movementShader.SetFloat(WaveAmplitudeID, settings.waveAmplitude);
            //movementShader.SetFloat(WaveTurbulenceID, settings.waveTurbulence);
            //movementShader.SetVector(BoundsID, settings.bounds);
        }

        private void DispatchComputeShaders() {
            int movementThreadGroupsX = Mathf.CeilToInt(settings.maxParticles / 64f);
            
            movementShader.Dispatch(_moveKernelID, movementThreadGroupsX, 1, 1);
        }

        private void RenderParticles() {
            settings.particleMaterial.SetBuffer(ParticleBufferID, _particleBuffer);

            // todo: DrawMeshInstancedIndirect is obsolete, switch to Graphics.RenderMeshIndirect
            Graphics.DrawMeshInstancedIndirect(
                settings.particleMesh,
                0,
                settings.particleMaterial,
                _bounds,
                _indirectArgsBuffer
            );
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, settings.bounds);
        }

        private void OnEnable() {
            InitializeBuffers();
        }

        private void OnDisable() {
            ReleaseBuffers();
        }

        private void OnDestroy() {
            ReleaseBuffers();
        }

        private void ReleaseBuffers() {
            _particleBuffer?.Dispose();
            _indirectArgsBuffer?.Dispose();
        }
    }
}