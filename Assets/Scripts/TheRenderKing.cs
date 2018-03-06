using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheRenderKing : MonoBehaviour {

    // SET IN INSPECTOR!!!::::
    public Camera mainCam;
    public Material pointStrokeDisplayMat;

    // Source Data:::
    public Agent[] agentsArray;
    public Agent playerAgent;

    // AGENT LAYERS:
    // Primer:      -- The backdrop for agent, provides minimum silhouette and bg color/shape
    // Body:
    // Decorations:

    PointStrokeData[] pointStrokeDataArray;

    //public Vector3[] agentPositionsArray;
    public AgentSimData[] agentSimDataArray;
    private ComputeBuffer quadVerticesCBuffer;
    private ComputeBuffer agentPointStrokesCBuffer;
    private ComputeBuffer agentSimDataCBuffer;

    public struct PointStrokeData {
        public int parentIndex;  // what agent/object is this attached to?
        public Vector2 localScale;
        public Vector2 localPos;
        public Vector2 localDir;
        public Vector3 hue;   // RGB color tint
        public float strength;  // abstraction for pressure of brushstroke + amount of paint 
        public int brushType;  // what texture/mask/brush pattern to use
    }

    public struct AgentSimData {
        public Vector2 worldPos;
        public Vector2 velocity;
        public Vector2 heading;
    }

    public struct CurveStrokeData {
        public int parentIndex;
    }

    // Use this for initialization
    void Start () {

        // Holds info on Agent Positions and current status:
        agentSimDataArray = new AgentSimData[65];
        for (int i = 0; i < agentSimDataArray.Length; i++) {
            agentSimDataArray[i] = new AgentSimData();
        }
        agentSimDataCBuffer = new ComputeBuffer(agentSimDataArray.Length, sizeof(float) * 6);

        // Set up Quad Mesh billboard for brushStroke rendering
        quadVerticesCBuffer = new ComputeBuffer(6, sizeof(float) * 3);
        quadVerticesCBuffer.SetData(new[] {
            new Vector3(-0.5f, 0.5f),
            new Vector3(0.5f, 0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f)
        });

        pointStrokeDisplayMat.SetPass(0);
        pointStrokeDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
        pointStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);

        //SetPointStrokesBuffer();  // Create and send brushStroke info to GPU

        
    }

    public void Tick() {
        if (agentsArray != null) {
            SetSimDataArrays();
        }
    }

    private void OnRenderObject() {
        
        if (Camera.current == mainCam) {

            pointStrokeDisplayMat.SetPass(0);
            pointStrokeDisplayMat.SetBuffer("agentSimDataCBuffer", agentSimDataCBuffer);
            pointStrokeDisplayMat.SetBuffer("pointStrokesCBuffer", agentPointStrokesCBuffer);
            pointStrokeDisplayMat.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, agentPointStrokesCBuffer.count);
        }
    }

    public void SetPointStrokesBuffer() {

        // Populate Point strokes!!!::::::
        // Main Body Brush:::::
        /*for(int i = 0; i < agentsArray.Length; i++) {        
            pointStrokeDataArray[i] = GeneratePointStrokeData(i, Vector2.one, Vector2.zero, new Vector2(0f, 1f), agentsArray[i].hue);
        }
        pointStrokeDataArray[agentsArray.Length] = GeneratePointStrokeData(agentsArray.Length, Vector2.one, Vector2.zero, new Vector2(0f, 1f), playerAgent.hue);

        // Decorations Brushstrokes::::
        float minScale = 0.2f;
        float maxScale = 0.5f;
        for (int i = 0; i < agentsArray.Length; i++) {
            UnityEngine.Random.InitState(agentsArray[i].randomColorSeed);
            
            Vector2 scale = new Vector2(UnityEngine.Random.Range(minScale, maxScale), UnityEngine.Random.Range(minScale, maxScale));
            Vector2 pos = UnityEngine.Random.insideUnitCircle;
            Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
            Vector3 col = UnityEngine.Random.insideUnitSphere * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);

            pointStrokeDataArray[i + agentsArray.Length + 1] = GeneratePointStrokeData(i, scale, pos, dir, col);           
        }
        // PLAYER:
        UnityEngine.Random.InitState(playerAgent.randomColorSeed);
        Vector2 scale0 = new Vector2(UnityEngine.Random.Range(minScale, maxScale), UnityEngine.Random.Range(minScale, maxScale));
        Vector2 pos0 = UnityEngine.Random.insideUnitCircle;
        Vector2 dir0 = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 col0 = UnityEngine.Random.insideUnitSphere * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);

        pointStrokeDataArray[agentsArray.Length + agentsArray.Length + 1] = GeneratePointStrokeData(agentsArray.Length, scale0, pos0, dir0, col0);
        */

        int numDecorationStrokes = 8;
        int numPointStrokes = (65) * (1 + numDecorationStrokes);

        if (agentPointStrokesCBuffer == null) {
            agentPointStrokesCBuffer = new ComputeBuffer(numPointStrokes, sizeof(int) * 2 + sizeof(float) * 10);
        }
        if (pointStrokeDataArray == null) {
            pointStrokeDataArray = new PointStrokeData[agentPointStrokesCBuffer.count];
        }

        for (int i = 0; i < agentsArray.Length; i++) {
            int baseIndex = i * 9;

            //pointStrokeDataArray[baseIndex] = agentsArray[i].bodyPointStroke;
            pointStrokeDataArray[baseIndex].strength = agentsArray[i].bodyPointStroke.strength;
            pointStrokeDataArray[baseIndex].parentIndex = agentsArray[i].bodyPointStroke.parentIndex;
            pointStrokeDataArray[baseIndex].localScale = agentsArray[i].bodyPointStroke.localScale;
            pointStrokeDataArray[baseIndex].localPos = agentsArray[i].bodyPointStroke.localPos;
            pointStrokeDataArray[baseIndex].localDir = agentsArray[i].bodyPointStroke.localDir;
            pointStrokeDataArray[baseIndex].hue = agentsArray[i].bodyPointStroke.hue;

            for (int j = 0; j < 8; j++) {
                int index = baseIndex + j + 1;

                pointStrokeDataArray[index].strength = agentsArray[i].decorationPointStrokesArray[j].strength;
                pointStrokeDataArray[index].parentIndex = agentsArray[i].decorationPointStrokesArray[j].parentIndex;
                pointStrokeDataArray[index].localScale = agentsArray[i].decorationPointStrokesArray[j].localScale;
                pointStrokeDataArray[index].localPos = agentsArray[i].decorationPointStrokesArray[j].localPos;
                pointStrokeDataArray[index].localDir = agentsArray[i].decorationPointStrokesArray[j].localDir;
                pointStrokeDataArray[index].hue = agentsArray[i].decorationPointStrokesArray[j].hue;
            }            
        }
        // Player:
        int playerBaseIndex = agentsArray.Length * 9;
        //pointStrokeDataArray[playerBaseIndex] = playerAgent.bodyPointStroke;
        pointStrokeDataArray[playerBaseIndex].strength = playerAgent.bodyPointStroke.strength;
        pointStrokeDataArray[playerBaseIndex].parentIndex = playerAgent.bodyPointStroke.parentIndex;
        pointStrokeDataArray[playerBaseIndex].localScale = playerAgent.bodyPointStroke.localScale;
        pointStrokeDataArray[playerBaseIndex].localPos = playerAgent.bodyPointStroke.localPos;
        pointStrokeDataArray[playerBaseIndex].localDir = playerAgent.bodyPointStroke.localDir;
        pointStrokeDataArray[playerBaseIndex].hue = playerAgent.bodyPointStroke.hue;
        for (int k = 0; k < 8; k++) {
            int playerIndex = playerBaseIndex + k + 1;

            //pointStrokeDataArray[playerIndex] = playerAgent.decorationPointStrokesArray[k];

            pointStrokeDataArray[playerIndex].strength = playerAgent.decorationPointStrokesArray[k].strength;
            pointStrokeDataArray[playerIndex].parentIndex = playerAgent.decorationPointStrokesArray[k].parentIndex;
            pointStrokeDataArray[playerIndex].localScale = playerAgent.decorationPointStrokesArray[k].localScale;
            pointStrokeDataArray[playerIndex].localPos = playerAgent.decorationPointStrokesArray[k].localPos;
            pointStrokeDataArray[playerIndex].localDir = playerAgent.decorationPointStrokesArray[k].localDir;
            pointStrokeDataArray[playerIndex].hue = playerAgent.decorationPointStrokesArray[k].hue;
        }

        agentPointStrokesCBuffer.SetData(pointStrokeDataArray);
    }

    public PointStrokeData GeneratePointStrokeData(int index, Vector2 size, Vector2 pos, Vector2 dir, Vector3 hue, float str) {
        PointStrokeData pointStroke = new PointStrokeData();
        pointStroke.parentIndex = index;
        pointStroke.localScale = size;
        pointStroke.localPos = pos;
        pointStroke.localDir = dir;
        pointStroke.hue = hue;
        pointStroke.strength = str; // temporarily used to lerp btw primary & secondary Agent Hues

        return pointStroke;
    }

    public void SetSimDataArrays() {

        for (int i = 0; i < agentSimDataArray.Length - 1; i++) {
            agentSimDataArray[i].worldPos = new Vector2(agentsArray[i].transform.position.x, agentsArray[i].transform.position.y);
            agentSimDataArray[i].velocity = agentsArray[i].smoothedThrottle; // new Vector2(agentsArray[i].testModule.ownRigidBody2D.velocity.x, agentsArray[i].testModule.ownRigidBody2D.velocity.y);
            agentSimDataArray[i].heading = agentsArray[i].facingDirection; // new Vector2(0f, 1f); // Update later -- store inside Agent class?            
        } // Player:
        agentSimDataArray[agentSimDataArray.Length - 1].worldPos = new Vector2(playerAgent.transform.position.x, playerAgent.transform.position.y);
        agentSimDataArray[agentSimDataArray.Length - 1].velocity = playerAgent.smoothedThrottle;
        agentSimDataArray[agentSimDataArray.Length - 1].heading = playerAgent.facingDirection;
        agentSimDataCBuffer.SetData(agentSimDataArray);        
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void OnDisable() {
        if(agentPointStrokesCBuffer != null) {
            agentPointStrokesCBuffer.Release();
        }
        if (quadVerticesCBuffer != null) {
            quadVerticesCBuffer.Release();
        }
        if (agentSimDataCBuffer != null) {
            agentSimDataCBuffer.Release();
        }
    }
}
