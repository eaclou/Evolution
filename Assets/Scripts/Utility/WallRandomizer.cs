using UnityEngine;

public class WallRandomizer : MonoBehaviour {

    public Mesh[] meshPool;
    
	void Start () {
        transform.rotation = Random.rotation;

        if(meshPool != null) {
            Mesh randMesh = meshPool[Random.Range(0, meshPool.Length)];
            GetComponent<MeshFilter>().sharedMesh = randMesh;
        }
	}
}
