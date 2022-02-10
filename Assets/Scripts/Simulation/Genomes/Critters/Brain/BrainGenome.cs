using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Playcraft;

[Serializable]
public class BrainGenome 
{
    Lookup lookup => Lookup.instance;
    MetaNeuron hiddenTemplate => lookup.hiddenTemplate;
    
    public BrainGenome(BodyGenome bodyGenome, float initialConnectionDensity, int hiddenNeuronCount) 
    {
        InitializeRandomBrainGenome(bodyGenome, 1f, initialConnectionDensity, hiddenNeuronCount); 
    }
    
    public BrainGenome(BrainGenome parentGenome, BodyGenome bodyGenome, MutationSettingsInstance mutationSettings)
    {
        SetToMutatedCopyOfParentGenome(parentGenome, bodyGenome, mutationSettings);
    }
    
    public List<LinkGenome> links;
    public List<NeuronGenome> inOutNeurons;
    public List<NeuronGenome> hiddenNeurons;
    List<NeuronGenome> inputNeuronList = new List<NeuronGenome>();
    List<NeuronGenome> outputNeuronList = new List<NeuronGenome>();
    
    public int linkCount => links.Count;
    
    public void InitializeNewBrainGenomeLists() 
    {
        inOutNeurons = new List<NeuronGenome>();
        hiddenNeurons = new List<NeuronGenome>();
        links = new List<LinkGenome>();
    }

    // WPP: Never used
    //public void SetBodyNeuronsFromTemplate(BodyGenome templateBody) {
    //    InitializeBodyNeuronList();
    //    InitializeBodyNeurons(templateBody);
    //}

    public void InitializeRandomBrainGenome(BodyGenome bodyGenome, float initialWeightMultiplier, float initialConnectionDensity, int numInitHiddenNeurons) {
        InitializeNewBrainGenomeLists();
        InitializeIONeurons(bodyGenome);
        InitializeAxons(initialWeightMultiplier, initialConnectionDensity, numInitHiddenNeurons);
    }
    
    void InitializeBodyNeuronList() {
        if (inOutNeurons == null) inOutNeurons = new List<NeuronGenome>();
        else inOutNeurons.Clear();
    }

    public void InitializeIONeurons(BodyGenome bodyGenome) 
    {
        bodyGenome.InitializeBrainGenome(inOutNeurons);   
        
        foreach (var neuron in inOutNeurons) {
            switch (neuron.data.io) {
                case NeuronType.In: inputNeuronList.Add(neuron); break;
                case NeuronType.Out: outputNeuronList.Add(neuron); break;
            }
        }  
    }

    public void InitializeAxons(float initialWeightMultiplier, float initialConnectionDensity, int numInitHiddenNeurons) 
    {
        for (int i = 0; i < numInitHiddenNeurons; i++) {
            //NeuronGenome neuron = new NeuronGenome("Hid" + i, NeuronType.Hid, BrainModuleID.Undefined, i);
            var neuron = new NeuronGenome(hiddenTemplate, i);
            hiddenNeurons.Add(neuron);
        }
        
        //Debug.Log($"Linking layers with connection density of {initialConnectionDensity}");
        //Debug.Log($"{inputNeuronList.Count} input neurons to {outputNeuronList.Count} output neurons");
        LinkLayers(inputNeuronList, outputNeuronList, initialConnectionDensity, initialWeightMultiplier);
        //Debug.Log($"{inputNeuronList.Count} input neurons to {hiddenNeurons.Count} hidden neurons");
        LinkLayers(inputNeuronList, hiddenNeurons, initialConnectionDensity, initialWeightMultiplier);
        //Debug.Log($"{hiddenNeurons.Count} hidden neurons to {outputNeuronList.Count} output neurons");
        LinkLayers(hiddenNeurons, outputNeuronList, initialConnectionDensity, initialWeightMultiplier);
    }
    
