using UnityEngine;

public interface ISelectableByMouse
{
	Vector3 ClickCenter { get; }
	bool IsPointEnter { get; set; }
	bool IsSelectMouse { get; set; }
	void OnPointEnter() { }
	void OnPointExit() { }
	bool HasPass(out ISelectableByMouse pass)
	{
		pass = null;
		return false;
	}
	bool OnSelect();
	bool OnDeselect();
	void OnSingleSelect();
	void OnSingleDeselect();
	void OnFirstSelect();
	void OnLastDeselect();
}
