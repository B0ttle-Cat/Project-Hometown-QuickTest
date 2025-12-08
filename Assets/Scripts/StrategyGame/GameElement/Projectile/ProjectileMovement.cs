using UnityEngine;

public interface IProjectileMovement
{
	IProjectileMovement ThisMovement { get; }
	int OrderElementID { get; }
	int TargetElementID { get; }
	Vector3 StartPosition{ get; }
    Vector3 TargetPosition { get; }

	public void SetTarget(int orderID, int targetID);
}

public class ProjectileMovement : MonoBehaviour, IProjectileMovement
{
    private int orderElementID;
    private int targetElementID;
    private Vector3 startPosition;
    private Vector3 targetPosition;

    IProjectileMovement IProjectileMovement.ThisMovement => this;

    int IProjectileMovement.OrderElementID { get => orderElementID; }
    int IProjectileMovement.TargetElementID { get => targetElementID; }
    Vector3 IProjectileMovement.StartPosition { get => startPosition; }
    Vector3 IProjectileMovement.TargetPosition { get => targetPosition; }

    void IProjectileMovement.SetTarget(int orderID, int targetID)
    {
        //StrategyManager.Collector.OtherList<ITargetableCombatant>()
    }
}