    // WPP: build once on full initialization and cache result
    // * Consider moving to global static
    /*(List<MetaNeuron>, List<MetaNeuron>) GetIOLists() {
        List<MetaNeuron> inputNeuronList = new List<MetaNeuron>();
        List<MetaNeuron> outputNeuronList = new List<MetaNeuron>();
        
        foreach (var neuron in inOutNeurons) {
            switch (neuron.io) {
                case NeuronType.In: inputNeuronList.Add(neuron); break;
                case NeuronType.Out: outputNeuronList.Add(neuron); break;
            }
        }
        
        return (inputNeuronList, outputNeuronList);        
    }*/
    
    void LinkLayers(List<NeuronGenome> fromList, List<NeuronGenome> toList, float initialConnectionDensity, float initialWeightMultiplier) 
    {
        //int debugConnectionCount = 0;
        
        foreach (var toElement in toList) 
        {
            foreach (var fromElement in fromList) 
            {
                var connectNeurons = RandomStatics.CoinToss(initialConnectionDensity);
                //Debug.Log($"{initialConnectionDensity} {connectNeurons}");
                if (!connectNeurons) continue;
                var randomWeight = Gaussian.GetRandomGaussian() * initialWeightMultiplier;
                var linkGenome = new LinkGenome(fromElement, toElement, randomWeight, true);
                links.Add(linkGenome);
                
                //debugConnectionCount++;
            }
        }
        
        //Debug.Log($"{debugConnectionCount} links found out of possible {fromList.Count * toList.Count}");
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
        bodyGenome.InitializeBrainGenome(inOutNeurons);  

        hiddenNeurons = MutateHiddenNeurons(parentGenome.hiddenNeurons);
        links = MutateLinks(parentGenome.links, settings);

        if (RandomStatics.CoinToss(settings.brainCreateNewLinkChance))
            AddNewLink(settings);

        if (RandomStatics.CoinToss(settings.brainCreateNewHiddenNodeChance)) 
            AddNewHiddenNeuron();

        //RemoveVestigialLinks();
    }
    
    List<NeuronGenome> MutateHiddenNeurons(List<NeuronGenome> original)
    {
        var result = new List<NeuronGenome>();
        
        foreach (var element in original) 
        {
            var newHiddenNeuron = new NeuronGenome(element);
            result.Add(newHiddenNeuron);
            
            // Create new neuron as a copy of parent neuron
            //NeuronGenome newHiddenNeuronGenome = new NeuronGenome(parentGenome.hiddenNeurons[i]);  
            
            // Might be able to simply copy hiddenNeuronList or individual hiddenNeuronGenomes from parent if they are functionally identical...
            // for now going with the thorough approach of a reference-less copy
            //hiddenNeurons.Add(newHiddenNeuronGenome);
        }
        
        return result;       
    }
    
    List<LinkGenome> MutateLinks(List<LinkGenome> original, MutationSettingsInstance settings)
    {
        var result = new List<LinkGenome>();
        
        foreach (var element in original)
        {
            LinkGenome newLinkGenome = new LinkGenome(element.from, element.to, element.weight, true);
            
            // Remove fully??? *****
            if (RandomStatics.CoinToss(settings.brainRemoveLinkChance))
                newLinkGenome.weight = 0f;

            if (RandomStatics.CoinToss(settings.brainWeightMutationChance)) 
            {
                float randomWeight = Gaussian.GetRandomGaussian();
                newLinkGenome.weight += Mathf.Lerp(0f, randomWeight, settings.brainWeightMutationStepSize);
            }

            newLinkGenome.weight *= settings.brainWeightDecayAmount;
            result.Add(newLinkGenome);
        }
        
        return result;
    }
    
