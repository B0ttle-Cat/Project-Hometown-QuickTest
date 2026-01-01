using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

namespace GameUI
{

	[RequireComponent(typeof(RectTransform))]
	public abstract partial class GameUIController : MonoBehaviour
		, IShowStackController
		, IFindUIObject
		, IPanelItem
	{
		public IShowStackController StackController => this;
		public IFindUIObject ThisUIFinder => this;
		public IPanelItem ThisPanel => this;
		public GameUIController RootUI { get => this; set { } }
		public RectTransform ThisRect
		{
			get
			{
				if (thisRect.IsNullRef())
					thisRect = GetComponent<RectTransform>();
				return thisRect;
			}
		}

		[SerializeField, PropertyOrder(-10000)]
		private RectTransform thisRect;
		[SerializeField, PropertyOrder(-9998)]
		private List<IFindUIObject.KeyPairObject> keyPairs;

#if UNITY_EDITOR
		[ShowInInspector, InlineButton("TestKeyPair"), PropertyOrder(-9999)]
		private string testKeyPair { get; set; }
		void TestKeyPair(string key)
		{
			int count = ThisUIFinder.KeyPairs == null ? 0 : ThisUIFinder.KeyPairs.Count;
			for (int i = 0 ; i < count ; i++)
			{
				ThisUIFinder.KeyPairs[i].testFindit = ThisUIFinder.TestIsPathMatch(key, ThisUIFinder.KeyPairs[i].Key);
			}
		}
#endif
		public virtual IShowStackController.GroupShowStack ShowStack { get; protected set; }
		List<IFindUIObject.KeyPairObject> IFindUIObject.KeyPairs { get => keyPairs; set => keyPairs = value; }

		protected virtual void Awake()
		{
			ShowStack = new IShowStackController.GroupShowStack();
		}
		protected virtual void OnDestroy()
		{
			Dispose();
		}
		public void Dispose()
		{
			ShowStack?.Clear();
			ShowStack = null;
		}

#if UNITY_EDITOR
		[ButtonGroup, PropertyOrder(-9997)]
		private void TestShow()
		{
			if (this is not IShowHide showHide) return;
			showHide.OnShow();
		}
		[ButtonGroup, PropertyOrder(-9997)]
		private void TestHide()
		{
			if (this is not IShowHide showHide) return;
			showHide.OnHide();
		}
#endif
		public void OnShow()
		{
			Show();
		}
		public void OnHide()
		{
			Hide();
		}
		protected abstract void Show();
		protected abstract void Hide();

	}

	public interface IShowHideController
	{
		public void OnShow(IShowHide showHide, Action awaitCallback = null);
		public void OnHide(IShowHide showHide, Action awaitCallback = null);
		public void OnShowImmediate(IShowHide showHide);
		public void OnHideImmediate(IShowHide showHide);
		public void PairingShowHide(IShowHide thisShowHide);
		public void UnpairingShowHide(IShowHide thisShowHide);
	}
	public abstract partial class GameUIController : IShowHideController
	{
		private static readonly Dictionary<RectTransform, HashSet<IShowHide>> pairingShowHide = new ();
		private static readonly Dictionary<RectTransform, CancellationTokenSource> pairingToken = new ();

		public HashSet<IShowHide> GetPairingSameRect(IShowHide thisShowHide)
		{
			if (thisShowHide.IsNullRef()) return new HashSet<IShowHide>() { thisShowHide };
			thisRect = thisShowHide.ThisRect;
			if (thisRect.IsNullRef()) return new HashSet<IShowHide>() { thisShowHide };
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				IShowHide[] showHides = thisRect.GetComponents<IShowHide>();
				var list = showHides.ToHashSet();
				list.Add(thisShowHide);
				return list;
			}
#endif
			if (pairingShowHide.ContainsKey(thisRect))
			{
				var list = pairingShowHide[thisRect];
				list.Add(thisShowHide);
				return list;
			}
			return new HashSet<IShowHide>() { thisShowHide };
		}
		void IShowHideController.PairingShowHide(IShowHide thisShowHide)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif
			if (thisShowHide.IsNullRef()) return;
			thisRect = thisShowHide.ThisRect;
			if (thisRect.IsNullRef()) return;

