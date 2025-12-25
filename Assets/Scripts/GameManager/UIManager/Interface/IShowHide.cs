using System;
using System.Threading;
using System.Threading.Tasks;

using UnityEditor;

using UnityEngine;

namespace GameUI
{
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
	public interface IShowHide
	{
		IShowHide ThisShowHide { get; }
		bool IsShow { get; set; }
		bool IsHide { get => !IsShow; set => IsShow = !value; }
		void OnShow()
		{
			OnShowImmediate();
		}
		void OnHide()
		{
			OnHideImmediate();
		}

		[Obsolete("특수한 경우가 아니면, 이 메서드를 직접 호출하지 말것")]
		void Show();
		[Obsolete("특수한 경우가 아니면, 이 메서드를 직접 호출하지 말것")]
		void Hide();

		void OnShowImmediate()
		{
			if (IsShow) return; IsShow = true;
			Show();
		}
		void OnHideImmediate()
		{
			if (IsHide) return; IsHide = true;
			Hide();
		}
	}
	public interface IShowHideAsync : IShowHide
	{
		new IShowHideAsync ThisShowHide { get; }

		bool IsAwait => ShowHideCancellationTokenSource != null;
		CancellationTokenSource ShowHideCancellationTokenSource { get; set; }
		async void IShowHide.OnShow()
		{
			await OnShowAsync(null, null);
		}
		async void IShowHide.OnHide()
		{
			await OnHideAsync(null, null);
		}
		void OnSkipAwait()
		{
			if (IsAwait)
			{
				ShowHideCancellationTokenSource.Cancel();
				TokenSourceDispose();

				if (IsShow) Show();
				else Hide();
			}
		}
		async void OnShow(Action onShow) => await OnShowAsync(onShow, null);
		async void OnHide(Action onHide) => await OnHideAsync(onHide, null);
		async void OnShow(Action onShow, Action cancelCallback) => await OnShowAsync(onShow, cancelCallback);
		async void OnHide(Action onHide, Action cancelCallback) => await OnHideAsync(onHide, cancelCallback);
		async Awaitable OnShowAsync(Action cancelCallback) => await OnShowAsync(null, cancelCallback);
		async Awaitable OnHideAsync(Action cancelCallback) => await OnHideAsync(null, cancelCallback);
		async Awaitable OnShowAsync(Action onShow, Action cancelCallback)
		{
			if (IsShow) return; IsShow = true;

			if (IsAwait)
			{
				ShowHideCancellationTokenSource.Cancel();
				TokenSourceDispose();
			}
			ShowHideCancellationTokenSource = new CancellationTokenSource();

			var token = ShowHideCancellationTokenSource.Token;
			using (CancellationTokenRegistration registration = cancelCallback != null ? token.Register(cancelCallback) : default)
			{
				try
				{
#if UNITY_EDITOR
					await ExecuteWithEditorLoop(Show(token), token);
#else
					await Show(token);
#endif
				}
				catch (OperationCanceledException)
				{
					IsShow = false;
					Debug.Log("Show Async가 취소됨");
				}
				catch (Exception ex)
				{
					IsShow = false;
					Debug.LogException(ex);
				}
				finally
				{
					TokenSourceDispose();
				}
			}

			if (token.IsCancellationRequested) return;
			Show();
			onShow?.Invoke();
		}
		async Awaitable OnHideAsync(Action onHide, Action cancelCallback)
		{
			if (IsHide) return; IsHide = true;

			if (IsAwait)
			{
				ShowHideCancellationTokenSource.Cancel();
				TokenSourceDispose();
			}
			ShowHideCancellationTokenSource = new CancellationTokenSource();

			var token = ShowHideCancellationTokenSource.Token;
			using (CancellationTokenRegistration registration = cancelCallback != null ? token.Register(cancelCallback) : default)
			{
				try
				{
#if UNITY_EDITOR
					await ExecuteWithEditorLoop(Hide(token), token);
#else
					await Hide(token);
#endif
				}
				catch (OperationCanceledException)
				{
					IsHide = false;
					Debug.Log("Hide Async가 취소됨");
				}
				catch (Exception ex)
				{
					IsHide = false;
					Debug.LogException(ex);
				}
				finally
				{
					TokenSourceDispose();

				}
			}

			if (token.IsCancellationRequested) return;
			Hide();
			onHide?.Invoke();
		}

		void IShowHide.OnShowImmediate()
		{
			OnSkipAwait();
			if (IsShow) return; IsShow = true;
			Show();
		}
		void IShowHide.OnHideImmediate()
		{
			OnSkipAwait();
			if (IsHide) return; IsHide = true;
			Hide();
		}

		[Obsolete("특수한 경우가 아니면, 이 메서드를 직접 호출하지 말것")]
		Awaitable Show(CancellationToken cancellationToken);
		[Obsolete("특수한 경우가 아니면, 이 메서드를 직접 호출하지 말것")]
		Awaitable Hide(CancellationToken cancellationToken);


		private void TokenSourceDispose()
		{
			if (ShowHideCancellationTokenSource != null)
			{
				ShowHideCancellationTokenSource.Dispose();
				ShowHideCancellationTokenSource = null;

				Debug.Log("TokenSourceDispose");
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// 에디터 모드에서 PlayerLoop가 멈추지 않도록 강제로 업데이트를 큐잉하며 대기합니다.
		/// </summary>
		private async Awaitable ExecuteWithEditorLoop(Awaitable task, CancellationToken token)
		{
			EditrUpdate();
			await task;
			async void EditrUpdate()
			{
				if (!Application.isPlaying)
				{
					// Task가 완료될 때까지 에디터 루프를 강제로 트리거
					while (IsAwait)
					{
						if (token.IsCancellationRequested) break;

						EditorApplication.QueuePlayerLoopUpdate();
						UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
						int delta = Mathf.FloorToInt((1f / 60f) * 1000f);
						await Task.Delay(delta);
					}
				}
			}
		}
#endif
	}
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
}
