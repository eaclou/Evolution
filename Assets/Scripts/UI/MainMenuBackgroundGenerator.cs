using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuBackgroundGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct VoronoiRegion
    {
        public int ID;
        public string name;
        public Vector3 rootCoords01;  // resolution-independent
        public Vector3 rootPixelPos; // resolution-specific
    }
    [SerializeField]
    public List<VoronoiRegion> voronoiRegionsList;
    //[ReadOnly]
    //public Vector3 res;

    // computeShader;
    // DisplayRenderTexture;
    // shader needs to do stuff;
    // Regions should instead be ScriptableObjects/MonoBehaviors and Set within UI panel?
    //shader may have to be hardcoded for the Region-specific graphics?
    //animate transion Lerp variables and pipe that into gpuShader for animated transitions
    
    // Start is called before the first frame update
    void Start()
    {
        //res.x = Screen.currentResolution.width;
        //res.y = Screen.currentResolution.height;
        if(voronoiRegionsList == null) {
            voronoiRegionsList = new List<VoronoiRegion>();
            Debug.LogError("no voronoi regions list!!");
        }
        RefreshRegionsCenterPixelPos();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RefreshRegionsCenterPixelPos() {
        for(int i = 0; i < voronoiRegionsList.Count; i++) {
            VoronoiRegion region = voronoiRegionsList[i];

            Vector3 newPixelPos = Vector3.zero;
            newPixelPos.x = region.rootCoords01.x * Screen.currentResolution.width;
            newPixelPos.y = region.rootCoords01.y * Screen.currentResolution.height;
            region.rootPixelPos = newPixelPos;
            voronoiRegionsList[i] = region;
        };
    }

    public int FindNearestRegion(Vector3 PixelPosition) {  // for mouseclick determine which region/button is pressed
        int closestID = -1;
        float closestDistance = float.PositiveInfinity;
        foreach(var region in voronoiRegionsList) {
            float sqDistanceToPixel = (region.rootPixelPos - PixelPosition).sqrMagnitude;
            if(sqDistanceToPixel < closestDistance) {
                closestDistance = sqDistanceToPixel;
                closestID = region.ID;
            }
        }
        if(closestID == -1) {
            Debug.LogError("no regions!");
        }

        return closestID;
    }
}
