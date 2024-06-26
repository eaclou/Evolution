﻿using System.Collections.Generic;
using UnityEngine;

// WPP: trivial functionality, data stored in NeuronData.iconPosition -> consider removal
public struct SocketInitData 
{
    public Vector3 position;
}

public class GenerateBrainVisualization : MonoBehaviour 
{
    [ReadOnly] public Brain brain;
    [ReadOnly] public Brain brainFromGenome;
    List<Neuron> neurons => brain.allNeurons;
    List<Axon> axons => brain.allAxons;
    int numNeurons => neurons.Count;
    int inputCount => brain.inputNeurons.Count;
    int outputCount => brain.outputNeurons.Count;

    #region Internal data
    
    /// 2-triangle Quad mesh (6 vertices)
    private ComputeBuffer quadVerticesCBuffer;

    /// Initial positions for each neuron
    private ComputeBuffer neuronInitDataCBuffer;  
    
    /// Current value -- separate so CPU only has to push the bare-minimum data to GPU every n frames
    private ComputeBuffer neuronFeedDataCBuffer;  
    
    /// Updatable purely on GPU, like neuron Positions
    private ComputeBuffer neuronSimDataCBuffer;   
    
    /// Axon weights (and from/to neuron IDs?)
    private ComputeBuffer axonInitDataCBuffer;   
    
    /// Axons can be updated entirely on GPU by referencing neuron positions.
    /// Includes all spline data --> positions, vertex colors, uv, radii, pulse positions, etc.
    private ComputeBuffer axonSimDataCBuffer;

    /// Some other secondary buffers for decorations later
    public ComputeBuffer argsCoreCBuffer; 
    
    private ComputeBuffer appendTrianglesCoreCBuffer;           
    
    private uint[] argsCore = new uint[5] { 0, 0, 0, 0, 0 };

    public bool initialized;
    
    //public ComputeBuffer floatingGlowyBitsCBuffer;  // holds information for placement and attributes of each instance of quadVertices to draw
    //public ComputeBuffer extraBallsCBuffer;
    //private ComputeBuffer axonBallCBuffer;
    //private ComputeBuffer neuronBallCBuffer;
    //private ComputeBuffer subNeuronSimDataCBuffer;
    //private ComputeBuffer cableInitDataCBuffer;   // initial data required to calculate positions of cables
    //private ComputeBuffer cableSimDataCBuffer;    // holds the positions of the control points of the cable's underlying bezier curve
    //private ComputeBuffer socketInitDataCBuffer;  // holds the positions of the sockets on wall that cables plug into
    // will likely split these out into seperate ones later to support multiple materials/layers, but all-in-one for now...
    //private ComputeBuffer appendTrianglesCablesCBuffer;
    //public ComputeBuffer argsCablesCBuffer;
    //private uint[] argsCables = new uint[5] { 0, 0, 0, 0, 0 };
    //int numAxons = 270;    
    #endregion
    
    #region Structs for compute buffers
    
    public struct NeuronInitData 
    {
        public float radius;
        /// in/out/hidden
        public float type;  
        public float age;
        
        public NeuronInitData(float type)
        {
            radius = 1.6f;
            this.type = type;
            age = Random.Range(0f, 1f); // refBrain.neuronList[x].age;
        }
    }
    
