using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Playcraft;

[Serializable]
public class BrainGenome 
{
    public List<NeuronGenome> bodyNeuronList;
    public List<NeuronGenome> hiddenNeuronList;
    public List<LinkGenome> linkList;
    
    public void InitializeNewBrainGenomeLists() {
        bodyNeuronList = new List<NeuronGenome>();
        hiddenNeuronList = new List<NeuronGenome>();
        linkList = new List<LinkGenome>();
    }

    public void SetBodyNeuronsFromTemplate(BodyGenome templateBody) {
        InitializeBodyNeuronList();
        InitializeBodyNeurons(templateBody);
    }

    public void InitializeRandomBrainGenome(BodyGenome bodyGenome, float initialWeightMultiplier, float initialConnectionDensity, int numInitHiddenNeurons) {
        InitializeNewBrainGenomeLists();
        InitializeBodyNeurons(bodyGenome);
        InitializeAxons(initialWeightMultiplier, initialConnectionDensity, numInitHiddenNeurons);
    }
    
    void InitializeBodyNeuronList() {
        if (bodyNeuronList == null) bodyNeuronList = new List<NeuronGenome>();
        else bodyNeuronList.Clear();
    }

    public void InitializeBodyNeurons(BodyGenome bodyGenome) {
        bodyGenome.InitializeBrainGenome(bodyNeuronList);        
    }

    public void InitializeAxons(float initialWeightMultiplier, float initialConnectionDensity, int numInitHiddenNeurons) {
        var (inputNeuronList, outputNeuronList) = GetIOLists();

        // Create Hidden nodes TEMP!!!!
        for (int i = 0; i < numInitHiddenNeurons; i++) {
            NeuronGenome neuron = new NeuronGenome("Hid" + i, NeuronType.Hid, BrainModuleID.Undefined, i);
            hiddenNeuronList.Add(neuron);
        }
        
        LinkLayers(inputNeuronList, outputNeuronList, initialConnectionDensity, initialWeightMultiplier);
        LinkLayers(inputNeuronList, hiddenNeuronList, initialConnectionDensity, initialWeightMultiplier);
        LinkLayers(hiddenNeuronList, outputNeuronList, initialConnectionDensity, initialWeightMultiplier);

        //PrintBrainGenome();
        //Debug.Log("numAxons: " + linkList.Count.ToString());
    }
    
    // WPP: delegated function used in multiple locations
    // * consider moving to global static
    (List<NeuronGenome>, List<NeuronGenome>) GetIOLists() {
        List<NeuronGenome> inputNeuronList = new List<NeuronGenome>();
        List<NeuronGenome> outputNeuronList = new List<NeuronGenome>();
        
        foreach (var neuron in bodyNeuronList) {
            switch (neuron.neuronType) {
                case NeuronType.In: inputNeuronList.Add(neuron); break;
                case NeuronType.Out: outputNeuronList.Add(neuron); break;
            }
        }
        
        return (inputNeuronList, outputNeuronList);        
    }
    
    // WPP: simplified repetitive code with method
    void LinkLayers(List<NeuronGenome> fromList, List<NeuronGenome> toList, float initialConnectionDensity, float initialWeightMultiplier) {
        foreach (var toElement in toList) {
            foreach (var fromElement in fromList) {
                if (!RandomStatics.CoinToss(initialConnectionDensity)) continue;
                var randomWeight = Gaussian.GetRandomGaussian() * initialWeightMultiplier;
                var linkGenome = new LinkGenome(fromElement.nid.moduleID, fromElement.nid.neuronID, toElement.nid.moduleID, toElement.nid.neuronID, randomWeight, true);
                linkList.Add(linkGenome);
            }
        }          
    }

