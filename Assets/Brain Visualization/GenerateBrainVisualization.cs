using System.Collections.Generic;
using UnityEngine;

public struct SocketInitData 
{
    public Vector3 position;
}

public class GenerateBrainVisualization : MonoBehaviour 
{
    List<Neuron> neurons;
    List<Axon> axons;
    
    int numNeurons => neurons.Count;

    #region Internal data

    private ComputeBuffer quadVerticesCBuffer;  // holds information for a 2-triangle Quad mesh (6 vertices)
    public ComputeBuffer floatingGlowyBitsCBuffer;  // holds information for placement and attributes of each instance of quadVertices to draw
    public ComputeBuffer extraBallsCBuffer;
    private ComputeBuffer axonBallCBuffer;
    private ComputeBuffer neuronBallCBuffer;

    private ComputeBuffer neuronInitDataCBuffer;  // sets initial positions for each neuron
    private ComputeBuffer neuronFeedDataCBuffer;  // current value -- separate so CPU only has to push the bare-minimum data to GPU every n frames
    private ComputeBuffer neuronSimDataCBuffer;   // holds data that is updatable purely on GPU, like neuron Positions
    //private ComputeBuffer subNeuronSimDataCBuffer; 
    private ComputeBuffer axonInitDataCBuffer;    // holds axon weights ( as well as from/to neuron IDs ?)
    private ComputeBuffer axonSimDataCBuffer;     // axons can be updated entirely on GPU by referencing neuron positions.
    // .^.^.^. this includes all spline data --> positions, vertex colors, uv, radii, etc., pulse positions
    private ComputeBuffer cableInitDataCBuffer;   // initial data required to calculate positions of cables
    private ComputeBuffer cableSimDataCBuffer;    // holds the positions of the control points of the cable's underlying bezier curve
    private ComputeBuffer socketInitDataCBuffer;  // holds the positions of the sockets on wall that cables plug into
    public ComputeBuffer argsCoreCBuffer;        // Some other secondary buffers for decorations later
    private uint[] argsCore = new uint[5] { 0, 0, 0, 0, 0 };
    
    // will likely split these out into seperate ones later to support multiple materials/layers, but all-in-one for now...
    private ComputeBuffer appendTrianglesCablesCBuffer; 
    private ComputeBuffer appendTrianglesCoreCBuffer;
    public ComputeBuffer argsCablesCBuffer;
    
    private uint[] argsCables = new uint[5] { 0, 0, 0, 0, 0 };
    
    int numAxons = 270;
    public bool initialized;
    
    #endregion
    
    #region Structs for compute buffers
    
    public struct NeuronInitData 
    {
        public float radius;
        public float type;  // in/out/hidden
        public float age;
        
        public NeuronInitData(float type)
        {
            radius = 1.6f;
            this.type = type;
            age = Random.Range(0f, 1f); // refBrain.neuronList[x].age;
        }
    }
    
    public struct NeuronFeedData {
        public float curValue;  // [-1,1]  // set by CPU continually
    }
    
    // WPP: duplicate of NeuronInitData
    //public struct NeuronSimData {
    //    public Vector3 pos;
    //}
    
    // Set once at start
    public struct AxonInitData 
    {  
        public float weight;
        public int fromID;
        public int toID;

        public AxonInitData(Axon axon)
        {
            weight = axon.weight;
            fromID = axon.from.index;
            toID = axon.to.index;
        }
    }
    
    public struct AxonSimData {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;
        public float pulsePos;
    }
    
    public struct CableInitData {
        public int socketID;
        public int neuronID;
    }
    
    public struct CableSimData {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;
    }

    public struct ExtraBallsAxonsData {
        public int axonID;
        public float t;  // how far along the spline
        public float angle;  // angle of rotation around the axis
        public float baseScale;
    }
    
    public struct ExtraBallsNeuronsData {
        public int neuronID;
        public Vector3 direction;  // where on the neuron???
        public float baseScale;
    }
    
    public struct BallData {
        public Vector3 worldPos;        
        public float value;
        public float inOut;
        public float scale;
        // per-particle size / rotation etc.???
    }
    
    public struct GlowyBitData {
        public Vector3 worldPos;
        public Vector3 color;
    }
    
