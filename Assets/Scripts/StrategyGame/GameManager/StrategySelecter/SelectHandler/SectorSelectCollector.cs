using System;
using System.Collections.Generic;

namespace StrategyManagerModule
{
	public class SectorSelectCollector : SelectCollector<SectorObject>, IList<SectorObject>
	{
		ProcessOverrider sectorSelect;

		protected override void Init()
		{
			sectorSelect?.Dispose();
			sectorSelect = null;
		}
		protected override void Deinit()
		{
			sectorSelect?.Dispose();
			sectorSelect = null;
		}
		protected override void OnSelected(SectorObject selectItem)
		{
			if (sectorSelect == null) sectorSelect = new ProcessOverrider_PointingAtSector(PointingAtSector);
			else sectorSelect.ReProcess();

			ThisSelecter.CallSectorSelected(selectItem);
		}
        protected override void OnDeselected(SectorObject selectItem)
		{
			if (Count == 0)
			{
				sectorSelect?.Dispose();
				sectorSelect = null;
			}
			ThisSelecter.CallSectorDeselected(selectItem);
		}
		protected override void OnPointing(SectorObject selectable) => ThisSelecter.CallSectorPointing(selectable);
		private void PointingAtSector(SectorObject selectable)
		{
			var target = selectable;
			
			int length = Count;
			for (int i = 0 ; i < length ; i++)
            {
				var start = Items[i];


			}
        }
	}
	public partial class StrategySelecter
	{
		private SectorSelectCollector selectSector;
		public SectorSelectCollector SelectSector
		{
			get
			{
				if (selectSector == null)
				{
					selectSector = gameObject.GetComponent<SectorSelectCollector>();
				}
				return selectSector;
			}
		}
		public event Action<SectorObject,bool> OnSectorSelectChange;
		public event Action<SectorObject> OnSectorPointing;

		internal void CallSectorSelected(SectorObject sectorObject)
		{
			OnSectorSelectChange?.Invoke(sectorObject, true);
		}
		internal void CallSectorDeselected(SectorObject sectorObject)
		{
			OnSectorSelectChange?.Invoke(sectorObject, false);
		}
		internal void CallSectorPointing(SectorObject sectorObject)
		{
			OnSectorPointing?.Invoke(sectorObject);
		}
	}
}