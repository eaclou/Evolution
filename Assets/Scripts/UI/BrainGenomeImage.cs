using UnityEngine;

public class BrainGenomeImage : MonoBehaviour
{
    [SerializeField] Material mat;
    [SerializeField] string mainTexture = "_MainTex";

    // Barcode
    Texture2D texture;
    
    const int WIDTH = 256;

    void Start() 
    {
        texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        mat.SetTexture(mainTexture, texture);
    }
    
    // * WPP: expose values in editor
    public void SetTexture(BrainGenome brain) 
    {
        int width = Mathf.Min(WIDTH, brain.linkCount);
        texture.Resize(width, 1);

        for (int x = 0; x < width; x++) 
        {                              
            Color testColor;
            Vector3 huePri = SelectionManager.instance.currentSelection.candidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
            Vector3 hueSec = SelectionManager.instance.currentSelection.candidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;
            //image.color = value ?  : new Color(hueSec.x, hueSec.y, hueSec.z);

            if (brain.linkCount > x) 
            {
                float weightVal = brain.links[x].weight;
                testColor = new Color(Mathf.Lerp(huePri.x, hueSec.x, weightVal * 0.5f + 0.5f), Mathf.Lerp(huePri.y, hueSec.y, weightVal * 0.5f + 0.5f), Mathf.Lerp(huePri.z, hueSec.z, weightVal * 0.5f + 0.5f));
                //testColor = 
                /*if(weightVal < -0.25f) 
                {
                    testColor = Color.Lerp(testColor, Color.black, 0.15f);
                }
                else if(weightVal > 0.25f) 
                {
                    testColor = Color.Lerp(testColor, Color.white, 0.15f);
                }
                else 
                {
                    testColor = Color.Lerp(testColor, Color.gray, 0.15f);
                }*/
            }
            else 
            {
                testColor = Color.black; // CLEAR
            }
                
            texture.SetPixel(x, 0, testColor);
        }
        
        texture.Apply();
    }
}
