using GameUI;

using UnityEngine;

public class UnitCardGroupPanel : CardGroupPanelComponent
{
	protected override void Hide()
	{
		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{
			playerFaction.RemoveChangeCaptured(OnChangeItem);
		}
		int length = Count;
		for (int i = 0 ; i < length ; i++)
		{
			this[i].Hide();
		}
	}

	protected override void Show()
	{
		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{
			playerFaction.AddChangeUnit(OnChangeItem);
		}

		int length = Count;
		for (int i = 0 ; i < length ; i++)
		{
			this[i].Show();
		}
	}

	private void OnChangeItem(IStrategyElement element, bool added)
	{
		if (element is not UnitObject item) return;

		if (added)
		{
			this.Add(item);

		}
		else
		{
			this.Remove(item);
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

		protected override void ChangeUI()
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
