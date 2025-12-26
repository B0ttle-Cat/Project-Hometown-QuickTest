using GameUI;

using UnityEngine;


public class StrategySectorCardView : PanelCardGroupComponent
{
    protected override CardPanel CardFactory<T>(GameObject newUIObject, T item) where T : class
	{
		if (item is SectorObject sectorObject)
		{
			return new SectorCard(newUIObject, sectorObject);
		}
        return null;
	}
	protected override void Hide()
	{
		int length = Count;
		for (int i = 0 ; i < length ; i++)
		{
			this[i].Hide();
		}
	}

    protected override void Show()
    {
        int length = Count;
        for (int i = 0 ; i < length ; i++)
        {
            this[i].Show();
        }
    }

    public class SectorCard : CardPanel<SectorObject>
    {
        public SectorCard(GameObject thisObject, SectorObject item = null) : base(thisObject, item)
        {
            
        }
        public override void Dispose()
        {
            base.Dispose();

        }
        protected override void AttachUI()
        {
			//item.
		}

        protected override void ChangeUI()
        {
        }

        protected override void ClearUI()
        {
        }

        protected override void ReleaseUI()
        {
        }
    }
}
