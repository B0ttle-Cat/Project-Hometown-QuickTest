using System;
using System.Linq;

using static StrategyGamePlayData;

public static class FactionAPI
{
	public static bool IsAlive(this Faction faction)
		=> faction != null && faction.FactionID >= 0;

	public static bool IsNotAlive(this Faction faction)
		=> !faction.IsAlive();

	#region Unit Count
	public static void API_UnitCounter(this Faction faction, int value)
	{
		if (faction.IsNotAlive()) return;

		StatsValue nowValue = faction.FactionStats.GetValue(StatsType.세력_병력_현재);
		nowValue += value;
		faction.FactionStats.SetValue(nowValue);
	}
	#endregion

	#region CanAffor
	public static bool API_CanAffordMaterial(this Faction faction, int value)
	{
		if (faction.IsNotAlive()) return false;

		StatsValue nowValue = faction.FactionStats.GetValue(StatsType.세력_물자_현재);
		return nowValue >= value;
	}

	public static bool API_CanAffordElectric(this Faction faction, int value)
	{
		if (faction.IsNotAlive()) return false;

		StatsValue nowValue = faction.FactionStats.GetValue(StatsType.세력_전력_현재);
		return nowValue >= value;
	}
	#endregion

	#region PayFor
	public static void API_PayForMaterial(this Faction faction, SectorObject sector, int value)
	{
		if (faction.IsNotAlive()) return;

		var sectorList = StrategyManager.Collector.SectorList;
		var values = sectorList.Where(i => i.CaptureFaction == faction).Select(i => i.GetElectric().value).ToArray();
	}

	public static void API_PayForElectric(this Faction faction, int value)
	{
		if (faction.IsNotAlive()) return;

		var sectorList = StrategyManager.Collector.SectorList;
		var values = sectorList.Where(i => i.CaptureFaction == faction).Select(i => i.GetElectric().value).ToArray();

		StatsValue totalValue = faction.FactionStats.GetValue(StatsType.세력_전력_현재);
		if(PayCostByCutDown(value, totalValue, values))
		{
			int total = 0;
			int length = values.Length;
            for (int i = 0 ; i < length ; i++)
            {
				var sector = sectorList[i];
				total += values[i];
				sector.SetElectric(values[i]);
			}
			
			totalValue.Value = value;
			faction.FactionStats.SetValue(totalValue);
		}
	}
	#endregion

	#region Supply
	public static void API_SupplyMaterial(this Faction faction, int value)
	{
		if (faction.IsNotAlive()) return;

		StatsValue maxValue = faction.FactionStats.GetValue(StatsType.세력_물자_최대);
		StatsValue nowValue = faction.FactionStats.GetValue(StatsType.세력_물자_현재);
		if (nowValue >= maxValue) return;

		nowValue += value;
		nowValue.Clamp(0, maxValue);
		faction.FactionStats.SetValue(nowValue);
	}

	public static void API_SupplyElectric(this Faction faction, int value)
	{
		if (faction.IsNotAlive()) return;

		StatsValue maxValue = faction.FactionStats.GetValue(StatsType.세력_전력_최대);
		StatsValue nowValue = faction.FactionStats.GetValue(StatsType.세력_전력_현재);
		if (nowValue >= maxValue) return;

		nowValue += value;
		nowValue.Clamp(0, maxValue);
		faction.FactionStats.SetValue(nowValue);
	}
	#endregion

	#region IsFull
	public static bool API_IsMaterialFull(this Faction faction)
	{
		if (faction.IsNotAlive()) return true;

		StatsValue maxValue = faction.FactionStats.GetValue(StatsType.세력_물자_최대);
		StatsValue nowValue = faction.FactionStats.GetValue(StatsType.세력_물자_현재);
		return nowValue >= maxValue;
	}

	public static bool API_IsElectricFull(this Faction faction)
	{
		if (faction.IsNotAlive()) return true;

		StatsValue maxValue = faction.FactionStats.GetValue(StatsType.세력_전력_최대);
		StatsValue nowValue = faction.FactionStats.GetValue(StatsType.세력_전력_현재);
		return nowValue >= maxValue;
	}
	#endregion