			if (!pairingShowHide.ContainsKey(thisRect))
			{
				pairingShowHide.Add(thisRect, new HashSet<IShowHide>());
			}
			pairingShowHide[thisRect].Add(thisShowHide);
		}
		void IShowHideController.UnpairingShowHide(IShowHide thisShowHide)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif

			if (thisShowHide.IsNullRef()) return;
			thisRect = thisShowHide.ThisRect;
			if (thisRect.IsNullRef()) return;

			if (pairingShowHide.ContainsKey(thisRect))
			{
				pairingShowHide[thisRect].Remove(thisShowHide);
			}
			else
			{
				if (thisRect.IsNullRef())
				{
					foreach (var pairing in pairingShowHide)
					{
						var removeItem = new HashSet<IShowHide>();
						removeItem.Remove(thisShowHide);
					}
				}
			}
		}

#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
		void IShowHideController.OnShow(IShowHide showHide, Action awaitCallback)
		{
			if (showHide.IsNullRef()) return;
			if (showHide.IsShow) return;
			var thisRect = showHide.ThisRect;
			if (thisRect.IsNullRef()) return;

			CancellationTokenSource tokenSource = null;

			if (pairingToken.ContainsKey(thisRect))
			{
				tokenSource = pairingToken[thisRect];
				pairingToken.Remove(thisRect);
				tokenSource.Cancel();
				tokenSource.Dispose();
				tokenSource = null;
			}

			HashSet<IShowHide> pairing = GetPairingSameRect(showHide);
			HashSet<Func<CancellationToken, Awaitable>> async = new();
			HashSet<Action> sync = new();
			foreach (var item in pairing)
			{
				if (item.IsShow) continue;
				item.IsShow = true;
				if (item is IShowHideAsync showHideAsync)
				{
					if (tokenSource == null)
					{
						tokenSource = new CancellationTokenSource();
						pairingToken.Add(thisRect, tokenSource);
					}
					async.Add(showHideAsync.AsyncShow);
				}
				sync.Add(item.EndedShow);

				item.StartShow();
			}

			ShowHideExecute(thisRect, tokenSource, async, sync, awaitCallback);
		}
		void IShowHideController.OnHide(IShowHide showHide, Action awaitCallback)
		{
			if (showHide.IsNullRef()) return;
			if (showHide.IsHide) return;
			var thisRect = showHide.ThisRect;
			if (thisRect.IsNullRef()) return;

			CancellationTokenSource tokenSource = null;

			if (pairingToken.ContainsKey(thisRect))
			{
				tokenSource = pairingToken[thisRect];
				pairingToken.Remove(thisRect);
				tokenSource.Cancel();
				tokenSource.Dispose();
				tokenSource = null;
			}

			HashSet<IShowHide> pairing = GetPairingSameRect(showHide);
			HashSet<Func<CancellationToken, Awaitable>> async = new();
			HashSet<Action> sync = new();
			foreach (var item in pairing)
			{
				if (item.IsHide) continue;
				item.IsHide = true;
				if (item is IShowHideAsync showHideAsync)
				{
					if (tokenSource == null)
					{
						tokenSource = new CancellationTokenSource();
						pairingToken.Add(thisRect, tokenSource);
					}
					async.Add(showHideAsync.AsyncHide);
				}
				sync.Add(item.EndedHide);

				item.StartHide();
			}

			ShowHideExecute(thisRect, tokenSource, async, sync, awaitCallback);
		}
		void IShowHideController.OnShowImmediate(IShowHide showHide)
		{
			if (showHide.IsNullRef()) return;
			if (showHide.IsShow) return;
			var thisRect = showHide.ThisRect;
			if (thisRect.IsNullRef()) return;

			if (pairingToken.ContainsKey(thisRect))
			{
				CancellationTokenSource tokenSource = pairingToken[thisRect];
				pairingToken.Remove(thisRect);
				tokenSource.Cancel();
				tokenSource.Dispose();
				tokenSource = null;
			}

			HashSet<IShowHide> pairing = GetPairingSameRect(showHide);
			foreach (var item in pairing)
			{
				if (item.IsShow) continue;
				item.IsShow = true;
				item.EndedShow();
			}
		}
		void IShowHideController.OnHideImmediate(IShowHide showHide)
		{
			if (showHide.IsNullRef()) return;
			if (showHide.IsHide) return;
			var thisRect = showHide.ThisRect;
			if (thisRect.IsNullRef()) return;

			if (pairingToken.ContainsKey(thisRect))
			{
				CancellationTokenSource tokenSource = pairingToken[thisRect];
				pairingToken.Remove(thisRect);
				tokenSource.Cancel();
				tokenSource.Dispose();
				tokenSource = null;
			}

			HashSet<IShowHide> pairing = GetPairingSameRect(showHide);
			foreach (var item in pairing)
			{
				if (item.IsHide) continue;
				item.IsHide = true;
				item.EndedHide();
			}
		}
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
		private async void ShowHideExecute(RectTransform thisRect, CancellationTokenSource tokenSource, HashSet<Func<CancellationToken, Awaitable>> async, HashSet<Action> sync, Action awaitCallback)
		{
			int asyncCount = async.Count;
			if (asyncCount > 0)
			{
				if(tokenSource.IsCancellationRequested) return;

				CancellationToken token = tokenSource.Token;
#if UNITY_EDITOR
				EditrUpdate(token);
#endif
				foreach (var item in async)
				{
					Async(item, token);
				}
				while (asyncCount > 0)
				{
					await Awaitable.NextFrameAsync();
				}

				if (thisRect.IsNullRef())
				{
					if (tokenSource != null)
					{
						tokenSource.Dispose();
						tokenSource = null;
					}

					List<RectTransform> keysToRemove = new List<RectTransform>();
					foreach (var item in pairingToken.Keys)
					{
						keysToRemove.Add(item);
					}
					int removeCount = keysToRemove.Count;
					for (int i = 0 ; i < removeCount ; i++)
					{
						pairingToken.Remove(keysToRemove[i]);
					}
				}
				else
				{
					if (pairingToken.Remove(thisRect))
					{
						if (tokenSource != null)
						{
							tokenSource.Dispose();
							tokenSource = null;
						}
					}
				}		 

				if (token.IsCancellationRequested)
					return;
			}
			if (sync.Count > 0)
			{
				foreach (var item in sync)
				{
					item();
				}
			}

			awaitCallback?.Invoke();
			async void Async(Func<CancellationToken, Awaitable> task, CancellationToken token)
			{
				try
				{
					await task(token);
				}
				catch (OperationCanceledException)
				{
					Debug.Log("EndedShow Async가 취소됨");
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
				finally
				{
					asyncCount--;
				}
			}
#if UNITY_EDITOR
			async void EditrUpdate(CancellationToken token)
			{
				if (!Application.isPlaying)
				{
					const long TIMEOUT_MS = 30000; // 30초

					System.Diagnostics.Stopwatch timeoutStopwatch = System.Diagnostics.Stopwatch.StartNew();
					int delta = Mathf.FloorToInt(Time.fixedDeltaTime * 1000);
					int minDelta = Mathf.FloorToInt((1f / 60f) * 1000f);
					if (delta < minDelta)
					{
						delta = minDelta;
					}
					while (asyncCount > 0)
					{
						if (token.IsCancellationRequested) break;
						if (timeoutStopwatch.ElapsedMilliseconds >= TIMEOUT_MS)
						{
							UnityEngine.Debug.LogWarning("ShowHideAsync: 30초 타임아웃으로 에디터 비동기 루프를 강제 종료합니다.");
							break;
						}

						// Task가 완료될 때까지 에디터 루프를 강제로 트리거
						EditorApplication.QueuePlayerLoopUpdate();
						UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
						await Task.Delay(delta);
					}
					timeoutStopwatch.Stop();
				}
			}
#endif
		}
	}
}
