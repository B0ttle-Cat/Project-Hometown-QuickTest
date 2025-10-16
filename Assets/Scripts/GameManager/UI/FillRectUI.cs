using UnityEngine;
using UnityEngine.UI;

public class FillRectUI : MonoBehaviour
{
	[SerializeField, Range(0f,1f)]
	private float fillAmount;
	[SerializeField]
	private FillDiraction fillDiraction;

	[SerializeField]
	private RectMask2D fillRectMask2D;
	private RectTransform fillRect;

	public enum FillDiraction
	{
		LeftToRight,
		RightToLeft,
		TopToBottom,
		BottomToTop,
	}

    public void Reset()
    {
		fillAmount = 0.5f;
		fillDiraction = FillDiraction.LeftToRight;
		FillUpdate();
	}
    public void OnValidate()
	{
		FillUpdate();
	}

	public float Value
	{
		get => Mathf.Clamp01(fillAmount);
		set { fillAmount = value; FillUpdate(); }
	}

    public FillDiraction Diraction
	{
		get => fillDiraction;
		set { fillDiraction = value; FillUpdate(); }
	}

    private void Init()
	{
		if(fillRectMask2D == null)
		{
			fillRectMask2D = GetComponentInChildren<RectMask2D>();
		}
		if (fillRectMask2D != null && fillRect == null)
		{
			fillRect = fillRectMask2D.GetComponent<RectTransform>();
			fillRect.anchorMin = Vector2.zero;
			fillRect.anchorMax = Vector2.one;
			fillRect.anchoredPosition = Vector2.zero;
			fillRect.sizeDelta = Vector2.zero;
			fillRect.pivot = Vector2.one * 0.5f;
		}
	}
	public void FillUpdate()
	{
		Init();
		if (fillRectMask2D == null || fillRect == null) return;

		/// <summary>
		/// Padding to be applied to the masking
		/// X = Left
		/// Y = Bottom
		/// Z = Right
		/// W = Top
		/// </summary>
		switch (Diraction)
		{
			case FillDiraction.LeftToRight:
			{
				float length = fillRect.rect.width;
				float fill = length * (1f-Value);
				fillRectMask2D.padding = new Vector4(0f,0f, fill, 0f);
			} break;
			case FillDiraction.RightToLeft:
			{
				float length = fillRect.rect.width;
				float fill = length * (1f-Value);
				fillRectMask2D.padding = new Vector4(fill, 0f, 0f, 0f);
			}
			break;
			case FillDiraction.TopToBottom:
			{
				float length = fillRect.rect.height;
				float fill = length * (1f-Value);
				fillRectMask2D.padding = new Vector4(0f, fill, 0f, 0f);
			}
			break;
			case FillDiraction.BottomToTop:
			{
				float length = fillRect.rect.height;
				float fill = length * (1f-Value);
				fillRectMask2D.padding = new Vector4(0f, 0f, 0f, fill);
			}
			break;
		}
	}
}
