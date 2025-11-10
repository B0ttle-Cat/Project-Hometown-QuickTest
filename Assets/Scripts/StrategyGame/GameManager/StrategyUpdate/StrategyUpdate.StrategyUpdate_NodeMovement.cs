using System.Collections;
using System.Collections.Generic;

using static StrategyUpdate.StrategyUpdate_NodeMovement;

public partial class StrategyUpdate
{
	public class StrategyUpdate_NodeMovement : StrategyUpdateSubClass<Movement>
	{
		public StrategyUpdate_NodeMovement(StrategyUpdate updater) : base(updater)
		{
		}

		protected override void Start()
		{
			UpdateList = new();
			var iList = StrategyManager.Collector.GetAllEnumerable();
			foreach (IList list in iList)
			{
				if (list is List<UnitObject> unitList)
				{
					int length = list.Count;
					for (int i = 0 ; i < length ; i++)
					{
						var item = unitList[i];
						if (item == null || item.IsTroopBelong || item is not INodeMovement movement) continue;
						UpdateList.Add(new(movement, this));
					}
				}
			}
			StrategyManager.Collector.AddChangeListener<UnitObject>(ChangeList);
		}
		protected override void Dispose()
		{
			StrategyManager.Collector.RemoveChangeListener<UnitObject>(ChangeList);
		}
		private void ChangeList(IStrategyElement element, bool isAdd)
		{
			if (element is not INodeMovement movement) return;

			if (isAdd)
			{
				UpdateList.Add(new Movement(movement, this));
			}
			else
			{
				int findIndex = UpdateList.FindIndex(l => l.thisMovement == element);
				if (findIndex >= 0) return;
				UpdateList.RemoveAt(findIndex);
			}
		}

		protected override void Update(in float deltaTime)
		{
			int length = UpdateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var update = updateList[i];
				if (update == null) continue;
				update.Update(deltaTime);
			}
		}

		public class Movement : UpdateLogic
		{
			public INodeMovement thisMovement;

			public Movement(INodeMovement movement, StrategyUpdateSubClass<Movement> thisSubClass) : base(thisSubClass)
			{
				thisMovement = movement;
			}

			protected override void OnDispose()
			{
			}

			protected override void OnUpdate(in float deltaTime)
			{
			}
		}
	}
}