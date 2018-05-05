using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestControllerInput : MonoBehaviour {

    public Text outputText;
    public Slider horSlider;
    public Slider verSlider;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float horInput = Input.GetAxis("Horizontal");
        float verInput = Input.GetAxis("Vertical");

        horSlider.value = horInput;
        verSlider.value = verInput;

        outputText.text = "Hor: " + horInput.ToString("F3") + ",  Ver: " + verInput.ToString("F3");
	}
}
