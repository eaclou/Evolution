using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiffusionReactionScript : MonoBehaviour {

    public Shader blitShader;
    private Material blitMat;
    public Shader brushClickShader;
    //private Material brushClickMat;

    public Material displayMat;
    public float refreshDelay = 1.0f;
    //private float lastUpdateTime = 0f;

    public int resolution = 128;

    private RenderTexture mainRT;
    private RenderTexture tempRT;
    private Texture2D seedTexture;

    public float diffusionRateA = 1.0f;
    public float diffusionRateB = 0.5f;
    public float feedRate = 0.055f;
    public float killRate = 0.062f;

    public float minFeedRate = 0.005f;
    public float maxFeedRate = 0.06f;
    public float minKillRate = 0.04f;
    public float maxKillRate = 0.07f;

	// Use this for initialization
	/*void Start () {
        FirstTimeInit();
        
    }
	
	// Update is called once per frame
	void Update () {
        if (Time.time > lastUpdateTime + refreshDelay) {
            for(int i = 0; i < 12; i++) {
                Tick();
            }
            lastUpdateTime = Time.time;
        }
    }

    private void FixedUpdate() {
        brushClickMat.SetFloat("_PaintA", 0f);
        brushClickMat.SetFloat("_PaintB", 0f);

        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                Debug.Log("RayCast HIT!!! " + hit.textureCoord.ToString());

                brushClickMat.SetFloat("_CoordX", hit.textureCoord.x);
                brushClickMat.SetFloat("_CoordY", hit.textureCoord.y);
                brushClickMat.SetFloat("_PaintA", 1f);
                //brushClickMat.SetFloat("_PaintB", 0f);

                Graphics.Blit(mainRT, tempRT, brushClickMat, 0);
                Graphics.Blit(tempRT, mainRT);
            }
        }

        if (Input.GetMouseButton(1)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                Debug.Log("RayCast HIT!!! " + hit.textureCoord.ToString());

                brushClickMat.SetFloat("_CoordX", hit.textureCoord.x);
                brushClickMat.SetFloat("_CoordY", hit.textureCoord.y);
                //brushClickMat.SetFloat("_PaintA", 1f);
                brushClickMat.SetFloat("_PaintB", 1f);

                Graphics.Blit(mainRT, tempRT, brushClickMat, 0);
                Graphics.Blit(tempRT, mainRT);
            }
        }
    }*/

    private void Tick() {
        blitMat.SetFloat("_DiffusionRateA", diffusionRateA);
        blitMat.SetFloat("_DiffusionRateB", diffusionRateB);
        blitMat.SetFloat("_MinFeedRate", minFeedRate);
        blitMat.SetFloat("_MaxFeedRate", maxFeedRate);
        blitMat.SetFloat("_MinKillRate", minKillRate);        
        blitMat.SetFloat("_MaxKillRate", maxKillRate);

        Graphics.Blit(mainRT, tempRT, blitMat, 0);
        Graphics.Blit(tempRT, mainRT);
    }

    private void FirstTimeInit() {

        // Random Initial Conditions:
        seedTexture = new Texture2D(resolution, resolution, TextureFormat.RGBAFloat, false, true);
        for(int x = 0; x < resolution; x++) {
            for(int y = 0; y < resolution; y++) {
                float randVal = UnityEngine.Random.Range(0f, 0.05f);
                Color pixColor = new Color(1, randVal, 0f);
                seedTexture.SetPixel(x, y, pixColor); 
                
                if(x > 280 && x < 300) {
                    seedTexture.SetPixel(x, y, new Color(0f, 1f, 0f));
                }
                if (y > 400 && y < 600) {
                    seedTexture.SetPixel(x, y, new Color(0f, 1f, 0f));
                }
            }
        }        
        
        seedTexture.Apply();

        mainRT = new RenderTexture(resolution, resolution, 1, RenderTextureFormat.ARGBFloat);
        tempRT = new RenderTexture(resolution, resolution, 1, RenderTextureFormat.ARGBFloat);

        Graphics.Blit(seedTexture, mainRT);

        mainRT.filterMode = FilterMode.Point;
        mainRT.wrapMode = TextureWrapMode.Repeat;

        blitMat = new Material(blitShader);
        blitMat.SetFloat("_Resolution", resolution);

        //brushClickMat = new Material(brushClickShader);

        displayMat.SetTexture("_MainTex", mainRT);
    }
}

