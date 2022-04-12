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
        
        MonoSim.instance.SimInvoke(DelayPrintAxonError, 0.2f);
    }
    
    void DelayPrintAxonError()
    {
        Debug.Log($"Normal Construction failure: {HasInvalidAxons()}");
    }

    public BrainGenome(BrainGenome parent, BodyGenome self, MutationSettingsInstance mutationSettings)
    {
        if (HasInvalidAxons(parent)) // NG
            Debug.LogError("Invalid axons detected in parent: mutation construction");
    
        SetToMutatedCopy(parent, self, mutationSettings);
        
        //if (HasInvalidAxons())   // NG: downstream error of parent
        //    Debug.LogError("Invalid axons detected: mutation construction");
    }

    public void ClearNeuronsAndAxons() 
    {
        neurons.Clear();
        axons.Clear();
    }

    public void InitializeRandomBrainGenome(BodyGenome bodyGenome) 
    {
        ClearNeuronsAndAxons();
        InitializeIONeurons(bodyGenome);
        InitializeAxons();
    }

    /*void InitializeBodyNeuronList() {
        if (inOutNeurons == null) inOutNeurons = new List<Neuron>();
        else inOutNeurons.Clear();
    }*/

    public void InitializeIONeurons(BodyGenome bodyGenome) 
    {
        neurons.ClearSublist(neurons.inOut);

        var newNeurons = bodyGenome.GetUnlockedNeurons(neurons.allCount);
        foreach (var neuron in newNeurons)
            neurons.Add(neuron);
        //SortIONeurons();
    }

    #region [WPP] Obsolete with list management system
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
    #endregion

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
        
        if (from.io == to.io)  // OK
            Debug.LogError($"Connection requested between matching io {from.io}");
        
        var randomWeight = Gaussian.GetRandomGaussian() * weightMultiplier;
        var axon = new Axon(from, to, randomWeight);  // OK
        axons.Add(axon);
    }

    public void SetToMutatedCopy(BrainGenome parentGenome, BodyGenome bodyGenome, MutationSettingsInstance settings) 
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
        
        // NG: downstream error of SpeciesGenomePool.Mutate()
        //if (HasInvalidAxons(parentGenome))
        //    Debug.LogError("BrainGenome.SetToMutatedCopy(): Invalid axon(s) detected");

        // * WPP: this seems excessive...
        InitializeIONeurons(bodyGenome);
        //hiddenNeurons = MutateHiddenNeurons(parentGenome.hiddenNeurons);
        neurons.Sync(neurons.hidden, MutateHiddenNeurons(parentGenome.neurons.hidden));
        //ValidateAxons();

        var mutatedAxons = MutateLinks(parentGenome.axons.all, settings);
        axons.Reset(mutatedAxons);

        //neurons.PrintCounts();
        //axons.PrintCounts();

        // * WPP: how many times should this run (current is once/generation)?
        if (RandomStatics.CoinToss(settings.brainCreateNewLinkChance))
            AddRandomConnection(settings);
        
        // * WPP: how many times should this run (current is once/generation)?
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
    
    /// Creates a mutated copy of a list of axons.
    /// Modifies weights and potentially removes axons.
    List<Axon> MutateLinks(List<Axon> original, MutationSettingsInstance settings)
    {
        var result = new List<Axon>();
        
        foreach (var element in original)
        {
            //if (!axons.IsValid(element))   // NG
            //    Debug.LogError($"Invalid mutated axon: matching io {element.from.io} - {element.to.io}");        
        
            Axon newAxon = new Axon(element.from, element.to, element.weight);   // NG

            // Random chance of removing each axon
            if (RandomStatics.CoinToss(settings.brainRemoveLinkChance))
                continue;

            // Random chance of modifying weight of each axon
            if (RandomStatics.CoinToss(settings.brainWeightMutationChance)) 
            {
                float randomWeight = Gaussian.GetRandomGaussian();
                newAxon.weight += Mathf.Lerp(0f, randomWeight, settings.brainWeightMutationStepSize);
            }

            newAxon.weight *= settings.brainWeightDecayAmount;
            result.Add(newAxon);
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
            // WPP: only adds in->out connections 
            // (should also allow for in->hidden, hidden->hidden, and hidden->out)
            var from = neurons.RandomNeuron(neurons.input);
            var to = neurons.RandomNeuron(neurons.output);

            if (LinkExists(from, to)) continue;
            
            if (!axons.IsValid(from, to))   // OK
                Debug.LogError($"Invalid random connection: matching io {from} - {to}");

            float randomWeight = Gaussian.GetRandomGaussian() * settings.brainWeightMutationStepSize;
            Axon axon = new Axon(from, to, randomWeight);    // OK                
            axons.Add(axon);

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
        
            // Hackfix applied in Brain.ValidateAxons
            /*if (axon.from.name == from.name && axon.to.name == to.name)
            {
                Debug.LogError("Axon found by name, not by reference");
                return true;
            }*/
        }
        
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
        var newHiddenNeuron = new Neuron(hiddenTemplate, neurons.all.Count);
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
            if (!axons.IsValid(axon))
                return true;
                
        return false;
    }
    
    #region Hackfix for mismatch between axon connection targets and actual neurons
    
    public void ValidateAxons()
    {
        foreach (var axon in axons.all)
            if (!neurons.all.Contains(axon.from))
                axon.from = FindNeuron(axon.from);
                
        foreach (var axon in axons.all)
            if (!neurons.all.Contains(axon.to))
                axon.to = FindNeuron(axon.to);
    }
    
    Neuron FindNeuron(Neuron invalidNeuron)
    {
        foreach (var neuron in neurons.all)
            if (neuron.index == invalidNeuron.index)
                return neuron;
                
        return invalidNeuron;
    }
    
    #endregion
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