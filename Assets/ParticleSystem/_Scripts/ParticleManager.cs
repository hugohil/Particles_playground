using UnityEngine;

namespace ParticleSystem  {
    public class ParticleManager : MonoBehaviour {
        [SerializeField] private ParticleSystemSettings settings;

        // main particle buffer (used accross all shaders)
        private ComputeBuffer _particleBuffer;
        
        // proximity compute buffers
        private ComputeBuffer _sortedParticleBuffer;
        private ComputeBuffer _cellIndicesBuffer;
        private ComputeBuffer _particleIndicesBuffer;
        private ComputeBuffer _cellCountsBuffer;
        private ComputeBuffer _cellOffsetsBuffer;
        private ComputeBuffer _cellOffsetCountersBuffer;

        // rendering
        private ComputeBuffer _indirectArgsBuffer;
        private uint[] _indirectArgs = new uint[5];

        [SerializeField] private ComputeShader movementShader;
        private int _moveKernelID;

        [SerializeField] private ComputeShader proximityShader;
        private int _cellIndicesKernelID;
        private int _countPerCellKernelID;
        private int _upSweepKernelID;
        private int _downSweepKernelID;
        private int _cellOffsetsKernelID;
        private int _sortKernelID;
        private int _collisionKernelID;

        private static readonly int ParticlesID = Shader.PropertyToID("particles");
        private static readonly int ParticleCountID = Shader.PropertyToID("num_particle");
        private static readonly int DeltaTimeID = Shader.PropertyToID("delta_time");
        private static readonly int TimeID = Shader.PropertyToID("time");
        private static readonly int AreaID = Shader.PropertyToID("area");
        private static readonly int RotationID = Shader.PropertyToID("rotation");
        private static readonly int VelocityID = Shader.PropertyToID("velocity");
        private static readonly int SpeedID = Shader.PropertyToID("speed");
        private static readonly int MaxVelID = Shader.PropertyToID("max_vel");
        private static readonly int SortedParticlesID = Shader.PropertyToID("sorted_particles");
        private static readonly int CellIndicesID = Shader.PropertyToID("cell_indices");
        private static readonly int ParticleIndicesID = Shader.PropertyToID("particle_indices");
        private static readonly int CellCountsID = Shader.PropertyToID("cell_counts");
        private static readonly int CellOffsetsID = Shader.PropertyToID("cell_offsets");
        private static readonly int CellOffsetCountersID = Shader.PropertyToID("cell_offset_counters");
        private static readonly int GridSizeID = Shader.PropertyToID("grid_size");
        private static readonly int RadiusID = Shader.PropertyToID("radius");

        private Bounds _bounds;

        private int _max = (int)ParticleCount._1024;
        private int _gridSize1D;

        public Vector3 rotation;
        public Vector3 velocity;
        
        [Range(0f, 3f)]
        public float speed = 1f;

        public float maxVel = 10f;

        [Range(0, 512)]
        public int debugItem;

        public Mesh debugMesh;
        public Material debugMat;

        private RenderParams debugRP;

        struct GridDebugInstance {
            public Matrix4x4 objectToWorld;
        }

        private GridDebugInstance[] _gridDebugInstances;

        public bool debugGrid = false;

        private void Awake() {
            _max = (int)settings.maxParticles;
            debugMesh = WireCubeMesh.CreateWireCube(1f);
        }

        private void Start() {
            InitializeBuffers();
            InitializeParticles();
            SetupComputeShaders();
            SetupBounds();

            debugRP = new RenderParams(debugMat);

            Debug.Log($"Starting Particle System with {_max} particles.");
        }

        private void InitializeBuffers() {
            _particleBuffer = new ComputeBuffer(_max, ParticleData.Size());

            _indirectArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            _indirectArgs = new uint[5] {
                settings.particleMesh.GetIndexCount(0),
                (uint)_max,
                0, 0, 0
            };
            _indirectArgsBuffer.SetData(_indirectArgs);

            _gridSize1D = Mathf.CeilToInt(settings.gridSize.x * settings.gridSize.y * settings.gridSize.z);
            
            _sortedParticleBuffer = new ComputeBuffer(_max, ParticleData.Size());
            _cellIndicesBuffer = new ComputeBuffer(_gridSize1D, sizeof(uint));
            _particleIndicesBuffer = new ComputeBuffer(_max, sizeof(uint));
            _cellCountsBuffer = new ComputeBuffer(_gridSize1D, sizeof(uint));
            _cellOffsetsBuffer = new ComputeBuffer(_gridSize1D, sizeof(uint) * 2);
            _cellOffsetCountersBuffer = new ComputeBuffer(_gridSize1D, sizeof(uint));

            _gridDebugInstances = new GridDebugInstance[_gridSize1D];
            for (int x = 0; x < settings.gridSize.x; x++) {
                for (int y = 0; y < settings.gridSize.y; y++) {
                    for (int z = 0; z < settings.gridSize.z; z++) {
                        float posX = x - (settings.gridSize.x/2f) + 0.5f;
                        float posY = y - (settings.gridSize.y/2f) + 0.5f;
                        float posZ = z - (settings.gridSize.z/2f) + 0.5f;

                        Vector3 pos = new Vector3(posX, posY, posZ);

                        GridDebugInstance inst = new GridDebugInstance();
                        inst.objectToWorld = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);

                        int linearCellIndex = x + 
                                            y * Mathf.FloorToInt(settings.gridSize.x) + 
                                            z * Mathf.FloorToInt(settings.gridSize.x) * Mathf.FloorToInt(settings.gridSize.y);
                        _gridDebugInstances[linearCellIndex] = inst;
                    }
                }
            }
        }
        
