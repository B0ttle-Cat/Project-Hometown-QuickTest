using System.Collections;
using System.Collections.Generic;

using UnityEngine;

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
						if (item == null || item is not INodeMovement movement || movement != movement.ParentMovement) continue;
						UpdateList.Add(new(movement, this));
					}
				}
				if (list is List<OperationObject> opList)
				{
					int length = list.Count;
					for (int i = 0 ; i < length ; i++)
					{
						var item = opList[i];
						if (item == null || item is not INodeMovement movement) continue;
						UpdateList.Add(new(movement, this));
					}
				}
			}
			StrategyManager.Collector.AddChangeListener<UnitObject>(ChangeList);
			StrategyManager.Collector.AddChangeListener<OperationObject>(ChangeList);
		}
		protected override void Dispose()
		{
			StrategyManager.Collector.RemoveChangeListener<UnitObject>(ChangeList);
			StrategyManager.Collector.RemoveChangeListener<OperationObject>(ChangeList);
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
				if (findIndex < 0) return;
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
			private bool moveState;

			public Movement(INodeMovement movement, StrategyUpdateSubClass<Movement> thisSubClass) : base(thisSubClass)
			{
				thisMovement = movement;
				moveState = false;
			}

			protected override void OnDispose()
			{
				thisMovement = null;
			}

			protected override void OnUpdate(in float deltaTime)
			{
				if (thisMovement == null || thisMovement.ParentMovement != null)
				{
					return;
				}
				if (thisMovement.EmptyPath)
				{
					OnMoveStop();
					thisMovement.OnStayUpdate(in deltaTime);
					return;
				}
				else
				{
					OnMoveStart();
				}

				if (thisMovement.FindNextMovementTarget(out var nextPoint))
				{
					nextPoint = thisMovement.NextSmoothMovement(in nextPoint, out var velocity, in deltaTime);
					Vector3 delteMove = nextPoint - thisMovement.CurrentPosition;
					thisMovement.SetPositionAndVelocity(in nextPoint, in delteMove, in velocity, in deltaTime);
				}
				else
				{
					Vector3 position = thisMovement.CurrentPosition;
					Vector3 delteMove = Vector3.zero;
					Vector3 velocity = thisMovement.CurrentVelocity;
					thisMovement.SetPositionAndVelocity(in position, in delteMove, in velocity, in deltaTime);
				}
			}
			private void OnMoveStart()
			{
				if (moveState) return;
				moveState = true;
				thisMovement.OnMoveStart();
			}
			private void OnMoveStop()
			{
				if (!moveState) return;
				moveState = false;
				thisMovement.OnMoveStop();
			}
		}
	}
}