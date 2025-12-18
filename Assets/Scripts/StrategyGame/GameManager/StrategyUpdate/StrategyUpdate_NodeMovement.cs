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
					this.Add(new Movement(movement, this));
				}
				else
				{
					int findIndex = this.FindIndex(l => l.thisMovement == element);
					if (findIndex < 0) return;
					this.RemoveAt(findIndex);
				}
			}

			protected override void Update(in float deltaTime)
			{
				int length = this.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var update = this[i];
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
					if (thisMovement.IsNullRef() || thisMovement.ParentMovement != null)
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