using UnityEngine;
public interface IMapSelectable : ISelectable
{
	Vector3 SelectCenter { get; }
	void OnPointEnter() { }
	void OnPointExit() { }
}