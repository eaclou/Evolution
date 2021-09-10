﻿using System.Collections.Generic;
using UnityEngine;

public class GenerateBrainVisualization : MonoBehaviour 
{
    public List<Neuron> neurons;
    public List<Axon> axons;
    
    #region Internal data

    private ComputeBuffer quadVerticesCBuffer;  // holds information for a 2-triangle Quad mesh (6 vertices)
    private ComputeBuffer floatingGlowyBitsCBuffer;  // holds information for placement and attributes of each instance of quadVertices to draw
    private ComputeBuffer extraBallsCBuffer;
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
    private ComputeBuffer argsCoreCBuffer;        // Some other secondary buffers for decorations later
    private uint[] argsCore = new uint[5] { 0, 0, 0, 0, 0 };
    
    // will likely split these out into seperate ones later to support multiple materials/layers, but all-in-one for now...
    private ComputeBuffer appendTrianglesCablesCBuffer; 
    private ComputeBuffer appendTrianglesCoreCBuffer;
    private ComputeBuffer argsCablesCBuffer;
    
    private uint[] argsCables = new uint[5] { 0, 0, 0, 0, 0 };
    
    int numAxons = 270;
    bool initialized;
    
    #endregion
    
    // Data sent into compute buffers through arrays
    #region Structs
    
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
    
    public struct NeuronSimData {
        public Vector3 pos;
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
            fromID = axon.fromID;
            toID = axon.toID;
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
    
    public struct SocketInitData {
        public Vector3 pos;
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
    Material displayMaterialCore => settings.displayMaterialCore;
    Material displayMaterialCables => settings.displayMaterialCables;
    Material floatingGlowyBitsMaterial => settings.floatingGlowyBitsMaterial;
    Material extraBallsMaterial => settings.extraBallsMaterial;

    int numNeurons => settings.numNeurons; 

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

    #endregion

    /*void Start () 
    {
        //Debug.Log(Quaternion.identity.w.ToString() + ", " + Quaternion.identity.x.ToString() + ", " + Quaternion.identity.y.ToString() + ", " + Quaternion.identity.z.ToString() + ", ");
        argsCoreCBuffer = new ComputeBuffer(1, argsCore.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsCablesCBuffer = new ComputeBuffer(1, argsCables.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        
        InitializeComputeBuffers();
    }*/

    void SetCoreBrainDataSharedParameters(ComputeShader computeShader) 
    {
        // Core Sizes:
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

        // Noise Parameters:
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

        // Forces:
        computeShader.SetFloat("neuronAttractForce", neuronAttractForce);
        computeShader.SetFloat("neuronRepelForce", neuronRepelForce);
        computeShader.SetFloat("axonPerpendicularityForce", axonPerpendicularityForce);
        computeShader.SetFloat("axonAttachStraightenForce", axonAttachStraightenForce);
        computeShader.SetFloat("axonAttachSpreadForce", axonAttachSpreadForce);
        computeShader.SetFloat("axonRepelForce", axonRepelForce);
        computeShader.SetFloat("cableAttractForce", cableAttractForce);

        computeShader.SetFloat("time", Time.fixedTime);
    }
    
    public void Initialize()
    {
        InitializeComputeBuffers();
        initialized = true;
    }