    public struct NeuronFeedData {
        /// Continually set by CPU
        public float curValue;  // [-1,1]   
    }

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
            //Debug.Log($"Creating axon from {axon.from.neuronType} {axon.from.data.name} to " +
            //          $"{axon.to.neuronType} {axon.to.data.name}");   // OK
        }
    }
    
    public struct AxonSimData {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;
        public float pulsePos;
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
    //public Material floatingGlowyBitsMaterial => settings.floatingGlowyBitsMaterial;
    //public Material extraBallsMaterial => settings.extraBallsMaterial;
    
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
        
        // Forces
        computeShader.SetFloat("neuronAttractForce", neuronAttractForce);
        computeShader.SetFloat("neuronRepelForce", neuronRepelForce);
        computeShader.SetFloat("axonPerpendicularityForce", axonPerpendicularityForce);
        computeShader.SetFloat("axonAttachStraightenForce", axonAttachStraightenForce);
        computeShader.SetFloat("axonAttachSpreadForce", axonAttachSpreadForce);
        computeShader.SetFloat("axonRepelForce", axonRepelForce);
        computeShader.SetFloat("cableAttractForce", cableAttractForce);

        computeShader.SetFloat("time", Time.fixedTime);
    }
    
    
    public void Initialize(Brain brain, ref SocketInitData[] sockets)
    {
        this.brain = brain;
        //Debug.Log($"Initializing brain visualization with {neurons.Count} neurons and {axons.Count} axons");

        InitializeComputeBuffers(ref sockets);
        initialized = true;

        //PrintNeuronPositions(neurons, ref sockets);
        //PrintAxonPositions(axons);
    }

    #region Debug - print to console

    public void PrintNeuronPositions(List<Neuron> neurons, ref SocketInitData[] sockets) {
        Vector3[] nPos = new Vector3[sockets.Length];
        neuronSimDataCBuffer.GetData(nPos);
        string neuronText = neurons.Count + " Neurons\n";
        for (int i = 0; i < sockets.Length; i++) {
            neuronText += i + "[" + neurons[i].io + "] " + sockets[i].position + ", " + nPos[i] + ", " + neurons[i].name + "\n";
        }        
        Debug.Log(neuronText);
    }

    public void PrintAxonPositions(List<Axon> axons) {
        string axonText = axons.Count + " Axons\n";
        for (int i = 0; i < axons.Count; i++) {
            axonText += i + "[" + axons[i].from.name + axons[i].from.index + "->" + axons[i].to.name + axons[i].to.index + ", " + axons[i].weight + "\n";
        }        
        Debug.Log(axonText);
    }
    
    void PrintConnection(int fromID, int toID, int axonID)
    {
        var fromNeuron = neurons[fromID];
        var toNeuron = neurons[toID];
        Debug.Log($"axon [{axons[axonID].index}] connecting " + 
            $"from [{fromNeuron.index}] {fromNeuron.io} '{fromNeuron.name}' " + 
            $"to [{toNeuron.index}] {toNeuron.io} '{toNeuron.name}'");
        
        if (neurons[fromID].index != fromID)
            Debug.LogError("Index mismatch on source neuron");
        if (neurons[toID].index != toID)
            Debug.LogError("Index mismatch on destination neuron");
        if (axons[axonID].index != axonID)
            Debug.LogError("Index mismatch on axon");
        if (fromNeuron.io == NeuronType.Out)
            Debug.LogError($"Axon starting from an output neuron");
        if (toNeuron.io == NeuronType.In)
            Debug.LogError($"Axon going to an input neuron");
    }
    
    #endregion

    void InitializeComputeBuffers(ref SocketInitData[] sockets) 
    {
        argsCoreCBuffer?.Dispose();
        argsCoreCBuffer = new ComputeBuffer(1, argsCore.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        //if (argsCablesCBuffer != null) argsCablesCBuffer.Dispose();
        //argsCablesCBuffer = new ComputeBuffer(1, argsCables.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        InitializeNeuronCBufferData();
        InitializeNeuronFeedDataCBuffer(); // RENAME -- Set every frame, don't really need an Initialization function

        neuronSimDataCBuffer?.Release();
        neuronSimDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 3); //***EAC only vec3 position so far        
        neuronSimDataCBuffer.SetData(sockets);
        
        InitializeAxons();
        
        int maxTriangles = numNeurons * maxTrisPerNeuron + axons.Count * maxTrisPerAxon; // Both Neurons and Axons combine their trianlges into this buffer
        AppendTriangles(ref appendTrianglesCoreCBuffer, maxTriangles);        
        //maxTriangles = axons.Count * maxTrisPerAxon;
        //AppendTriangles(ref appendTrianglesCablesCBuffer, maxTriangles);

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
        
        // Hook Buffers Up to Shaders!!!
        // populate initial data for neurons
        // populate initial data for axons
        // feed neuronValues data to shader (encapsulate in function since this is ongoing)
        // simulate movements / animation parameters
        // generate neuron triangles
        // generate axon triangles
        SetCoreBrainDataSharedParameters(shaderComputeBrain);
        
        int neuronTrianglesKernelID = SetShaderBuffer(shaderComputeBrain, "CSGenerateNeuronTriangles");
        shaderComputeBrain.SetBuffer(neuronTrianglesKernelID, "appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);
        shaderComputeBrain.Dispatch(neuronTrianglesKernelID, numNeurons, 1, 1); // create all triangles from Neurons

        displayMaterialCore.SetPass(0);
        displayMaterialCore.SetBuffer("appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);   // link computeBuffer to both computeShader and displayShader so they share the same one!!
        
        int initKernelID = SetComputeBrainBuffer("CSInitializeAxonSimData");      
        shaderComputeBrain.Dispatch(initKernelID, axons.Count, 1, 1); // initialize axon positions and attributes
        //int simNeuronAttractKernelID = SetComputeBrainBuffer("CSSimNeuronAttract");
        //shaderComputeBrain.Dispatch(simNeuronAttractKernelID, axons.Count, 1, 1); // Simulate!! move neuron and axons around
        //int simNeuronRepelKernelID = shaderComputeBrain.FindKernel("CSSimNeuronRepel");  
        //shaderComputeBrain.Dispatch(simNeuronRepelKernelID, numNeurons, numNeurons, 1); // Simulate!! move neuron and axons around
        //int simAxonRepelKernelID = shaderComputeBrain.FindKernel("CSSimAxonRepel");
        //shaderComputeBrain.Dispatch(simAxonRepelKernelID, axons.Count, axons.Count, 1); // Simulate!! move neuron and axons around

        SetArgsBuffer(argsCore, argsCoreCBuffer, appendTrianglesCoreCBuffer);
        //SetArgsBuffer(argsCables, argsCablesCBuffer, appendTrianglesCablesCBuffer);
    }

    void AppendTriangles(ref ComputeBuffer computeBuffer, int maxTriangles)
    {
        computeBuffer?.Release();
        computeBuffer = new ComputeBuffer(maxTriangles, sizeof(float) * 45, ComputeBufferType.Append);
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
    
    int SetComputeBrainBuffer(string kernelName)
    {
        int kernelID = SetShaderBuffer(shaderComputeBrain, kernelName);
        shaderComputeBrain.SetBuffer(kernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(kernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        return kernelID;
    }
    
    void SetCoreTrianglesBuffer(string kernelName, int x, int y = 1, int z = 1) {
        int kernelID = SetShaderBuffer(shaderComputeBrain, kernelName);        
        shaderComputeBrain.SetBuffer(kernelID, "appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);
        shaderComputeBrain.Dispatch(kernelID, x, y, z);
    }
    
    void SetAxonTrianglesBuffer(string kernelName, int x, int y = 1, int z = 1) {
        int kernelID = SetShaderBuffer(shaderComputeBrain, kernelName);        
        shaderComputeBrain.SetBuffer(kernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(kernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(kernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
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
            NeuronInitData neuronData = new NeuronInitData((float)neurons[x].io / 2.0f);
            neuronInitDataArray[x] = neuronData;
        }
        
        neuronInitDataCBuffer.SetData(neuronInitDataArray);
    }
    
    void InitializeNeuronFeedDataCBuffer()
    {
        neuronFeedDataCBuffer?.Release();
        neuronFeedDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 1);
        NeuronFeedData[] neuronValuesArray = new NeuronFeedData[numNeurons];
        
        for (int i = 0; i < neuronValuesArray.Length; i++)
            neuronValuesArray[i].curValue = neurons[i].currentValue;

        neuronFeedDataCBuffer.SetData(neuronValuesArray);
    }
    
    // For now only one seed data
    void InitializeAxons()
    {
        // Debug.Log($"Initializing {axons.Count} axons");
        axonInitDataCBuffer?.Release();
        axonInitDataCBuffer = new ComputeBuffer(axons.Count, sizeof(float) * 1 + sizeof(int) * 2);
        
        AxonInitData[] axonInitDataArray = new AxonInitData[axons.Count];

        for (int x = 0; x < axonInitDataArray.Length; x++) 
        {
            AxonInitData axonData = new AxonInitData(axons[x]);
            axonData.fromID = axons[x].from.index;
            axonData.toID = axons[x].to.index; 
            axonInitDataArray[x] = axonData;
            //PrintConnection(axonData.fromID, axonData.toID, x);
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
            neuronValuesArray[i].curValue = neurons[i].currentValue;
        }
        neuronFeedDataCBuffer.SetData(neuronValuesArray);

        SetCoreBrainDataSharedParameters(shaderComputeBrain);
        
        UpdateNeuronBuffer("CSSimNeuronAttract", axons.Count, 1);
        UpdateNeuronBuffer("CSSimNeuronRepel", numNeurons, numNeurons);
        //UpdateNeuronBuffer("CSSimAxonRepel", axons.Count, axons.Count);

        // Regenerate triangles
        appendTrianglesCoreCBuffer?.Release();
        //appendTrianglesCBuffer = new ComputeBuffer(numNeurons * maxTrisPerNeuron + numAxons * maxTrisPerAxon, sizeof(float) * 45, ComputeBufferType.Append); // vector3 position * 3 verts
        int maxTris = numNeurons * maxTrisPerNeuron;
        appendTrianglesCoreCBuffer = new ComputeBuffer(maxTris, sizeof(float) * 45, ComputeBufferType.Append); 
        appendTrianglesCoreCBuffer.SetCounterValue(0);

        SetCoreTrianglesBuffer("CSGenerateNeuronTriangles", neurons.Count);
        SetAxonTrianglesBuffer("CSGenerateAxonTriangles", axons.Count);

        displayMaterialCore.SetPass(0);
        displayMaterialCore.SetBuffer("appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);
        
        SetArgsBuffer(argsCore, argsCoreCBuffer, appendTrianglesCoreCBuffer);
        //SetArgsBuffer(argsCables, argsCablesCBuffer, appendTrianglesCablesCBuffer);
    }

    /// Sets metadata for append buffer
    void SetArgsBuffer(uint[] args, ComputeBuffer argsBuffer, ComputeBuffer trianglesBuffer)
    {
        args[0] = 0;  // set later by counter;
        args[1] = 1;  // 1 instance/copy
        argsBuffer.SetData(args);
        ComputeBuffer.CopyCount(trianglesBuffer, argsBuffer, 0);
        argsBuffer.GetData(args);
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
        //floatingGlowyBitsCBuffer?.Release();
        quadVerticesCBuffer?.Release();
        //extraBallsCBuffer?.Release();
        //axonBallCBuffer?.Release();
        //neuronBallCBuffer?.Release();
        //cableInitDataCBuffer?.Release();
        //cableSimDataCBuffer?.Release();
        //socketInitDataCBuffer?.Release();
        //appendTrianglesCablesCBuffer?.Release();
        //argsCablesCBuffer?.Release();
    }
}

#region Dead Code
/*void SetTrianglesBuffer(string kernelName, int x, int y = 1, int z = 1, bool refreshAxonBuffer = true)
{
    int kernelID = SetShaderBuffer(shaderComputeBrain, kernelName);
    
    if (refreshAxonBuffer)
    {
        shaderComputeBrain.SetBuffer(kernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(kernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
    }
    
    shaderComputeBrain.SetBuffer(kernelID, "appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);
    shaderComputeBrain.Dispatch(kernelID, x, y, z); // Create all triangles for SubNeurons
}*/

/*void InitializeCables(int inputCount, int outputCount)
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
}*/

/*void MoveCables()
{
    int updateCablePositionsKernelID = SetShaderBuffer(shaderComputeBrain, "CSUpdateCablePositions");
    shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
    shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "socketInitDataCBuffer", socketInitDataCBuffer);
    //shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
    shaderComputeBrain.Dispatch(updateCablePositionsKernelID, numNeurons, 1, 1); // Simulate!! move neuron and axons around
}*/
    
/*void CreateSplineGeometryForCables()
{
    int generateCablesTrianglesKernelID = shaderComputeBrain.FindKernel("CSGenerateCablesTriangles");
    shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
    shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
    shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
    shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
    displayMaterialCables.SetPass(0);
    displayMaterialCables.SetBuffer("appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
    shaderComputeBrain.Dispatch(generateCablesTrianglesKernelID, numNeurons, 1, 1); // create all geometry for Axons
}*/
#endregion