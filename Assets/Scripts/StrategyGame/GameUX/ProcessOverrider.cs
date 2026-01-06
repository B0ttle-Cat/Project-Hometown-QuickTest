using System;
using System.Collections.Generic;

namespace StrategyManagerModule
{
	public abstract record ProcessOverrider : IDisposable
	{
		public bool isDisposable;
		public object Equality;
		protected ProcessOverrider(object equality)
		{
			Equality = equality;
			isDisposable = false;
			OnOverride();
		}
		public void Dispose()
		{
			if (isDisposable) return;
			isDisposable = true;
			OnDispose();
		}
		public void OnProcess()
		{
			isDisposable = false;
			OnOverride();
		}
		protected abstract void OnOverride();
		protected abstract void OnDispose();
        public virtual bool Equals(ProcessOverrider overrider)
        {
            return overrider is not null &&
                   EqualityComparer<object>.Default.Equals(Equality, overrider.Equality);
        }
        public override int GetHashCode()
        {
            return System.HashCode.Combine(Equality);
        }
    }

}