using UnityEngine;

using static StrategyManagerModule.StrategyUpdate.StrategyUpdate_NavMovement;

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_NavMovement : StrategyUpdateSubClass<Movement>
		{
			public StrategyUpdate_NavMovement(StrategyUpdate updater) : base(updater)
			{
			}

			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<UnitObject>(OnChangeValue, true);
			}
			private void OnChangeValue(IStrategyElement element, bool added)
			{
				if (element is not UnitObject item) return;
				if (item == null || item.ThisNavMovement == null) return;

				if (added)
				{
					this.Add(new(item.ThisNavMovement, this));
				}
				else
				{
					int findIndex = this.FindIndex(f => f.thisMovement == item.ThisNavMovement);
					if (findIndex >= 0) this.RemoveAt(findIndex);
				}
			}
			protected override void Dispose()
			{
				StrategyManager.Collector.RemoveChangeListener<UnitObject>(OnChangeValue);
			}

			public class Movement : UpdateLogic
			{
				public INavMovement thisMovement;
				private bool moveState;

				public Movement(INavMovement movement, StrategyUpdateSubClass<Movement> thisSubClass) : base(thisSubClass)
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
					if (thisMovement == null)
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