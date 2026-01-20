using System.Collections.Generic;

namespace StrategyManagerModule
{
	public interface IStrategyProcess
	{
		IStrategyProcess ThisProcess { get; }
		void OnInit() { }
		void OnStart() { }
		void OnStop() { }
		void Update() { }
		List<ProcessOverrider> OverriderList { get; }
		void OnRemoveProcessOverride(ProcessOverrider processOverride)
		{
			OverriderList.Remove(processOverride);
		}
		void OnAddProcessOverride(ProcessOverrider processOverride)
		{
			if (OverriderList.Contains(processOverride))
			{
				if(OverriderList[^1] == processOverride) return;
				OverriderList.Remove(processOverride);
				OverriderList.Add(processOverride);
			}
			else
			{
				OverriderList.Add(processOverride);
			}
			
		}
		bool TryGetProcessOverrider<T>(out T processOverrider) where T : ProcessOverrider
		{
			int length = OverriderList == null ? 0 : OverriderList.Count;
			for (int i = length - 1 ; i >= 0 ; i--)
			{
				var item = OverriderList[i];
				if (item.IsNullRef()) continue;
				if (item is not T tItem) continue;
				processOverrider = tItem;
				return true;
			}
			processOverrider = null;
			return false;
		}
		bool HasProcessOverride<T>()
		{
			int length = OverriderList == null ? 0 : OverriderList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = OverriderList[i];
				if (item.IsNullRef()) continue;
				if (item is not T tItem) continue;
				return true;
			}
			return false;
		}

		void OnClaerAll()
		{
			if(OverriderList == null || OverriderList.Count == 0) return;
			OverriderList.Clear();
		}
		void OnClaerType<T>(bool equalType = false)
		{
			int length = OverriderList == null ? 0 : OverriderList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = OverriderList[i];
				if (item.IsNullRef()) continue;
				if (equalType)
				{
					if (item.GetType() != typeof(T)) continue;
				}
				else
				{
					if (item is not T) continue;
				}
				OverriderList.RemoveAt(i);
				--length;
				--i;
			}
		}
	}

}