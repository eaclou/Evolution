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
        mutationSettingsSupervised = new MutationSettings(0.5f, 0.015f, 0.6f, 0.005f, 1f, 0.1f, 0.001f);
        mutationSettingsPersistent = new MutationSettings(0.25f, 0.02f, 0.5f, 0.0f, 0.999f, 0.0f, 0.0f);
    }
}
