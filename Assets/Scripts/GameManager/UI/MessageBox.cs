using System;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;

[RequireComponent(typeof(CanvasGroupUI))]
public class MessageBox : MonoBehaviour
{
	[SerializeField, ReadOnly]
	TMP_Text textUI;
	[SerializeField, ReadOnly]
	CanvasGroupUI canvasGroupUI;

	public void Reset()
	{
		canvasGroupUI = GetComponent<CanvasGroupUI>();
		textUI = GetComponentInChildren<TMP_Text>(true);
	}
	public void OnShow(Action awaitCallback = null)
	{
		canvasGroupUI.ThisShowHide.OnShow(awaitCallback);
	}
	public void OnHide(Action awaitCallback = null)
	{
		canvasGroupUI.ThisShowHide.OnHide(awaitCallback);
	}
	public string Text
	{
		get { return textUI == null ? "" : textUI.text; }
		set { if(textUI != null) textUI.text = value; }
	}
}
