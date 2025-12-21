using System;
using System.Threading;

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
			if (IsShow) return; IsShow = true;

			Show();
		}
		void OnHide()
		{
			if (IsHide) return; IsHide = true;

			Hide();
		}

		[Obsolete("이 메서드를 직접 호출하지 말것")]
		void Show();
		[Obsolete("이 메서드를 직접 호출하지 말것")]
		void Hide();
	}
	public interface IAsyncShowHide : IShowHide
	{
		IShowHide IShowHide.ThisShowHide => this;
		IAsyncShowHide ThisAsyncShowHide { get; }

		bool IsAwait => ShowHideCancellationTokenSource != null;
		CancellationTokenSource ShowHideCancellationTokenSource { get; set; }
		void IShowHide.OnShow()
		{
			if (IsShow) return; IsShow = true;
			if (IsAwait)
			{
				ShowHideCancellationTokenSource.Cancel();
				ShowHideCancellationTokenSource.Dispose();
				ShowHideCancellationTokenSource = null;
			}

			Show();
		}
		void IShowHide.OnHide()
		{
			if (IsHide) return; IsHide = true;
			if (IsAwait)
			{
				ShowHideCancellationTokenSource.Cancel();
				ShowHideCancellationTokenSource.Dispose();
				ShowHideCancellationTokenSource = null;
			}

			Hide();
		}
		void OnSkipAwait()
		{
			if (IsAwait)
			{
				ShowHideCancellationTokenSource.Cancel();
				ShowHideCancellationTokenSource.Dispose();

				if (IsShow) Show();
				else Hide();
			}
		}
		async void OnShowAsync() => await OnShow(null, null);
		async void OnHideAsync() => await OnShow(null, null);
		async void OnShowAsync(Action onShow) => await OnShow(onShow, null);
		async void OnHideAsync(Action onHide) => await OnShow(onHide, null);
		async void OnShowAsync(Action onShow, Action cancelCallback) => await OnShow(onShow, cancelCallback);
		async void OnHideAsync(Action onHide, Action cancelCallback) => await OnShow(onHide, cancelCallback);
		async Awaitable OnShow(Action cancelCallback) => await OnShow(null, cancelCallback);
		async Awaitable OnHide(Action cancelCallback) => await OnHide(null, cancelCallback);
		async Awaitable OnShow(Action onShow, Action cancelCallback)
		{
			if (IsShow) return; IsShow = true;

			if (IsAwait)
			{
				ShowHideCancellationTokenSource.Cancel();
				ShowHideCancellationTokenSource.Dispose();
			}
			ShowHideCancellationTokenSource = new CancellationTokenSource();

			var token = ShowHideCancellationTokenSource.Token;
			using (CancellationTokenRegistration registration = cancelCallback != null ? token.Register(cancelCallback) : default)
			{
				try
				{
					await Show(token);
				}
				catch (OperationCanceledException) { Debug.Log("Show Async가 취소됨"); }
				catch (Exception ex) { Debug.LogException(ex); }
				finally
				{
					IsShow = false;
					ShowHideCancellationTokenSource.Dispose();
					ShowHideCancellationTokenSource = null;
				}
			}

			if (token.IsCancellationRequested) return;
			Show();
			onShow?.Invoke();
		}
		async Awaitable OnHide(Action onHide, Action cancelCallback)
		{
			if (IsHide) return; IsHide = true;

			if (IsAwait)
			{
				ShowHideCancellationTokenSource.Cancel();
				ShowHideCancellationTokenSource.Dispose();
			}
			ShowHideCancellationTokenSource = new CancellationTokenSource();

			var token = ShowHideCancellationTokenSource.Token;
			using (CancellationTokenRegistration registration = cancelCallback != null ? token.Register(cancelCallback) : default)
			{
				try
				{
					await Hide(token);
				}
				catch (OperationCanceledException) { Debug.Log("Hide Async가 취소됨"); }
				catch (Exception ex) { Debug.LogException(ex); }
				finally
				{
					IsHide = false;
					ShowHideCancellationTokenSource.Dispose();
					ShowHideCancellationTokenSource = null;
				}
			}

			if (token.IsCancellationRequested) return;
			Hide();
			onHide?.Invoke();
		}
		[Obsolete("이 메서드를 직접 호출하지 말것")]
		Awaitable Show(CancellationToken cancellationToken);
		[Obsolete("이 메서드를 직접 호출하지 말것")]
		Awaitable Hide(CancellationToken cancellationToken);
	}
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
}
