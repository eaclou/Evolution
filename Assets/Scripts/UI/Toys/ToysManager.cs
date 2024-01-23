﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToysManager : MonoBehaviour
{
    [SerializeField] public ToyReactionDiffusion toyReactionDiffusion;
    [SerializeField] public ToyAttractRepel toyAttractRepel;    
    [SerializeField] public ToyFluidSimulation toyFluidSim;
    [SerializeField] public ToyFractals toyFractals;
    [SerializeField] public Image imageSelectedToyHighlightBG;

    [SerializeField] public Button buttonToyReactionDiffusion;
    [SerializeField] public Button buttonToyAttractRepel;    
    [SerializeField] public Button buttonToyFluidSimulation;
    [SerializeField] public Button buttonToyFractals;

    private enum ToyType
    {
        ReactionDiffusion,
        AttractRepel,
        FluidSim,
        Fractals
    }
    private ToyType selectedToy;

    public void OpenToysMenu() { // this handled from MainMenuUI???
        this.gameObject.SetActive(true);
        if(selectedToy == ToyType.ReactionDiffusion) {
            OpenToyReactionDiffusion();
        }
        else if(selectedToy == ToyType.AttractRepel) {
            OpenToyAttractRepel();
        }
        else if(selectedToy == ToyType.FluidSim) {
            OpenToyFluidSim();
        }
        else if(selectedToy == ToyType.Fractals) {
            OpenToyFractals();
        }
        //RefreshUI();
    }

    public void CloseToysMenu() {

    }

    public void RefreshUI() {
        if(selectedToy == ToyType.ReactionDiffusion) {
            imageSelectedToyHighlightBG.gameObject.transform.position = buttonToyReactionDiffusion.gameObject.transform.position;
        
        }
        else if(selectedToy == ToyType.AttractRepel) {
            imageSelectedToyHighlightBG.gameObject.transform.position = buttonToyAttractRepel.gameObject.transform.position;
        }
        else if(selectedToy == ToyType.FluidSim) {
            imageSelectedToyHighlightBG.gameObject.transform.position = buttonToyFluidSimulation.gameObject.transform.position;
        }
        else if(selectedToy == ToyType.Fractals) {
            imageSelectedToyHighlightBG.gameObject.transform.position = buttonToyFractals.gameObject.transform.position;
        }
    }
    

    public void OpenToyReactionDiffusion() {
        toyAttractRepel.gameObject.SetActive(false);
        toyFluidSim.gameObject.SetActive(false);
        toyFractals.gameObject.SetActive(false);

        selectedToy = ToyType.ReactionDiffusion;
        toyReactionDiffusion.gameObject.SetActive(true);
        toyReactionDiffusion.Open();
        RefreshUI();
    }
    public void CloseToyReactionDiffusion() {

    }

    public void OpenToyAttractRepel() {
        toyReactionDiffusion.gameObject.SetActive(false);
        toyFluidSim.gameObject.SetActive(false);
        toyFractals.gameObject.SetActive(false);

        selectedToy = ToyType.AttractRepel;
        toyAttractRepel.gameObject.SetActive(true);
        //toyAttractRepel.Open();
        RefreshUI();
    }

    public void OpenToyFluidSim() {
        toyReactionDiffusion.gameObject.SetActive(false);
        toyAttractRepel.gameObject.SetActive(false);
        toyFractals.gameObject.SetActive(false);

        selectedToy = ToyType.FluidSim;
        toyFluidSim.gameObject.SetActive(true);
        //toyAttractRepel.Open();
        RefreshUI();
    }

    public void OpenToyFractals() {
        toyReactionDiffusion.gameObject.SetActive(false);
        toyAttractRepel.gameObject.SetActive(false);
        toyFluidSim.gameObject.SetActive(false);

        selectedToy = ToyType.Fractals;
        toyFractals.gameObject.SetActive(true);
        //toyFractals.Open();
        RefreshUI();
    }

}