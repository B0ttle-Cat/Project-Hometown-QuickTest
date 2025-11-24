using UnityEngine;

public enum SectorFSMType
{
    None = 0,

}

[RequireComponent(typeof(SectorObject))]
public class SectorFiniteStateMachine : FiniteStateMachine<SectorFSMType>
{
    public override IState<SectorFSMType>[] GetStateList()
	{
		return new IState<SectorFSMType>[]
        {
             new EmptyState(this, SectorFSMType.None),
        };
	}
}
