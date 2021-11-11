using UnityEngine;
using UnityEngine.UI;

public class CreaturePortraitPanel : MonoBehaviour
{
    SelectionManager selectionManager => SelectionManager.instance;
    CandidateAgentData candidate => selectionManager.focusedCandidate;
    
    [SerializeField] Text candidateName;
    [SerializeField] Image background;
    
    //[SerializeField] [Range(0, 1)] float backgroundHueModifier = .75f;
    [SerializeField] [Range(0, 1)] float nameHueModifier = 2f;
    
    string title;
    Vector3 hue;

    public void SetTitleText()
    {
        title = "<size=18>Critter</size> " + candidate.name + "<size=18>";
        
        if(candidate.isBeingEvaluated) 
            title += "\n(following)";
        else 
            title += candidate.numCompletedEvaluations > 0 ? "\nFossil" : "\nUnborn";
        
        title += "</size>";
        candidateName.text = title;
        
        if (candidate.candidateGenome?.bodyGenome?.appearanceGenome != null)
            SetColors(candidate.candidateGenome.bodyGenome.appearanceGenome);
    }

    
    public void SetColors(CritterModuleAppearanceGenome appearance)
    {
        hue = Vector3.Lerp(appearance.hueSecondary, Vector3.one, 0.5f);
        //hue = appearance.hueSecondary * nameHueModifier + Vector3.one * 0.5f;
        candidateName.color = new Color(hue.x, hue.y, hue.z);        
    }
}
