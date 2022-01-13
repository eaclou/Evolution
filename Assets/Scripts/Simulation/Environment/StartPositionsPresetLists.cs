using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPositionsPresetLists : MonoBehaviour 
{
    public List<SpawnZone> spawnZonesList;
    public StartPositionGenome[] foodStartGenomesArray;

	void Start () 
	{
		if (spawnZonesList == null) return;
		
        foodStartGenomesArray = new StartPositionGenome[spawnZonesList.Count];
        
        for(int i = 0; i < spawnZonesList.Count; i++) 
        {
            Vector3 parentForward = spawnZonesList[i].transform.up;
            Vector3 startPos = new Vector3(spawnZonesList[i].transform.position.x, spawnZonesList[i].transform.position.y, 0f) + parentForward * 0.5f;
            StartPositionGenome startPosGenome = new StartPositionGenome(startPos, Quaternion.identity);
            foodStartGenomesArray[i] = startPosGenome;
        }
	}
}
