using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeOfLifeManager {
    
    private Vector3 treeOriginPos;
    private GameObject treeOfLifeAnchorGO;

    private float colliderBaseScaleMultiplier = 0.125f;

    // Instantiated Objects:
    public List<TreeOfLifeNodeRaycastTarget> nodeRaycastTargetsList;

    public int selectedID = 1;
    public int hoverID = -1;
    public int draggingID = -1;

    public float camScale = 1f;
    public float treeOfLifeScale = 1f;
    
	public TreeOfLifeManager(GameObject anchorGO, UIManager uiManagerRef) { //, Camera renderCamera) {
        treeOfLifeAnchorGO = anchorGO;        
    }

    public void FirstTimeInitialize(MasterGenomePool masterGenomePool) {        
        nodeRaycastTargetsList = new List<TreeOfLifeNodeRaycastTarget>();

        // Create Nodes ( Copy from MasterGenomePool CompleteSpeciesList )
        for(int i = 0; i < masterGenomePool.completeSpeciesPoolsList.Count; i++) {  // should only be one species at this point?
            AddNewSpecies(masterGenomePool, i);            
        }      
    }

    public void UpdateNodePositionsFromGPU(CameraManager cam, Vector3[] posArray) {
        //camScale = 720f / (float)cam.cameraRef.pixelHeight;  // Can be improved precision-wise, but maybe close enough for now        
        //float screenScaleX = camScale;
        //float screenScaleY = camScale;
        treeOfLifeScale = 1f;

        for(int i = 0; i < nodeRaycastTargetsList.Count; i++) {

            //Vector4 localPos = new Vector3(dataArray[i].localPos.x, dataArray[i].localPos.y, dataArray[i].localPos.z);
            //Vector3 worldPos = cam.worldSpaceTopLeft + cam.worldSpaceCameraRightDir * camScale - cam.worldSpaceCameraUpDir * camScale * 0.5f + localPos * camScale;

            if (i < 32) {  // temp hack: prevent indexOutOfRangeError from posArray capping at 32
                Vector3 pos = posArray[i]; // SOURCE OF ERROR: posArray doesn't go above 32, but nodeRaycastTargetsList does
                nodeRaycastTargetsList[i].transform.position = pos;  
            }            
        }
    }

    public void AddNewSpecies(MasterGenomePool masterGenomePool, int newSpeciesID) {

        SpeciesGenomePool speciesPool = masterGenomePool.completeSpeciesPoolsList[newSpeciesID];
        // RaycastColliderGameObject:
        GameObject speciesNodeColliderGO = new GameObject("SpeciesNodeRaycastCollider_" + newSpeciesID.ToString());
        speciesNodeColliderGO.transform.parent = treeOfLifeAnchorGO.transform;
        speciesNodeColliderGO.transform.localPosition = Vector3.zero; // new Vector3(-1f * (float)speciesPool.depthLevel, UnityEngine.Random.Range(-2f, 0f), 0f) * scaleMultiplier;
        speciesNodeColliderGO.transform.localScale = Vector3.one * colliderBaseScaleMultiplier * camScale;
        TreeOfLifeNodeRaycastTarget rayTarget = speciesNodeColliderGO.AddComponent<TreeOfLifeNodeRaycastTarget>();
        rayTarget.Initialize(speciesPool);
        rayTarget.rayCollider = speciesNodeColliderGO.AddComponent<CapsuleCollider>();
        rayTarget.rayCollider.isTrigger = true;

        nodeRaycastTargetsList.Add(rayTarget);                 
    }

    public void RemoveExtinctSpecies(int speciesID) {
        //CSExctinctSpecies
        nodeRaycastTargetsList[speciesID].gameObject.SetActive(false);
    }

    public void UpdateVisualUI(bool isOn) {
        treeOfLifeAnchorGO.SetActive(isOn);
        for(int i = 0; i < nodeRaycastTargetsList.Count; i++) {
            //nodeRaycastTargetsList[i].gameObject.SetActive(isOn);
            nodeRaycastTargetsList[i].gameObject.transform.localScale = Vector3.one * colliderBaseScaleMultiplier * camScale;
        }

        
    }

    public void ClickedOnSpeciesNode(int speciesID) {
        selectedID = speciesID;
    }
    public void HoverOverSpeciesNode(int speciesID) {
        Debug.Log("HoverOverSpeciesNode [" + speciesID.ToString() + "]");
        hoverID = speciesID;
    }
    public void HoverAllOff() {
        hoverID = -1;
    }
}
