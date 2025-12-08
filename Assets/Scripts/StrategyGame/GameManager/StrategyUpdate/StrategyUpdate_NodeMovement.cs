using System.Collections;

using UnityEngine;

using static StrategyManagerModule.StrategyUpdate.StrategyUpdate_NodeMovement;

namespace StrategyManagerModule
{
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
				var iList = StrategyManager.Collector.GetAllElementLists();
				foreach (IList list in iList)
				{
					if (list is ElementList<UnitObject> unitList)
					{
						int length = list.Count;
						for (int i = 0 ; i < length ; i++)
						{
							var item = unitList[i];
							if (item == null || item.ThisNodeMovement == null || null != item.ParentMovement) continue;
							UpdateList.Add(new(item.ThisNodeMovement, this));
						}
					}
					if (list is ElementList<OperationObject> opList)
					{
						int length = list.Count;
						for (int i = 0 ; i < length ; i++)
						{
							var item = opList[i];
							if (item == null || item.ThisNodeMovement == null) continue;
							UpdateList.Add(new(item.ThisNodeMovement, this));
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

					if (!thisMovement.IsMovableState()) return;

					if (thisMovement.EmptyPath)
					{
						MoveStop();
						StayUpdate(in deltaTime);
						return;
					}
					else
					{
						MoveStart();
						MoveUpdate(in deltaTime);
					}
				}
				private void StayUpdate(in float deltaTime)
				{
					thisMovement.OnStayUpdate(in deltaTime);
				}
				private void MoveUpdate(in float deltaTime)
				{
					if (thisMovement.FindNextMovementTarget())
					{
						Vector3 nextPoint = thisMovement.NextMovePosition;
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
				private void MoveStart()
				{
					if (moveState) return;
					moveState = true;
					thisMovement.MoveStart();
				}
				private void MoveStop()
				{
					if (!moveState) return;
					moveState = false;
					thisMovement.MoveStop();
				}
			}
		}
	} 
}