    public void SetToMutatedCopyOfParentGenome(BrainGenome parentGenome, BodyGenome bodyGenome, MutationSettingsInstance settings) 
    {
        //this.bodyNeuronList = parentGenome.bodyNeuronList; // UNSUSTAINABLE!!! might work now since all neuronLists are identical ******

        // Copy from parent brain or rebuild neurons from scratch based on the new mutated bodyGenome???? --- ******
        /*for(int i = 0; i < parentGenome.bodyNeuronList.Count; i++) {
            NeuronGenome newBodyNeuronGenome = new NeuronGenome(parentGenome.bodyNeuronList[i]);
            this.bodyNeuronList.Add(newBodyNeuronGenome);
        }*/
        // Alternate: SetBodyNeuronsFromTemplate(BodyGenome templateBody);
        // Rebuild BodyNeuronGenomeList from scratch based on bodyGenome!!!!
        InitializeBodyNeuronList();
        bodyGenome.InitializeBrainGenome(bodyNeuronList);

        // Existing Hidden Neurons!!
        hiddenNeuronList = new List<NeuronGenome>();
        for (int i = 0; i < parentGenome.hiddenNeuronList.Count; i++) {
            // Create new neuron as a copy of parent neuron
            NeuronGenome newHiddenNeuronGenome = new NeuronGenome(parentGenome.hiddenNeuronList[i]);  
            
            // Might be able to simply copy hiddenNeuronList or individual hiddenNeuronGenomes from parent if they are functionally identical...
            // for now going with the thorough approach of a reference-less copy
            hiddenNeuronList.Add(newHiddenNeuronGenome);
        }

        // Existing Links!!
        linkList = new List<LinkGenome>();
        for (int i = 0; i < parentGenome.linkList.Count; i++) {
            LinkGenome newLinkGenome = new LinkGenome(parentGenome.linkList[i].fromModuleID, parentGenome.linkList[i].fromNeuronID, parentGenome.linkList[i].toModuleID, parentGenome.linkList[i].toNeuronID, parentGenome.linkList[i].weight, true);
            
            if (RandomStatics.CoinToss(settings.brainRemoveLinkChance)) {
                newLinkGenome.weight = 0f;  // Remove fully??? *****
            }

            if (RandomStatics.CoinToss(settings.brainWeightMutationChance)) {
                float randomWeight = Gaussian.GetRandomGaussian();
                newLinkGenome.weight += Mathf.Lerp(0f, randomWeight, settings.brainWeightMutationStepSize);
            }

            newLinkGenome.weight *= settings.brainWeightDecayAmount;
            
            linkList.Add(newLinkGenome);
        }

        // Add new link:
        if (RandomStatics.CoinToss(settings.brainCreateNewLinkChance))
        {
            var (inputNeuronList, outputNeuronList) = GetIOLists();

            foreach (var neuron in hiddenNeuronList)
            {
                inputNeuronList.Add(neuron);
                outputNeuronList.Add(neuron);
            }

            // Try x times to find new connection -- random scattershot approach at first:
            // other methods:
            //      -- make sure all bodyNeurons are fully-connected when modifying body
            int maxChecks = 8;
            for (int k = 0; k < maxChecks; k++) {
                int randInputID = Random.Range(0, inputNeuronList.Count);
                NID fromNID = inputNeuronList[randInputID].nid;

                int randOutputID = Random.Range(0, outputNeuronList.Count);
                NID toNID = outputNeuronList[randOutputID].nid;

                // check if it exists:
                bool linkExists = false;    // Dictionary might be faster??? *****
                for (int l = 0; l < linkList.Count; l++) {
                    if (linkList[l].fromModuleID == fromNID.moduleID && linkList[l].fromNeuronID == fromNID.neuronID && linkList[l].toModuleID == toNID.moduleID && linkList[l].toNeuronID == toNID.neuronID) {
                        linkExists = true;
                        break;
                    }
                }

                if (linkExists) continue;
                float randomWeight = Gaussian.GetRandomGaussian() * settings.brainWeightMutationStepSize;
                LinkGenome linkGenome = new LinkGenome(fromNID.moduleID, fromNID.neuronID, toNID.moduleID, toNID.neuronID, randomWeight, true);                    
                linkList.Add(linkGenome);

                //Debug.Log("New Link! from: [" + fromNID.moduleID.ToString() + ", " + fromNID.neuronID.ToString() + "], to: [" + toNID.moduleID.ToString() + ", " + toNID.neuronID.ToString() + "]");
                break;
            }
        }

        // Add Brand New Hidden Neuron:
        if (RandomStatics.CoinToss(settings.brainCreateNewHiddenNodeChance)) {
            // find a link and expand it:
            int randLinkID = Random.Range(0, linkList.Count);
            
            // create new neuron
            NeuronGenome newNeuronGenome = new NeuronGenome("HidNew", NeuronType.Hid, BrainModuleID.Undefined, hiddenNeuronList.Count);
            hiddenNeuronList.Add(newNeuronGenome);
            // create 2 new links
            LinkGenome linkGenome1 = new LinkGenome(linkList[randLinkID].fromModuleID, linkList[randLinkID].fromNeuronID, newNeuronGenome.nid.moduleID, newNeuronGenome.nid.neuronID, 1f, true);
            LinkGenome linkGenome2 = new LinkGenome(newNeuronGenome.nid.moduleID, newNeuronGenome.nid.neuronID, linkList[randLinkID].toModuleID, linkList[randLinkID].toNeuronID, linkList[randLinkID].weight, true);

            // delete old link
            linkList[randLinkID].enabled = false; // *** un-needed? ****
            linkList.RemoveAt(randLinkID);
            // add new links
            linkList.Add(linkGenome1);
            linkList.Add(linkGenome2);

            //Debug.Log("New Neuron! " + newNeuronGenome.nid.neuronID.ToString() + " - from: [" + linkGenome1.fromModuleID.ToString() + ", " + linkGenome1.fromNeuronID.ToString() + "], to: [" + linkGenome2.toModuleID.ToString() + ", " + linkGenome2.toNeuronID.ToString() + "]");
        }

        RemoveVestigialLinks();
    }

