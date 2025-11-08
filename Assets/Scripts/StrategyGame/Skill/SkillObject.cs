using UnityEngine;

public partial class SkillObject : MonoBehaviour
{
	public void Init(SkillContext skillContext, SkillProfile skillProfile)
	{
		casterUnitID = skillContext.CasterUnitID;
		skillID = skillContext.SillID;
		skillName = skillContext.SkillName;
	}
	public string SkillName { get => skillName; set => skillName = value; }
	public int SkillID { get => skillID; set => skillID = value; }
	public UnitObject CasterUnit
	{
		get => StrategyManager.Collector.FindUnit(casterUnitID);
		set => casterUnitID = value == null ? -1 : value.UnitID;
	}

	private string skillName;
	private int skillID;
	private int casterUnitID;
	private int skillInstanceID;
}

public partial class SkillObject : IStrategyElement
{
	public bool IsInCollector { get; set; }

	public IStrategyElement ThisElement => this;
	int IStrategyElement.ID { get => skillID; set => skillID = value; }

    public void InStrategyCollector()
	{
	}

	public void OutStrategyCollector()
	{
	}

    void IStartGame.OnStartGame()
    {
    }

    void IStartGame.OnStopGame()
    {
    }
}