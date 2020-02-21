using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderBaron : MonoBehaviour {

	public virtual void Initialize(TheRenderKing renderRef) {

    }
    public virtual void Initialize() {

    }

    public virtual void Tick(RenderTexture maskTex) {

    }

    public virtual void RenderCommands(ref CommandBuffer cmdBuffer, int frameBufferID) {

    }

    public virtual void Cleanup() {

    }
}
