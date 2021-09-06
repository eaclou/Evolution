using System.Collections.Generic;
using UnityEngine;

public class GenerateBrainVisualization : MonoBehaviour 
{
    public List<Neuron> tempNeuronList;
    public List<Axon> tempAxonList;

    public ComputeShader shaderComputeBrain;
    public ComputeShader shaderComputeFloatingGlowyBits;
    public ComputeShader shaderComputeExtraBalls;  // quads w/ nml maps to like like extra blobs attached to neurons & axons
    //public Shader shaderDisplayBrain;
    public Material displayMaterialCore;
    public Material displayMaterialCables;
    public Material floatingGlowyBitsMaterial;
    public Material extraBallsMaterial;

    private ComputeBuffer quadVerticesCBuffer;  // holds information for a 2-triangle Quad mesh (6 vertices)
    private ComputeBuffer floatingGlowyBitsCBuffer;  // holds information for placement and attributes of each instance of quadVertices to draw
    private ComputeBuffer extraBallsCBuffer;
    private ComputeBuffer axonBallCBuffer;
    private ComputeBuffer neuronBallCBuffer;

    private ComputeBuffer neuronInitDataCBuffer;  // sets initial positions for each neuron
    private ComputeBuffer neuronFeedDataCBuffer;  // current value -- separate so CPU only has to push the bare-minimum data to GPU every n frames
    private ComputeBuffer neuronSimDataCBuffer;  // holds data that is updatable purely on GPU, like neuron Positions
    //private ComputeBuffer subNeuronSimDataCBuffer; // just 
    private ComputeBuffer axonInitDataCBuffer;  // holds axon weights ( as well as from/to neuron IDs ?)
    private ComputeBuffer axonSimDataCBuffer;  // axons can be updated entirely on GPU by referencing neuron positions.
    // .^.^.^. this includes all spline data --> positions, vertex colors, uv, radii, etc., pulse positions
    private ComputeBuffer cableInitDataCBuffer;  // initial data required to calculate positions of cables
    private ComputeBuffer cableSimDataCBuffer;  // holds the positions of the control points of the cable's underlying bezier curve
    private ComputeBuffer socketInitDataCBuffer;  // holds the positions of the sockets on wall that cables plug into
        // Some other secondary buffers for decorations later
    private ComputeBuffer appendTrianglesCoreCBuffer; // will likely split this out into seperate ones later to support multiple materials/layers, but all-in-one for now...
    private ComputeBuffer argsCoreCBuffer;
    private uint[] argsCore = new uint[5] { 0, 0, 0, 0, 0 };
    private ComputeBuffer appendTrianglesCablesCBuffer; // will likely split this out into seperate ones later to support multiple materials/layers, but all-in-one for now...
    private ComputeBuffer argsCablesCBuffer;
    private uint[] argsCables = new uint[5] { 0, 0, 0, 0, 0 };

    //private Material displayMaterial;

    public struct NeuronInitData {
        //public Vector3 pos;  // can maybe remove this and just set NeuronSimData.pos once at start of program?
        public float radius;
        public float type;  // in/out/hidden
        public float age;
    }
    
    public struct NeuronFeedData {
        public float curValue;  // [-1,1]  // set by CPU continually
    }
    
    public struct NeuronSimData {
        public Vector3 pos;
    }
    
    public struct AxonInitData {  // set once at start
        public float weight;
        public int fromID;
        public int toID;
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

    int numNeurons = 33; // refBrain.neuronList.Count;
    int numAxons = 270; // refBrain.axonList.Count;
    int maxTrisPerNeuron = 1024;
    int maxTrisPerSubNeuron = 8 * 8 * 2 * 2;
    int maxTrisPerAxon = 2048;
    int maxTrisPerCable = 2048;
    int numFloatingGlowyBits = 8192 * 8;
    int numAxonBalls = 8 * 128;
    int numNeuronBalls = 128;
    
    // Core Sizes:
    public float minNeuronRadius = 0.05f;
    public float maxNeuronRadius = 0.5f;
    public float minAxonRadius = 0.05f;
    public float maxAxonRadius = 0.5f;
    public float minSubNeuronScale = 0.25f;
    public float maxSubNeuronScale = 0.75f;  // max size relative to parent Neuron
    public float maxAxonFlareScale = 0.9f;  // max axon flare size relative to SubNeuron
    public float minAxonFlareScale = 0.2f;
    public float axonFlarePos = 0.92f;
    public float axonFlareWidth = 0.08f;
    public float axonMaxPulseMultiplier = 2.0f;
    public float cableRadius = 0.05f;

