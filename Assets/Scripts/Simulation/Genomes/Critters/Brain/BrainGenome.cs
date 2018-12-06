using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BrainGenome {

    public List<NeuronGenome> bodyNeuronList;
    public List<NeuronGenome> hiddenNeuronList;
    public List<LinkGenome> linkList;

    public BrainGenome() {

    }

    public void InitializeNewBrainGenomeLists() {
        bodyNeuronList = new List<NeuronGenome>();
        hiddenNeuronList = new List<NeuronGenome>();
        linkList = new List<LinkGenome>();
    }

    public void SetBodyNeuronsFromTemplate(BodyGenome templateBody) {
        if (bodyNeuronList == null) {
            bodyNeuronList = new List<NeuronGenome>();
        }
        else {
            bodyNeuronList.Clear();
        }

        InitializeBodyNeurons(templateBody);
    }

    public void InitializeRandomBrainGenome(BodyGenome bodyGenome, float initialWeightMultiplier, int numInitHiddenNeurons) {
        InitializeNewBrainGenomeLists();
        InitializeBodyNeurons(bodyGenome);
        InitializeAxons(initialWeightMultiplier, 0.2f, numInitHiddenNeurons);
    }

    public void InitializeBodyNeurons(BodyGenome bodyGenome) {
        bodyGenome.InitializeBrainGenome(bodyNeuronList);        
    }

    public void InitializeAxons(float initialWeightMultiplier, float initialConnectionDensity, int numInitHiddenNeurons) {                
        // Create initial connections -- :
        List<NeuronGenome> inputNeuronList = new List<NeuronGenome>();
        List<NeuronGenome> outputNeuronList = new List<NeuronGenome>();
        for (int i = 0; i < bodyNeuronList.Count; i++) {
            if (bodyNeuronList[i].neuronType == NeuronGenome.NeuronType.In) {
                inputNeuronList.Add(bodyNeuronList[i]);
            }
            if (bodyNeuronList[i].neuronType == NeuronGenome.NeuronType.Out) {
                outputNeuronList.Add(bodyNeuronList[i]);
            }
        }
        //Create Hidden nodes TEMP!!!!
        for (int i = 0; i < numInitHiddenNeurons; i++) {
            NeuronGenome neuron = new NeuronGenome(NeuronGenome.NeuronType.Hid, -1, i);
            hiddenNeuronList.Add(neuron);
        }
        // Initialize partially connected with all weights Random
        
        for (int i = 0; i < outputNeuronList.Count; i++) {  // Direct Skip connection In to Out:
            for (int j = 0; j < inputNeuronList.Count; j++) {
                if(UnityEngine.Random.Range(0f, 1f) < initialConnectionDensity) {  // 0-1 % chance of link
                    float randomWeight = Gaussian.GetRandomGaussian() * initialWeightMultiplier;
                    LinkGenome linkGenome = new LinkGenome(inputNeuronList[j].nid.moduleID, inputNeuronList[j].nid.neuronID, outputNeuronList[i].nid.moduleID, outputNeuronList[i].nid.neuronID, randomWeight, true);
                    linkList.Add(linkGenome);
                }                
            }
        }
        for (int i = 0; i < hiddenNeuronList.Count; i++) {  // Input to Hidden Layer:
            for (int j = 0; j < inputNeuronList.Count; j++) {
                if(UnityEngine.Random.Range(0f, 1f) < initialConnectionDensity) {
                    float randomWeight = Gaussian.GetRandomGaussian() * initialWeightMultiplier;
                    LinkGenome linkGenome = new LinkGenome(inputNeuronList[j].nid.moduleID, inputNeuronList[j].nid.neuronID, hiddenNeuronList[i].nid.moduleID, hiddenNeuronList[i].nid.neuronID, randomWeight, true);
                    linkList.Add(linkGenome);
                }
                
            }
        }
        for (int i = 0; i < outputNeuronList.Count; i++) {   // Hidden Layer to Output:
            for (int j = 0; j < hiddenNeuronList.Count; j++) {
                if(UnityEngine.Random.Range(0f, 1f) < initialConnectionDensity) {
                    float randomWeight = Gaussian.GetRandomGaussian() * initialWeightMultiplier;
                    LinkGenome linkGenome = new LinkGenome(hiddenNeuronList[j].nid.moduleID, hiddenNeuronList[j].nid.neuronID, outputNeuronList[i].nid.moduleID, outputNeuronList[i].nid.neuronID, randomWeight, true);
                    linkList.Add(linkGenome);
                }
            }
        }        

        //PrintBrainGenome();
        //Debug.Log("numAxons: " + linkList.Count.ToString());
    }

    public void SetToMutatedCopyOfParentGenome(BrainGenome parentGenome, BodyGenome bodyGenome, MutationSettings settings) {

        //this.bodyNeuronList = parentGenome.bodyNeuronList; // UNSUSTAINABLE!!! might work now since all neuronLists are identical ******

        // Copy from parent brain or rebuild neurons from scratch based on the new mutated bodyGenome???? --- ******
        /*for(int i = 0; i < parentGenome.bodyNeuronList.Count; i++) {
            NeuronGenome newBodyNeuronGenome = new NeuronGenome(parentGenome.bodyNeuronList[i]);
            this.bodyNeuronList.Add(newBodyNeuronGenome);
        }*/
        // Alternate: SetBodyNeuronsFromTemplate(BodyGenome templateBody);
        // Rebuild BodyNeuronGenomeList from scratch based on bodyGenome!!!!
        if(this.bodyNeuronList != null) {
            this.bodyNeuronList.Clear();
        }
        else {
            this.bodyNeuronList = new List<NeuronGenome>();
        }
        bodyGenome.InitializeBrainGenome(this.bodyNeuronList);

        // Existing Hidden Neurons!!
        hiddenNeuronList = new List<NeuronGenome>();
        for (int i = 0; i < parentGenome.hiddenNeuronList.Count; i++) {
            NeuronGenome newHiddenNeuronGenome = new NeuronGenome(parentGenome.hiddenNeuronList[i]);  // create new neuron as a copy of parent neuron
            // Might be able to simply copy hiddenNeuronList or individual hiddenNeuronGenomes from parent if they are functionally identical...
            // for now going with the thorough approach of a reference-less copy
            hiddenNeuronList.Add(newHiddenNeuronGenome);
        }

        // Existing Links!!
        linkList = new List<LinkGenome>();
        for (int i = 0; i < parentGenome.linkList.Count; i++) {
            LinkGenome newLinkGenome = new LinkGenome(parentGenome.linkList[i].fromModuleID, parentGenome.linkList[i].fromNeuronID, parentGenome.linkList[i].toModuleID, parentGenome.linkList[i].toNeuronID, parentGenome.linkList[i].weight, true);

            float randZeroChance = UnityEngine.Random.Range(0f, 1f);
            if (randZeroChance < settings.removeLinkChance) {
                newLinkGenome.weight = 0f;  // Remove fully??? *****
            }

            float randMutationChance = UnityEngine.Random.Range(0f, 1f);
            if (randMutationChance < settings.mutationChance) {
                float randomWeight = Gaussian.GetRandomGaussian();
                newLinkGenome.weight = newLinkGenome.weight + Mathf.Lerp(0f, randomWeight, settings.mutationStepSize);
            }

            newLinkGenome.weight *= settings.weightDecayAmount;
            
            linkList.Add(newLinkGenome);
        }

        // Add Brand New Link:
        // 
        float randLink = UnityEngine.Random.Range(0f, 1f);
        if (randLink < settings.newLinkChance) {

            List<NeuronGenome> inputNeuronList = new List<NeuronGenome>(); // **** Make these Global ??? avoids traversing them multiple times....            
            List<NeuronGenome> outputNeuronList = new List<NeuronGenome>();
            for (int j = 0; j < bodyNeuronList.Count; j++) {
                if (bodyNeuronList[j].neuronType == NeuronGenome.NeuronType.In) {
                    inputNeuronList.Add(bodyNeuronList[j]);
                }
                if (bodyNeuronList[j].neuronType == NeuronGenome.NeuronType.Out) {
                    outputNeuronList.Add(bodyNeuronList[j]);
                }
            }
            for (int j = 0; j < hiddenNeuronList.Count; j++) {
                inputNeuronList.Add(hiddenNeuronList[j]);
                outputNeuronList.Add(hiddenNeuronList[j]);
            }

            // Try x times to find new connection -- random scattershot approach at first:
            // other methods:
            //      -- make sure all bodyNeurons are fully-connected when modifying body
            int maxChecks = 8;
            for (int k = 0; k < maxChecks; k++) {
                int randInputID = UnityEngine.Random.Range(0, inputNeuronList.Count);
                NID fromNID = inputNeuronList[randInputID].nid;

                int randOutputID = UnityEngine.Random.Range(0, outputNeuronList.Count);
                NID toNID = outputNeuronList[randOutputID].nid;

                // check if it exists:
                bool linkExists = false;    // Dictionary might be faster??? *****
                for (int l = 0; l < linkList.Count; l++) {
                    if (linkList[l].fromModuleID == fromNID.moduleID && linkList[l].fromNeuronID == fromNID.neuronID && linkList[l].toModuleID == toNID.moduleID && linkList[l].toNeuronID == toNID.neuronID) {
                        linkExists = true;
                        break;
                    }
                }

                if (linkExists) {

                }
                else {
                    float randomWeight = Gaussian.GetRandomGaussian() * settings.mutationStepSize;
                    LinkGenome linkGenome = new LinkGenome(fromNID.moduleID, fromNID.neuronID, toNID.moduleID, toNID.neuronID, randomWeight, true);                    
                    linkList.Add(linkGenome);

                    //Debug.Log("New Link! from: [" + fromNID.moduleID.ToString() + ", " + fromNID.neuronID.ToString() + "], to: [" + toNID.moduleID.ToString() + ", " + toNID.neuronID.ToString() + "]");
                    break;
                }
            }
        }

        // Add Brand New Hidden Neuron:
        float randNeuronChance = UnityEngine.Random.Range(0f, 1f);
        if (randNeuronChance < settings.newHiddenNodeChance) {
            // find a link and expand it:
            int randLinkID = UnityEngine.Random.Range(0, linkList.Count);
            // create new neuron
            NeuronGenome newNeuronGenome = new NeuronGenome(NeuronGenome.NeuronType.Hid, -1, hiddenNeuronList.Count);
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

    public void RemoveVestigialLinks() {

        //create a master list of all neurons:
        List<NeuronGenome> allNeuronsList = new List<NeuronGenome>();
        for (int j = 0; j < bodyNeuronList.Count; j++) {
            allNeuronsList.Add(bodyNeuronList[j]);
        }
        for (int j = 0; j < hiddenNeuronList.Count; j++) {
            allNeuronsList.Add(hiddenNeuronList[j]);
        }

        List<int> axonsToRemoveIndicesList = new List<int>();

        // keep table of linearIndex positions for all neuron 2-dimensional ID's
        Dictionary<NID, int> IDs = new Dictionary<NID, int>();
        for (int j = 0; j < allNeuronsList.Count; j++) {
            IDs.Add(allNeuronsList[j].nid, j);
        }
        for (int j = 0; j < linkList.Count; j++) {
            bool remove = false;
            // find out neuronIDs:
            int fromID = -1;
            if (IDs.TryGetValue(new NID(linkList[j].fromModuleID, linkList[j].fromNeuronID), out fromID)) {

            }
            else {
                remove = true;
                //Debug.LogError("fromNID NOT FOUND " + linkList[j].fromModuleID.ToString() + ", " + linkList[j].fromNeuronID.ToString());
            }
            int toID = -1;
            if (IDs.TryGetValue(new NID(linkList[j].toModuleID, linkList[j].toNeuronID), out toID)) {

            }
            else {
                remove = true;
                //Debug.LogError("toNID NOT FOUND " + linkList[j].fromModuleID.ToString() + ", " + linkList[j].fromNeuronID.ToString());
            }

            if(remove) {
                axonsToRemoveIndicesList.Add(j);
            }
        }

        if(axonsToRemoveIndicesList.Count > 0) {
            for(int k = (axonsToRemoveIndicesList.Count - 1); k >= 0; k--) {  // traverse backwards to preserve indices as they are removed
                linkList.RemoveAt(axonsToRemoveIndicesList[k]);
            }
        }
        
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
