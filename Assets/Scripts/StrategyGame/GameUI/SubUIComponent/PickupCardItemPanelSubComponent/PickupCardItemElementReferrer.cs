using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PickupCardItemElementReferrer : MonoBehaviour
{

	[SerializeField] private TMP_Text titleText;
	[SerializeField] private Image mainImage;
	[Space]
	[SerializeField] private Button plusButton;
	[SerializeField] private Button minusButton;
	[Space]
	[SerializeField] private RectTransform countTextRect;
	[SerializeField, Indent(1)] private TMP_Text countText;
	[SerializeField] private RectTransform costTextRect;
	[SerializeField, Indent(1), InlineButton("ResetTextFormat")] private TMP_Text costText;

	[SerializeField] private EventTrigger cardEventTrigger;

	public event UnityAction OnPlusButtonClick;
	public event UnityAction OnMinusButtonClick;

	public event UnityAction<BaseEventData> OnPointEnterInCard;
	public event UnityAction<BaseEventData> OnPointExitInCard;

	[SerializeField, TextArea]
	private string costPersonnelTextFormat = "<sprite=\"icon_resource\" index=0 color=#000000>{0}";
	[SerializeField, TextArea]
	private string costMaterialTextFormat = "<sprite=\"icon_resource\" index=1 color=#000000>{0}";
	[SerializeField, TextArea]
	private string costElectricTextFormat = "<sprite=\"icon_resource\" index=2 color=#000000>{0}";

#if UNITY_EDITOR
	private void ResetTextFormat()
	{
		costPersonnelTextFormat = @"<sprite=""icon_resource"" index=0 color=#000000>{0}";
		costMaterialTextFormat = @"<sprite=""icon_resource"" index=1 color=#000000>{0}";
		costElectricTextFormat = @"<sprite=""icon_resource"" index=2 color=#000000>{0}";
	}
#endif

	public void Init()
	{
		if (plusButton.IsNotNullRef())
		{
			plusButton.onClick.RemoveAllListeners();
			plusButton.onClick.AddListener(OnPlusButtonClick);
		}

		if (minusButton.IsNotNullRef())
		{
			plusButton.onClick.RemoveAllListeners();
			plusButton.onClick.AddListener(OnPlusButtonClick);
		}

		if (cardEventTrigger.IsNotNullRef())
		{
			cardEventTrigger.RemoveAllListeners(EventTriggerType.PointerEnter);
			cardEventTrigger.RemoveAllListeners(EventTriggerType.PointerExit);
			cardEventTrigger.AddListener(EventTriggerType.PointerEnter, OnPointEnterInCard);
			cardEventTrigger.AddListener(EventTriggerType.PointerExit, OnPointExitInCard);
		}
	}
	public void Deinit()
	{
		if (plusButton.IsNotNullRef())
		{
			plusButton.onClick.RemoveAllListeners();
		}

		if (minusButton.IsNotNullRef())
		{
			plusButton.onClick.RemoveAllListeners();
		}

		if (cardEventTrigger.IsNotNullRef())
		{
			cardEventTrigger.RemoveAllListeners(EventTriggerType.PointerEnter);
			cardEventTrigger.RemoveAllListeners(EventTriggerType.PointerExit);
		}
	}

	public void SetTItleText(string text)
	{
		if (titleText.IsNullRef()) return;
		titleText.text = text;
	}
	public void SetTitleImage(Sprite sprite)
	{
		if (mainImage.IsNullRef()) return;
		mainImage.sprite = sprite;
	}
	public void OnShowCountRect(bool show)
	{
		if (countTextRect.IsNullRef()) return;
		countTextRect.gameObject.SetActive(show);
	}
	public void SetCountText(int count)
	{
		if (countText.IsNullRef()) return;
		countText.text = $"{count}";
	}
	public void SetCostText(int costPersonnel, int costMaterial, int costElectric)
	{
		if (costText.IsNullRef()) return;

		var sb = new System.Text.StringBuilder(12);
		bool hasAny = false;
		AppendCost(ref hasAny, sb, costPersonnel, costPersonnelTextFormat);
		AppendCost(ref hasAny, sb, costMaterial, costMaterialTextFormat);
		AppendCost(ref hasAny, sb, costElectric, costElectricTextFormat);

		costText.text = sb.ToString();

		static void AppendCost(ref bool hasAny, System.Text.StringBuilder sb, int cost, string format)
		{
			if (cost <= 0) return;

			if (hasAny)
				sb.Append(' ');

			sb.AppendFormat(format, cost);
			hasAny = true;
		}

	}
}