    // Noise Parameters:
    public float neuronExtrudeNoiseFreq = 1.5f;
    public float neuronExtrudeNoiseAmp = 0.0f;
    public float neuronExtrudeNoiseScrollSpeed = 0.6f;
    public float axonExtrudeNoiseFreq = 0.33f;
    public float axonExtrudeNoiseAmp = 0.33f;
    public float axonExtrudeNoiseScrollSpeed = 1.0f;
    public float axonPosNoiseFreq = 0.14f;
    public float axonPosNoiseAmp = 0f;
    public float axonPosNoiseScrollSpeed = 10f;
    public float axonPosSpiralFreq = 20.0f;
    public float axonPosSpiralAmp = 0f;

    // Forces:
    public float neuronAttractForce = 0.004f;
    public float neuronRepelForce = 2.0f;
    public float axonPerpendicularityForce = 0.01f;
    public float axonAttachStraightenForce = 0.01f;
    public float axonAttachSpreadForce = 0.025f;
    public float axonRepelForce = 0.2f;
    public float cableAttractForce = 0.01f;

    // Extra Balls:
    public float neuronBallMaxScale = 1f;

    // Use this for initialization
    void Start () 
    {
        //Debug.Log(Quaternion.identity.w.ToString() + ", " + Quaternion.identity.x.ToString() + ", " + Quaternion.identity.y.ToString() + ", " + Quaternion.identity.z.ToString() + ", ");
        argsCoreCBuffer = new ComputeBuffer(1, argsCore.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsCablesCBuffer = new ComputeBuffer(1, argsCables.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        
        //UpdateBuffers();
        InitializeComputeBuffers();
    }

    private void SetCoreBrainDataSharedParameters(ComputeShader computeShader) 
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

    // * WPP: extract repetitive code into functions
    private void InitializeComputeBuffers() 
    {
        // first-time setup for compute buffers (assume new brain)
        if(tempNeuronList == null || tempAxonList == null) {
            CreateDummyBrain();
        }

        // NEURON INIT DATA
        neuronInitDataCBuffer?.Release();
        neuronInitDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 3);
        NeuronInitData[] neuronInitDataArray = new NeuronInitData[numNeurons]; // for now only one seed data
        for (int x = 0; x < neuronInitDataArray.Length; x++) {
            NeuronInitData neuronData = new NeuronInitData();
            //neuronData.pos = Vector3.zero; // refBrain.neuronList[x].pos;
            neuronData.radius = 1.6f;
            neuronData.type = (float)tempNeuronList[x].neuronType / 2.0f;
            neuronData.age = Random.Range(0f, 1f); // refBrain.neuronList[x].age;
            neuronInitDataArray[x] = neuronData;
        }
        neuronInitDataCBuffer.SetData(neuronInitDataArray);

        // NEURON FEED DATA
        neuronFeedDataCBuffer?.Release();
        neuronFeedDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 1);
        NeuronFeedData[] neuronValuesArray = new NeuronFeedData[numNeurons];
        Debug.Log(neuronValuesArray.Length + ", numNeurons: " + numNeurons);
        for(int i = 0; i < neuronValuesArray.Length; i++) {
            neuronValuesArray[i].curValue = tempNeuronList[i].currentValue[0];
        }
        neuronFeedDataCBuffer.SetData(neuronValuesArray);

        // NEURON SIM DATA
        neuronSimDataCBuffer?.Release();
        neuronSimDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 3);
        
        // One-time initialization of positions::::
        NeuronSimData[] neuronSimDataArray = new NeuronSimData[numNeurons];
        for (int i = 0; i < neuronSimDataArray.Length; i++) {
            neuronSimDataArray[i].pos = Random.insideUnitSphere * 1f; //refBrain.neuronList[i].pos;
            var polarity = tempNeuronList[i].neuronType == NeuronGenome.NeuronType.In ? -1f : 1f;
            neuronSimDataArray[i].pos.z = polarity * Mathf.Abs(neuronSimDataArray[i].pos.z);
        }
        neuronSimDataCBuffer.SetData(neuronSimDataArray);

        // AXON INIT DATA
        axonInitDataCBuffer?.Release();
        axonInitDataCBuffer = new ComputeBuffer(tempAxonList.Count, sizeof(float) * 1 + sizeof(int) * 2);
        AxonInitData[] axonInitDataArray = new AxonInitData[tempAxonList.Count]; // for now only one seed data
        for (int x = 0; x < axonInitDataArray.Length; x++) {
            AxonInitData axonData = new AxonInitData();
            axonData.weight = tempAxonList[x].weight;
            axonData.fromID = tempAxonList[x].fromID;
            axonData.toID = tempAxonList[x].toID;
            axonInitDataArray[x] = axonData;
        }
        
