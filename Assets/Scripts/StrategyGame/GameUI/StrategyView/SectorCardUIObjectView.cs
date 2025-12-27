using System.Collections.Generic;

using GameUI;

using UnityEngine;

public class SectorCardUIObjectView : PanelItemComponent, IFindUIObject
{
	public IFindUIObject ThisUIFinder => this;
	[SerializeField]
	private List<IFindUIObject.KeyPairObject> keyPairs;
	private ISectorCardUIObject sectorCard;
	List<IFindUIObject.KeyPairObject> IFindUIObject.KeyPairs => keyPairs;

	public void SetUITarget(SectorObject sectorObject)
	{
		sectorCard = sectorObject;
	}

	protected override void Hide()
	{
	}

	protected override void Show()
	{
	}

	internal void Attach(ISectorCardUIObject item)
	{
		sectorCard = item;
	}

	internal void ClearUI()
	{
	}

	internal void Release()
	{
		sectorCard = null;
	}

	internal void RePating()
	{
	}
}
