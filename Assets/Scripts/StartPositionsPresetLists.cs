using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPositionsPresetLists : MonoBehaviour {

    public List<GameObject> foodSpawnPositionsList;

    public StartPositionGenome[] foodStartGenomesArray;

	// Use this for initialization
	void Start () {
		if(foodSpawnPositionsList != null) {
            foodStartGenomesArray = new StartPositionGenome[foodSpawnPositionsList.Count];
            for(int i = 0; i < foodSpawnPositionsList.Count; i++) {
                Vector3 parentForward = foodSpawnPositionsList[i].transform.up;
                Vector3 startPos = new Vector3(foodSpawnPositionsList[i].transform.position.x, foodSpawnPositionsList[i].transform.position.y, 0f) + parentForward * 0.5f;
                StartPositionGenome startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
                foodStartGenomesArray[i] = startPosGenome;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
