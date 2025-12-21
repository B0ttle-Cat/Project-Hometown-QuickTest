using System;
using System.Collections.Generic;

namespace GameUI
{
	public interface IKeyPairValue<TKey, TValue> : IDisposable
		where TValue : class, IPanelItem
	{
		IKeyPairValue<TKey, TValue> KeyPair { get; }
		Dictionary<TKey, TValue> Dictionary { get; }
		public IKeyPairValue<TKey, TValue> Find(TKey key, out TValue value)
		{
			if (KeyPair.IsNullRef() || Dictionary == null)
			{
				value = null;
				return this;
			}
			if (!Dictionary.TryGetValue(key, out value) || value.IsNullRef())
			{
				value = null;
			}
			return this;
		}
		public IKeyPairValue<TKey, TValue> Finds(TKey[] keys, out TValue[] values)
		{
			if (keys == null || keys.Length == 0)
			{
				values = new TValue[0];
				return this;
			}
			int length = keys.Length;
			values = new TValue[length];

			for (int i = 0 ; i < length ; i++)
			{
				Find(keys[i], out var find);
				values[i] = find.IsNullRef() ? null : find;
			}
			return this;
		}
		public void Add(TKey key, TValue value)
		{
			if (KeyPair.IsNullRef() || Dictionary == null) return;

			if (Dictionary.ContainsKey(key))
			{
				Dictionary[key] = value;
			}
			else
			{
				Dictionary.Add(key, value);
			}
		}
	}
}
