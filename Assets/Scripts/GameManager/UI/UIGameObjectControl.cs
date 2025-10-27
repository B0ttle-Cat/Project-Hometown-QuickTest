using UnityEngine;

public class UIGameObjectControl : MonoBehaviour
{
	public void SetActive(bool value)
	{
		gameObject.SetActive(value);
	}
	public void SetDeactive(bool value)
	{
		gameObject.SetActive(!value);
	}
}