        axonInitDataCBuffer.SetData(axonInitDataArray);

        // AXON SIM DATA
        axonSimDataCBuffer?.Release();
        axonSimDataCBuffer = new ComputeBuffer(tempAxonList.Count, sizeof(float) * 13);

        // SOCKET LOCATIONS DATA:::::
        socketInitDataCBuffer?.Release();
        socketInitDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 3);
        // One-time initialization of positions::::
        List<Neuron> inputNeuronsList = new List<Neuron>();
        List<Neuron> outputNeuronsList = new List<Neuron>();
        SocketInitData[] socketInitDataArray = new SocketInitData[numNeurons];
        
        for (int i = 0; i < numNeurons; i++) {
            var list = tempNeuronList[i].neuronType == NeuronGenome.NeuronType.In ? inputNeuronsList : outputNeuronsList;
            list.Add(tempNeuronList[i]);
        }
        
        //traverse inputNeurons and assign them positions:
        for(int i = 0; i < inputNeuronsList.Count; i++) {
            float x = 0.6f * (float)i / (float)inputNeuronsList.Count - 0.3f;
            int tier = Random.Range(0, 5);
            socketInitDataArray[i].pos = new Vector3(x, (float)(tier - 2) * 0.12f, -0.9f);
        }
         
        // //traverse outputNeurons and assign them positions:
        for (int i = 0; i < outputNeuronsList.Count; i++) {
            float x = 0.6f * (float)i / (float)outputNeuronsList.Count - 0.3f;
            int tier = Random.Range(0, 5);
            socketInitDataArray[i + inputNeuronsList.Count].pos = new Vector3(x, (float)(tier - 2) * 0.12f, 0.9f);
        }
        
        socketInitDataCBuffer.SetData(socketInitDataArray);
        socketInitDataCBuffer.GetData(socketInitDataArray);
        Debug.Log(socketInitDataArray[0].pos.ToString());

        // CABLE INIT DATA
        cableInitDataCBuffer?.Release();
        cableInitDataCBuffer = new ComputeBuffer(numNeurons, sizeof(int) * 2);
        
        // One-time initialization of positions
        CableInitData[] cableInitDataArray = new CableInitData[numNeurons];
        
        //traverse inputNeurons and assign them positions:
        for (int i = 0; i < inputNeuronsList.Count; i++) {
            cableInitDataArray[i].neuronID = i;
            cableInitDataArray[i].socketID = i;
        } 
        
        //traverse outputNeurons and assign them positions:
        for (int i = 0; i < outputNeuronsList.Count; i++) {
            cableInitDataArray[i].neuronID = inputNeuronsList.Count + i;
            cableInitDataArray[i].socketID = inputNeuronsList.Count + i;
        }  
              
        cableInitDataCBuffer.SetData(cableInitDataArray);
        
