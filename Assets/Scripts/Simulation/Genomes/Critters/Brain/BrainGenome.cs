using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Playcraft;

// * WPP: merge with Brain, delegate static and dynamic data to a struct and class, respectively

/// Stores/process brain data, wires up neurons and axons.
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
    
    public BrainGenome(BrainGenome parent, BodyGenome self, MutationSettingsInstance mutationSettings)
    {
        SetToMutatedCopy(parent, self, mutationSettings);
    }

    public void ClearNeuronsAndAxons() 
    {
        neurons.Clear();
        axons.Clear();
    }

    /// For creating new species
    public void InitializeRandomBrainGenome(BodyGenome bodyGenome) 
    {
        ClearNeuronsAndAxons();
        InitializeIONeurons(bodyGenome);
        InitializeAxons();
    }
    
    public void InitializeIONeurons(BodyGenome bodyGenome) 
    {
        neurons.ClearSublist(neurons.inOut);

        var newNeurons = bodyGenome.GetUnlockedNeurons();
        foreach (var neuron in newNeurons)
            neurons.Add(neuron);
    }

    public void InitializeAxons() 
    {
        for (int i = 0; i < initialHiddenNeuronCount; i++) {
            var neuron = new Neuron(hiddenTemplate);
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

    public void SetToMutatedCopy(BrainGenome parentGenome, BodyGenome bodyGenome, MutationSettingsInstance settings) 
    {
        InitializeIONeurons(bodyGenome);
        MutateHiddenNeurons(parentGenome.neurons.hidden);
        
        var newNeurons = GetNewlyUnlockedNeurons(bodyGenome);
        
        foreach (var neuron in newNeurons)
            neurons.Add(neuron);

        foreach (var axon in parentGenome.axons.all)
            axons.Add(new Axon(FindNeuron(axon.from), FindNeuron(axon.to), axon.weight));
        
        MutateAxons(axons.all, settings);

        foreach (var neuron in newNeurons)
            LinkNeuronToLayer(neuron);
            
        // TBD: add logic to make this happen more than once (requires design decisions)
        if (RandomStatics.CoinToss(settings.brainCreateNewLinkChance))
            AddRandomConnection(settings);
            
        // TBD: add logic to make this happen more than once (requires design decisions)     
        if (RandomStatics.CoinToss(settings.brainCreateNewHiddenNodeChance)) 
            AddNewHiddenNeuron();

        //RemoveVestigialLinks();
    }

    // Future use
    void MutateHiddenNeurons(List<Neuron> parentHiddenNeurons)
    {
        neurons.Sync(neurons.hidden, parentHiddenNeurons);
    
        foreach (var neuron in neurons.hidden) 
        {
            // * Random mutations go here  
        }
    }
    
    /// Modifies weights and potentially removes axons.
    void MutateAxons(List<Axon> list, MutationSettingsInstance settings)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            // Random chance of removing each axon
            if (RandomStatics.CoinToss(settings.brainRemoveLinkChance))
            {
                axons.Remove(list[i]);
                continue;
            }

            // Random chance of modifying weight of each axon
            if (RandomStatics.CoinToss(settings.brainWeightMutationChance)) 
            {
                float randomWeight = Gaussian.GetRandomGaussian();
                list[i].weight += Mathf.Lerp(0f, randomWeight, settings.brainWeightMutationStepSize);
            }

            list[i].weight *= settings.brainWeightDecayAmount;
        }
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
                foreach (var outputNeuron in neurons.output)
                    RequestConnection(neuron, outputNeuron, initialConnectionChance, initialWeightMultiplier);
                break;
            case NeuronType.Out:
                foreach (var inputNeuron in neurons.input)
                    RequestConnection(inputNeuron, neuron, initialConnectionChance, initialWeightMultiplier);
                break;
        }
    }
    
    void AddRandomConnection(MutationSettingsInstance settings)
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
            var from = neurons.RandomNeuron(neurons.inHidden);
            var to = neurons.RandomNeuron(neurons.hiddenOut);

            if (LinkExists(from, to)) continue;

            float randomWeight = Gaussian.GetRandomGaussian() * settings.brainWeightMutationStepSize;
            Axon axon = new Axon(from, to, randomWeight);               
            axons.Add(axon);

            break;
        }
    }
    
    bool LinkExists(Neuron from, Neuron to)
    {
        foreach (var axon in axons.all)
            if (axon.from == from && axon.to == to)
                return true;

        return false;        
    }
    
    /// Replaces an In -> Out connection with In -> Hidden -> Out
    /// or extends a chain of hidden neurons
    void AddNewHiddenNeuron()
    {
        if (axonCount == 0)
        {
            Debug.LogError("Cannot create new hidden neuron because axon count is zero");
            return;
        }
        
        // Create new hidden neuron
        var newHiddenNeuron = new Neuron(hiddenTemplate);
        neurons.Add(newHiddenNeuron);
        
        // Create 2 new axons
        var removed = axons.all[Random.Range(0, axons.all.Count)];
        var newInput = new Axon(removed.from, newHiddenNeuron, 1f);
        var newOutput = new Axon(newHiddenNeuron, removed.to, removed.weight);

        // Delete old axon
        axons.Remove(removed);
            
        // Add new links
        axons.Add(newInput);
        axons.Add(newOutput);
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
    
    public bool HasInvalidAxons() { return HasInvalidAxons(this); }
    public bool HasInvalidAxons(BrainGenome brain)
    {
        foreach (var axon in brain.axons.all)
        {
            if (!axons.IsValid(axon))
            {
                Debug.LogError($"Invalid axon [{axon.index}] {axon.uniqueID} detected connecting " +
                               $"[{axon.from.index}] {axon.from.io} {axon.from.name} {axon.from.data.uniqueID} " +
                               $"to [{axon.to.index}] {axon.to.io} {axon.to.name} {axon.to.data.uniqueID}");
                return true;
            }
        }
                
        return false;
    }
    
    
    Neuron FindNeuron(Neuron oldNeuron)
    {
        var nameToFind = oldNeuron.name;

        foreach (var neuron in neurons.all)
            if (neuron.name == nameToFind)
                return neuron;

        // Reconsider what should happen here if it comes up (create new neuron with old data?)
        Debug.LogError($"Unable to find neuron [{oldNeuron.index}] {nameToFind}");
        
        return oldNeuron;
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