    public struct Triangle {
        public Vector3 vertA;
        public Vector3 normA;
        public Vector3 tanA;
        public Vector3 uvwA;
        public Vector3 colorA;

        public Vector3 vertB;
        public Vector3 normB;
        public Vector3 tanB;
        public Vector3 uvwB;
        public Vector3 colorB;

        public Vector3 vertC;
        public Vector3 normC;
        public Vector3 tanC;
        public Vector3 uvwC;
        public Vector3 colorC;
    }
    
    #endregion

    #region Settings
    
    [SerializeField] BrainSettings settings;
    
    ComputeShader shaderComputeBrain => settings.shaderComputeBrain;
    ComputeShader shaderComputeFloatingGlowyBits => settings.shaderComputeFloatingGlowyBits;
    ComputeShader shaderComputeExtraBalls => settings.shaderComputeExtraBalls;  // quads w/ nml maps to like like extra blobs attached to neurons & axons
    //public Shader shaderDisplayBrain;
    public Material displayMaterialCore => settings.displayMaterialCore;
    public Material displayMaterialCables => settings.displayMaterialCables;
    public Material floatingGlowyBitsMaterial => settings.floatingGlowyBitsMaterial;
    public Material extraBallsMaterial => settings.extraBallsMaterial;
    
    int maxTrisPerNeuron => settings.maxTrisPerNeuron;
    int maxTrisPerSubNeuron => settings.maxTrisPerSubNeuron;
    int maxTrisPerAxon => settings.maxTrisPerAxon;
    int maxTrisPerCable => settings.maxTrisPerCable;
    int numFloatingGlowyBits => settings.numFloatingGlowyBits;
    int numAxonBalls => settings.numAxonBalls;
    int numNeuronBalls => settings.numNeuronBalls;
    
    float minNeuronRadius => settings.minNeuronRadius;
    float maxNeuronRadius => settings.maxNeuronRadius;
    float minAxonRadius => settings.minAxonRadius;
    float maxAxonRadius => settings.maxAxonRadius;
    float minSubNeuronScale => settings.minSubNeuronScale;
    float maxSubNeuronScale => settings.maxSubNeuronScale;  // max size relative to parent Neuron
    float maxAxonFlareScale => settings.maxAxonFlareScale;  // max axon flare size relative to SubNeuron
    float minAxonFlareScale => settings.minAxonFlareScale;
    float axonFlarePos => settings.axonFlarePos;
    float axonFlareWidth => settings.axonFlareWidth;
    float axonMaxPulseMultiplier => settings.axonMaxPulseMultiplier;
    float cableRadius => settings.cableRadius;
    float neuronBallMaxScale => settings.neuronBallMaxScale;

    float neuronExtrudeNoiseFreq => settings.neuronExtrudeNoiseFreq;
    float neuronExtrudeNoiseAmp => settings.neuronExtrudeNoiseAmp;
    float neuronExtrudeNoiseScrollSpeed => settings.neuronExtrudeNoiseScrollSpeed;
    float axonExtrudeNoiseFreq => settings.axonExtrudeNoiseFreq;
    float axonExtrudeNoiseAmp => settings.axonExtrudeNoiseAmp;
    float axonExtrudeNoiseScrollSpeed => settings.axonExtrudeNoiseScrollSpeed;
    float axonPosNoiseFreq => settings.axonPosNoiseFreq;
    float axonPosNoiseAmp => settings.axonPosNoiseAmp;
    float axonPosNoiseScrollSpeed => settings.axonPosNoiseScrollSpeed;
    float axonPosSpiralFreq => settings.axonPosSpiralFreq;
    float axonPosSpiralAmp => settings.axonPosSpiralAmp;
    
    float neuronAttractForce => settings.neuronAttractForce;
    float neuronRepelForce => settings.neuronRepelForce;
    float axonPerpendicularityForce => settings.axonPerpendicularityForce;
    float axonAttachStraightenForce => settings.axonAttachStraightenForce;
    float axonAttachSpreadForce => settings.axonAttachSpreadForce;
    float axonRepelForce => settings.axonRepelForce;
    float cableAttractForce => settings.cableAttractForce;
    