        private void InitializeParticles()  {
            ParticleData[] particles = new ParticleData[_max];
            
            for (int i = 0; i < _max; i++) {
                Vector3 _position = new Vector3(
                    Random.Range(-settings.gridSize.x, settings.gridSize.x),
                    Random.Range(-settings.gridSize.y, settings.gridSize.y),
                    Random.Range(-settings.gridSize.z, settings.gridSize.z)
                );
                _position *= 0.5f;

                Vector3 _rotation = new Vector3(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
                Vector3 _scale = (Vector3.one * settings.particleSize);

                particles[i] = new ParticleData  {
                    mat = Matrix4x4.TRS(_position, Quaternion.Euler(_rotation), _scale),
                    base_position = _position,
                    position = _position,
                    velocity = new Vector3(0, 0, 0),
                    rotation =  rotation,
                    angular_velocity = new Vector3(0, 0, 0),
                    scale = _scale,
                    force = 0
                };
            }

            _particleBuffer.SetData(particles);
        }

        private void SetupComputeShaders() {
            _moveKernelID = movementShader.FindKernel("CSMovement");

            _cellIndicesKernelID = proximityShader.FindKernel("ComputeCellIndices");
            _countPerCellKernelID = proximityShader.FindKernel("CountParticlesPerCell");
            _upSweepKernelID = proximityShader.FindKernel("UpSweep");
            _downSweepKernelID = proximityShader.FindKernel("DownSweep");
            _cellOffsetsKernelID = proximityShader.FindKernel("BuildCellOffsets");
            _sortKernelID = proximityShader.FindKernel("SortParticles");
            _collisionKernelID = proximityShader.FindKernel("Collision");
        }
        
        private void SetupBounds() {
            _bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
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
        }

        private void UpdateComputeShaderParameters() {
            movementShader.SetInt(ParticleCountID, _max);
            movementShader.SetFloat(DeltaTimeID, Time.deltaTime);
            movementShader.SetFloat(TimeID, Time.time);
            movementShader.SetVector(AreaID, settings.gridSize);
            movementShader.SetVector(RotationID, rotation);
            movementShader.SetVector(VelocityID, velocity);
            movementShader.SetFloat(SpeedID, speed);
            movementShader.SetFloat(MaxVelID, maxVel);
            
            proximityShader.SetInt(ParticleCountID, _max);
            proximityShader.SetVector(GridSizeID, settings.gridSize);
            proximityShader.SetFloat(DeltaTimeID, Time.deltaTime);
            proximityShader.SetFloat(TimeID, Time.time);
            proximityShader.SetFloat(RadiusID, (settings.radius * settings.particleSize));
        }

        private void DispatchComputeShaders() {
            int particleThreadGroup = Mathf.CeilToInt(_max / 64f);

            // ComputeCellIndices kernel
            
            // CountParticlesPerCell kernel
            
            // UpSweep kernel
            
            // DownSweep kernel
            
            // BuildCellOffsets kernel
            
            // SortParticles kernel
            
            // Collision kernel
            

            // main kernel
            movementShader.SetBuffer(_moveKernelID, ParticlesID, _particleBuffer);
            movementShader.Dispatch(_moveKernelID, particleThreadGroup, 1, 1);
        }

        private void RenderParticles() {
            settings.particleMaterial.SetBuffer(ParticlesID, _particleBuffer);

            // todo: DrawMeshInstancedIndirect is obsolete, switch to Graphics.RenderMeshIndirect
            Graphics.DrawMeshInstancedIndirect(
                settings.particleMesh,
                0,
                settings.particleMaterial,
                _bounds,
                _indirectArgsBuffer
            );

            if (debugGrid) {
                //debugRP.material.SetBuffer("gridIDs", _gridSortedIndicesBuffer);
                //debugRP.material.SetInt("debugItem", debugItem);
                //Graphics.RenderMeshInstanced(debugRP, debugMesh, 0, _gridDebugInstances);

                /*
                int[] gridIDs = new int[_max];
                _gridSortedIndicesBuffer.GetData(gridIDs);
                Debug.Log(gridIDs[debugItem]);
                */
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, settings.gridSize);
        }

        private void OnDestroy() {
            ReleaseBuffers();
        }

        private void ReleaseBuffers() {
            _particleBuffer?.Dispose();
            _particleBuffer = null;

            _sortedParticleBuffer?.Dispose();
            _sortedParticleBuffer = null;

            _cellIndicesBuffer?.Dispose();
            _cellIndicesBuffer = null;

            _particleIndicesBuffer?.Dispose();
            _particleIndicesBuffer = null;

            _cellCountsBuffer?.Dispose();
            _cellCountsBuffer = null;

            _cellOffsetsBuffer?.Dispose();
            _cellOffsetsBuffer = null;

            _cellOffsetCountersBuffer?.Dispose();
            _cellOffsetCountersBuffer = null;

            _indirectArgsBuffer?.Dispose();
            _indirectArgsBuffer = null;
        }
    }
}