    void InitializeComputeBuffers() 
    {
        argsCoreCBuffer = new ComputeBuffer(1, argsCore.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsCablesCBuffer = new ComputeBuffer(1, argsCables.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        // first-time setup for compute buffers (assume new brain)
        //if(neurons == null || axons == null) {
        //    CreateDummyBrain();
        //}
        
        InitializeNeuronCBufferData();
        InitializeNeuronFeedDataCBuffer();

        neuronSimDataCBuffer?.Release();
        neuronSimDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 3);
        
        InitializeNeuralPositions();
        InitializeAxons();

        // SOCKET LOCATIONS DATA:::::
        socketInitDataCBuffer?.Release();
        socketInitDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 3);
        // One-time initialization of positions::::
        List<Neuron> inputNeurons = new List<Neuron>();
        List<Neuron> outputNeurons = new List<Neuron>();
        
        SocketInitData[] socketInitDataArray = new SocketInitData[numNeurons];
        
        for (int i = 0; i < numNeurons; i++) {
            var list = neurons[i].neuronType == NeuronGenome.NeuronType.In ? inputNeurons : outputNeurons;
            list.Add(neurons[i]);
        }
        
        AssignNeuralPositions(ref socketInitDataArray, inputNeurons.Count, 0, -.9f);
        AssignNeuralPositions(ref socketInitDataArray, outputNeurons.Count, inputNeurons.Count, 0.9f);

        socketInitDataCBuffer.SetData(socketInitDataArray);
        socketInitDataCBuffer.GetData(socketInitDataArray);

        // CABLE INIT DATA
        cableInitDataCBuffer?.Release();
        cableInitDataCBuffer = new ComputeBuffer(numNeurons, sizeof(int) * 2);
        
        CableInitData[] cableInitDataArray = InitializeCableArray(numNeurons, inputNeurons.Count, outputNeurons.Count);

        cableInitDataCBuffer.SetData(cableInitDataArray);
        
        // CABLE SIM DATA
        cableSimDataCBuffer?.Release();
        cableSimDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 12);
        
        int maxTriangles = numNeurons * maxTrisPerNeuron + axons.Count * maxTrisPerAxon + maxTrisPerSubNeuron * axons.Count * 2;
        AppendTriangles(ref appendTrianglesCoreCBuffer, maxTriangles);
        
        maxTriangles = numNeurons * maxTrisPerCable;
        AppendTriangles(ref appendTrianglesCablesCBuffer, maxTriangles);

        // FREE-FLOATING CAMERA-FACING QUADS:::::::::::
        //Create quad buffer
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

