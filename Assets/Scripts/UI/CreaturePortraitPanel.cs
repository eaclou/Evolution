using UnityEngine;
using UnityEngine.UI;

public class CreaturePortraitPanel : MonoBehaviour
{
    SelectionManager selectionManager => SelectionManager.instance;
    CandidateAgentData candidate => selectionManager.currentSelection.candidate;
    
    [SerializeField] Text candidateName;
    [SerializeField] Image background;
    
    //[SerializeField] [Range(0, 1)] float backgroundHueModifier = .75f;
    [SerializeField] [Range(0, 1)] float nameHueModifier = 2f;
    
    string title;
    Vector3 hue;

    public void SetTitleText()
    {
        title = candidate.candidateGenome.name + "-" + candidate.candidateID;
        candidateName.text = title;
        
        SetColors(candidate.candidateGenome.bodyGenome.appearanceGenome);
    }
    
    public void SetColors(CritterModuleAppearanceGenome appearance)
    {
        hue = Vector3.Lerp(appearance.hueSecondary, Vector3.one, 0.5f);
        //hue = appearance.hueSecondary * nameHueModifier + Vector3.one * 0.5f;
        candidateName.color = new Color(hue.x, hue.y, hue.z);        
    }
}
