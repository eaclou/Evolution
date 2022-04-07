using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Playcraft;

// * WPP: merge with Brain, delegate static and dynamic data to a struct and class, respectively

[Serializable]
public class BrainGenome 
{
    Lookup lookup => Lookup.instance;
    MetaNeuron hiddenTemplate => lookup.hiddenTemplate;
    
    public NeuronList neurons = new NeuronList();
    public AxonList axons = new AxonList();
    public int axonCount => axons.all.Count;
    
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

    public void InitializeNewBrainGenomeLists() 
    {
        neurons.Clear();
        axons.Clear();
    }

    public void InitializeRandomBrainGenome(BodyGenome bodyGenome) {
        InitializeNewBrainGenomeLists();
        neurons.Clear();
        
        InitializeIONeurons(bodyGenome);
        InitializeAxons();
    }

    /*void InitializeBodyNeuronList() {
        if (inOutNeurons == null) inOutNeurons = new List<Neuron>();
        else inOutNeurons.Clear();
    }*/

    public void InitializeIONeurons(BodyGenome bodyGenome) 
    {
        var newNeurons = bodyGenome.GetUnlockedNeurons(neurons.allCount);
        foreach (var neuron in newNeurons)
            neurons.Add(neuron);
        //SortIONeurons();
    }

    // WPP: obsolete with list management system
    /*void SortIONeurons()
    {
        inputNeurons.Clear();
        outputNeurons.Clear();
        
        foreach (var neuron in inOutNeurons) 
            SortIONeuron(neuron);
    }
    
    void SortIONeuron(Neuron neuron)
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
    }*/

    public void InitializeAxons() 
    {
        for (int i = 0; i < initialHiddenNeuronCount; i++) {
            var neuron = new Neuron(hiddenTemplate, neurons.inOutCount + i);
            neurons.Add(neuron);
        }
        
        LinkLayers(neurons.input, neurons.output, initialConnectionChance, initialWeightMultiplier);
        LinkLayers(neurons.input, neurons.hidden, initialConnectionChance, initialWeightMultiplier);
        LinkLayers(neurons.hidden, neurons.output, initialConnectionChance, initialWeightMultiplier);
    }

    void LinkLayers(List<Neuron> fromList, List<Neuron> toList, float connectionChance, float weight) 
    {
        foreach (var toElement in toList)
            foreach (var fromElement in fromList)
                RequestConnection(fromElement, toElement, connectionChance, weight);
    }
    
    /// Apply a random chance of connecting two neurons with a random weight
    void RequestConnection(Neuron from, Neuron to, float connectionChance, float weightMultiplier)
    {
        var connectNeurons = RandomStatics.CoinToss(connectionChance);
        if (!connectNeurons) return;
        
        var randomWeight = Gaussian.GetRandomGaussian() * weightMultiplier;
        var axon = new Axon(from, to, randomWeight);
        axons.Add(axon);
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
        //InitializeBodyNeuronList();
        neurons.ClearSublist(neurons.inOut);
        InitializeIONeurons(bodyGenome);

        //hiddenNeurons = MutateHiddenNeurons(parentGenome.hiddenNeurons);
        neurons.Sync(neurons.hidden, MutateHiddenNeurons(parentGenome.neurons.hidden));
        
        var mutatedAxons = MutateLinks(parentGenome.axons.all, settings);
        axons.Reset(mutatedAxons);

        //neurons.PrintCounts();
        //axons.PrintCounts();

        if (RandomStatics.CoinToss(settings.brainCreateNewLinkChance))
            AddNewLink(settings);

        if (RandomStatics.CoinToss(settings.brainCreateNewHiddenNodeChance)) 
            AddNewHiddenNeuron();

        var newNeurons = GetNewlyUnlockedNeurons(bodyGenome);
        //Debug.Log($"Initializing axons with {newNeurons.Count} new neurons from " +
        //          $"{bodyGenome.newlyUnlockedNeuronInfo.Count} new tech.");
        foreach (var neuron in newNeurons)
        {
            //SortIONeuron(neuron);
            neurons.Add(neuron);
            LinkNeuronToLayer(neuron);
        }
        //RemoveVestigialLinks();
    }

    List<Neuron> MutateHiddenNeurons(List<Neuron> original)
    {
        var result = new List<Neuron>();
        
        foreach (var element in original) 
        {
            var newHiddenNeuron = new Neuron(element);
            result.Add(newHiddenNeuron);
            
            // Create new neuron as a copy of parent neuron
            //NeuronGenome newHiddenNeuronGenome = new NeuronGenome(parentGenome.hiddenNeurons[i]);  
            
            // Might be able to simply copy hiddenNeuronList or individual hiddenNeuronGenomes from parent if they are functionally identical...
            // for now going with the thorough approach of a reference-less copy
            //hiddenNeurons.Add(newHiddenNeuronGenome);
        }
        
        return result;       
    }
    
