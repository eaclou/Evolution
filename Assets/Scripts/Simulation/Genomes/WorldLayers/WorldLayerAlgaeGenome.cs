using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerAlgaeGenome {

    public string name;
    public string textDescriptionMutation;
    public float metabolicRate;
    //public float decomposerIntakeRate;
    public float growthEfficiency;
    public Color displayColor;
    //public VegetationManager.AlgaeParticleData algaeRepData;
    /*representativeAlgaeLayerGenome = algaeParticlesArray[0];
        algaeParticlesRepresentativeGenomeCBuffer = new ComputeBuffer(1, GetAlgaeParticleDataSize());
        AlgaeParticleData[] algaeParticlesRepresentativeGenomeArray = new AlgaeParticleData[1];
        algaeParticlesRepresentativeGenomeArray[0] = representativeAlgaeLayerGenome;
        algaeParticlesRepresentativeGenomeCBuffer.SetData(algaeParticlesRepresentativeGenomeArray);
	*/
    public WorldLayerAlgaeGenome() {   // construction
        displayColor = Color.white;
    }
}
