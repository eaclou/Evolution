using UnityEngine;
using UnityEngine.UI;

public class CreaturePortraitPanel : MonoBehaviour
{
    CandidateAgentData candidate => UIManager.instance.selectionManager.focusedCandidate;
    
    [SerializeField] Text candidateName;
    [SerializeField] Image background;
    
    //[SerializeField] [Range(0, 1)] float backgroundHueModifier = .75f;
    [SerializeField] [Range(0, 1)] float nameHueModifier = 2f;
    
    string title;
    Vector3 hue;

    public void SetTitleText()
    {
        title = "<size=18>Critter</size> " + GetCreatureNameFromID(candidate.candidateID) + "<size=18>";
        
        if(candidate.isBeingEvaluated) 
            title += "\n(following)";
        else 
            title += candidate.numCompletedEvaluations > 0 ? "\nFossil" : "\nUnborn";
        
        title += "</size>";
        candidateName.text = title;
        
        SetColors(candidate.candidateGenome.bodyGenome.appearanceGenome);
    }

    private string GetCreatureNameFromID(int candID) {
        string name = "";
        string[] letters = new string[26];
        letters[0] = "A";
        letters[1] = "B";
        letters[2] = "C";
        letters[3] = "D";
        letters[4] = "E";
        letters[5] = "F";
        letters[6] = "G";
        letters[7] = "H";
        letters[8] = "I";
        letters[9] = "J";
        letters[10] = "K";
        letters[11] = "L";
        letters[12] = "M";
        letters[13] = "N";
        letters[14] = "O";
        letters[15] = "P";
        letters[16] = "Q";
        letters[17] = "R";
        letters[18] = "S";
        letters[19] = "T";
        letters[20] = "U";
        letters[21] = "V";
        letters[22] = "W";
        letters[23] = "X";
        letters[24] = "Y";
        letters[25] = "Z";

        if (candID < 0)
            return "-1";

        int onesColumn = candID % 26;
        name = letters[onesColumn] + name;
        if(candID > 26) {
            int tensColummn = Mathf.FloorToInt((float)candID / 26f) % 26;
            name = letters[tensColummn] + name;

            if(candID > 26 * 26) {
                int hundredsColummn = Mathf.FloorToInt((float)candID / (26f * 26f)) % 26;
                name = letters[hundredsColummn] + name;

                if (candID > 26 * 26 * 26) {
                    int thousandsColummn = Mathf.FloorToInt((float)candID / (26f * 26f * 26f)) % 26;
                    name = letters[thousandsColummn] + name;
                }
            }
        }

        return name;
    }
    
    public void SetColors(CritterModuleAppearanceGenome appearance)
    {
        hue = Vector3.Lerp(appearance.hueSecondary, Vector3.one, 0.5f);
        //hue = appearance.hueSecondary * nameHueModifier + Vector3.one * 0.5f;
        candidateName.color = new Color(hue.x, hue.y, hue.z);        
    }
}
