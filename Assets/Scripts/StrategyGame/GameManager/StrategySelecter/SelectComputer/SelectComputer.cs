using UnityEngine;

namespace StrategyManagerModule
{
	public abstract class SelectComputer : MonoBehaviour
	{
		public abstract void Init(StrategySelecter selecter);
		public abstract void Deinit();
		public abstract bool IsVaild();
		public abstract void InputUpdate();
		public abstract void Compute();
	}

}