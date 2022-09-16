using UnityEngine;
using System.Collections.Generic;

namespace RGSK
{
    public class Wheels : MonoBehaviour
    {
        private SurfaceManager surfaceManager;

        private Texture2D currentTexture;
        private PhysicMaterial currentPhysicMaterial;

        private WheelCollider wheelCollider;
        private WheelHit wheelHit;
        private Skidmark skidmarks;
        private int lastSkid;

        private Terrain terrain;
        private TerrainData terrainData;
        private SplatPrototype[] splatPrototypes;

        private AudioSource skidAudioSource;

        [HideInInspector]
        public bool shouldEmit = false;
        [HideInInspector]
        public bool wheelOnPenaltySurface;
        private ParticleSystem particleToEmit;
        private bool useSkidmarks;
        private List<string> wheelParticleChildren = new List<string>();

        void Start()
        {
            //Get the surface manager
            if (GameObject.FindObjectOfType(typeof(SurfaceManager)))
            {
                surfaceManager = GameObject.FindObjectOfType(typeof(SurfaceManager)) as SurfaceManager;
            }
            else
            {
                return;
            }

            SetupWheelComponents();

            GetTerrainInfo();
        }

        void SetupWheelComponents()
        {
            if (!wheelCollider) wheelCollider = GetComponent<WheelCollider>();

            //Get the skidmarks object
            if (GameObject.FindObjectOfType(typeof(Skidmark)))
            {
                skidmarks = GameObject.FindObjectOfType(typeof(Skidmark)) as Skidmark;
            }

            //Configure the sound
            if (GetComponent<AudioSource>()) skidAudioSource = GetComponent<AudioSource>();

            if (skidAudioSource)
            {
                skidAudioSource.spatialBlend = 1.0f;
                skidAudioSource.loop = true;
            }


            //Instantiate all particles as child objects
            if (surfaceManager.terrainSurfaceTypes.Count > 0)
            {
                for (int i = 0; i < surfaceManager.terrainSurfaceTypes.Count; i++)
                {
                    if (surfaceManager.terrainSurfaceTypes[i].skidParticle)
                    {
                        if (!wheelParticleChildren.Contains(surfaceManager.terrainSurfaceTypes[i].skidParticle.transform.name + "(Clone)")) //make sure that theres no duplicates
                        {
                            GameObject particle = (GameObject)Instantiate(surfaceManager.terrainSurfaceTypes[i].skidParticle, transform.position, Quaternion.identity);
                            particle.transform.parent = transform;
                            var em = particle.GetComponent<ParticleSystem>().emission;
                            em.enabled = false;
                            wheelParticleChildren.Add(particle.transform.name);
                        }
                    }
                }
            }


            if (surfaceManager.physicMaterialSurface.Count > 0)
            {
                for (int i = 0; i < surfaceManager.physicMaterialSurface.Count; i++)
                {
                    if (surfaceManager.physicMaterialSurface[i].skidParticle)
                    {
                        if (!wheelParticleChildren.Contains(surfaceManager.physicMaterialSurface[i].skidParticle.transform.name + "(Clone)")) //make sure that theres no duplicates
                        {
                            GameObject particle = (GameObject)Instantiate(surfaceManager.physicMaterialSurface[i].skidParticle, transform.position, Quaternion.identity);
                            particle.transform.parent = transform;
                            var em = particle.GetComponent<ParticleSystem>().emission;
                            em.enabled = false;
                            wheelParticleChildren.Add(particle.transform.name);
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (!surfaceManager) return;

            //Ensure that the audiosource is always playing
            if (skidAudioSource && !skidAudioSource.isPlaying)
                skidAudioSource.Play();

            //Get wheel hit
            wheelCollider.GetGroundHit(out wheelHit);

            //Get the surfaces
            PhysicMaterialSurfaceDetection();

            TerrainSurfaceDetection();

            //Emit effect particles/sounds
            Emit();
        }

        void PhysicMaterialSurfaceDetection()
        {
            if (wheelHit.collider)
            {
                //Are we on a renderer
                if (wheelHit.collider.material)
                {
                    currentPhysicMaterial = wheelHit.collider.material;

                    for (int i = 0; i < surfaceManager.physicMaterialSurface.Count; i++)
                    {
                        if (currentPhysicMaterial.name.Replace(" (Instance)", "") == surfaceManager.physicMaterialSurface[i].physicMaterial.name)
                        {
                            //Set the skid sound clip 
                            if (skidAudioSource && surfaceManager.physicMaterialSurface[i].skidSound) skidAudioSource.clip = surfaceManager.physicMaterialSurface[i].skidSound;

                            // Get the particle to emit
                            foreach (Transform t in transform)
                            {
                                if (t.name == surfaceManager.physicMaterialSurface[i].skidParticle.name + "(Clone)")
                                    particleToEmit = t.GetComponent<ParticleSystem>();
                            }

                            //Is the surface type continious
                            if (surfaceManager.physicMaterialSurface[i].surfaceType == SurfaceManager.SurfaceType.OffRoad)
                            {
                                if (skidAudioSource && wheelCollider.attachedRigidbody.velocity.magnitude > 5.0f) { shouldEmit = true; skidAudioSource.volume = .5f; }
                            }

                            //Does this surface use skidmarks
                            useSkidmarks = surfaceManager.physicMaterialSurface[i].allowSkidmark;

                            wheelOnPenaltySurface = surfaceManager.physicMaterialSurface[i].isPenaltySurface;
                        }
                    }
                }
            }
        }

        void TerrainSurfaceDetection()
        {
            if (terrain == null) return;

            if (wheelHit.collider)
            {
                //Are we on a terrain
                if (wheelHit.collider.GetComponent<Terrain>())
                {
                    currentTexture = splatPrototypes[GetTerrainTexture(transform.position)].texture;
                }
                else
                {
                    currentTexture = null;
                }
            }

            //Get & Set surface details
            for (int i = 0; i < surfaceManager.terrainSurfaceTypes.Count; i++)
            {
                if (currentTexture == surfaceManager.terrainSurfaceTypes[i].texture)
                {
                    //Set the skid sound clip 
                    if (surfaceManager.terrainSurfaceTypes[i].skidSound)
                    {
                        if (skidAudioSource && surfaceManager.terrainSurfaceTypes[i].skidSound) skidAudioSource.clip = surfaceManager.terrainSurfaceTypes[i].skidSound;
                    }

                    // Get the particle to emit
                    foreach (Transform t in transform)
                    {
                        if (t.name == surfaceManager.terrainSurfaceTypes[i].skidParticle.name + "(Clone)")
                        {

                            particleToEmit = t.GetComponent<ParticleSystem>();
                        }
                    }

                    //Is the surface type continious
                    if (surfaceManager.terrainSurfaceTypes[i].surfaceType == SurfaceManager.SurfaceType.OffRoad)
                    {
                        if (skidAudioSource && wheelCollider.attachedRigidbody.velocity.magnitude > 5.0f) { shouldEmit = true; skidAudioSource.volume = .5f; }
                    }

                    //Does this surface use skidmarks
                    useSkidmarks = surfaceManager.terrainSurfaceTypes[i].allowSkidmark;

                    wheelOnPenaltySurface = surfaceManager.terrainSurfaceTypes[i].isPenaltySurface;
                }
            }
        }

        public void Emit()
        {
            if (shouldEmit && wheelCollider.isGrounded)
            {
                //Particle
                if (particleToEmit) particleToEmit.Emit(1);

                //Skidmarks
                Vector3 skidPoint = wheelHit.point + (wheelCollider.attachedRigidbody.velocity * Time.fixedDeltaTime);

                if (skidmarks != null)
                {
                    if (useSkidmarks)
                        lastSkid = skidmarks.AddSkidMark(skidPoint, wheelHit.normal, 0.5f, lastSkid);
                    else
                        lastSkid = -1;
                }

                //Volume
                if (skidAudioSource) skidAudioSource.volume = Mathf.Abs(wheelHit.sidewaysSlip) + Mathf.Abs(wheelHit.forwardSlip) + 0.5f;
            }
            else
            {
                lastSkid = -1;

                //Volume Lerp
                if (skidAudioSource) skidAudioSource.volume -= Time.deltaTime;
            }
        }

        //---Terrain releated functions---\\
        private void GetTerrainInfo()
        {
            if (Terrain.activeTerrain)
            {
                terrain = Terrain.activeTerrain;
                terrainData = terrain.terrainData;
                splatPrototypes = terrain.terrainData.splatPrototypes;
            }
        }

        private float[] GetTextureMix(Vector3 worldPos)
        {

            terrain = Terrain.activeTerrain;
            terrainData = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;

            int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];
            for (int n = 0; n < cellMix.Length; ++n)
            {
                cellMix[n] = splatmapData[0, 0, n];
            }

            return cellMix;
        }

        private int GetTerrainTexture(Vector3 worldPos)
        {

            float[] mix = GetTextureMix(worldPos);
            float maxMix = 0;
            int maxIndex = 0;

            for (int n = 0; n < mix.Length; ++n)
            {

                if (mix[n] > maxMix)
                {
                    maxIndex = n;
                    maxMix = mix[n];
                }
            }

            return maxIndex;
        }

        public void SetupWheelCollider(float mass, float spring, float damper)
        {
            if (!wheelCollider) wheelCollider = GetComponent<WheelCollider>();

            JointSpring _spring = wheelCollider.suspensionSpring;
            _spring.spring = spring;
            _spring.damper = damper;
            wheelCollider.suspensionSpring = _spring;
            wheelCollider.mass = mass;
            wheelCollider.wheelDampingRate = 1.0f;
        }
    }
}
