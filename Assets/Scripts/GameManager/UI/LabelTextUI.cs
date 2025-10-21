using TMPro;

using UnityEngine;

public class LabelTextUI : MonoBehaviour
{
	[SerializeField]
	private TMP_Text label;
	[SerializeField]
	private TMP_Text text;

	public void Reset()
	{
		Init();
	}
	public void Init()
	{
		if (text != null) return;

		TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
		if (texts.Length == 1)
		{
			label = null;
			text = texts[0];
		}
		else if (texts.Length > 1)
		{
			label = texts[0];
			text = texts[1];
		}
	}

	public void SetText(string label, string text)
	{
		Init();

		if (this.label != null)
			this.label.text = label;
		if(this.text != null)
			this.text.text = text;
	}
	public void SetText(string text)
	{
		Init();

		if (this.text != null)
			this.text.text = text;
	}
}
