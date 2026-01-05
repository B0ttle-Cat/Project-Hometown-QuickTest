using static StrategyGamePlayData.ISupplyStats;
using static StrategyManagerModule.StrategyPathfinding;
using static StrategyManagerModule.StrategyUpdate.StrategyUpdate_SectorResourcesDistribution;

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_SectorResourcesDistribution : StrategyUpdateSubClass<Distribution>
		{
			public StrategyUpdate_SectorResourcesDistribution(StrategyUpdate updater) : base(updater)
			{
			}
			protected override void Dispose()
			{
				StrategyManager.Collector.RemoveChangeListener<SectorObject>(OnChangeSector);
			}
			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<SectorObject>(OnChangeSector);
			}
			private void OnChangeSector(IStrategyElement element, bool isAdd)
			{
				if (element == null || element is not SectorObject sector || sector == null) return;

				if (isAdd)
				{
					this.Add(new Distribution(sector, this));
				}
				else
				{
					int findIndex = this.FindIndex(i=>i.Sector.Equals(sector));
					if (findIndex < 0) return;
					this.RemoveAt(findIndex);
				}
			}
			protected override void Update(in float deltaTime)
			{
				int length = this.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = this[i];
					if (item == null) continue;
					item.Start();
				}
				for (int i = 0 ; i < length ; i++)
				{
					var item = this[i];
					if (item == null) continue;
					item.Update(in deltaTime);
				}
				for (int i = 0 ; i < length ; i++)
				{
					var item = this[i];
					if (item == null) continue;
					item.Update(in deltaTime);
				}
			}

			public class Distribution : UpdateLogic
			{
				private SectorObject sector;
				public SectorObject Sector => sector;

				private SectorNetwork network;
				private int[] neighborIndexs; // => Distribution neighbor = thisSubClass.this[neighborIndex]

				public int currPoint;
				public int maxPoint;
				public int plusPoint;
				public bool updateThisTurn;
				public bool updateNextTurn;
				public Distribution(SectorObject sector, StrategyUpdateSubClass<Distribution> thisSubClass) : base(thisSubClass)
				{


					this.sector = sector;
					network = null;

					if (StrategyManager.Pathfinding.GetSectorNetwork(Sector, out network))
					{
						FindingNeighbors();
					}

				}

				protected override void OnDispose()
				{
				}

				// 매 프레임 호출된다.
				public void Start()
				{
					// TempSupplyValue 는 자원 분배에 참고할 값을 가지고 있다.
					// 약 1초마가 갱신되며, personnelIsUpdate 가 true이다.
					if (TempData.TryGetValue<TempSupplyValue>(TempSupplyValue.SectorTempSupplyValueKey(sector), out var tempValue))
					{
						if (tempValue.personnelIsUpdate)
						{
							currPoint = tempValue.material;                 // 현제 이 SectorObject 에 추가 예정인 값 (Clamp 되지 않음)
							maxPoint = tempValue.materialMax;               // 이 SectorObject에 추가 가능한 최대값
							plusPoint = tempValue.materialSupply;           // 이번 턴에 SectorObject에 얼마만큼 추가 되었는지?
							updateThisTurn = tempValue.personnelIsUpdate;    // 이 턴에 업데이트가 필요한가?
							updateNextTurn = true;                          // 다음턴에 업데이트가 필요한가?
						}
					}

					if (network == null)
					{
						if (StrategyManager.Pathfinding.GetSectorNetwork(Sector, out network))
						{
							FindingNeighbors();
						}
					}
				}
				private void FindingNeighbors()
				{
					if (network == null || network.neighbors == null)
					{
						neighborIndexs = null;
						return;
					}

					StrategyManager.Pathfinding.FindSectorNeighbors(sector, 0, out var neighbors);
					
					//var neigh = network.neighbors;
					//var list = thisSubClass;
					//
					//neighborIndexs = new int[neigh.Count];
					//
					//for (int i = 0 ; i < neigh.Count ; i++)
					//{
					//	SectorObject targetSector = neigh[i].iamge;
					//	int found = -1;
					//	for (int j = 0 ; j < list.Count ; j++)
					//	{
					//		if (list[j].iamge == targetSector)
					//		{
					//			found = j;
					//			break;
					//		}
					//	}
					//	neighborIndexs[i] = found;
					//}
				}

				// Start 이후 4회 연속으로 호출된다. 
				protected override void OnUpdate(in float deltaTime)
				{
					if (network == null || neighborIndexs == null) return;

					while (CheckUpdate())
					{
						OnDistribute();
						OnRecover();
					}
				}

				protected bool CheckUpdate()
				{
					// TODO :: 이곳에서 자원 분배가 가능한 상태인지 확인한다.
					return true;
				}

				protected void OnDistribute()
				{
					// TODO :: 이곳에서 자원을 분배한다.
				}

				protected void OnRecover()
				{
					// TODO :: 이곳에서 초과량에 대한 자원을 회수한다.
				}
			}
		}
	}
}