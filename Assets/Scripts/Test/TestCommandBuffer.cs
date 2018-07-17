using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TestCommandBuffer : MonoBehaviour {

    public Camera camera;
    public CommandBuffer cmdBuffer;

	// Use this for initialization
	void Start () {
		cmdBuffer = new CommandBuffer();
        cmdBuffer.name = "cmdBuffer";
        camera.AddCommandBuffer(CameraEvent.AfterEverything, cmdBuffer);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnWillRenderObject() {  // requires MeshRenderer Component to be called
        cmdBuffer.Clear();
        //mainRenderCam
        RenderTargetIdentifier renderTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        cmdBuffer.SetRenderTarget(renderTarget);  // Set render Target
        cmdBuffer.ClearRenderTarget(true, true, Color.cyan, 1.0f);  // clear -- needed???
    }

    private void OnDisable() {
        if(camera != null) {
            camera.RemoveAllCommandBuffers();
        }

        if(cmdBuffer != null) {
            cmdBuffer.Release();
        }
        
    }
}
