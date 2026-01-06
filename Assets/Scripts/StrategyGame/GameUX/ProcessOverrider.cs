using System;

namespace StrategyManagerModule
{
	public partial class StrategyGameUX
    {
        public abstract class ProcessOverrider : IDisposable
		{
			public bool isDisposable;
			protected ProcessOverrider()
			{
				isDisposable = false;
				OnOverride();
			}

			public void Dispose()
			{
				if (isDisposable) return;
				isDisposable = true;
				OnDispose();
			}
			protected abstract void OnOverride();
			protected abstract void OnDispose();
		}
	}

}