    void AddNewLink(MutationSettingsInstance settings)
    {
        foreach (var neuron in hiddenNeurons)
        {
            inputNeuronList.Add(neuron);
            outputNeuronList.Add(neuron);
        }
        
        if (inputNeuronList.Count <= 0 || outputNeuronList.Count <= 0)
        {
            Debug.LogError("Cannot create new list because input or output list count is zero.  " + 
                           $"Input count = {inputNeuronList.Count}, output count = {outputNeuronList.Count}");
            return;
        }

        // Try x times to find new connection -- random scattershot approach at first:
        // other methods:
        //      -- make sure all bodyNeurons are fully-connected when modifying body
        int maxChecks = 8;
        for (int k = 0; k < maxChecks; k++) 
        {
            int randInputID = Random.Range(0, inputNeuronList.Count);
            var from = inputNeuronList[randInputID];

            int randOutputID = Random.Range(0, outputNeuronList.Count);
            var to = outputNeuronList[randOutputID];

            // * WPP: delegate to function
            bool linkExists = false;    // Dictionary might be faster??? *****
            foreach (var link in links) 
            {
                if (link.from != from || link.to != to) 
                    continue;
                    
                linkExists = true;
                break;
            }
            if (linkExists) continue;
                
            float randomWeight = Gaussian.GetRandomGaussian() * settings.brainWeightMutationStepSize;
            LinkGenome linkGenome = new LinkGenome(from, to, randomWeight, true);                    
            links.Add(linkGenome);

            //Debug.Log("New Link! from: [" + fromNID.moduleID.ToString() + ", " + fromNID.neuronID.ToString() + "], to: [" + toNID.moduleID.ToString() + ", " + toNID.neuronID.ToString() + "]");
            break;
        }
    }
    
    // Finds a link and expands it
    void AddNewHiddenNeuron()
    {
        if (linkCount == 0)
        {
            Debug.LogError("Cannot create new hidden neuron because link list count is zero");
            return;
        }
    
        int randLinkID = Random.Range(0, linkCount);
            
        // Create new neuron
        //NeuronGenome newNeuronGenome = new NeuronGenome("HidNew", NeuronType.Hid, BrainModuleID.Undefined, hiddenNeurons.Count);
        var newHiddenNeuron = new NeuronGenome(hiddenTemplate, hiddenNeurons.Count);
        hiddenNeurons.Add(newHiddenNeuron);
        
        // create 2 new links
        LinkGenome linkGenome1 = new LinkGenome(links[randLinkID].from, newHiddenNeuron, 1f, true);
        LinkGenome linkGenome2 = new LinkGenome(newHiddenNeuron, links[randLinkID].to, links[randLinkID].weight, true);

        // delete old link
        links[randLinkID].enabled = false; // *** not needed? ****
        links.RemoveAt(randLinkID);
            
        // add new links
        links.Add(linkGenome1);
        links.Add(linkGenome2);

        //Debug.Log("New Neuron! " + newNeuronGenome.nid.neuronID.ToString() + 
            // " - from: [" + linkGenome1.fromModuleID.ToString() + ", " + 
            // linkGenome1.fromNeuronID.ToString() + "], to: [" + linkGenome2.toModuleID.ToString() + 
            // ", " + linkGenome2.toNeuronID.ToString() + "]");        
    }

    // WPP: what is the purpose of this?
    // (errors since 10/11/21 update)
    public void RemoveVestigialLinks() 
    {
        // Create a master list of all neurons:
        List<NeuronGenome> allNeurons = new List<NeuronGenome>();
        foreach (var neuron in inOutNeurons)
            allNeurons.Add(neuron);
        foreach (var neuron in hiddenNeurons)
            allNeurons.Add(neuron);

        List<int> axonsToRemoveIndicesList = new List<int>();

        // Keep a table of linearIndex positions for all neuron 2-dimensional ID's
        //Dictionary<NeuronGenome, int> IDs = new Dictionary<NeuronGenome, int>();
        //for (int j = 0; j < allNeuronsList.Count; j++) 
        //{
        //    IDs.Add(allNeuronsList[j], j);
        //}
        
        for (int j = 0; j < links.Count; j++) 
        {
            //bool remove = !IDs.TryGetValue(new NeuronGenome(links[j].from), out int fromID) ||
            //              !IDs.TryGetValue(new NeuronGenome(links[j].to), out int toID);
            bool remove = !links[j].IsInList(allNeurons);

            //Debug.LogError("fromNID NOT FOUND " + linkList[j].fromModuleID.ToString() + ", " + linkList[j].fromNeuronID.ToString());

            if (remove) axonsToRemoveIndicesList.Add(j);
        }

        //Debug.Log($"Removing {axonsToRemoveIndicesList.Count} vestigial links");
        if (axonsToRemoveIndicesList.Count > 0)
            for (int k = axonsToRemoveIndicesList.Count - 1; k >= 0; k--)
                links.RemoveAt(axonsToRemoveIndicesList[k]);
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
