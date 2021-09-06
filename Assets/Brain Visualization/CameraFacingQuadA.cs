using UnityEngine;

// Free-floating camera-facing quads
// * WPP: RENAME
public class CameraFacingQuadA : MonoBehaviour 
{
    [SerializeField] Material floatingGlowyBitsMaterial;
    [SerializeField] int floatingGlowyBitsCount = 64;
    [SerializeField] float radius = 2f;

    ComputeBuffer quadVerticesCBuffer;       // holds information for a 2-triangle Quad mesh (6 vertices)
    ComputeBuffer floatingGlowyBitsCBuffer;  // holds information for placement and attributes of each instance of quadVertices to draw

    void Start () 
    {
        CreateQuadBuffer();
        
        // At first, populate this on CPU....later, do so within a compute shader!!
        var initialGlowyBitsPositions = GetRandomPositions(floatingGlowyBitsCount, radius);
        InitializeComputeBuffer(initialGlowyBitsPositions);
        InitializeMaterial();
    }
    
    void CreateQuadBuffer()
    {
        quadVerticesCBuffer = new ComputeBuffer(6, sizeof(float) * 3);
        quadVerticesCBuffer.SetData(new[] {
            new Vector3(-0.5f, 0.5f),
            new Vector3(0.5f, 0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f)
        });
    }
    
    Vector3[] GetRandomPositions(int count, float radius)
    {
        Vector3[] result = new Vector3[count];  
        
        for (int i = 0; i < count; i++) {
            result[i] = Random.insideUnitSphere * radius;
        }
        
        return result;
    }
    
    void InitializeComputeBuffer(Vector3[] positions)
    {
        floatingGlowyBitsCBuffer = new ComputeBuffer(floatingGlowyBitsCount, sizeof(float) * 3);
        floatingGlowyBitsCBuffer.SetData(positions);
    }
    
    void InitializeMaterial()
    {
        floatingGlowyBitsMaterial.SetPass(0);
        floatingGlowyBitsMaterial.SetBuffer("floatingGlowyBitsCBuffer", floatingGlowyBitsCBuffer);
        floatingGlowyBitsMaterial.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);        
    }

    void OnRenderObject() 
    {        
        floatingGlowyBitsMaterial.SetPass(0);
        floatingGlowyBitsMaterial.SetBuffer("floatingGlowyBitsCBuffer", floatingGlowyBitsCBuffer);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, floatingGlowyBitsCBuffer.count);
    }

    void OnDestroy() 
    {        
        floatingGlowyBitsCBuffer?.Release();
        quadVerticesCBuffer?.Release();
    }
}
