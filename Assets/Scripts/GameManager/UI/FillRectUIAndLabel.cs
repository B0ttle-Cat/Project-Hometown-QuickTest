using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;

public class FillRectUIAndLabel : MonoBehaviour
{
	[SerializeField, InlineEditor]
	private TMP_Text label;
	[SerializeField, InlineEditor]
	private FillRectUI fillRectUI;
    public void Reset()
	{
		label = GetComponentInChildren<TMP_Text>();
		fillRectUI = GetComponentInChildren<FillRectUI>();
	}
	private TMP_Text _label
	{
		get
		{
			if (label == null)
			{
				label = GetComponentInChildren<TMP_Text>();
			}
			return label;
		}
	}
	private FillRectUI _fillRectUI
	{
		get
		{
			if (fillRectUI == null)
			{
				fillRectUI = GetComponentInChildren<FillRectUI>();
			}
			return fillRectUI;
		}
	}
	public float Value
	{
		get => _fillRectUI.Value;
		set => _fillRectUI.SetValue(value);
	}
	public string Text
	{
		get => _fillRectUI.Text;
		set => _fillRectUI.Text = value;
	}
	public string Label
	{
		get => _label.text;
		set => _label.text = value;
	}
	public void SetValueText(float value, string text)  => _fillRectUI.SetValueText(value,text);
	public void FillUpdate() => _fillRectUI.FillUpdate();
}
