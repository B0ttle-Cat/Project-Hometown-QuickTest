using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

public interface IViewAndControlModeChange
{
	public void OnChangeMode(ViewAndControlModeType changeMode);
}
public enum ViewAndControlModeType
{
	None = 0,
	OperationsMode,
	TacticsMode,
}

public class StrategyViewAndControlModeChanger : MonoBehaviour, IStrategyStartGame
{
	[SerializeField, HideInPlayMode]
	private ViewAndControlModeType startingMode;
	[SerializeField,ReadOnly]
	private ViewAndControlModeType currentMode;
	private IViewAndControlModeChange[] interfaceList;
	public ViewAndControlModeType CurrentMode => currentMode;
	private void Awake()
	{
		currentMode = ViewAndControlModeType.None;
	}
	public void Init()
	{
		var allComponent = GameObject.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
		interfaceList = allComponent.Where(c => c is IViewAndControlModeChange).Select(c => c as IViewAndControlModeChange).ToArray();
	}
	[Button(Style = ButtonStyle.CompactBox)]
	public void ModeChange(ViewAndControlModeType changeMode)
	{
		if (currentMode == changeMode) return;
		currentMode = changeMode;
		int length = interfaceList == null ? 0 : interfaceList.Length;
		for (int i = 0 ; i < length ; i++)
		{
			var item = interfaceList[i];
			if (item == null) continue;
			item.OnChangeMode(changeMode);
		}
	}
	void IStrategyStartGame.OnStartGame()
	{
		ModeChange(startingMode);
	}
	void IStrategyStartGame.OnStopGame()
	{
		ModeChange(ViewAndControlModeType.None);
	}

#if UNITY_EDITOR
	// === 테스트용 런타임 버튼 ===
	private void OnGUI()
	{
		const float width = 160f;
		const float height = 40f;
		float x = 10f;
		float y = 10f;

		foreach (ViewAndControlModeType mode in System.Enum.GetValues(typeof(ViewAndControlModeType)))
		{
			GUI.enabled = currentMode != mode;

			if (GUI.Button(new Rect(x, y, width, height), $"Switch: {mode}"))
			{
				ModeChange(mode);
			}

			y += height + 5f;
		}
		GUI.enabled = true;
	}
#endif
}
