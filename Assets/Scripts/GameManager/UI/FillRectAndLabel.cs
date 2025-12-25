using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;

public class FillRectAndLabel : MonoBehaviour
{
	[SerializeField, InlineEditor]
	private TMP_Text label;
	[SerializeField, InlineEditor]
	private FillRectUI_Old fillRectUI;

#if UNITY_EDITOR
	[ShowInInspector, HideInPlayMode, HorizontalGroup(order: -1), ToggleLeft]
	private bool showLabel {
		get => label == null ? false : label.gameObject.activeInHierarchy;
		set { if (value) OnShowLabel(); else OnHideLabel(); } 
	}
	[ShowInInspector, HideInPlayMode, HorizontalGroup, ToggleLeft]
	private bool showFillRect
	{
		get => fillRectUI == null ? false : fillRectUI.gameObject.activeInHierarchy;
		set { if (value) OnShowFillRect(); else OnHideFillRect(); }
	}
#endif
	public void Reset()
	{
		label = GetComponentInChildren<TMP_Text>(true);
		fillRectUI = GetComponentInChildren<FillRectUI_Old>(true);
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
	private FillRectUI_Old _fillRectUI
	{
		get
		{
			if (fillRectUI == null)
			{
				fillRectUI = GetComponentInChildren<FillRectUI_Old>();
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

	public void OnShowLabel()
	{
		if (label == null) return;
		var target = label.transform;

		if (target.parent != null && target.parent != transform)
		{
			target.parent.gameObject.SetActive(true);
		}
		else
		{
			target.gameObject.SetActive(true);
		}
	}
	public void OnHideLabel()
	{
		if (label == null) return;
		var target = label.transform;

		if (target.parent != null && target.parent != transform)
		{
			target.parent.gameObject.SetActive(false);
		}
		else
		{
			target.gameObject.SetActive(false);
		}
	}

	public void OnShowFillRect()
	{
		if (fillRectUI == null) return;
		var target = fillRectUI.transform;

		if (target.parent != null && target.parent != transform)
		{
			target.parent.gameObject.SetActive(true);
		}
		else
		{
			target.gameObject.SetActive(true);
		}
	}
	public void OnHideFillRect()
	{
		if (fillRectUI == null) return;
		var labelTr = fillRectUI.transform;

		if (labelTr.parent != null && labelTr.parent != transform)
		{
			labelTr.parent.gameObject.SetActive(false);
		}
		else
		{
			labelTr.gameObject.SetActive(false);
		}
	}
}
