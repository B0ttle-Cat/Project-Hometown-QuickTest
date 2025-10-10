using System.Collections.Generic;

using UnityEngine;

public class StrategyGameUpdate : MonoBehaviour
{
	private StrategyElementCollector collector;
	private List<ControlBase> controlBases;
	public void OnEnable()
	{
		collector = GetComponent<StrategyElementCollector>();
		controlBases = collector.ControlBaseList;
	}
	public void OnDisable()
	{
		
	}

	private void Update()
	{
		Update_ControlBase();

		void Update_ControlBase()
		{
			int count = controlBases.Count;
			for (int i = 0 ; i < count ; i++)
			{
				var target = controlBases[i];
				if (target == null || !target.enabled) continue;

				target.UpdateControlBase();
			}
		}
	}
	private void LateUpdate()
	{	
	}





}
