using UnityEngine;

public class RotateOnYAxis : MonoBehaviour 
{
	[SerializeField] float rotationRate = 0.5f;
	[SerializeField] float rotateZ = 150f;

	void Update()
	{
        transform.localRotation = Quaternion.Euler(0f, Time.fixedTime * rotationRate, rotateZ);
	}
}
