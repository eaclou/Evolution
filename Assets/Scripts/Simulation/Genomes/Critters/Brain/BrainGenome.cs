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
    
    float initialWeightMultiplier;
    float initialConnectionChance;
    int initialHiddenNeuronCount;
    
    public BrainGenome(BodyGenome bodyGenome, float initialConnectionDensity, int hiddenNeuronCount) 
    {
        initialWeightMultiplier = 1f;
        initialConnectionChance = initialConnectionDensity;
        initialHiddenNeuronCount = hiddenNeuronCount;
        InitializeRandomBrainGenome(bodyGenome); 
    }
    
    public BrainGenome(BrainGenome parentGenome, BodyGenome bodyGenome, MutationSettingsInstance mutationSettings)
    {
        SetToMutatedCopyOfParentGenome(parentGenome, bodyGenome, mutationSettings);
    }
    
    public List<LinkGenome> links;
    public List<NeuronGenome> inOutNeurons;
    public List<NeuronGenome> hiddenNeurons;
    List<NeuronGenome> inputNeurons = new List<NeuronGenome>();
    List<NeuronGenome> outputNeurons = new List<NeuronGenome>();
    
    public int linkCount => links.Count;
    
    public void InitializeNewBrainGenomeLists() 
    {
        inOutNeurons = new List<NeuronGenome>();
        hiddenNeurons = new List<NeuronGenome>();
        links = new List<LinkGenome>();
    }

    public void InitializeRandomBrainGenome(BodyGenome bodyGenome) {
        InitializeNewBrainGenomeLists();
        InitializeIONeurons(bodyGenome);
        //PrintNeuronCounts();
        InitializeAxons();
    }

    void InitializeBodyNeuronList() {
        if (inOutNeurons == null) inOutNeurons = new List<NeuronGenome>();
        else inOutNeurons.Clear();
    }

    public void InitializeIONeurons(BodyGenome bodyGenome) 
    {
        bodyGenome.InitializeBrainGenome(inOutNeurons);   
        SortIONeurons();
    }
    
    void SortIONeurons()
    {
        inputNeurons.Clear();
        outputNeurons.Clear();
        
        foreach (var neuron in inOutNeurons) 
            SortIONeuron(neuron);
    }
    
    void SortIONeuron(NeuronGenome neuron)
    {
        switch (neuron.data.io) 
        {
            case NeuronType.In: 
                if (!inputNeurons.Contains(neuron)) 
                    inputNeurons.Add(neuron); 
                break;
            case NeuronType.Out: 
                if (!outputNeurons.Contains(neuron)) 
                    outputNeurons.Add(neuron); 
                break;
        }        
    }

    public void InitializeAxons() 
    {
        for (int i = 0; i < initialHiddenNeuronCount; i++) {
            var neuron = new NeuronGenome(hiddenTemplate, i);
            hiddenNeurons.Add(neuron);
        }
        
        LinkLayers(inputNeurons, outputNeurons, initialConnectionChance, initialWeightMultiplier);
        LinkLayers(inputNeurons, hiddenNeurons, initialConnectionChance, initialWeightMultiplier);
        LinkLayers(hiddenNeurons, outputNeurons, initialConnectionChance, initialWeightMultiplier);
    }
    
    void PrintNeuronCounts()
    {
        Debug.Log($"Linking layers with connection density of {initialConnectionChance}\n" +
                  $"{inputNeurons.Count} input neurons to {outputNeurons.Count} output neurons\n" +
                  $"{inputNeurons.Count} input neurons to {hiddenNeurons.Count} hidden neurons)" +
                  $"{hiddenNeurons.Count} hidden neurons to {outputNeurons.Count} output neurons");
    }

    void LinkLayers(List<NeuronGenome> fromList, List<NeuronGenome> toList, float connectionChance, float weight) 
    {
        foreach (var toElement in toList)
            foreach (var fromElement in fromList)
                RequestConnection(fromElement, toElement, connectionChance, weight);
    }
    
    /// Apply a random chance of connecting two neurons with a random weight
    void RequestConnection(NeuronGenome from, NeuronGenome to, float connectionChance, float weightMultiplier)
    {
        var connectNeurons = RandomStatics.CoinToss(connectionChance);
        if (!connectNeurons) return;
        var randomWeight = Gaussian.GetRandomGaussian() * weightMultiplier;
        var linkGenome = new LinkGenome(from, to, randomWeight, true);
        links.Add(linkGenome);        
    }

    public void SetToMutatedCopyOfParentGenome(BrainGenome parentGenome, BodyGenome bodyGenome, MutationSettingsInstance settings) 
    {
        /*
        //this.bodyNeuronList = parentGenome.bodyNeuronList; // UNSUSTAINABLE!!! might work now since all neuronLists are identical ******

        // Copy from parent brain or rebuild neurons from scratch based on the new mutated bodyGenome???? --- ******
        for(int i = 0; i < parentGenome.bodyNeuronList.Count; i++) {
            NeuronGenome newBodyNeuronGenome = new NeuronGenome(parentGenome.bodyNeuronList[i]);
            this.bodyNeuronList.Add(newBodyNeuronGenome);
        }
        // Alternate: SetBodyNeuronsFromTemplate(BodyGenome templateBody);
        */
        
        // Rebuild BodyNeuronGenomeList from scratch based on bodyGenome
        InitializeBodyNeuronList();
        InitializeIONeurons(bodyGenome);

        hiddenNeurons = MutateHiddenNeurons(parentGenome.hiddenNeurons);
        links = MutateLinks(parentGenome.links, settings);

        if (RandomStatics.CoinToss(settings.brainCreateNewLinkChance))
            AddNewLink(settings);

        if (RandomStatics.CoinToss(settings.brainCreateNewHiddenNodeChance)) 
            AddNewHiddenNeuron();

        var newNeurons = GetNewlyUnlockedNeurons(bodyGenome);
        //Debug.Log($"Initializing axons with {newNeurons.Count} new neurons from " +
        //          $"{bodyGenome.newlyUnlockedNeuronInfo.Count} new tech.");
        foreach (var neuron in newNeurons)
        {
            SortIONeuron(neuron);
            LinkNeuronToLayer(neuron);
        }
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
    
    List<NeuronGenome> GetNewlyUnlockedNeurons(BodyGenome genome)
    {
        var result = new List<NeuronGenome>();
        
        foreach (var neuron in inOutNeurons)
            foreach (var newNeuron in genome.newlyUnlockedNeuronInfo)
                if (neuron.data == newNeuron)
                    result.Add(neuron);
        
        return result;
    }
    
    /// For neurons created post-initialization, such as from mutations
    public void LinkNeuronToLayer(NeuronGenome neuron)
    {
        switch (neuron.io)
        {
            case NeuronType.In:
                //Debug.Log($"Requesting connections from {neuron.data.name} to {outputNeurons.Count} output neurons");
                foreach (var outputNeuron in outputNeurons)
                    RequestConnection(neuron, outputNeuron, initialConnectionChance, initialWeightMultiplier);
                break;
            case NeuronType.Out:
                //Debug.Log($"Requesting connections from input neurons {inputNeurons.Count} to {neuron.data.name}");
                foreach (var inputNeuron in inputNeurons)
                    RequestConnection(inputNeuron, neuron, initialConnectionChance, initialWeightMultiplier);
                break;
        }
    }
    
    void AddNewLink(MutationSettingsInstance settings)
    {
        foreach (var neuron in hiddenNeurons)
        {
            inputNeurons.Add(neuron);
            outputNeurons.Add(neuron);
        }
        
        if (inputNeurons.Count <= 0 || outputNeurons.Count <= 0)
        {
            Debug.LogError("Cannot create new list because input or output list count is zero.  " + 
                           $"Input count = {inputNeurons.Count}, output count = {outputNeurons.Count}");
            return;
        }

        // Try x times to find new connection -- random scattershot approach at first:
        // other methods:
        //      -- make sure all bodyNeurons are fully-connected when modifying body
        int maxChecks = 8;
        for (int k = 0; k < maxChecks; k++) 
        {
            int randInputID = Random.Range(0, inputNeurons.Count);
            var from = inputNeurons[randInputID];

            int randOutputID = Random.Range(0, outputNeurons.Count);
            var to = outputNeurons[randOutputID];

            if (LinkExists(from, to)) continue;

            float randomWeight = Gaussian.GetRandomGaussian() * settings.brainWeightMutationStepSize;
            LinkGenome linkGenome = new LinkGenome(from, to, randomWeight, true);                    
            links.Add(linkGenome);

            //Debug.Log("New Link! from: [" + fromNID.moduleID.ToString() + ", " + fromNID.neuronID.ToString() + "], to: [" + toNID.moduleID.ToString() + ", " + toNID.neuronID.ToString() + "]");
            break;
        }
    }
    
    bool LinkExists(NeuronGenome from, NeuronGenome to)
    {
        foreach (var link in links)
            if (link.from == from && link.to == to)
                return true;
        
        return false;        
    }
    
    /// Finds a link and expands it
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


}

#region Dead code
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
#endregion