    List<Axon> MutateLinks(List<Axon> original, MutationSettingsInstance settings)
    {
        var result = new List<Axon>();
        
        foreach (var element in original)
        {
            Axon newLinkGenome = new Axon(element.from, element.to, element.weight);
            
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
    
    List<Neuron> GetNewlyUnlockedNeurons(BodyGenome genome)
    {
        var result = new List<Neuron>();
        
        foreach (var neuron in neurons.inOut)
            foreach (var newNeuron in genome.newlyUnlockedNeuronInfo)
                if (neuron.template == newNeuron)
                    result.Add(neuron);
        
        return result;
    }
    
    /// For neurons created post-initialization, such as from mutations
    public void LinkNeuronToLayer(Neuron neuron)
    {
        switch (neuron.io)
        {
            case NeuronType.In:
                //Debug.Log($"Requesting connections from {neuron.data.name} to {outputNeurons.Count} output neurons");
                foreach (var outputNeuron in neurons.output)
                    RequestConnection(neuron, outputNeuron, initialConnectionChance, initialWeightMultiplier);
                break;
            case NeuronType.Out:
                //Debug.Log($"Requesting connections from input neurons {inputNeurons.Count} to {neuron.data.name}");
                foreach (var inputNeuron in neurons.input)
                    RequestConnection(inputNeuron, neuron, initialConnectionChance, initialWeightMultiplier);
                break;
        }
    }
    
    void AddNewLink(MutationSettingsInstance settings)
    {
        // What is this?!
        /*foreach (var neuron in hiddenNeurons)
        {
            inputNeurons.Add(neuron);
            outputNeurons.Add(neuron);
        }*/
        
        if (neurons.inCount <= 0 || neurons.outCount <= 0)
        {
            Debug.LogError("Cannot create new list because input or output list count is zero.  " + 
                           $"Input count = {neurons.inCount}, output count = {neurons.outCount}");
            return;
        }

        // Try x times to find new connection -- random scattershot approach at first:
        // other methods:
        //      -- make sure all bodyNeurons are fully-connected when modifying body
        int maxChecks = 8;
        for (int k = 0; k < maxChecks; k++) 
        {
            var from = neurons.input[Random.Range(0, neurons.inCount)];
            var to = neurons.output[Random.Range(0, neurons.outCount)];

            if (LinkExists(from, to)) continue;

            float randomWeight = Gaussian.GetRandomGaussian() * settings.brainWeightMutationStepSize;
            Axon linkGenome = new Axon(from, to, randomWeight);                    
            axons.Add(linkGenome);
            
            break;
        }
    }
    
    bool LinkExists(Neuron from, Neuron to)
    {
        foreach (var axon in axons.all)
        {
            // NG: links found by name but not by reference, 
            // suggesting that neurons are (somehow) not sharing references?!
            if (axon.from == from && axon.to == to)
            {
                Debug.Log("Link Exists by reference");
                return true;
            }
        
            if (axon.from.name == from.name && axon.to.name == to.name)
            {
                Debug.LogError("Axon found by name, not by reference");
                return true;
            }
        }
        
        return false;        
    }
    
    /// Replaces an In -> Out connection with In -> Hidden -> Out
    /// (assumes no hidden -> hidden connections)
    void AddNewHiddenNeuron()
    {
        if (axonCount == 0)
        {
            Debug.LogError("Cannot create new hidden neuron because axon count is zero");
            return;
        }
        
        // Create new hidden neuron
        var newHiddenNeuron = new Neuron(hiddenTemplate, neurons.all.Count);
        neurons.Add(newHiddenNeuron);
        
        // Create 2 new links
        var removedAxon = axons.inOut[Random.Range(0, axons.inOut.Count)];
        var inToHidden = new Axon(removedAxon.from, newHiddenNeuron, 1f);
        var hiddenToOut = new Axon(newHiddenNeuron, removedAxon.to, removedAxon.weight);

        // Delete old link
        axons.Remove(removedAxon);
            
        // Add new links
        axons.Add(inToHidden);
        axons.Add(hiddenToOut);
    }

    // WPP: what is the purpose of this?
    // (errors since 10/11/21 update)
    public void RemoveVestigialLinks() 
    {
        // Create a master list of all neurons:
        List<Neuron> allNeurons = new List<Neuron>();
        foreach (var neuron in neurons.inOut)
            allNeurons.Add(neuron);
        foreach (var neuron in neurons.hidden)
            allNeurons.Add(neuron);

        List<int> axonsToRemoveIndicesList = new List<int>();

        for (int j = 0; j < axonCount; j++) 
        {
            bool remove = !axons.all[j].IsInList(allNeurons);
            if (remove) axonsToRemoveIndicesList.Add(j);
        }

        //Debug.Log($"Removing {axonsToRemoveIndicesList.Count} vestigial links");
        if (axonsToRemoveIndicesList.Count > 0)
            for (int k = axonsToRemoveIndicesList.Count - 1; k >= 0; k--)
                axons.Remove(axonsToRemoveIndicesList[k]);
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