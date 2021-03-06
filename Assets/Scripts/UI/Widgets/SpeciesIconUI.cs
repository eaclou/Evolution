﻿using UnityEngine;
using UnityEngine.UI;

public class SpeciesIconUI : MonoBehaviour {
    UIManager uiManager => UIManager.instance;

    //public AllSpeciesTreePanelUI allSpeciesTreePanelUI;
    public int index;
    public int speciesID;

    public SpeciesGenomePool linkedPool;

    public Vector2 targetCoords; // UI canvas
    private Vector2 currentCoords;

    public void Initialize(int index, SpeciesGenomePool pool) {        
        this.index = index;
        this.linkedPool = pool;
        this.speciesID = pool.speciesID;

        targetCoords = Vector2.zero;

        //Debug.Log("NEW BUTTON! " + index + ", " + pool.speciesID);
    }

    // Updates focusedCandidate in uiManager 
    public void Clicked() {
        uiManager.SetSelectedSpeciesUI(speciesID);        
    }
    public void SetTargetCoords(Vector2 newCoords) {
        targetCoords = newCoords;
    }
    public void UpdateIconDisplay(int panelPixelSize, bool isSelected) {
        // POSITION
        currentCoords = Vector2.Lerp(currentCoords, targetCoords, 0.75f);

        gameObject.transform.localPosition = new Vector3(currentCoords.x * (float)panelPixelSize, currentCoords.y * (float)panelPixelSize, 0f);

        // APPEARANCE
        if (isSelected) {
            gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            gameObject.GetComponent<Image>().color = Color.white;
        }
        else {
            gameObject.transform.localScale = Vector3.one;
            Color color = new Color(linkedPool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary.x, linkedPool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary.y, linkedPool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary.z);
            gameObject.GetComponent<Image>().color = color;
        }  
        
        if(linkedPool.isExtinct) {
            gameObject.GetComponentInChildren<Text>().color = Color.gray * 0.05f;
            gameObject.transform.localScale = Vector3.one * 0.5f;
        }
        else {
            if (linkedPool.isFlaggedForExtinction) {
                gameObject.GetComponentInChildren<Text>().color = Color.gray;
            }
            else {
                gameObject.GetComponentInChildren<Text>().color = Color.white;
            }            
        } 
        
    }
}
