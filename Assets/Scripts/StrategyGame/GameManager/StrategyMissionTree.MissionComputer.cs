using System;
using System.Linq;

using static StrategyGamePlayData.MissionTreeData;

public partial class StrategyMissionTree // MissionCompute
{
    /// <summary>
    /// <see cref="MissionType"/>
    /// </summary>
    public abstract class MissionComputer : IDisposable
	{
		public abstract string ConverToText(in ItemStruct itemStruct);
		public abstract ResultTyoe Compute(in ItemStruct itemStruct);
		protected string MissionTypeText(MissionType type) => type switch
		{
			MissionType.Kill => "정해진 대상을 처치 또는 파괴하세요.",
			MissionType.Protect => "정해진 대상이 처치 또는 파괴되지 않도록 보호하세요.",
			MissionType.ControlBase_Count => "아무 거점을 정해진 수 많큼 점령하세요.",
			MissionType.CaptureAndSecureBase => "정해진 거점을 점령 또는 보호 하세요.",

			_ => "",
		};

        public virtual void Dispose()
        {
            
        }
    }

	#region CustomFunction_Mission
	public class CustomFunction_Mission : MissionComputer
	{
		private string description;
		private Func<ItemStruct, ResultTyoe> condition;
		public CustomFunction_Mission(string description, Func<ItemStruct, ResultTyoe> condition)
		{
			this.condition = condition;
		}
		public override ResultTyoe Compute(in ItemStruct itemStruct) => condition?.Invoke(itemStruct) ?? ResultTyoe.Succeed;
		public override string ConverToText(in ItemStruct itemStruct) => description;
	}
	#endregion

	#region Kill_Mission
	public class Kill_Mission : MissionComputer
	{
		public override ResultTyoe Compute(in ItemStruct itemStruct)
		{
			ComparisonType comparisonType = itemStruct.comparisonType;
			int targetCount = itemStruct.count;
			int computeCount = ComputeCount(in itemStruct);

			return comparisonType switch
			{
				ComparisonType.동등 => targetCount == computeCount ? ResultTyoe.Succeed : ResultTyoe.Wait,
				ComparisonType.이하 => targetCount >= computeCount ? ResultTyoe.Succeed : ResultTyoe.Wait,
				ComparisonType.이상 => targetCount <= computeCount ? ResultTyoe.Succeed : ResultTyoe.Wait,
				_ => ResultTyoe.Succeed,
			};
		}
		public override string ConverToText(in ItemStruct itemStruct)
		{
			var missionType = itemStruct.missionType;
			var targets= itemStruct.targets;
			var comparisonType = itemStruct.comparisonType;
			var targetCount = itemStruct.count;
			var computeCount = ComputeCount(in itemStruct);
			//정해진 대상을 처치 또는 파괴하세요
			return $"{MissionTypeText(missionType)}" +
				$"\n\t{TargetText()}" +
				$"\n\t{ComparisonText()}" +
				$"\n\t{ProgressText()}";
			string TargetText() => $"대상: {string.Join(", ", targets)}";
			string ComparisonText() => comparisonType switch
			{
				ComparisonType.동등 => $"다음 수 많큼 파괴 할 것: 정확히 {targetCount}",
				ComparisonType.이하 => $"다음 수 많큼 파괴 할 것: {targetCount} 이하 ",
				ComparisonType.이상 => $"다음 수 많큼 파괴 할 것: {targetCount} 이상 ",
				_ => ""
			};
			string ProgressText() => $"현재까지 파괴된 목표 수: {computeCount}";
		}
		private int ComputeCount(in ItemStruct itemStruct)
		{
			int computeCount = 0;
			var statsKeys = itemStruct.targets.Select(s => $"제거 및 파괴/{s}").ToArray();
			int length = statsKeys.Length;
			for (int i = 0 ; i < length ; i++)
			{
				var statsKey = statsKeys[i];
				//if (StrategyManager.Statistics.TryGetStatsValue(statsKey, out string Name,out int capture))
				//{
				//	computeCount += capture;
				//}
			}
			return computeCount;
		}
	}
	#endregion

	#region Protect_Mission
	public class Protect_Mission : MissionComputer
	{
		public override ResultTyoe Compute(in ItemStruct itemStruct)
		{
			ComparisonType comparisonType = itemStruct.comparisonType;
			int targetCount = itemStruct.count;
			int computeCount = ComputeCount(in itemStruct);

			return comparisonType switch
			{
				ComparisonType.동등 => targetCount == computeCount ? ResultTyoe.Succeed : ResultTyoe.Wait,
				ComparisonType.이하 => targetCount >= computeCount ? ResultTyoe.Succeed : ResultTyoe.Wait,
				ComparisonType.이상 => targetCount <= computeCount ? ResultTyoe.Succeed : ResultTyoe.Wait,
				_ => ResultTyoe.Succeed,
			};
		}
		public override string ConverToText(in ItemStruct itemStruct)
		{
			var missionType = itemStruct.missionType;
			var targets= itemStruct.targets;
			var comparisonType = itemStruct.comparisonType;
			var targetCount = itemStruct.count;
			var computeCount = ComputeCount(in itemStruct);

			//정해진 대상이 처치 또는 파괴되지 않도록 보호하세요
			return $"{MissionTypeText(missionType)}" +
				$"\n\t{TargetText()}" +
				$"\n\t{ComparisonText()}" +
				$"\n\t{ProgressText()}";
			string TargetText() => $"대상: {string.Join(", ", targets)}";
			string ComparisonText() => comparisonType switch
			{
				ComparisonType.동등 => $"보호 목표 수량: 정확히 {targetCount}",
				ComparisonType.이하 => $"보호 목표 수량: {targetCount} 이하",
				ComparisonType.이상 => $"보호 목표 수량: {targetCount} 이상",
				_ => ""
			};
			string ProgressText() => $"남아있는 보호 목표 수: {computeCount}";
		}
		private int ComputeCount(in ItemStruct itemStruct)
		{
			var targets = itemStruct.targets;
			int computeCount = 0;
			if (targets == null || targets.Length == 0) return computeCount;

			StrategyManager.Collector.ForEachUnit(unit =>
			{
				if (targets.Contains(unit.UnitName))
				{
					computeCount++;
				}
			});

			return computeCount;
		}
	}
	#endregion

