using UnityEngine;
using UnityEngine.UI;

public class ClockPanelUI : MonoBehaviour
{
    public Text textCurYear;

    public Image imageClockHandA;
	public Image imageClockHandB;
    public Image imageClockHandC;
    
    SimulationManager simulation => SimulationManager.instance;

    public void Tick() {
        textCurYear.text = (simulation.curSimYear + 1).ToString();

        int numTicks = simulation.simAgeTimeSteps;
        float angVelA = -2.25f;
        float angVelB = -0.25f;
        float angVelC = -0.002f;
        imageClockHandA.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, (float)numTicks * angVelA);
        imageClockHandB.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, (float)numTicks * angVelB);
        imageClockHandC.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, (float)numTicks * angVelC);
	
    }
}
