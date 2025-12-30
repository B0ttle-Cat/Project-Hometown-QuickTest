using System.Collections.Generic;

using GameUI;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class UnitCardItemPanel : PanelItemComponent, IFindUIObject
{
	private IUnitCardUIObject unitCard;
	public IFindUIObject ThisUIFinder => this;
	[SerializeField, PropertyOrder(-90)] private List<IFindUIObject.KeyPairObject> keyPairs;
	List<IFindUIObject.KeyPairObject> IFindUIObject.KeyPairs { get => keyPairs; set => keyPairs = value; }
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