	#region ControlBase_Count_Mission
	public class ControlBase_Count_Mission : MissionComputer
	{
		public override ResultTyoe Compute(in ItemStruct itemStruct)
		{
			ComparisonType comparisonType = itemStruct.comparisonType;
			int targetCount = itemStruct.count;
			int computeCount = ComputeCount(in itemStruct);

			return comparisonType switch
			{
				ComparisonType.동등 => targetCount == computeCount ? ResultTyoe.Succeed : ResultTyoe.Wait,
				ComparisonType.이하 => targetCount >= computeCount ? ResultTyoe.Succeed : ResultTyoe.Wait,
				ComparisonType.이상 => targetCount <= computeCount ? ResultTyoe.Succeed : ResultTyoe.Wait,
				_ => ResultTyoe.Succeed,
			};
		}
		public override string ConverToText(in ItemStruct itemStruct)
		{
			var missionType = itemStruct.missionType;
			var comparisonType = itemStruct.comparisonType;
			var targetCount = itemStruct.count;
			var computeCount = ComputeCount(in itemStruct);

			//정해진 수 만큼의 거점을 점령하세요
			return $"{MissionTypeText(missionType)}" +
				$"\n\t{ComparisonText()}" +
				$"\n\t{ProgressText()}";

			string ComparisonText() => comparisonType switch
			{
				ComparisonType.동등 => $"점령된 거점의 수: 정확히 {targetCount}",
				ComparisonType.이하 => $"점령된 거점의 수: {targetCount} 이하",
				ComparisonType.이상 => $"점령된 거점의 수: {targetCount} 이상",
				_ => ""
			};
			string ProgressText() => $"현재 점령 중인 거점의 수: {computeCount}";
		}
		private int ComputeCount(in ItemStruct itemStruct)
		{
			int computeCount = 0;
			StrategyManager.Collector.ForEachControlBase(cb =>
			{
				if(cb.CaptureFactionID == StrategyGamePlayData.PlayerFactionID)
				{
					computeCount++;
				}
			});
			return computeCount;
		}
	}
	#endregion

	#region CaptureAndSecureBase_Mission
	public class CaptureAndSecureBase_Mission : MissionComputer
	{
		public override ResultTyoe Compute(in ItemStruct itemStruct)
		{
			ComparisonType comparisonType = itemStruct.comparisonType;
			int targetCount = itemStruct.count;
			int computeCount = ComputeCount(in itemStruct, out _);

			return comparisonType switch
			{
				ComparisonType.동등 => targetCount == computeCount ? ResultTyoe.Succeed : ResultTyoe.Wait,
				ComparisonType.이하 => targetCount >= computeCount ? ResultTyoe.Succeed : ResultTyoe.Wait,
				ComparisonType.이상 => targetCount <= computeCount ? ResultTyoe.Succeed : ResultTyoe.Wait,
				_ => ResultTyoe.Succeed,
			};
		}
		public override string ConverToText(in ItemStruct itemStruct)
		{
			var missionType = itemStruct.missionType;
			var targets= itemStruct.targets;
			var comparisonType = itemStruct.comparisonType;
			var targetCount = itemStruct.count;
			var computeCount = ComputeCount(in itemStruct, out string[] leaveTargets);

			//정해진 대상의 거점을 점령하세요
			return $"{MissionTypeText(missionType)}" +
				$"\n\t{TargetText()}" +
				$"\n\t{ComparisonText()}" +
				$"\n\t{ProgressText()}";

			string TargetText() => $"점령 및 보호 거점: {string.Join(", ", targets)}";
			string ComparisonText() => comparisonType switch
			{
			 	ComparisonType.동등 => $"점령 및 보호 거점의 수: 정확히 {targetCount}",
				ComparisonType.이하 => $"점령 및 보호 거점의 수: {targetCount} 이하",
				ComparisonType.이상 => $"점령 및 보호 거점의 수: {targetCount} 이상",
				_ => ""
			};
			string ProgressText() => $"현재 점령 중인 거점의 수: {computeCount}, 남은 거점: {string.Join(", ", leaveTargets)}";
		}
		private int ComputeCount(in ItemStruct itemStruct, out string[] leaves)
		{
			var targetList = itemStruct.targets.ToList();
			int computeCount = 0;
			StrategyManager.Collector.ForEachControlBase(cb =>
			{
				if (cb.CaptureFactionID == StrategyGamePlayData.PlayerFactionID 
					&& targetList.Remove(cb.ControlBaseName))
				{
					computeCount++;
				}
			});
			leaves = targetList.ToArray();
			return computeCount;
		}
	}
	#endregion
}