    bool enablePhysics => !settings.disablePhysics;
    bool enableCables => !settings.disableCables;

    #endregion
    
    void SetCoreBrainDataSharedParameters(ComputeShader computeShader) 
    {
        // Core Sizes
        computeShader.SetFloat("minNeuronRadius", minNeuronRadius);
        computeShader.SetFloat("maxNeuronRadius", maxNeuronRadius);
        computeShader.SetFloat("minAxonRadius", minAxonRadius);
        computeShader.SetFloat("maxAxonRadius", maxAxonRadius);
        computeShader.SetFloat("minSubNeuronScale", minSubNeuronScale);
        computeShader.SetFloat("maxSubNeuronScale", maxSubNeuronScale);
        computeShader.SetFloat("minAxonFlareScale", minAxonFlareScale);
        computeShader.SetFloat("maxAxonFlareScale", maxAxonFlareScale);        
        computeShader.SetFloat("axonFlarePos", axonFlarePos);
        computeShader.SetFloat("axonFlareWidth", axonFlareWidth);
        computeShader.SetFloat("axonMaxPulseMultiplier", axonMaxPulseMultiplier);
        computeShader.SetFloat("cableRadius", cableRadius);

        // Noise Parameters
        computeShader.SetFloat("neuronExtrudeNoiseFreq", neuronExtrudeNoiseFreq);
        computeShader.SetFloat("neuronExtrudeNoiseAmp", neuronExtrudeNoiseAmp);
        computeShader.SetFloat("neuronExtrudeNoiseScrollSpeed", neuronExtrudeNoiseScrollSpeed);
        computeShader.SetFloat("axonExtrudeNoiseFreq", axonExtrudeNoiseFreq);
        computeShader.SetFloat("axonExtrudeNoiseAmp", axonExtrudeNoiseAmp);
        computeShader.SetFloat("axonExtrudeNoiseScrollSpeed", axonExtrudeNoiseScrollSpeed);
        computeShader.SetFloat("axonPosNoiseFreq", axonPosNoiseFreq);
        computeShader.SetFloat("axonPosNoiseAmp", axonPosNoiseAmp);
        computeShader.SetFloat("axonPosNoiseScrollSpeed", axonPosNoiseScrollSpeed);
        computeShader.SetFloat("axonPosSpiralFreq", axonPosSpiralFreq);
        computeShader.SetFloat("axonPosSpiralAmp", axonPosSpiralAmp);
            
        if (enablePhysics)
        {
            // Forces
            computeShader.SetFloat("neuronAttractForce", neuronAttractForce);
            computeShader.SetFloat("neuronRepelForce", neuronRepelForce);
            computeShader.SetFloat("axonPerpendicularityForce", axonPerpendicularityForce);
            computeShader.SetFloat("axonAttachStraightenForce", axonAttachStraightenForce);
            computeShader.SetFloat("axonAttachSpreadForce", axonAttachSpreadForce);
            computeShader.SetFloat("axonRepelForce", axonRepelForce);
            computeShader.SetFloat("cableAttractForce", cableAttractForce);
        }

        computeShader.SetFloat("time", Time.fixedTime);
    }
    
    public void Initialize(List<Neuron> neurons, List<Axon> axons, ref SocketInitData[] sockets, int inputCount, int outputCount)
    {
        this.neurons = neurons;
        this.axons = axons;
        //Debug.Log($"Initializing brain visualization with {neurons.Count} neurons and {axons.Count} axons");

        InitializeComputeBuffers(ref sockets, inputCount, outputCount);
        initialized = true;
    }