        // CABLE SIM DATA
        cableSimDataCBuffer?.Release();
        cableSimDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 12);
        
        // TRIANGLE BUFFERZZZ:
        // SET UP GEO BUFFER and REFS
        appendTrianglesCoreCBuffer?.Release();
        int maxTris = numNeurons * maxTrisPerNeuron + tempAxonList.Count * maxTrisPerAxon + maxTrisPerSubNeuron * tempAxonList.Count * 2;
        Debug.Log("Max Tris: " + maxTris);
        appendTrianglesCoreCBuffer = new ComputeBuffer(maxTris, sizeof(float) * 45, ComputeBufferType.Append); // vector3 position * 3 verts
        appendTrianglesCoreCBuffer.SetCounterValue(0);
        ////////////////////////////////////////////////////////////  CABLES:::
        appendTrianglesCablesCBuffer?.Release();
        int maxTrisCable = numNeurons * maxTrisPerCable;
        Debug.Log("Max Tris Cable: " + maxTrisCable);
        appendTrianglesCablesCBuffer = new ComputeBuffer(maxTrisCable, sizeof(float) * 45, ComputeBufferType.Append); // vector3 position * 3 verts
        appendTrianglesCablesCBuffer.SetCounterValue(0);

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
        
        int initGlowyBitsKernelID = shaderComputeFloatingGlowyBits.FindKernel("CSInitializePositions");
        shaderComputeFloatingGlowyBits.SetFloat("minRadius", 0.0f);
        shaderComputeFloatingGlowyBits.SetFloat("maxRadius", 0.8f);
        shaderComputeFloatingGlowyBits.SetBuffer(initGlowyBitsKernelID, "floatingGlowyBitsCBuffer", floatingGlowyBitsCBuffer);
        shaderComputeFloatingGlowyBits.Dispatch(initGlowyBitsKernelID, numFloatingGlowyBits / 64, 1, 1); // initialize axon positions and attributes

        //  EXTRA BALLS NORMAL-MAPPED CAMERA-FACING QUADS:::::::::::
        extraBallsCBuffer?.Release();
        extraBallsCBuffer = new ComputeBuffer(numAxonBalls + numNeuronBalls, sizeof(float) * 6);
        extraBallsMaterial.SetPass(0);
        extraBallsMaterial.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        extraBallsMaterial.SetBuffer("extraBallsCBuffer", extraBallsCBuffer);

        axonBallCBuffer?.Release();
        axonBallCBuffer = new ComputeBuffer(numAxonBalls, sizeof(float) * 3 + sizeof(int) * 1);
        neuronBallCBuffer?.Release();
        neuronBallCBuffer = new ComputeBuffer(numNeuronBalls, sizeof(float) * 4 + sizeof(int) * 1);

        int initAxonBallsKernelID = shaderComputeExtraBalls.FindKernel("CSInitializeAxonBallData");
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "axonBallCBuffer", axonBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "neuronBallCBuffer", neuronBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "extraBallsCBuffer", extraBallsCBuffer);
        shaderComputeExtraBalls.Dispatch(initAxonBallsKernelID, numAxonBalls, 1, 1); // initialize axon positions and attributes

        int initNeuronBallsKernelID = shaderComputeExtraBalls.FindKernel("CSInitializeNeuronBallData");
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "axonBallCBuffer", axonBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "neuronBallCBuffer", neuronBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "extraBallsCBuffer", extraBallsCBuffer);
        shaderComputeExtraBalls.Dispatch(initNeuronBallsKernelID, numNeuronBalls, 1, 1); // initialize axon positions and attributes

        // Hook Buffers Up to Shaders!!!
        // populate initial data for neurons
        // populate initial data for axons
        // feed neuronValues data to shader (encapsulate in function since this is ongoing)
        // simulate movements / animation parameters
        // generate neuron triangles
        // generate axon triangles
        SetCoreBrainDataSharedParameters(shaderComputeBrain);

        int initKernelID = shaderComputeBrain.FindKernel("CSInitializeAxonSimData");
        shaderComputeBrain.SetBuffer(initKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(initKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(initKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(initKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(initKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);

        int simNeuronAttractKernelID = shaderComputeBrain.FindKernel("CSSimNeuronAttract");
        shaderComputeBrain.SetBuffer(simNeuronAttractKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(simNeuronAttractKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(simNeuronAttractKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(simNeuronAttractKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(simNeuronAttractKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        int simNeuronRepelKernelID = shaderComputeBrain.FindKernel("CSSimNeuronRepel");
        int simAxonRepelKernelID = shaderComputeBrain.FindKernel("CSSimAxonRepel");
        
        // CABLES Init:
        int initCablesKernelID = shaderComputeBrain.FindKernel("CSInitializeCableSimData");
        shaderComputeBrain.SetBuffer(initCablesKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(initCablesKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(initCablesKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(initCablesKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
        shaderComputeBrain.SetBuffer(initCablesKernelID, "socketInitDataCBuffer", socketInitDataCBuffer);
        //CABLES  // create spline geometry for cables:
        int generateCablesTrianglesKernelID = shaderComputeBrain.FindKernel("CSGenerateCablesTriangles");
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "socketInitDataCBuffer", socketInitDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        
        int neuronTrianglesKernelID = shaderComputeBrain.FindKernel("CSGenerateNeuronTriangles");
        shaderComputeBrain.SetBuffer(neuronTrianglesKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(neuronTrianglesKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(neuronTrianglesKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(neuronTrianglesKernelID, "appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);

        int subNeuronTrianglesKernelID = shaderComputeBrain.FindKernel("CSGenerateSubNeuronTriangles");
        shaderComputeBrain.SetBuffer(subNeuronTrianglesKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(subNeuronTrianglesKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(subNeuronTrianglesKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(subNeuronTrianglesKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(subNeuronTrianglesKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeBrain.SetBuffer(subNeuronTrianglesKernelID, "appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);

        int axonTrianglesKernelID = shaderComputeBrain.FindKernel("CSGenerateAxonTriangles");
        shaderComputeBrain.SetBuffer(axonTrianglesKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(axonTrianglesKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(axonTrianglesKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(axonTrianglesKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(axonTrianglesKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeBrain.SetBuffer(axonTrianglesKernelID, "appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);
                
        displayMaterialCore.SetPass(0);
        displayMaterialCore.SetBuffer("appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);   // link computeBuffer to both computeShader and displayShader so they share the same one!!
        
        shaderComputeBrain.Dispatch(initKernelID, numAxons, 1, 1); // initialize axon positions and attributes
        shaderComputeBrain.Dispatch(simNeuronAttractKernelID, numAxons, 1, 1); // Simulate!! move neuron and axons around
        shaderComputeBrain.Dispatch(simNeuronRepelKernelID, numNeurons, numNeurons, 1); // Simulate!! move neuron and axons around
        shaderComputeBrain.Dispatch(simAxonRepelKernelID, numAxons, numAxons, 1); // Simulate!! move neuron and axons around
        shaderComputeBrain.Dispatch(neuronTrianglesKernelID, numNeurons, 1, 1); // create all triangles from Neurons
        shaderComputeBrain.Dispatch(subNeuronTrianglesKernelID, tempAxonList.Count * 2, 1, 1); // create all triangles for SubNeurons
        shaderComputeBrain.Dispatch(axonTrianglesKernelID, numAxons, 1, 1); // create all geometry for Axons
        // CABLES:::
        displayMaterialCables.SetPass(0);
        displayMaterialCables.SetBuffer("appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        shaderComputeBrain.Dispatch(initCablesKernelID, numNeurons, 1, 1); // initialize axon positions and attributes
        shaderComputeBrain.Dispatch(generateCablesTrianglesKernelID, numNeurons, 1, 1); // create all geometry for Axons
        
        SetCoreBrainDataSharedParameters(shaderComputeExtraBalls);

        int positionAxonBallsKernelID = shaderComputeExtraBalls.FindKernel("CSUpdateAxonBallPositions");
        shaderComputeExtraBalls.SetFloat("minRadius", 2f);
        shaderComputeExtraBalls.SetFloat("maxRadius", 4f);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "axonBallCBuffer", axonBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "neuronBallCBuffer", neuronBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "extraBallsCBuffer", extraBallsCBuffer);
        shaderComputeExtraBalls.Dispatch(positionAxonBallsKernelID, numAxonBalls, 1, 1); // initialize axon positions and attributes
        int positionNeuronBallsKernelID = shaderComputeExtraBalls.FindKernel("CSUpdateNeuronBallPositions");
        shaderComputeExtraBalls.SetFloat("minRadius", 2f);
        shaderComputeExtraBalls.SetFloat("maxRadius", 4f);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "axonBallCBuffer", axonBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "neuronBallCBuffer", neuronBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "extraBallsCBuffer", extraBallsCBuffer);
        shaderComputeExtraBalls.Dispatch(positionNeuronBallsKernelID, numNeuronBalls, 1, 1); // initialize axon positions and attributes

        argsCore[0] = 0; // set later by counter;// 3;  // 3 vertices to start
        argsCore[1] = 1;  // 1 instance/copy
        argsCoreCBuffer.SetData(argsCore);
        ComputeBuffer.CopyCount(appendTrianglesCoreCBuffer, argsCoreCBuffer, 0);
        argsCoreCBuffer.GetData(argsCore);
        Debug.Log("triangle count core: " + argsCore[0]);
        
        argsCables[0] = 0; // set later by counter;// 3;  // 3 vertices to start
        argsCables[1] = 1;  // 1 instance/copy
        argsCablesCBuffer.SetData(argsCables);
        ComputeBuffer.CopyCount(appendTrianglesCablesCBuffer, argsCablesCBuffer, 0);
        argsCablesCBuffer.GetData(argsCables);
        Debug.Log("triangle count cables: " + argsCables[0]);
    }

    // * WPP: extract repetitive code into function
    private void UpdateBrainDataAndBuffers() 
    {
        // NEURON FEED DATA
        //if (neuronFeedDataCBuffer != null)
        //    neuronFeedDataCBuffer.Release();
        //neuronFeedDataCBuffer = new ComputeBuffer(numNeurons, sizeof(float) * 1);
        if (tempNeuronList == null || tempAxonList == null)
            return;

        NeuronFeedData[] neuronValuesArray = new NeuronFeedData[tempNeuronList.Count];
        for (int i = 0; i < neuronValuesArray.Length; i++) {
            neuronValuesArray[i].curValue = Mathf.Sin(Time.fixedTime * 1.25f + tempNeuronList[i].currentValue[0]);
        }
        neuronFeedDataCBuffer.SetData(neuronValuesArray);

        // For some reason I have to setBuffer on all of these for it to WORK!!!!!!!! (even though they are all the same in the shader...)
        SetCoreBrainDataSharedParameters(shaderComputeBrain);
        SetCoreBrainDataSharedParameters(shaderComputeExtraBalls);
        
        int simNeuronAttractKernelID = shaderComputeBrain.FindKernel("CSSimNeuronAttract");        
        shaderComputeBrain.SetBuffer(simNeuronAttractKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(simNeuronAttractKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(simNeuronAttractKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(simNeuronAttractKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(simNeuronAttractKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeBrain.Dispatch(simNeuronAttractKernelID, numAxons, 1, 1); 
        
        // Simulate!! move neuron and axons around
        int simNeuronRepelKernelID = shaderComputeBrain.FindKernel("CSSimNeuronRepel");
        shaderComputeBrain.SetBuffer(simNeuronRepelKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(simNeuronRepelKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(simNeuronRepelKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(simNeuronRepelKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(simNeuronRepelKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeBrain.Dispatch(simNeuronRepelKernelID, numNeurons, numNeurons, 1); 
        
        // Simulate!! move neuron and axons around
        int simAxonRepelKernelID = shaderComputeBrain.FindKernel("CSSimAxonRepel");
        shaderComputeBrain.SetBuffer(simAxonRepelKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(simAxonRepelKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(simAxonRepelKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(simAxonRepelKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(simAxonRepelKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeBrain.Dispatch(simAxonRepelKernelID, numAxons, numAxons, 1); // Simulate!! move neuron and axons around

        // Add Cables Movement Here
        //))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))
        int updateCablePositionsKernelID = shaderComputeBrain.FindKernel("CSUpdateCablePositions");
        shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
        shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "socketInitDataCBuffer", socketInitDataCBuffer);
        //shaderComputeBrain.SetBuffer(updateCablePositionsKernelID, "appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        shaderComputeBrain.Dispatch(updateCablePositionsKernelID, numNeurons, 1, 1); // Simulate!! move neuron and axons around
        // ((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((
        
        // Extra BALLS!  // Do I need to re-initialize here?
        int initAxonBallsKernelID = shaderComputeExtraBalls.FindKernel("CSInitializeAxonBallData");
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "axonBallCBuffer", axonBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "neuronBallCBuffer", neuronBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(initAxonBallsKernelID, "extraBallsCBuffer", extraBallsCBuffer);
        shaderComputeExtraBalls.Dispatch(initAxonBallsKernelID, numAxonBalls, 1, 1); 
        
        // initialize axon positions and attributes
        int initNeuronBallsKernelID = shaderComputeExtraBalls.FindKernel("CSInitializeNeuronBallData");
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "axonBallCBuffer", axonBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "neuronBallCBuffer", neuronBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(initNeuronBallsKernelID, "extraBallsCBuffer", extraBallsCBuffer);
        shaderComputeExtraBalls.Dispatch(initNeuronBallsKernelID, numNeuronBalls, 1, 1); 
        
        // initialize axon positions and attributes
        int positionAxonBallsKernelID = shaderComputeExtraBalls.FindKernel("CSUpdateAxonBallPositions");
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "axonBallCBuffer", axonBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "neuronBallCBuffer", neuronBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionAxonBallsKernelID, "extraBallsCBuffer", extraBallsCBuffer);
        shaderComputeExtraBalls.Dispatch(positionAxonBallsKernelID, axonBallCBuffer.count, 1, 1); 
        
        // initialize axon positions and attributes
        int positionNeuronBallsKernelID = shaderComputeExtraBalls.FindKernel("CSUpdateNeuronBallPositions");
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "axonBallCBuffer", axonBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "neuronBallCBuffer", neuronBallCBuffer);
        shaderComputeExtraBalls.SetBuffer(positionNeuronBallsKernelID, "extraBallsCBuffer", extraBallsCBuffer);
        shaderComputeExtraBalls.Dispatch(positionNeuronBallsKernelID, neuronBallCBuffer.count, 1, 1);

        extraBallsMaterial.SetPass(0);
        //extraBallsMaterial.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        extraBallsMaterial.SetBuffer("extraBallsCBuffer", extraBallsCBuffer);

        // Re-Generate TRIANGLES!
        // SET UP GEO BUFFER and REFS:::::
        appendTrianglesCoreCBuffer?.Release();
        //Debug.Log("Max Tris: " + (numNeurons * maxTrisPerNeuron + numAxons * maxTrisPerAxon).ToString());
        //appendTrianglesCBuffer = new ComputeBuffer(numNeurons * maxTrisPerNeuron + numAxons * maxTrisPerAxon, sizeof(float) * 45, ComputeBufferType.Append); // vector3 position * 3 verts
        int maxTris = numNeurons * maxTrisPerNeuron + tempAxonList.Count * maxTrisPerAxon + maxTrisPerSubNeuron * tempAxonList.Count * 2;
        //Debug.Log("Max Tris: " + maxTris.ToString());
        appendTrianglesCoreCBuffer = new ComputeBuffer(maxTris, sizeof(float) * 45, ComputeBufferType.Append); // vector3 position * 3 verts
        appendTrianglesCoreCBuffer.SetCounterValue(0);
        /////$%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        appendTrianglesCablesCBuffer?.Release();
        int maxTrisCable = numNeurons * maxTrisPerNeuron + tempAxonList.Count * maxTrisPerAxon + maxTrisPerSubNeuron * tempAxonList.Count * 2;
        //Debug.Log("Max Tris Cable: " + maxTrisCable.ToString());
        appendTrianglesCablesCBuffer = new ComputeBuffer(maxTrisCable, sizeof(float) * 45, ComputeBufferType.Append); // vector3 position * 3 verts
        appendTrianglesCablesCBuffer.SetCounterValue(0);
        
        int neuronTrianglesKernelID = shaderComputeBrain.FindKernel("CSGenerateNeuronTriangles");
        shaderComputeBrain.SetBuffer(neuronTrianglesKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(neuronTrianglesKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(neuronTrianglesKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        //shaderComputeBrain.SetBuffer(neuronTrianglesKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        //shaderComputeBrain.SetBuffer(neuronTrianglesKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeBrain.SetBuffer(neuronTrianglesKernelID, "appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);

        int subNeuronTrianglesKernelID = shaderComputeBrain.FindKernel("CSGenerateSubNeuronTriangles");
        shaderComputeBrain.SetBuffer(subNeuronTrianglesKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(subNeuronTrianglesKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(subNeuronTrianglesKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(subNeuronTrianglesKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(subNeuronTrianglesKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeBrain.SetBuffer(subNeuronTrianglesKernelID, "appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);

        int axonTrianglesKernelID = shaderComputeBrain.FindKernel("CSGenerateAxonTriangles");
        shaderComputeBrain.SetBuffer(axonTrianglesKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeBrain.SetBuffer(axonTrianglesKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(axonTrianglesKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(axonTrianglesKernelID, "axonInitDataCBuffer", axonInitDataCBuffer);
        shaderComputeBrain.SetBuffer(axonTrianglesKernelID, "axonSimDataCBuffer", axonSimDataCBuffer);
        shaderComputeBrain.SetBuffer(axonTrianglesKernelID, "appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);
        
        displayMaterialCore.SetPass(0);
        displayMaterialCore.SetBuffer("appendTrianglesCoreCBuffer", appendTrianglesCoreCBuffer);
        
        shaderComputeBrain.Dispatch(neuronTrianglesKernelID, tempNeuronList.Count, 1, 1); // create all triangles from Neurons
        shaderComputeBrain.Dispatch(subNeuronTrianglesKernelID, tempAxonList.Count * 2, 1, 1); // create all triangles for SubNeurons
        shaderComputeBrain.Dispatch(axonTrianglesKernelID, tempAxonList.Count, 1, 1); // create all geometry for Axons

        //CABLES  // create spline geometry for cables:
        int generateCablesTrianglesKernelID = shaderComputeBrain.FindKernel("CSGenerateCablesTriangles");
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "cableSimDataCBuffer", cableSimDataCBuffer);
        shaderComputeBrain.SetBuffer(generateCablesTrianglesKernelID, "appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        // CABLES:::
        displayMaterialCables.SetPass(0);
        displayMaterialCables.SetBuffer("appendTrianglesCablesCBuffer", appendTrianglesCablesCBuffer);
        shaderComputeBrain.Dispatch(generateCablesTrianglesKernelID, numNeurons, 1, 1); // create all geometry for Axons
        
        //GlowyBitData FLOATING BITS:
        SetCoreBrainDataSharedParameters(shaderComputeFloatingGlowyBits);

        floatingGlowyBitsCBuffer?.Release();
        floatingGlowyBitsCBuffer = new ComputeBuffer(numFloatingGlowyBits, sizeof(float) * 6);
        floatingGlowyBitsMaterial.SetPass(0);
        floatingGlowyBitsMaterial.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        floatingGlowyBitsMaterial.SetBuffer("floatingGlowyBitsCBuffer", floatingGlowyBitsCBuffer);
        
        int initGlowyBitsKernelID = shaderComputeFloatingGlowyBits.FindKernel("CSInitializePositions");
        shaderComputeFloatingGlowyBits.SetFloat("minRadius", 0.0f);
        shaderComputeFloatingGlowyBits.SetFloat("maxRadius", 0.8f);
        shaderComputeFloatingGlowyBits.SetFloat("time", Time.fixedTime);
        shaderComputeFloatingGlowyBits.SetBuffer(initGlowyBitsKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeFloatingGlowyBits.SetBuffer(initGlowyBitsKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeFloatingGlowyBits.SetBuffer(initGlowyBitsKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeFloatingGlowyBits.SetBuffer(initGlowyBitsKernelID, "floatingGlowyBitsCBuffer", floatingGlowyBitsCBuffer);
        shaderComputeFloatingGlowyBits.Dispatch(initGlowyBitsKernelID, numFloatingGlowyBits / 64, 1, 1); 
        
        // initialize axon positions and attributes
        int updateGlowyBitsKernelID = shaderComputeFloatingGlowyBits.FindKernel("CSUpdateColor");
        shaderComputeFloatingGlowyBits.SetFloat("minRadius", 0.0f);
        shaderComputeFloatingGlowyBits.SetFloat("maxRadius", 0.8f);
        shaderComputeFloatingGlowyBits.SetFloat("time", Time.fixedTime);
        shaderComputeFloatingGlowyBits.SetBuffer(updateGlowyBitsKernelID, "floatingGlowyBitsCBuffer", floatingGlowyBitsCBuffer);
        shaderComputeFloatingGlowyBits.SetBuffer(updateGlowyBitsKernelID, "neuronInitDataCBuffer", neuronInitDataCBuffer);
        shaderComputeFloatingGlowyBits.SetBuffer(updateGlowyBitsKernelID, "neuronSimDataCBuffer", neuronSimDataCBuffer);
        shaderComputeFloatingGlowyBits.SetBuffer(updateGlowyBitsKernelID, "neuronFeedDataCBuffer", neuronFeedDataCBuffer);
        shaderComputeFloatingGlowyBits.Dispatch(updateGlowyBitsKernelID, numFloatingGlowyBits / 64, numNeurons, 1);

        argsCore[0] = 0; // set later by counter;// 3;  // 3 vertices to start
        argsCore[1] = 1;  // 1 instance/copy
        argsCoreCBuffer.SetData(argsCore);
        ComputeBuffer.CopyCount(appendTrianglesCoreCBuffer, argsCoreCBuffer, 0);
        argsCoreCBuffer.GetData(argsCore);
        //Debug.Log("triangle count " + args[0]);
        argsCables[0] = 0; // set later by counter;// 3;  // 3 vertices to start
        argsCables[1] = 1;  // 1 instance/copy
        argsCablesCBuffer.SetData(argsCables);
        ComputeBuffer.CopyCount(appendTrianglesCablesCBuffer, argsCablesCBuffer, 0);
        argsCablesCBuffer.GetData(argsCables);
        //Debug.Log("triangle count cables: " + argsCables[0]);
    }

    // create a random small genome brain to test
    private void CreateDummyBrain() 
    {
        // Neurons!
        tempNeuronList = new List<Neuron>();
        int numInputs = Random.Range(Mathf.RoundToInt((float)numNeurons * 0.2f), Mathf.RoundToInt((float)numNeurons * 0.8f));
        for (int i = 0; i < numNeurons; i++) {
            Neuron neuron = new Neuron();
            if(i < numInputs) {
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            else {
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            neuron.currentValue = new float[1];
            neuron.currentValue[0] = Random.Range(-2f, 2f);
            tempNeuronList.Add(neuron);
        }

        tempAxonList = new List<Axon>();
        // Axons:
        for (int i = 0; i < numInputs; i++) {
            for(int j = 0; j < numNeurons - numInputs; j++) {
                if (j + i * numInputs < numAxons) {
                    Axon axon = new Axon(i, numInputs + j, Random.Range(-1f, 1f));
                    tempAxonList.Add(axon);
                }                
            }
        }

        numAxons = tempAxonList.Count;
    }

    private void OnRenderObject() 
    {
        displayMaterialCables.SetPass(0);
        Graphics.DrawProceduralIndirectNow(MeshTopology.Points, argsCablesCBuffer, 0);  // not sure why at this used to work with Triangles but now requires Points....

        displayMaterialCore.SetPass(0);
        Graphics.DrawProceduralIndirectNow(MeshTopology.Points, argsCoreCBuffer, 0);  // not sure why at this used to work with Triangles but now requires Points....
        //Graphics.DrawProceduralIndirect(MeshTopology.Triangles, argsCBuffer, 0);

        floatingGlowyBitsMaterial.SetPass(0);
        //floatingGlowyBitsMaterial.SetBuffer("floatingGlowyBitsCBuffer", floatingGlowyBitsCBuffer);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, floatingGlowyBitsCBuffer.count);

        extraBallsMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, extraBallsCBuffer.count);
    }

    // Update is called once per frame
    void Update () 
    {
        UpdateBrainDataAndBuffers();
    }

    private void OnDestroy() 
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
