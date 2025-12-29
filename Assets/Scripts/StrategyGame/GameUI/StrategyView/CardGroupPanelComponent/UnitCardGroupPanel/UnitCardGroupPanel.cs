using GameUI;

using UnityEngine;

public class UnitCardGroupPanel : CardGroupPanelComponent
{
	
	protected override void Hide()
	{
		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{
			playerFaction.RemoveChangeCaptured(OnChangeValue);
		}
		base.Hide();
	}

	protected override void Show()
	{
		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{
			playerFaction.AddChangeUnit(OnChangeValue);
			InitCardList<UnitObject>(playerFaction.UnitList.CardUIType);
		}
		base.Show();
	}

	private void OnChangeValue(IStrategyElement element, bool added)
	{
		if (element is not UnitObject item) return;

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
		if (item is UnitObject sectorObject)
		{
			return new UnitCard(newUIObject, sectorObject);
		}
		return null;
	}
	public class UnitCard : CardPanel<IUnitCardUIObject>
	{
		private UnitCardItemPanel cardUIObjectView;

		public UnitCard(GameObject thisObject, UnitObject item = null) : base(thisObject, item)
		{
			if (thisObject.TryGetComponent<UnitCardItemPanel>(out cardUIObjectView))
			{
				cardUIObjectView.SetUITarget(item);
			}
		}
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
				cardUIObjectView.Attach(item);
			}
		}

		protected override void UpdateUI()
		{
			if (cardUIObjectView.IsNotNullRef())
			{
				cardUIObjectView.RePating();
			}
		}

		protected override void ClearUI()
		{
			if (cardUIObjectView.IsNotNullRef())
			{
				cardUIObjectView.ClearUI();
			}
		}
	}
}
