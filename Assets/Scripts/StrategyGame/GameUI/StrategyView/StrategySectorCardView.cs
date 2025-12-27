using GameUI;

using UnityEngine;


public class StrategySectorCardView : PanelCardGroupComponent
{

	protected override void Hide()
	{
		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{
			playerFaction.RemoveChangeCaptured(OnChangeCaptured);
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
			playerFaction.AddChangeCaptured(OnChangeCaptured);
		}

		int length = Count;
		for (int i = 0 ; i < length ; i++)
		{
			this[i].Show();
		}
	}

	private void OnChangeCaptured(IStrategyElement element, bool added)
	{
		if (element is not SectorObject sector) return;

		if (added)
		{
			this.Add(sector);

		}
		else
		{
			this.Remove(sector);
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
		private SectorCardUIObjectView sectorCardUIObjectView;

		public SectorCard(GameObject thisObject, SectorObject item = null) : base(thisObject, item)
		{
			if(thisObject.TryGetComponent<SectorCardUIObjectView>(out sectorCardUIObjectView))
			{
				sectorCardUIObjectView.SetUITarget(item);
			}
		}
		public override void Dispose()
		{
			if (sectorCardUIObjectView.IsNotNullRef())
			{
				sectorCardUIObjectView.Release();
				sectorCardUIObjectView = null;
			}

			base.Dispose();	
		}
		protected override void ReleaseUI()
		{
			if (sectorCardUIObjectView.IsNotNullRef())
			{
				sectorCardUIObjectView.Release();
			}

		}
		protected override void AttachUI()
		{
			if (sectorCardUIObjectView.IsNotNullRef())
			{
				sectorCardUIObjectView.Attach(item);
			}
		}

		protected override void ChangeUI()
		{
			if (sectorCardUIObjectView.IsNotNullRef())
			{
				sectorCardUIObjectView.RePating();
			}
		}

		protected override void ClearUI()
		{
			if (sectorCardUIObjectView.IsNotNullRef())
			{
				sectorCardUIObjectView.ClearUI();
			}
		}
	}
}
