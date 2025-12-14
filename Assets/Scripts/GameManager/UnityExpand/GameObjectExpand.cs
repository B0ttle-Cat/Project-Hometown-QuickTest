using UnityEngine;

public static class GameObjectExpand
{
	public static void SetDeactive(this GameObject gameObject, bool value)
	{
		gameObject.SetActive(!value);
	}
	public static bool IsNullRef<T>(this T obj) where T : class
	{
		// 1. C# 레벨에서의 순수한 null 검사
		// 이는 Object.ReferenceEquals를 사용하여 오버로드된 == 연산자를 회피합니다.
		if (ReferenceEquals(obj, null))
		{
			return true;
		}

		// 2. UnityEngine.Object 타입으로 캐스팅 가능한지 확인
		// 인터페이스, 델리게이트 등으로 보관된 값이 실제 Unity 컴포넌트나 게임 오브젝트인지 확인합니다.
		if (obj is UnityEngine.Object unityObject)
		{
			// 3. Unity의 오버로드된 == 연산자를 사용하여 유효성 검사
			// 네이티브 오브젝트가 파괴되었으면 true를 반환합니다.
			// *주의: 캐스팅된 unityObject를 == null 로 검사해야 오버로드된 연산자가 작동합니다.*
			if (unityObject == null)
			{
				return true;
			}
			// 네이티브 객체가 유효한 경우 (false)
		}

		// 4. 순수 C# 객체이거나, Unity 객체이지만 파괴되지 않은 경우 (false)
		return false;
	}
	public static bool IsNotNullRef<T>(this T obj) where T : class
	{
		return !IsNullRef(obj);
	}
}