using GameUI;

using UnityEngine;


public class SectorCardGroupPanel : CardGroupPanelComponent, IShowHideAsync
{

	void IShowHide.EndedHide()
	{
		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{
			playerFaction.RemoveChangeCaptured(OnChangeValue);
		}
		AllHideAndClear();
	}

	void IShowHide.StartShow()
	{
		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{
			playerFaction.AddChangeCaptured(OnChangeValue, false);
			InitCardList<SectorObject>(playerFaction.CapturedList.CardUIList);
		}
		AllShow();
	}

	private void OnChangeValue(IStrategyElement element, bool added)
	{
		if (element is not SectorObject item) return;

		if (added)
		{
			this.AddPoolData(item);

		}
		else
		{
			this.RemovePoolData(item);
		}
	}
	protected override CardPanel CardFactory<T>(GameObject newUIObject, T item) where T : class
	{
		if (item is SectorObject sectorObject)
		{
			return new SectorCard(newUIObject, sectorObject);
		}
		return null;
	}
	public class SectorCard : CardPanel<ISectorCardUIObject>
	{
		private SectorCardItemPanel cardUIObjectView;

		public SectorCard(GameObject thisObject, SectorObject item = null) : base(thisObject, item){}
		public override void Dispose()
		{
			if (cardUIObjectView.IsNotNullRef())
			{
				cardUIObjectView.Release();
				cardUIObjectView = null;
			}

			base.Dispose();	
		}
		protected override void ReleaseUI()
		{
			if (cardUIObjectView.IsNotNullRef())
			{
				cardUIObjectView.Release();
			}

		}
		protected override void AttachUI()
		{
			if (cardUIObjectView.IsNotNullRef())
			{
				cardUIObjectView.Attach(Item);
			}
		}

		protected override void UpdateUI()
		{
			if (cardUIObjectView.IsNotNullRef())
			{
				cardUIObjectView.RePainting();
			}
		}
	}
}
