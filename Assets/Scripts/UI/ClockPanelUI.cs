using UnityEngine;
using UnityEngine.UI;

public class ClockPanelUI : MonoBehaviour
{
    public Text textCurYear;
    public ClockUI clockUI;
    
    SimulationManager simulation => SimulationManager.instance;

    public void Tick() {
        textCurYear.text = (simulation.curSimYear + 1).ToString();
        clockUI.UpdateClockUI(simulation.simAgeTimeSteps);
    }
}
