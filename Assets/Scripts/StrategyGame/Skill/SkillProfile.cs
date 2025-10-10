using UnityEngine;

[CreateAssetMenu(fileName = "SkillProfile", menuName = "Scriptable Objects/StrategyGame/SkillProfile")]
public class SkillProfile : ScriptableObject
{
	[SerializeField]
	private SkillObject skillPrefab;

	public string skillName;
	public int skillID;

	public int spConsumption;
	public int mpConsumption;
	public int epConsumption;

	public float range;
	public float cooldown;

	public virtual SkillObject Execute(SkillContext context)
	{
		Debug.Log("SkillProfile Execute");
		if (skillPrefab != null)
		{
			Vector3 startPos = StrategyGameManager.Collector.FindUnit(context.CasterUnitID).transform.position;
			Vector3 targetPos = context.TargetUnitID >= 0
				? StrategyGameManager.Collector.FindUnit(context.TargetUnitID).transform.position
				: context.TargetPos;

			Vector3 spawnPos = startPos;
			Quaternion spawnRot = Quaternion.LookRotation(targetPos - startPos, Vector3.up);
			return GameObject.Instantiate<SkillObject>(skillPrefab, spawnPos, spawnRot);
		}
		return null;
	}
}
