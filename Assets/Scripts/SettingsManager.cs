using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour {

    public MutationSettings mutationSettingsSupervised;
    public MutationSettings mutationSettingsPersistent;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Initialize() {
        mutationSettingsSupervised = new MutationSettings(0.015f, 0.6f, 0.005f, 1f, 0.1f, 0.001f);
        mutationSettingsPersistent = new MutationSettings(0.01f, 0.5f, 0.00001f, 0.9999f, 0.01f, 0.001f);
    }
}
