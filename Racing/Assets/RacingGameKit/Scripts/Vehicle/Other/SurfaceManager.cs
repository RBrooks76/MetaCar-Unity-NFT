using UnityEngine;
using System.Collections.Generic;

namespace RGSK
{
    public class SurfaceManager : MonoBehaviour
    {

        public enum SurfaceType { OnRoad, OffRoad};

        [System.Serializable]
        public class TerrainSurface
        {
            public string surfaceName;
            public SurfaceManager.SurfaceType surfaceType;         
            public Texture2D texture;
            public GameObject skidParticle;
            public AudioClip skidSound;
            public bool allowSkidmark;
            public bool isPenaltySurface;           
        }

        [System.Serializable]
        public class PhysicMaterialSurface
        {
            public string surfaceName;
            public SurfaceManager.SurfaceType surfaceType;         
            public PhysicMaterial physicMaterial;
            public GameObject skidParticle;
            public AudioClip skidSound;
            public bool allowSkidmark;            
            public bool isPenaltySurface;
        }

        [Header("Terrain Surface")]
        public List<TerrainSurface> terrainSurfaceTypes = new List<TerrainSurface>();

        [Header("PhysicMaterial Surface")]
        public List<PhysicMaterialSurface> physicMaterialSurface = new List<PhysicMaterialSurface>();
    }
}
