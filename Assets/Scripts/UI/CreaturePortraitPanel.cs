using UnityEngine;
using UnityEngine.UI;

public class CreaturePortraitPanel : MonoBehaviour
{
    CandidateAgentData candidate => UIManager.instance.focusedCandidate;
    
    [SerializeField] Text candidateName;
    [SerializeField] Image background;
    
    [SerializeField] [Range(0, 1)] float backgroundHueModifier = .75f;
    [SerializeField] [Range(0, 1)] float nameHueModifier = .25f;
    
    string title;
    Vector3 hue;

    public void SetTitleText()
    {
        title = "<size=18>Critter</size> " + candidate.candidateID + "<size=18>";
        
        if(candidate.isBeingEvaluated) 
            title += "\n(following)";
        else 
            title += candidate.numCompletedEvaluations > 0 ? "\nFossil" : "\nUnborn";
        
        title += "</size>";
        candidateName.text = title;
        
        SetColors(candidate.candidateGenome.bodyGenome.appearanceGenome);
    }
    
    public void SetColors(CritterModuleAppearanceGenome appearance)
    {
        hue = appearance.huePrimary * backgroundHueModifier;
        background.color = new Color(hue.x, hue.y, hue.z);
        
        hue = appearance.hueSecondary * nameHueModifier;
        candidateName.color = new Color(hue.x, hue.y, hue.z);        
    }
}
