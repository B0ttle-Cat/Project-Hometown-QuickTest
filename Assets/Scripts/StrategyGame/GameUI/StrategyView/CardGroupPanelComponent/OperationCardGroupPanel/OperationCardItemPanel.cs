using GameUI;

using TMPro;

using UnityEngine.UI;

public class OperationCardItemPanel : KeyPairPanelItemComponent
{
	private IOperationCardUIObject operationCard;

	public void SetUITarget(OperationObject operationObject)
	{
		operationCard = operationObject;
	}

	internal void Attach(IOperationCardUIObject item)
	{
		operationCard = item;

		if (ThisUIFinder.TryFind<Image>("..TitleImage", out var titleImage))
		{
			titleImage.sprite = operationCard.GetTitleImage();
		}
		if (ThisUIFinder.TryFind<TMP_Text>("..TitleText", out var titleText))
		{
			titleText.text = operationCard.GetTitleName();
		}

		// TODO :: 이어서 작업 진행
	}

	internal void ClearUI()
	{
	}

	internal void Release()
	{
		operationCard = null;
	}

	internal void RePating()
	{
	}
}
