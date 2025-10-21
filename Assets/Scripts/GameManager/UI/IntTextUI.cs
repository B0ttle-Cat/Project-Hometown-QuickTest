using TMPro;

using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class IntTextUI : MonoBehaviour
{
	TMP_Text text;

	[SerializeField]
	private bool showPlusSign;

	private void Init()
	{
		if (text != null) return;
		text = gameObject.GetComponent<TMP_Text>();
	}
	public void SetValue(float number)
	{
		Init();
		if (text == null) return;
		text.text = ToString(Mathf.RoundToInt(number));
	}
	public void SetValue(int number)
	{
		Init();
		if (text == null) return;
		text.text = ToString(number);
	}

	private string ToString(int number)
	{
		if(number<0)
		{
			return number.ToString();
		}
		else
		{
			return showPlusSign ? "+"+number.ToString() : number.ToString();
		}
	}
}
