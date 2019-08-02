using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerPlantGenome {

    public string name;
    public string textDescriptionMutation;
    public float plantUpkeep;
    public float plantIntakeRate;
    public float plantGrowthEfficiency;
    public Color displayColor;
    public VegetationManager.PlantParticleData plantRepData;
    /*
    representativeAlgaeLayerGenome = algaeParticlesArray[0];
    algaeParticlesRepresentativeGenomeCBuffer = new ComputeBuffer(1, GetAlgaeParticleDataSize());
    AlgaeParticleData[] algaeParticlesRepresentativeGenomeArray = new AlgaeParticleData[1];
    algaeParticlesRepresentativeGenomeArray[0] = representativeAlgaeLayerGenome;
    algaeParticlesRepresentativeGenomeCBuffer.SetData(algaeParticlesRepresentativeGenomeArray);
	*/
    public WorldLayerPlantGenome() {   // construction
        displayColor = Color.green;
    }
}
