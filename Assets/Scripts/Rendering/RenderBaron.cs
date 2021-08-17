using UnityEngine;
using UnityEngine.Rendering;

public class RenderBaron : MonoBehaviour 
{
	public virtual void Initialize() { }
	public virtual void Tick(RenderTexture maskTex) { }
	public virtual void RenderCommands(ref CommandBuffer cmdBuffer, int frameBufferID) { }
	public virtual void Cleanup() { }
}