    void InitializeComputeBuffers(ref SocketInitData[] sockets, int inputNeuronCount, int outputNeuronCount) 
    {
        argsCoreCBuffer = new ComputeBuffer(1, argsCore.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsCablesCBuffer = new ComputeBuffer(1, argsCables.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        InitializeNeuronCBufferData();
        InitializeNeuronFeedDataCBuffer();

        neuronSimDataCBuffer?.Release();
        neuronSimDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 3); //***EAC only vec3 position so far
        
        //for (int i = 0; i < sockets.Length; i++)
        //    Debug.Log($"socket {i} placed at {sockets[i].position}");

        neuronSimDataCBuffer.SetData(sockets);
        //InitializeNeuralPositions();  // WPP: removed, was overriding placement        
        
        InitializeAxons();
        
        socketInitDataCBuffer?.Release();
        socketInitDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 3);
        
        socketInitDataCBuffer.SetData(sockets);
        socketInitDataCBuffer.GetData(sockets);

        int maxTriangles = numNeurons * maxTrisPerNeuron + axons.Count * maxTrisPerAxon + maxTrisPerSubNeuron * axons.Count * 2;
        AppendTriangles(ref appendTrianglesCoreCBuffer, maxTriangles);
        
        maxTriangles = numNeurons * maxTrisPerCable;
        AppendTriangles(ref appendTrianglesCablesCBuffer, maxTriangles);

        // Create buffer of free-floating, camera-facing quads
        quadVerticesCBuffer?.Release();
        quadVerticesCBuffer = new ComputeBuffer(6, sizeof(float) * 3);
        quadVerticesCBuffer.SetData(new[] {
            new Vector3(-0.5f, 0.5f),
            new Vector3(0.5f, 0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f)
        });
        
        floatingGlowyBitsCBuffer?.Release();
        floatingGlowyBitsCBuffer = new ComputeBuffer(numFloatingGlowyBits, sizeof(float) * 6);
        floatingGlowyBitsMaterial.SetPass(0);
        floatingGlowyBitsMaterial.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        floatingGlowyBitsMaterial.SetBuffer("floatingGlowyBitsCBuffer", floatingGlowyBitsCBuffer);
        
        UpdateGlowyBits("CSInitializePositions", 0f, 0.8f, 1, false);

        // EXTRA BALLS NORMAL-MAPPED CAMERA-FACING QUADS
        extraBallsCBuffer?.Release();
        extraBallsCBuffer = new ComputeBuffer(numAxonBalls + numNeuronBalls, sizeof(float) * 6);
        extraBallsMaterial.SetPass(0);
        extraBallsMaterial.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        extraBallsMaterial.SetBuffer("extraBallsCBuffer", extraBallsCBuffer);

        axonBallCBuffer?.Release();
        axonBallCBuffer = new ComputeBuffer(numAxonBalls, sizeof(float) * 3 + sizeof(int) * 1);
        neuronBallCBuffer?.Release();
        neuronBallCBuffer = new ComputeBuffer(numNeuronBalls, sizeof(float) * 4 + sizeof(int) * 1);
        
        SetExtraBallsBuffer("CSInitializeAxonBallData", numAxonBalls);
        SetExtraBallsBuffer("CSInitializeNeuronBallData", numNeuronBalls);

        // Hook Buffers Up to Shaders!!!
        // populate initial data for neurons
        // populate initial data for axons
        // feed neuronValues data to shader (encapsulate in function since this is ongoing)
        // simulate movements / animation parameters
        // generate neuron triangles
        // generate axon triangles
        SetCoreBrainDataSharedParameters(shaderComputeBrain);

        int initKernelID = SetComputeBrainBuffer("CSInitializeAxonSimData");
        int simNeuronAttractKernelID = SetComputeBrainBuffer("CSSimNeuronAttract");

        if (enableCables)
            InitializeCables(inputNeuronCount, outputNeuronCount);
        
        int neuronTrianglesKernelID = SetShaderBuffer(shaderComputeBrain, "CSGenerateNeuronTriangles");
        shaderComputeBrain.SetBuffer(neuronTrianglesKernelID, "appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);
        shaderComputeBrain.Dispatch(neuronTrianglesKernelID, numNeurons, 1, 1); // create all triangles from Neurons

        SetTrianglesBuffer("CSGenerateSubNeuronTriangles", axons.Count * 2);
        SetTrianglesBuffer("CSGenerateAxonTriangles", numAxons);

        displayMaterialCore.SetPass(0);
        displayMaterialCore.SetBuffer("appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);   // link computeBuffer to both computeShader and displayShader so they share the same one!!
        
        int simNeuronRepelKernelID = shaderComputeBrain.FindKernel("CSSimNeuronRepel");
        int simAxonRepelKernelID = shaderComputeBrain.FindKernel("CSSimAxonRepel");
        shaderComputeBrain.Dispatch(initKernelID, numAxons, 1, 1); // initialize axon positions and attributes
        shaderComputeBrain.Dispatch(simNeuronAttractKernelID, numAxons, 1, 1); // Simulate!! move neuron and axons around
        shaderComputeBrain.Dispatch(simNeuronRepelKernelID, numNeurons, numNeurons, 1); // Simulate!! move neuron and axons around
        shaderComputeBrain.Dispatch(simAxonRepelKernelID, numAxons, numAxons, 1); // Simulate!! move neuron and axons around

        SetCoreBrainDataSharedParameters(shaderComputeExtraBalls);

        SetExtraBallsBuffer("CSUpdateAxonBallPositions", numAxonBalls, true, 2f, 4f);
        SetExtraBallsBuffer("CSUpdateNeuronBallPositions", numNeuronBalls, true, 2f, 4f);

        SetArgsBuffer(argsCore, argsCoreCBuffer, appendTrianglesCoreCBuffer);
        SetArgsBuffer(argsCables, argsCablesCBuffer, appendTrianglesCablesCBuffer);
    }
    
    void InitializeCables(int inputCount, int outputCount)
    {
        cableInitDataCBuffer?.Release();
        cableInitDataCBuffer = new ComputeBuffer(numNeurons, sizeof(int) * 2);
        
        CableInitData[] cableInitDataArray = InitializeCableArray(numNeurons, inputCount, outputCount);

        cableInitDataCBuffer.SetData(cableInitDataArray);
        
        cableSimDataCBuffer?.Release();
        cableSimDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 12);
        
        int initCablesKernelID = SetShaderBuffer(shaderComputeBrain, "CSInitializeCableSimData");
        shaderComputeBrain.SetBuffer(initCablesKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
        shaderComputeBrain.SetBuffer(initCablesKernelID, "socketInitDataCBuffer", socketInitDataCBuffer);
        
        // Create spline geometry for cables:
        int generateCablesTrianglesKernelID = SetShaderBuffer(shaderComputeBrain, "CSGenerateCablesTriangles");
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "socketInitDataCBuffer", socketInitDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        
        displayMaterialCables.SetPass(0);
        displayMaterialCables.SetBuffer("appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        shaderComputeBrain.Dispatch(initCablesKernelID, numNeurons, 1, 1); // initialize axon positions and attributes
        shaderComputeBrain.Dispatch(generateCablesTrianglesKernelID, numNeurons, 1, 1); // create all geometry for Axons
    }

    CableInitData[] InitializeCableArray(int full, int inputs, int outputs)
    {
        var data = new CableInitData[full];
    
        for (int i = 0; i < inputs; i++) {
            data[i].neuronID = i;
            data[i].socketID = i;
        } 
        
        for (int i = 0; i < outputs; i++) {
            data[i].neuronID = inputs + i;
            data[i].socketID = inputs + i;
        }
        
        return data; 
    }
    
    void AppendTriangles(ref ComputeBuffer computeBuffer, int maxTriangles)
    {
        computeBuffer?.Release();
        computeBuffer = new ComputeBuffer(maxTriangles, sizeof(float) * 45, ComputeBufferType.Append); // vector3 position * 3 verts
        computeBuffer.SetCounterValue(0);
    }
    
    int SetShaderBuffer(ComputeShader computeShader, string kernelName)
    {
        int kernelID = computeShader.FindKernel(kernelName);
        computeShader.SetBuffer(kernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        computeShader.SetBuffer(kernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        computeShader.SetBuffer(kernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        return kernelID; 
    }
    
    void SetExtraBallsBuffer(string kernelName, int ballCount, bool setRadius = false, float minRadius = 0f, float maxRadius = 0f)
    {
        int kernelID = SetShaderBuffer(shaderComputeExtraBalls, kernelName);
        
        if (setRadius)
        {
            shaderComputeExtraBalls.SetFloat("minRadius", minRadius);
            shaderComputeExtraBalls.SetFloat("maxRadius", maxRadius);
        }
        
        shaderComputeExtraBalls.SetBuffer(kernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(kernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(kernelID, "axonBallCBuffer", axonBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(kernelID, "neuronBallCBuffer", neuronBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(kernelID, "extraBallsCBuffer", extraBallsCBuffer);
        shaderComputeExtraBalls.Dispatch(kernelID, ballCount, 1, 1); // initialize axon positions and attributes
    }

    int SetComputeBrainBuffer(string kernelName)
    {
        int kernelID = SetShaderBuffer(shaderComputeBrain, kernelName);
        shaderComputeBrain.SetBuffer(kernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(kernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        return kernelID;
    }
    
    void SetTrianglesBuffer(string kernelName, int x, int y = 1, int z = 1, bool refreshAxonBuffer = true)
    {
        int kernelID = SetShaderBuffer(shaderComputeBrain, kernelName);
        
        if (refreshAxonBuffer)
        {
            shaderComputeBrain.SetBuffer(kernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
            shaderComputeBrain.SetBuffer(kernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        }
        
        shaderComputeBrain.SetBuffer(kernelID, "appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);
        shaderComputeBrain.Dispatch(kernelID, x, y, z); // Create all triangles for SubNeurons
    }

    // For now only one seed data
    void InitializeNeuronCBufferData()
    {
        neuronInitDataCBuffer?.Release();
        neuronInitDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 3);
        
        NeuronInitData[] neuronInitDataArray = new NeuronInitData[neurons.Count]; //[numNeurons]; 
        
        for (int x = 0; x < neuronInitDataArray.Length; x++) {
            NeuronInitData neuronData = new NeuronInitData((float)neurons[x].neuronType / 2.0f);
            neuronInitDataArray[x] = neuronData;
        }
        
        neuronInitDataCBuffer.SetData(neuronInitDataArray);
    }
    
    void InitializeNeuronFeedDataCBuffer()
    {
        neuronFeedDataCBuffer?.Release();
        neuronFeedDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 1);
        NeuronFeedData[] neuronValuesArray = new NeuronFeedData[numNeurons];
        
        for(int i = 0; i < neuronValuesArray.Length; i++) {
            neuronValuesArray[i].curValue = neurons[i].currentValue[0];
        }
        
        neuronFeedDataCBuffer.SetData(neuronValuesArray);
    }
    
    // WPP: ERROR: overwrites neuron placement with random values
    /*void InitializeNeuralPositions()
    {
        NeuronSimData[] neuronSimDataArray = new NeuronSimData[numNeurons];
        
        for (int i = 0; i < neuronSimDataArray.Length; i++) 
        {
            neuronSimDataArray[i].pos = Random.insideUnitSphere * 1f;   // ERROR: overwrites placement
            var polarity = neurons[i].neuronType == NeuronType.In ? -1f : 1f;
            neuronSimDataArray[i].pos.z = polarity * Mathf.Abs(neuronSimDataArray[i].pos.z);
        }
        
        neuronSimDataCBuffer.SetData(neuronSimDataArray);
    }*/
    
    // For now only one seed data
    void InitializeAxons()
    {
        //Debug.Log($"Initializing {axons.Count} axons");
        axonInitDataCBuffer?.Release();
        axonInitDataCBuffer = new ComputeBuffer(axons.Count, sizeof(float) * 1 + sizeof(int) * 2);
        
        AxonInitData[] axonInitDataArray = new AxonInitData[axons.Count]; 
        
        for (int x = 0; x < axonInitDataArray.Length; x++) {
            AxonInitData axonData = new AxonInitData(axons[x]);
            axonInitDataArray[x] = axonData;
        }
        
        axonInitDataCBuffer.SetData(axonInitDataArray);
        axonSimDataCBuffer?.Release();
        axonSimDataCBuffer = new ComputeBuffer(axons.Count, sizeof(float) * 13);
    }
    
    void UpdateNeuronBuffer(string kernelName, int x, int y)
    {
        int kernelID = SetShaderBuffer(shaderComputeBrain, kernelName);
        shaderComputeBrain.SetBuffer(kernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(kernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeBrain.Dispatch(kernelID, x, y, 1);
    }

    void UpdateBrainDataAndBuffers() 
    {
        if (neurons == null || axons == null)
            return;

        NeuronFeedData[] neuronValuesArray = new NeuronFeedData[neurons.Count];
        for (int i = 0; i < neuronValuesArray.Length; i++) {
            neuronValuesArray[i].curValue = neurons[i].currentValue[0]; // Mathf.Sin(Time.fixedTime * 1.25f + neurons[i].currentValue[0]);
        }
        neuronFeedDataCBuffer.SetData(neuronValuesArray);

        // For some reason I have to setBuffer on all of these for it to WORK!!!!!!!! (even though they are all the same in the shader...)
        SetCoreBrainDataSharedParameters(shaderComputeBrain);
        SetCoreBrainDataSharedParameters(shaderComputeExtraBalls);
        
        UpdateNeuronBuffer("CSSimNeuronAttract", numAxons, 1);
        UpdateNeuronBuffer("CSSimNeuronRepel", numNeurons, numNeurons);
        UpdateNeuronBuffer("CSSimAxonRepel", numAxons, numAxons);

        if (enableCables)
            MoveCables();

        SetExtraBallsBuffer("CSInitializeAxonBallData", numAxonBalls);
        SetExtraBallsBuffer("CSInitializeNeuronBallData", numNeuronBalls);
        SetExtraBallsBuffer("CSUpdateAxonBallPositions", axonBallCBuffer.count);
        SetExtraBallsBuffer("CSUpdateNeuronBallPositions", neuronBallCBuffer.count);

        extraBallsMaterial.SetPass(0);
        //extraBallsMaterial.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        extraBallsMaterial.SetBuffer("extraBallsCBuffer", extraBallsCBuffer);

        // Regenerate triangles
        appendTrianglesCoreCBuffer?.Release();
        //appendTrianglesCBuffer = new ComputeBuffer(numNeurons * maxTrisPerNeuron + numAxons * maxTrisPerAxon, sizeof(float) * 45, ComputeBufferType.Append); // vector3 position * 3 verts
        int maxTris = numNeurons * maxTrisPerNeuron + axons.Count * maxTrisPerAxon + maxTrisPerSubNeuron * axons.Count * 2;
        appendTrianglesCoreCBuffer = new ComputeBuffer(maxTris, sizeof(float) * 45, ComputeBufferType.Append); 
        appendTrianglesCoreCBuffer.SetCounterValue(0);
        appendTrianglesCablesCBuffer?.Release();
        
        int maxTrisCable = numNeurons * maxTrisPerNeuron + axons.Count * maxTrisPerAxon + maxTrisPerSubNeuron * axons.Count * 2;
        appendTrianglesCablesCBuffer = new ComputeBuffer(maxTrisCable, sizeof(float) * 45, ComputeBufferType.Append); 
        appendTrianglesCablesCBuffer.SetCounterValue(0);
        
        SetTrianglesBuffer("CSGenerateNeuronTriangles", neurons.Count, 1, 1, false);
        SetTrianglesBuffer("CSGenerateSubNeuronTriangles", axons.Count * 2);
        SetTrianglesBuffer("CSGenerateAxonTriangles", axons.Count);

        displayMaterialCore.SetPass(0);
        displayMaterialCore.SetBuffer("appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);
        
        if (enableCables)
            CreateSplineGeometryForCables();
        
        // FLOATING BITS:
        SetCoreBrainDataSharedParameters(shaderComputeFloatingGlowyBits);

        floatingGlowyBitsCBuffer?.Release();
        floatingGlowyBitsCBuffer = new ComputeBuffer(numFloatingGlowyBits, sizeof(float) * 6);
        floatingGlowyBitsMaterial.SetPass(0);
        floatingGlowyBitsMaterial.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        floatingGlowyBitsMaterial.SetBuffer("floatingGlowyBitsCBuffer", floatingGlowyBitsCBuffer);
        
        UpdateGlowyBits("CSInitializePositions", 0f, 0.8f);
        UpdateGlowyBits("CSUpdateColor", 0f, 0.8f, numNeurons);

        SetArgsBuffer(argsCore, argsCoreCBuffer, appendTrianglesCoreCBuffer);
        SetArgsBuffer(argsCables, argsCablesCBuffer, appendTrianglesCablesCBuffer);
    }
    
    void MoveCables()
    {
        int updateCablePositionsKernelID = SetShaderBuffer(shaderComputeBrain, "CSUpdateCablePositions");
        shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
        shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "socketInitDataCBuffer", socketInitDataCBuffer);
        //shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        shaderComputeBrain.Dispatch(updateCablePositionsKernelID, numNeurons, 1, 1); // Simulate!! move neuron and axons around
    }
    
    void CreateSplineGeometryForCables()
    {
        int generateCablesTrianglesKernelID = shaderComputeBrain.FindKernel("CSGenerateCablesTriangles");
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        displayMaterialCables.SetPass(0);
        displayMaterialCables.SetBuffer("appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        shaderComputeBrain.Dispatch(generateCablesTrianglesKernelID, numNeurons, 1, 1); // create all geometry for Axons
    }
    
    /// Sets metadata for append buffer
    void SetArgsBuffer(uint[] args, ComputeBuffer argsBuffer, ComputeBuffer trianglesBuffer)
    {
        args[0] = 0; // set later by counter;
        args[1] = 1;  // 1 instance/copy
        argsBuffer.SetData(args);
        ComputeBuffer.CopyCount(trianglesBuffer, argsBuffer, 0);
        argsBuffer.GetData(args);
    }
    
    /// Initialize axon positions and attributes
    void UpdateGlowyBits(string kernelName, float minRadius, float maxRadius, int yCount = 1, bool refreshTime = true)
    {
        int kernelID = refreshTime ?
            SetShaderBuffer(shaderComputeFloatingGlowyBits, kernelName):
            shaderComputeFloatingGlowyBits.FindKernel(kernelName);
        if (refreshTime)
            shaderComputeFloatingGlowyBits.SetFloat("time", Time.fixedTime);
        
        shaderComputeFloatingGlowyBits.SetFloat("minRadius", minRadius);
        shaderComputeFloatingGlowyBits.SetFloat("maxRadius", maxRadius);
        shaderComputeFloatingGlowyBits.SetBuffer(kernelID, "floatingGlowyBitsCBuffer", floatingGlowyBitsCBuffer);
        shaderComputeFloatingGlowyBits.Dispatch(kernelID, numFloatingGlowyBits / 64, yCount, 1);
    }

    void OnRenderObject() 
    {
        /*if (!initialized) return;
    
        displayMaterialCables.SetPass(0);
        // not sure why at this used to work with Triangles but now requires Points....
        Graphics.DrawProceduralIndirectNow(MeshTopology.Points, argsCablesCBuffer, 0);  

        displayMaterialCore.SetPass(0);
        Graphics.DrawProceduralIndirectNow(MeshTopology.Points, argsCoreCBuffer, 0);  
        //Graphics.DrawProceduralIndirect(MeshTopology.Triangles, argsCBuffer, 0);

        floatingGlowyBitsMaterial.SetPass(0);
        //floatingGlowyBitsMaterial.SetBuffer("floatingGlowyBitsCBuffer", floatingGlowyBitsCBuffer);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, floatingGlowyBitsCBuffer.count);

        extraBallsMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, extraBallsCBuffer.count);*/
    }

    void Update () 
    {
        if (!initialized) return;
        UpdateBrainDataAndBuffers();
    }

    void OnDestroy() 
    {
        neuronInitDataCBuffer?.Release();
        neuronFeedDataCBuffer?.Release();
        neuronSimDataCBuffer?.Release();
        axonInitDataCBuffer?.Release();
        axonSimDataCBuffer?.Release();
        argsCoreCBuffer?.Release();
        appendTrianglesCoreCBuffer?.Release();
        floatingGlowyBitsCBuffer?.Release();
        quadVerticesCBuffer?.Release();
        extraBallsCBuffer?.Release();
        axonBallCBuffer?.Release();
        neuronBallCBuffer?.Release();
        cableInitDataCBuffer?.Release();
        cableSimDataCBuffer?.Release();
        socketInitDataCBuffer?.Release();
        appendTrianglesCablesCBuffer?.Release();
        argsCablesCBuffer?.Release();
    }
}