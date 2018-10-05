using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeOfLifeManager {

    // species nodes
    // links
    // Data:
    //public List<TreeOfLifeSpeciesNodeData> speciesNodesList;
    //public List<TreeOfLifeStemSegmentData> stemSegmentsList;

    private Vector3 treeOriginPos;
    private GameObject treeOfLifeAnchorGO;

    private float colliderBaseScaleMultiplier = 0.167f;

    // Instantiated Objects:
    public List<TreeOfLifeNodeRaycastTarget> nodeRaycastTargetsList;

    public int selectedID = -1;
    public int hoverID = -1;

    public float camScale = 1f;
    public float treeOfLifeScale = 1f;

    //public float screenScaleX = 1f;
    //public float screenScaleY = 1f;
    

    //private int treeOfLifeDisplayResolution = 256;
    //public RenderTexture treeOfLifeDisplayRT;

	public TreeOfLifeManager(GameObject anchorGO, UIManager uiManagerRef) { //, Camera renderCamera) {
        treeOfLifeAnchorGO = anchorGO;

        //treeOfLifeDisplayRT = new RenderTexture(treeOfLifeDisplayResolution, treeOfLifeDisplayResolution, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
        //treeOfLifeDisplayRT.wrapMode = TextureWrapMode.Clamp;
        //treeOfLifeDisplayRT.enableRandomWrite = true;
        //treeOfLifeDisplayRT.Create();

        //uiManagerRef.imageTreeOfLifeDisplay.material.SetTexture("_MainTex", treeOfLifeDisplayRT);

        //renderCamera.targetTexture = treeOfLifeDisplayRT;
    }

    public void FirstTimeInitialize(MasterGenomePool masterGenomePool) {
        //speciesNodesList = new List<TreeOfLifeSpeciesNodeData>();
        //stemSegmentsList = new List<TreeOfLifeStemSegmentData>();
        // Do I need a separate List> or store object reference in SpeciesNodeData ?
        nodeRaycastTargetsList = new List<TreeOfLifeNodeRaycastTarget>();

        // Create Nodes ( Copy from MasterGenomePool CompleteSpeciesList )
        for(int i = 0; i < masterGenomePool.completeSpeciesPoolsList.Count; i++) {  // should only be one species at this point?

            AddNewSpecies(masterGenomePool, i);
            
            //TreeOfLifeSpeciesNodeData nodeData = new TreeOfLifeSpeciesNodeData(masterGenomePool.completeSpeciesPoolsList[i]);
            //speciesNodesList.Add(nodeData);
            
            // RaycastColliderGameObject:
            /*GameObject speciesNodeColliderGO = new GameObject("SpeciesNodeRaycastCollider_" + i.ToString());
            speciesNodeColliderGO.transform.parent = treeOfLifeAnchorGO.transform;
            speciesNodeColliderGO.transform.localPosition = new Vector3(-1f * (float)nodeData.speciesPool.depthLevel, -1f, 0f) * scaleMultiplier;
            speciesNodeColliderGO.transform.localScale = Vector3.one * scaleMultiplier;
            TreeOfLifeNodeRaycastTarget rayTarget = speciesNodeColliderGO.AddComponent<TreeOfLifeNodeRaycastTarget>();
            rayTarget.Initialize(nodeData);
            rayTarget.rayCollider = speciesNodeColliderGO.AddComponent<CapsuleCollider>();
            rayTarget.rayCollider.isTrigger = true;

            nodeRaycastTargetsList.Add(rayTarget);    
            */
        }

        // Create StemSegmentData:
        /*for(int i = 0; i < speciesNodesList.Count; i++) {
            
            //int curSpeciesID = nodeData.speciesPool.speciesID;
            SpeciesGenomePool curSpecies = speciesNodesList[i].speciesPool;

            // CONSOLIDATE CODE!!!! *************
            int backupCounter = 0;            
            bool reachedRootNode = false;
            while(!reachedRootNode) {
                backupCounter++;

                int speciesID = curSpecies.speciesID;
                int parentSpeciesID = curSpecies.parentSpeciesID;

                if(parentSpeciesID < 0) {
                    // Hit Root!
                    reachedRootNode = true;

                    TreeOfLifeStemSegmentData newStemSegmentData = new TreeOfLifeStemSegmentData(parentSpeciesID, speciesID);
                    stemSegmentsList.Add(newStemSegmentData);
                }
                else {
                    // Create StemSegment!                    
                    TreeOfLifeStemSegmentData newStemSegmentData = new TreeOfLifeStemSegmentData(parentSpeciesID, speciesID);
                    stemSegmentsList.Add(newStemSegmentData);

                    curSpecies = speciesNodesList[parentSpeciesID].speciesPool;  // set curSpecies to ParentSpecies (traverse up tree)
                    // Repeat!!!
                }
                

                if(backupCounter > 10000) {
                    Debug.LogError("INFINITE WHILE LOOP!");
                    break;
                }
            }
        }*/       
    }

    public void UpdateNodePositionsFromGPU(CameraManager cam, TheRenderKing.TreeOfLifeNodeColliderData[] dataArray) {
        camScale = 720f / (float)cam.cameraRef.pixelHeight;  // Can be improved precision-wise, but maybe close enough for now        
        //float screenScaleX = camScale;
        //float screenScaleY = camScale;
        treeOfLifeScale = 1f;

        for(int i = 0; i < nodeRaycastTargetsList.Count; i++) {
            
            Vector4 localPos = new Vector3(dataArray[i].localPos.x, dataArray[i].localPos.y, dataArray[i].localPos.z);
            Vector3 worldPos = cam.worldSpaceTopLeft + cam.worldSpaceCameraRightDir * camScale - cam.worldSpaceCameraUpDir * camScale * 0.5f + localPos * camScale;

            nodeRaycastTargetsList[i].transform.position = worldPos;
        }
    }

    public void AddNewSpecies(MasterGenomePool masterGenomePool, int newSpeciesID) {

        SpeciesGenomePool speciesPool = masterGenomePool.completeSpeciesPoolsList[newSpeciesID];
        // RaycastColliderGameObject:
        GameObject speciesNodeColliderGO = new GameObject("SpeciesNodeRaycastCollider_" + newSpeciesID.ToString());
        speciesNodeColliderGO.transform.parent = treeOfLifeAnchorGO.transform;
        speciesNodeColliderGO.transform.localPosition = Vector3.zero; // new Vector3(-1f * (float)speciesPool.depthLevel, UnityEngine.Random.Range(-2f, 0f), 0f) * scaleMultiplier;
        speciesNodeColliderGO.transform.localScale = Vector3.one * colliderBaseScaleMultiplier;
        TreeOfLifeNodeRaycastTarget rayTarget = speciesNodeColliderGO.AddComponent<TreeOfLifeNodeRaycastTarget>();
        rayTarget.Initialize(speciesPool);
        rayTarget.rayCollider = speciesNodeColliderGO.AddComponent<CapsuleCollider>();
        rayTarget.rayCollider.isTrigger = true;

        nodeRaycastTargetsList.Add(rayTarget);                 
    }

    public void RemoveExtinctSpecies(int speciesID) {
        //CSExctinctSpecies
    }
}
