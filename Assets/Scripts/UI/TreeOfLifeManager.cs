using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeOfLifeManager {

    // species nodes
    // links
    // Data:
    public List<TreeOfLifeSpeciesNodeData> speciesNodesList;
    public List<TreeOfLifeStemSegmentData> stemSegmentsList;

    private Vector3 treeOriginPos;
    private GameObject treeOfLifeAnchorGO;

    private float scaleMultiplier = 12f;

    // Instantiated Objects:
    public List<TreeOfLifeNodeRaycastTarget> nodeRaycastTargetsList;


	public TreeOfLifeManager(GameObject anchorGO) {
        treeOfLifeAnchorGO = anchorGO;
    }

    public void FirstTimeInitialize(MasterGenomePool masterGenomePool) {
        speciesNodesList = new List<TreeOfLifeSpeciesNodeData>();
        stemSegmentsList = new List<TreeOfLifeStemSegmentData>();
        // Do I need a separate List> or store object reference in SpeciesNodeData ?
        nodeRaycastTargetsList = new List<TreeOfLifeNodeRaycastTarget>();

        // Create Nodes ( Copy from MasterGenomePool CompleteSpeciesList )
        for(int i = 0; i < masterGenomePool.completeSpeciesPoolsList.Count; i++) {
            TreeOfLifeSpeciesNodeData nodeData = new TreeOfLifeSpeciesNodeData(masterGenomePool.completeSpeciesPoolsList[i]);
            speciesNodesList.Add(nodeData);
            
            // RaycastColliderGameObject:
            GameObject speciesNodeColliderGO = new GameObject("SpeciesNodeRaycastCollider_" + i.ToString());
            speciesNodeColliderGO.transform.parent = treeOfLifeAnchorGO.transform;
            speciesNodeColliderGO.transform.localPosition = new Vector3(-1f * (float)nodeData.speciesPool.depthLevel, -1f, 0f) * scaleMultiplier;
            speciesNodeColliderGO.transform.localScale = Vector3.one * scaleMultiplier;
            TreeOfLifeNodeRaycastTarget rayTarget = speciesNodeColliderGO.AddComponent<TreeOfLifeNodeRaycastTarget>();
            rayTarget.Initialize(nodeData);
            rayTarget.rayCollider = speciesNodeColliderGO.AddComponent<CapsuleCollider>();
            rayTarget.rayCollider.isTrigger = true;

            nodeRaycastTargetsList.Add(rayTarget);            
        }

        // Create StemSegmentData:
        for(int i = 0; i < speciesNodesList.Count; i++) {
            
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
            

        }       
    }

    public void AddNewSpecies(MasterGenomePool masterGenomePool, int newSpeciesID) {
        // Create speciesNode:

        TreeOfLifeSpeciesNodeData nodeData = new TreeOfLifeSpeciesNodeData(masterGenomePool.completeSpeciesPoolsList[newSpeciesID]);
        speciesNodesList.Add(nodeData);
            
        // RaycastColliderGameObject:
        GameObject speciesNodeColliderGO = new GameObject("SpeciesNodeRaycastCollider_" + newSpeciesID.ToString());
        speciesNodeColliderGO.transform.parent = treeOfLifeAnchorGO.transform;
        speciesNodeColliderGO.transform.localPosition = new Vector3(-1f * (float)nodeData.speciesPool.depthLevel, UnityEngine.Random.Range(-2f, 0f), 0f) * scaleMultiplier;
        speciesNodeColliderGO.transform.localScale = Vector3.one * scaleMultiplier;
        TreeOfLifeNodeRaycastTarget rayTarget = speciesNodeColliderGO.AddComponent<TreeOfLifeNodeRaycastTarget>();
        rayTarget.Initialize(nodeData);
        rayTarget.rayCollider = speciesNodeColliderGO.AddComponent<CapsuleCollider>();
        rayTarget.rayCollider.isTrigger = true;

        nodeRaycastTargetsList.Add(rayTarget);

        // Stem Segments:
        bool reachedRootNode = false;
        //int curSpeciesID = nodeData.speciesPool.speciesID;
        SpeciesGenomePool curSpecies = speciesNodesList[newSpeciesID].speciesPool;

        int backupCounter = 0;
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
    }

    public void RemoveExtinctSpecies() {

    }
}
