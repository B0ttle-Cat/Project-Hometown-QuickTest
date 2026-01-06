using UnityEngine;
public interface IMouseSelectable : ISelectable
{
	Vector3 SelectCenter { get; }
	void OnPointEnter() { }
	void OnPointExit() { }
}