	#region PayCostFunction
	// 가장 큰 위치부터 비용을 지불한다.
	private static bool PayCostByCutDown(int cost, int? totalValue, int[] values)
	{
		// 0 비용이면 바로 성공
		if (cost <= 0) return true;

		int length = values?.Length ?? 0;
		if (length == 0) return false;

		// 단일 요소 처리
		if (length == 1)
		{
			if (values[0] < cost) return false;
			values[0] -= cost;
			return true;
		}

		// 총합이 cost 이상인지 확인
		if (!totalValue.HasValue)
		{
			int total = 0;
			for (int i = 0 ; i < length ; i++)
			{
				total += values[i];
				if (total >= cost) break;
			}
			totalValue = total;
		}
		if (totalValue.Value < cost) return false;

		// 원본 배열을 건드리지 않고, 큰 값 순서의 인덱스 배열 생성
		int[] sortedIndices = new int[length];
		for (int i = 0 ; i < length ; i++)
			sortedIndices[i] = i;

		// values 배열 기준 내림차순으로 인덱스 정렬
		Array.Sort(sortedIndices, (a, b) => values[b].CompareTo(values[a]));

		// cut 계산
		int lastCutValue = 0;       // 현재까지 균등하게 맞출 값
		int accumulatedCost = 0;    // 이미 분배된 비용
		int cutRange = length;       // 비용 분배할 범위
		for (int i = 0 ; i < length ; i++)
		{
			int cut = (i < length - 1) ? values[sortedIndices[i]] - values[sortedIndices[i + 1]] : 0;
			if (cut == 0) continue;

			int cutCost = cut * (i + 1);

			if (accumulatedCost + cutCost <= cost)
			{
				accumulatedCost += cutCost;
				lastCutValue = values[sortedIndices[i]];
				continue;
			}

			// 현재 i까지의 범위까지만 균등 분배 가능
			cutRange = i + 1;
			break;
		}

		// 잔여 비용 계산
		cost -= accumulatedCost;
		if (cost < 0)
		{
			// 이미 총합 체크로 방지되므로 정상적으로는 이곳에 들어오지 않음
			return false;
		}

		// 정확히 나누어떨어지는 경우
		if (cost == 0)
		{
			for (int i = 0 ; i < cutRange ; i++)
			{
				values[sortedIndices[i]] = lastCutValue;
			}
			return true;
		}

		// 남은 비용을 균등 분배 (quotientCost + remainderCost)
		int quotient = cost / cutRange;
		int remainder = cost % cutRange;

		for (int i = 0 ; i < cutRange ; i++)
		{
			if (i < remainder)
				values[sortedIndices[i]] = lastCutValue - quotient - 1;
			else
				values[sortedIndices[i]] = lastCutValue - quotient;
		}

		return true;
	}
	// 모두가 동등하게 비용을 지불한다.
	private static bool PayCostByEqualDeduction(int cost, int? totalValue, int[] values)
	{
		// 0 비용이면 바로 성공
		if (cost <= 0) return true;

		int length = values?.Length ?? 0;
		if (length == 0) return false;

		// 단일 요소 처리
		if (length == 1)
		{
			if (values[0] < cost) return false;
			values[0] -= cost;
			return true;
		}

		// 총합이 cost 이상인지 확인
		if (!totalValue.HasValue)
		{
			int total = 0;
			for (int i = 0 ; i < length ; i++)
			{
				total += values[i];
				if (total >= cost) break;
			}
			totalValue = total;
		}
		if (totalValue.Value < cost) return false;

		// 아직 분배되지 않은 총 비용
		int unallocatedCost = cost;

		while (unallocatedCost > 0)
		{
			// 0보다 큰 값들의 개수
			int nonZeroCount = 0;
			for (int i = 0 ; i < length ; i++) if (values[i] > 0) nonZeroCount++;

			if (nonZeroCount == 0)
			{
				// 모든 값이 0이 되었는데 cost가 남음
				// 총합을 확인하는 과정은 이미 진행했기 때문에 수식에 문제가 있는 경우에만 발생함.
				return false;
			}

			// 이번 라운드에서 각 값이 균등하게 가져갈 비용
			int perValueCost = unallocatedCost / nonZeroCount;
			// 이번 라운드에서 균등 분배 후 남는 나머지
			int extraCost = unallocatedCost % nonZeroCount;

			if (perValueCost == 0)
			{
				// 남은 비용이 살아있는 값 개수보다 적으면 1씩만 분배
				for (int i = 0 ; i < length && extraCost > 0 ; i++)
				{
					if (values[i] <= 0) continue;
					values[i]--;
					extraCost--;
				}

				// 나머지가 남아있다면 수식에 문제가 있음.
				// 그외 수학적으로 발생하지 않음
				if (extraCost > 0) return false;
				break;
			}
			else
			{
				// 균등 분배 후, 나머지(extraCost) 비용과 음수로 내려간 값을 다음 라운드에 합산
				// perValueCost * nonZeroCount : 균등 분배에 고정으로 사용될 비용
				unallocatedCost += extraCost - (perValueCost * nonZeroCount);

				for (int i = 0 ; i < length ; i++)
				{
					int value = values[i];
					if (value <= 0) continue;
					value -= perValueCost;
					if (value < 0)
					{
						// 음수로 내려간 부분을 다시 분배
						unallocatedCost += -value;
						value = 0;
					}
					values[i] = value;
				}
			}
		}

		return true;
	}
	// 앞쪽부터 순서대로 비용을 지불한다.
	private static bool PayCostForindexOrder(int cost, int? totalValue, int[] values)
	{
		if (cost <= 0) return true;

		int length = values?.Length ?? 0;
		if (length == 0) return false;

		// 단일 요소 처리
		if (length == 1)
		{
			if (values[0] < cost) return false;
			values[0] -= cost;
			return true;
		}

		// 총합이 cost 이상인지 확인
		if (!totalValue.HasValue)
		{
			int total = 0;
			for (int i = 0 ; i < length ; i++)
			{
				total += values[i];
				if (total >= cost) break;
			}
			totalValue = total;
		}
		if (totalValue.Value < cost) return false;


		// cost 소진 가능, 안전하게 처리
		for (int i = 0 ; i < length ; i++)
		{
			if (values[i] <= 0) continue;

			if (cost >= values[i])
			{
				cost -= values[i];
				values[i] = 0;
			}
			else
			{
				values[i] -= cost;
				cost = 0;
				break;
			}
		}

		return true;
	}
	#endregion
}
