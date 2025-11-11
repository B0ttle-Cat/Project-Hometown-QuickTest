using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public static class EventTriggerHelper
{
	// EventTrigger가 없으면 자동 추가
	private static EventTrigger GetOrAddTrigger(GameObject obj)
	{
		if (!obj.TryGetComponent<EventTrigger>(out var trigger))
			trigger = obj.AddComponent<EventTrigger>();
		trigger.triggers ??= new List<EventTrigger.Entry>();
		return trigger;
	}

	/// <summary>
	/// 지정된 이벤트에 특정 콜백을 등록한다
	/// </summary>
	public static void AddListener(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> callback)
	{
		if (obj == null || callback == null) return;
		EventTrigger trigger = GetOrAddTrigger(obj);
		trigger.AddListener(type, callback);
	}
	public static void AddListener(this EventTrigger trigger, EventTriggerType type, UnityAction<BaseEventData> callback)
	{
		if (trigger == null || callback == null) return;

		// 기존 entry 확인
		EventTrigger.Entry entry = trigger.triggers.Find(e => e.eventID == type);
		if (entry == null)
		{
			entry = new EventTrigger.Entry { eventID = type };
			trigger.triggers.Add(entry);
		}
		entry.callback.RemoveListener(callback);
		entry.callback.AddListener(callback);
	}

	public static void AddListener(this EventTrigger trigger, params (EventTriggerType type, UnityAction<BaseEventData> callback)[] eventParams)
	{
		if (trigger == null || eventParams == null || eventParams.Length == 0) return;
		int length = eventParams.Length;
		for (int i = 0 ; i < length ; i++)
		{
			(EventTriggerType type, UnityAction<BaseEventData> callback) = eventParams[i];
			trigger.AddListener(type, callback);

		}
	}

	/// <summary>
	/// 지정된 이벤트 타입에서 특정 콜백만 제거한다.
	/// </summary>
	public static void RemoveListener(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> callback)
	{
		if (obj == null || callback == null) return;

		if (!obj.TryGetComponent<EventTrigger>(out var trigger)) return;

		trigger.RemoveListener(type, callback);
	}
	public static void RemoveListener(this EventTrigger trigger, EventTriggerType type, UnityAction<BaseEventData> callback)
	{
		if (trigger == null || callback == null) return;

		foreach (var entry in trigger.triggers)
		{
			if (entry.eventID == type)
				entry.callback.RemoveListener(callback);
		}
	}
	public static void RemoveListener(this EventTrigger trigger, params (EventTriggerType type, UnityAction<BaseEventData> callback)[] eventParams)
	{
		if (trigger == null || eventParams == null || eventParams.Length == 0) return;
		int length = eventParams.Length;
		for (int i = 0 ; i < length ; i++)
		{
			(EventTriggerType type, UnityAction<BaseEventData> callback) = eventParams[i];
			trigger.RemoveListener(type, callback);

		}
	}
	/// <summary>
	/// 모든 이벤트를 제거한다.
	/// type 이 있으면 해당 타입의 이벤트를 모두 제거한다.
	/// </summary>
	public static void RemoveAllListener(GameObject obj, EventTriggerType? type = null)
	{
		if (obj == null) return;
		if (!obj.TryGetComponent<EventTrigger>(out var trigger)) return;
		trigger.RemoveAllListener(type);
	}
	public static void RemoveAllListener(this EventTrigger trigger, EventTriggerType? type = null)
	{
		if (trigger == null) return;
		if (type.HasValue)
		{
			trigger.triggers.RemoveAll(e => e.eventID == type);
		}
		else
		{
			trigger.triggers.Clear();
		}
	}
}
