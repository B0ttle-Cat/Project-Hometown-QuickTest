using GameUI;

using TMPro;

using UnityEngine.UI;

public class UnitCardItemPanel : KeyPairPanelItemComponent
{
	private IUnitCardUIObject unitCard;

	public void SetUITarget(UnitObject operationObject)
	{
		unitCard = operationObject;
	}


	internal void Attach(IUnitCardUIObject item)
	{
		unitCard = item;

		if (ThisUIFinder.TryFind<Image>("..TitleImage", out var titleImage))
		{
			titleImage.sprite = unitCard.GetTitleImage();
		}
		if (ThisUIFinder.TryFind<TMP_Text>("..TitleText", out var titleText))
		{
			titleText.text = unitCard.GetTitleName();
		}

		// TODO :: 이어서 작업 진행
	}

	internal void ClearUI()
	{
	}

	internal void Release()
	{
		unitCard = null;
	}

	internal void RePating()
	{
	}
}
