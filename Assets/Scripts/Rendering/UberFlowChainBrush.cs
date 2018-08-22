using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UberFlowChainBrush {

    private int numChains = 1024 * 4;
    private int numLinksPerChain = 32;
    public ComputeBuffer chains0CBuffer;
    private ComputeBuffer chains1CBuffer;
    private ComputeBuffer quadVerticesCBuffer;  // quad mesh

    private ComputeShader computeShader;
    public Material renderMat;
    
    public struct ChainLinkData {
        public Vector2 worldPos;
    }

    public UberFlowChainBrush() {

    }

    public void Initialize(ComputeShader computeShader, Material renderMat) {
        this.renderMat = renderMat;
        this.computeShader = computeShader;

        quadVerticesCBuffer = new ComputeBuffer(6, sizeof(float) * 3);
        quadVerticesCBuffer.SetData(new[] {
            new Vector3(-0.5f, 0.5f),
            new Vector3(0.5f, 0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f)
        });

        ChainLinkData[] chainDataArray = new ChainLinkData[numChains * numLinksPerChain];
        for (int i = 0; i < chainDataArray.Length; i++) {
            //int agentIndex = (int)Mathf.Floor((float)i / numLinksPerChain);
            float trailPos = (float)i % (float)numLinksPerChain;
            Vector2 randPos = new Vector2(UnityEngine.Random.Range(-60f, 60f), UnityEngine.Random.Range(-60f, 60f));
            chainDataArray[i] = new ChainLinkData();
            chainDataArray[i].worldPos = randPos + new Vector2(0f, trailPos * -1f);
        }
        chains0CBuffer = new ComputeBuffer(chainDataArray.Length, sizeof(float) * 2);
        chains0CBuffer.SetData(chainDataArray);
        chains1CBuffer = new ComputeBuffer(chainDataArray.Length, sizeof(float) * 2);

        this.renderMat.SetPass(0);
        this.renderMat.SetBuffer("verticesCBuffer", quadVerticesCBuffer);
        this.renderMat.SetBuffer("chainsReadCBuffer", chains0CBuffer);
        this.renderMat.SetFloat("_MapSize", SimulationManager._MapSize);
        
    }

    public void Tick(RenderTexture velocityFlowTex) {
        // Set position of trail Roots:
        int kernelCSPinChainsData = computeShader.FindKernel("CSPinChainsData");        
        computeShader.SetBuffer(kernelCSPinChainsData, "chainsReadCBuffer", chains0CBuffer);
        computeShader.SetBuffer(kernelCSPinChainsData, "chainsWriteCBuffer", chains1CBuffer);
        computeShader.SetTexture(kernelCSPinChainsData, "velocityRead", velocityFlowTex);
        computeShader.Dispatch(kernelCSPinChainsData, chains0CBuffer.count / numLinksPerChain / 1024, 1, 1);        
        
        // Shift positions:::
        int kernelCSShiftChainsData = computeShader.FindKernel("CSShiftChainsData");
        computeShader.SetBuffer(kernelCSShiftChainsData, "chainsReadCBuffer", chains0CBuffer);
        computeShader.SetBuffer(kernelCSShiftChainsData, "chainsWriteCBuffer", chains1CBuffer);
        computeShader.SetTexture(kernelCSShiftChainsData, "velocityRead", velocityFlowTex);
        computeShader.Dispatch(kernelCSShiftChainsData, chains0CBuffer.count / 1024, 1, 1);              
        
        // Copy back to buffer1:::        
        int kernelCSSwapChainsData = computeShader.FindKernel("CSSwapChainsData");
        computeShader.SetBuffer(kernelCSSwapChainsData, "chainsReadCBuffer", chains1CBuffer);
        computeShader.SetBuffer(kernelCSSwapChainsData, "chainsWriteCBuffer", chains0CBuffer);
        computeShader.SetTexture(kernelCSSwapChainsData, "velocityRead", velocityFlowTex);
        computeShader.Dispatch(kernelCSSwapChainsData, chains0CBuffer.count / 1024, 1, 1);
    }

    public void RenderSetup(RenderTexture sourceColorTex) {
        renderMat.SetPass(0);
        renderMat.SetBuffer("verticesCBuffer", quadVerticesCBuffer);
        renderMat.SetBuffer("chainsReadCBuffer", chains0CBuffer);
        renderMat.SetTexture("_SourceColorTex", sourceColorTex); 
        
        //ChainLinkData[] chainDataArray = new ChainLinkData[numChains * numLinksPerChain];
        //chains0CBuffer.GetData(chainDataArray);
        //Debug.Log(chainDataArray[0].worldPos.ToString());
    }

    public void CleanUp() {
        if (chains0CBuffer != null) {
            chains0CBuffer.Release();
        }
        if (chains1CBuffer != null) {
            chains1CBuffer.Release();
        }
        if (quadVerticesCBuffer != null) {
            quadVerticesCBuffer.Release();
        }
    }
}