    public void RemoveVestigialLinks() 
    {
        // Create a master list of all neurons:
        List<NeuronGenome> allNeuronsList = new List<NeuronGenome>();
        foreach (var neuron in bodyNeuronList)
            allNeuronsList.Add(neuron);
        foreach (var neuron in hiddenNeuronList)
            allNeuronsList.Add(neuron);

        List<int> axonsToRemoveIndicesList = new List<int>();

        // Keep a table of linearIndex positions for all neuron 2-dimensional ID's
        Dictionary<NID, int> IDs = new Dictionary<NID, int>();
        for (int j = 0; j < allNeuronsList.Count; j++) {
            IDs.Add(allNeuronsList[j].nid, j);
        }
        
        for (int j = 0; j < linkList.Count; j++) 
        {
            bool remove = !IDs.TryGetValue(new NID(linkList[j].fromModuleID, linkList[j].fromNeuronID), out int fromID) ||
                          !IDs.TryGetValue(new NID(linkList[j].toModuleID, linkList[j].toNeuronID), out int toID);
            
            //Debug.LogError("fromNID NOT FOUND " + linkList[j].fromModuleID.ToString() + ", " + linkList[j].fromNeuronID.ToString());

            if (remove) {
                axonsToRemoveIndicesList.Add(j);
            }
        }

        if (axonsToRemoveIndicesList.Count > 0)
            for(int k = axonsToRemoveIndicesList.Count - 1; k >= 0; k--)
                linkList.RemoveAt(axonsToRemoveIndicesList[k]);
    }

    /*public void MutateRandomly(float mutationChance) {
        for(int i = 0; i < linkList.Count; i++) {
            float rand = UnityEngine.Random.Range(0f, 1f);
            if(rand < mutationChance) {                
                float randomWeight = Gaussian.GetRandomGaussian();
                //Debug.Log("mutation! old: " + linkList[i].weight.ToString() + ", new: " + randomWeight.ToString());
                LinkGenome mutatedLink = new LinkGenome(linkList[i].fromModuleID, linkList[i].fromNeuronID, linkList[i].toModuleID, linkList[i].toNeuronID, randomWeight, true);
                linkList[i] = mutatedLink;
            }            
        }
    }*/
}
