using UnityEngine;

namespace ParticleSystem  {
    public class ParticleManager : MonoBehaviour {
        [SerializeField] private ParticleSystemSettings settings;

        private ComputeBuffer _particleBuffer;
        private ComputeBuffer _gridSortedIndicesBuffer; // particle ids sorted by their position in the collision grid
        private ComputeBuffer _gridCellsBuffer; // all collision grid cells with start index and particle count
        private ComputeBuffer _indirectArgsBuffer;
        private uint[] _indirectArgs = new uint[5];

        [SerializeField] private ComputeShader movementShader;

        private int _moveKernelID;
        private int _gridResetKernelID;
        private int _gridAssignKernelID;
        private int _collisionKernelID;

        // shader property IDs for the compute shaders
        private static readonly int ParticlesID = Shader.PropertyToID("particles");
        private static readonly int GridCellsID = Shader.PropertyToID("grid_cells");
        private static readonly int GridIndicesID = Shader.PropertyToID("grid_indices");
        private static readonly int ParticleCountID = Shader.PropertyToID("size");
        private static readonly int DeltaTimeID = Shader.PropertyToID("delta_time");
        private static readonly int TimeID = Shader.PropertyToID("time");
        private static readonly int RadiusID = Shader.PropertyToID("radius");
        private static readonly int GridSizeID = Shader.PropertyToID("grid_size");

        // shader property IDs for the instance shader
        private static readonly int ParticleBufferID = Shader.PropertyToID("_ParticleBuffer");

        private Bounds _bounds;

        private int _max = (int)ParticleCount._128;
        private int _gridSize;

        public Vector3 rotation;
        public Vector3 velocity;
        
        [Range(0f, 3f)]
        public float speed;

        private void Awake() {
            _max = (int)settings.maxParticles;
        }

        private void Start() {
            InitializeBuffers();
            InitializeParticles();
            SetupComputeShaders();
            SetupBounds();

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

            _gridSize = Mathf.CeilToInt(settings.collisionGridSize.x * settings.collisionGridSize.y * settings.collisionGridSize.z);
            _gridCellsBuffer = new ComputeBuffer(_gridSize, sizeof(int) * 2);
            _gridSortedIndicesBuffer = new ComputeBuffer(_max, sizeof(uint));
        }
        
        private void InitializeParticles()  {
            ParticleData[] particles = new ParticleData[_max];
            
            for (int i = 0; i < _max; i++) {
                Vector3 position = new Vector3(
                    Random.Range(-settings.collisionGridSize.x, settings.collisionGridSize.x),
                    Random.Range(-settings.collisionGridSize.y, settings.collisionGridSize.y),
                    Random.Range(-settings.collisionGridSize.z, settings.collisionGridSize.z)
                );
                position *= 0.5f;
                //Vector3 rotation = Vector3.zero;
                Vector3 rotation = new Vector3(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
                //Quaternion rotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
                //Quaternion rotation = Quaternion.identity;
                //Vector3 scale = Vector3.one * (settings.particleSize * Random.Range(0.5f, 1.5f));
                Vector3 scale = (Vector3.one * settings.particleSize);

                particles[i] = new ParticleData  {
                    mat = Matrix4x4.TRS(position, Quaternion.Euler(rotation), scale),
                    base_position = position,
                    position = position,
                    velocity = new Vector3(0, 0, 0),
                    rotation =  rotation,
                    angular_velocity = new Vector3(0, 0, 0),
                    scale = scale,
                    force = 0
                };
            }

            _particleBuffer.SetData(particles);
        }

        private void SetupComputeShaders() {
            _moveKernelID = movementShader.FindKernel("CSMovement");
            _gridResetKernelID = movementShader.FindKernel("CSGridReset");
            _gridAssignKernelID = movementShader.FindKernel("CSGridAssign");
            _collisionKernelID = movementShader.FindKernel("CSCollision");
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
            movementShader.SetInt(ParticleCountID, _max);
            movementShader.SetFloat(DeltaTimeID, Time.deltaTime);
            movementShader.SetFloat(TimeID, Time.time);
            movementShader.SetVector(GridSizeID, settings.collisionGridSize);
            movementShader.SetVector("rotation", rotation);
            movementShader.SetVector("velocity", velocity);
            movementShader.SetFloat("speed", speed);
            movementShader.SetFloat(RadiusID, settings.radius);
        }

        private void DispatchComputeShaders() {
            int movementThreadGroupsX = Mathf.CeilToInt(_max / 64f);

            /*
            movementShader.SetBuffer(_gridResetKernelID, GridCellsID, _gridCellsBuffer);
            movementShader.Dispatch(_gridResetKernelID, movementThreadGroupsX, 1, 1);
            
            movementShader.SetBuffer(_gridAssignKernelID, ParticlesID, _particleBuffer);
            movementShader.SetBuffer(_gridAssignKernelID, GridCellsID, _gridCellsBuffer);
            movementShader.SetBuffer(_gridAssignKernelID, GridIndicesID, _gridSortedIndicesBuffer);
            movementShader.Dispatch(_gridAssignKernelID, movementThreadGroupsX, 1, 1);
            
            movementShader.SetBuffer(_collisionKernelID, ParticlesID, _particleBuffer);
            movementShader.SetBuffer(_collisionKernelID, GridCellsID, _gridCellsBuffer);
            movementShader.SetBuffer(_collisionKernelID, GridIndicesID, _gridSortedIndicesBuffer);
            movementShader.Dispatch(_collisionKernelID, movementThreadGroupsX, 1, 1);
            */

            movementShader.SetBuffer(_moveKernelID, ParticlesID, _particleBuffer);
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
            Gizmos.DrawWireCube(transform.position, settings.collisionGridSize);
        }

        private void OnDestroy() {
            ReleaseBuffers();
        }

        private void ReleaseBuffers() {
            _particleBuffer?.Dispose();
            _gridCellsBuffer?.Dispose();
            _gridSortedIndicesBuffer?.Dispose();
            _indirectArgsBuffer?.Dispose();
        }
    }
}