        // Initialize cables
        int initCablesKernelID = SetShaderBuffer(shaderComputeBrain, "CSInitializeCableSimData");
        shaderComputeBrain.SetBuffer(initCablesKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
        shaderComputeBrain.SetBuffer(initCablesKernelID, "socketInitDataCBuffer", socketInitDataCBuffer);
        
        // Create spline geometry for cables:
        int generateCablesTrianglesKernelID = SetShaderBuffer(shaderComputeBrain, "CSGenerateCablesTriangles");
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "socketInitDataCBuffer", socketInitDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        
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
        // CABLES:::
        displayMaterialCables.SetPass(0);
        displayMaterialCables.SetBuffer("appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        shaderComputeBrain.Dispatch(initCablesKernelID, numNeurons, 1, 1); // initialize axon positions and attributes
        shaderComputeBrain.Dispatch(generateCablesTrianglesKernelID, numNeurons, 1, 1); // create all geometry for Axons
        
        SetCoreBrainDataSharedParameters(shaderComputeExtraBalls);

        SetExtraBallsBuffer("CSUpdateAxonBallPositions", numAxonBalls, true, 2f, 4f);
        SetExtraBallsBuffer("CSUpdateNeuronBallPositions", numNeuronBalls, true, 2f, 4f);

        SetArgsBuffer(argsCore, argsCoreCBuffer, appendTrianglesCoreCBuffer);
        SetArgsBuffer(argsCables, argsCablesCBuffer, appendTrianglesCablesCBuffer);
    }
    
    void AssignNeuralPositions(ref SocketInitData[] data,int count, int offsetIndex, float zPosition)
    {
        for(int i = 0; i < count; i++) {
            float x = 0.6f * (float)i / (float)count - 0.3f;
            int tier = Random.Range(0, 5);
            data[i + offsetIndex].pos = new Vector3(x, (float)(tier - 2) * 0.12f, zPosition);
        }
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
        
        // * WPP: is this necessary to conditionally exclude?
        if (refreshAxonBuffer)
        {
            shaderComputeBrain.SetBuffer(kernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
            shaderComputeBrain.SetBuffer(kernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        }
        
        shaderComputeBrain.SetBuffer(kernelID, "appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);
        shaderComputeBrain.Dispatch(kernelID, x, y, z); // create all triangles for SubNeurons
    }

    // for now only one seed data
    void InitializeNeuronCBufferData()
    {
        neuronInitDataCBuffer?.Release();
        neuronInitDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 3);
        
        NeuronInitData[] neuronInitDataArray = new NeuronInitData[numNeurons]; 
        
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
        Debug.Log(neuronValuesArray.Length + ", numNeurons: " + numNeurons);
        
        for(int i = 0; i < neuronValuesArray.Length; i++) {
            neuronValuesArray[i].curValue = neurons[i].currentValue[0];
        }
        
        neuronFeedDataCBuffer.SetData(neuronValuesArray);
    }
    
    void InitializeNeuralPositions()
    {
        NeuronSimData[] neuronSimDataArray = new NeuronSimData[numNeurons];
        
        for (int i = 0; i < neuronSimDataArray.Length; i++) 
        {
            neuronSimDataArray[i].pos = Random.insideUnitSphere * 1f;
            var polarity = neurons[i].neuronType == NeuronGenome.NeuronType.In ? -1f : 1f;
            neuronSimDataArray[i].pos.z = polarity * Mathf.Abs(neuronSimDataArray[i].pos.z);
        }
        
        neuronSimDataCBuffer.SetData(neuronSimDataArray);
    }
    
    // For now only one seed data
    void InitializeAxons()
    {
        axonInitDataCBuffer?.Release();
        Debug.Log($"Initializing {axons.Count} axons");
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
            neuronValuesArray[i].curValue = Mathf.Sin(Time.fixedTime * 1.25f + neurons[i].currentValue[0]);
        }
        neuronFeedDataCBuffer.SetData(neuronValuesArray);

        // For some reason I have to setBuffer on all of these for it to WORK!!!!!!!! (even though they are all the same in the shader...)
        SetCoreBrainDataSharedParameters(shaderComputeBrain);
        SetCoreBrainDataSharedParameters(shaderComputeExtraBalls);
        
        UpdateNeuronBuffer("CSSimNeuronAttract", numAxons, 1);
        UpdateNeuronBuffer("CSSimNeuronRepel", numNeurons, numNeurons);
        UpdateNeuronBuffer("CSSimAxonRepel", numAxons, numAxons);

        // Add Cables Movement Here
        //))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))
        int updateCablePositionsKernelID = SetShaderBuffer(shaderComputeBrain, "CSUpdateCablePositions");
        shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
        shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "socketInitDataCBuffer", socketInitDataCBuffer);
        //shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        shaderComputeBrain.Dispatch(updateCablePositionsKernelID, numNeurons, 1, 1); // Simulate!! move neuron and axons around
        // ((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((

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
        
        // Create spline geometry for cables:
        int generateCablesTrianglesKernelID = shaderComputeBrain.FindKernel("CSGenerateCablesTriangles");
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        displayMaterialCables.SetPass(0);
        displayMaterialCables.SetBuffer("appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        shaderComputeBrain.Dispatch(generateCablesTrianglesKernelID, numNeurons, 1, 1); // create all geometry for Axons
        
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
    
    // * WPP: Rename (what are "args"?)
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

    public void CreateBrain(int inputCount) 
    {
        neurons = new List<Neuron>();
        
        for (int i = 0; i < numNeurons; i++) 
        {
            Neuron neuron = new Neuron(i, inputCount);
            neurons.Add(neuron);
        }
        
        axons = new List<Axon>();
        for (int i = 0; i < inputCount; i++) 
        {
            for(int j = 0; j < numNeurons - inputCount; j++) 
            {
                if (j + i * inputCount < numAxons) 
                {
                    Axon axon = new Axon(i, inputCount + j, Random.Range(-1f, 1f));
                    axons.Add(axon);
                }
            }
        }

        numAxons = axons.Count;
        
        if (numAxons <= 0)
        {
            Debug.LogError($"Invalid input count {inputCount}, cannot create brain." +
                           $"Input count must be greater than 0 and less than the number of neurons ({numNeurons})");
            return;
        }
        
        Initialize();
    }

    void OnRenderObject() 
    {
        if (!initialized) return;
    
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
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, extraBallsCBuffer.count);
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