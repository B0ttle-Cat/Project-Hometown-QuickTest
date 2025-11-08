using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

using static StrategyGamePlayData;
[CreateAssetMenu(fileName = "KeyPairUnitInfo", menuName = "Scriptable Objects/KeyPairAssets/KeyPairUnitInfo")]
public class KeyPairUnitInfo : KeyPairAssets<StrategyGamePlayData.UnitKey, KeyPairUnitInfo.UnitInfo>
{
	public static KeyPairUnitInfo Load(Language.Type type, string name)
	{
		string path = $"{nameof(KeyPairUnitInfo)}/{type.ToString()}/{name}";
		Debug.Log(path);
		return Resources.Load<KeyPairUnitInfo>(path);
	}

#if UNITY_EDITOR
	#region SetMachineNames

	[ButtonGroup(order: -5)]
	void SetMachineNames()
	{
		foreach (var item in MachineUnitNames)
		{
			AddAsset(item.Key, new(item.Key, item.Value));
		}
	}

	// ┌───────────────────────────────────────────────────────────────────────────────────────┐
	// │    ■ 분류코드 구조 : [등급+주무기분류+이동방식] / [역할+중요도] / [무기특성+장갑]     │
	// ├───────────────────────────────────────────────────────────────────────────────────────┤
	// │ 크기 등급  : S=소형, M=중형, L=대형, X=초대형                                         │
	// │ 주무기분류 : G=사격(소총), C=기관포, T=전차포, R=로켓/미사일, F=화염, E=에너지        │
	// │ 이동방식   : D=드론, W=보행/차륜, K=궤도형, H=공중부양                                │
	// │ 전장역할   : A=공격, D=방어, B=돌파, S=지원                                           │
	// │ 역할기여   : P=주력, S=보조                                                           │
	// │ 무기특성   : N=일반, P=관통, PP=관통특화, X=폭발, XX=폭발특화, E=에너지               │
	// │ 장갑등급   : N=일반, L=경장갑, M=중장갑, R=강화장갑, S=역장                           │
	// └───────────────────────────────────────────────────────────────────────────────────────┘
	Dictionary<StrategyGamePlayData.UnitKey, string> MachineUnitNames = new Dictionary<StrategyGamePlayData.UnitKey, string>
	{
		// ---------- 기계 세력 - 소형 (1100)
		[StrategyGamePlayData.UnitKey.기계_소형_사격드론]           = "SGD-AP-PL 사격형드론",                   // 소형_사격_드론     / 공격_주력 / 관통_경장갑
		[StrategyGamePlayData.UnitKey.기계_소형_자폭드론]           = "SRD-BP-XL 돌격형자폭드론",               // 소형_로켓_드론     / 돌파_주력 / 폭발_경장갑
		[StrategyGamePlayData.UnitKey.기계_소형_소총로봇]           = "SWG-AP-PL 전투보행로봇",                 // 소형_소총_보행     / 공격_주력 / 관통_경장갑
		[StrategyGamePlayData.UnitKey.기계_소형_로켓로봇]           = "SWR-AP-XL 로켓보행로봇",                 // 소형_로켓_보행     / 공격_주력 / 폭발_경장갑
		[StrategyGamePlayData.UnitKey.기계_소형_정찰드론]           = "SGD-SP-NL 정찰드론",                     // 소형_사격_드론     / 지원_보조 / 일반_경장갑
		[StrategyGamePlayData.UnitKey.기계_소형_전파방해드론]       = "SED-SS-EL 전파교란드론",                 // 소형_에너지_드론   / 지원_보조 / 에너지_경장갑
		[StrategyGamePlayData.UnitKey.기계_소형_스텔스드론]         = "SGD-AS-NL 스텔스드론",                   // 소형_사격_드론     / 공격_보조 / 일반_경장갑
																																		  
		[StrategyGamePlayData.UnitKey.기계_중형_사격드론]           = "MGD-AP-PL 하이에나 전투 드론",           // 중형_사격_드론     / 공격_주력 / 관통_경장갑
		[StrategyGamePlayData.UnitKey.기계_중형_로켓드론]           = "MRD-AP-XL 호넷 로켓 드론",               // 중형_로켓_드론     / 공격_주력 / 폭발_경장갑
		[StrategyGamePlayData.UnitKey.기계_중형_기관포로봇]         = "MWC-AP-PM 라이노 기관포 로봇",           // 중형_기관포_보행   / 공격_주력 / 관통_중장갑
		[StrategyGamePlayData.UnitKey.기계_중형_로켓포로봇]         = "MWR-AS-XL 매그파이 로켓포 로봇",         // 중형_로켓_보행     / 공격_보조 / 폭발_경장갑
		[StrategyGamePlayData.UnitKey.기계_중형_전차포로봇]         = "MTK-BP-PR 타이거 전차포 로봇",           // 중형_전차포_보행   / 돌파_주력 / 관통_강화장갑
		[StrategyGamePlayData.UnitKey.기계_중형_대전차로봇]         = "MTK-AP-PL 스콜피온 대전차 로봇",         // 중형_전차포_보행   / 공격_주력 / 관통_경장갑
		[StrategyGamePlayData.UnitKey.기계_중형_돌격로봇]           = "MWR-BP-XR 베어 돌격 로봇",               // 중형_로켓_보행     / 돌파_주력 / 폭발_강화장갑
		[StrategyGamePlayData.UnitKey.기계_중형_수리로봇]           = "MSW-SP-NL 오터 수리 로봇",               // 중형_지원_보행     / 지원_보조 / 일반_경장갑
		[StrategyGamePlayData.UnitKey.기계_중형_보급로봇]           = "MSW-SP-NL 비버 보급 로봇",               // 중형_지원_보행     / 지원_보조 / 일반_경장갑
		[StrategyGamePlayData.UnitKey.기계_중형_화염방사로봇]       = "MWF-BP-XL 버팔로 화염 로봇",             // 중형_화염_보행     / 돌파_주력 / 폭발_경장갑
		[StrategyGamePlayData.UnitKey.기계_중형_전파방해로봇]       = "MED-SS-EL 코요테 전자전 로봇",           // 중형_에너지_보행   / 지원_보조 / 에너지_경장갑
		[StrategyGamePlayData.UnitKey.기계_중형_지휘통제로봇]       = "MCW-SP-NM 레이븐 지휘 통제 로봇",        // 중형_지휘_보행     / 지원_보조 / 일반_중장갑
																																		  
		[StrategyGamePlayData.UnitKey.기계_대형_기관포로봇]         = "LWC-AP-PM 서지 기관포 로봇",             // 대형_기관포_보행   / 공격_주력 / 관통_중장갑
		[StrategyGamePlayData.UnitKey.기계_대형_로켓포롯봇]         = "LWR-AP-XL 볼케이노 로켓포 로봇",         // 대형_로켓_보행     / 공격_주력 / 폭발_경장갑
		[StrategyGamePlayData.UnitKey.기계_대형_폭격로봇]           = "LWR-AS-XXL 템페스트 폭격 로봇",          // 대형_로켓_보행     / 공격_보조 / 폭발특화_경장갑
		[StrategyGamePlayData.UnitKey.기계_대형_호버탱크]           = "LHT-BP-PR 레비아탄 호버 탱크",           // 대형_전차포_호버   / 돌파_주력 / 관통_강화장갑
		[StrategyGamePlayData.UnitKey.기계_대형_돌격로봇]           = "LWR-BP-XR 크러셔 돌격 로봇",             // 대형_로켓_보행     / 돌파_주력 / 폭발_강화장갑
		[StrategyGamePlayData.UnitKey.기계_대형_레이더차량]         = "LSW-SP-NL 미라지 레이더 차량",           // 대형_지원_차량     / 지원_보조 / 일반_경장갑
		[StrategyGamePlayData.UnitKey.기계_대형_역장전개차량]       = "LSD-DS-SR 배리어 역장 전개 차량",        // 대형_지원_드론     / 방어_주력 / 에너지_강화장갑
		[StrategyGamePlayData.UnitKey.기계_대형_보급로봇]           = "LSW-SP-NL 포트 보급 로봇",               // 대형_지원_보행     / 지원_보조 / 일반_경장갑
		[StrategyGamePlayData.UnitKey.기계_대형_전파방해로봇]       = "LRD-SS-EL 이클립스 전파교란 로봇",       // 대형_로켓_드론     / 지원_보조 / 에너지_경장갑
		[StrategyGamePlayData.UnitKey.기계_대형_신호교란로봇]       = "LRD-SP-EL 옵시디언 신호차단 로봇",       // 대형_로켓_드론     / 지원_보조 / 에너지_경장갑

		[StrategyGamePlayData.UnitKey.기계_초대형_육상부양전함]     = "XHT-BP-PR 아발론 육상부양 전함",         // 초대형_전차포_호버 / 돌파_주력 / 관통_강화장갑
		[StrategyGamePlayData.UnitKey.기계_초대형_미사일항공모함]   = "XBR-AP-XXM 세라프 미사일 항공모함",      // 초대형_로켓_항공   / 공격_주력 / 폭발특화_중장갑
		[StrategyGamePlayData.UnitKey.기계_초대형_저격자주포]       = "XBT-AS-PR 타르타로스 저격 자주포",       // 초대형_전차포_궤도 / 공격_보조 / 관통_강화장갑
		[StrategyGamePlayData.UnitKey.기계_초대형_이동식전술지휘소] = "XCW-SP-NM 프로메테우스 전술 지휘소",     // 초대형_지휘_보행   / 지원_보조 / 일반_중장갑
		[StrategyGamePlayData.UnitKey.기계_초대형_이동식군수공장]   = "XFW-SS-NM 헤파이스토스 이동식 군수공장", // 초대형_화염_보행   / 지원_보조 / 일반_중장갑
	};
	#endregion
	#region HumankindUnitNames
	[ButtonGroup]
	void SetHumankindNames()
	{
		foreach (var item in HumankindUnitNames)
		{
			AddAsset(item.Key, new(item.Key, item.Value));
		}
	}
	Dictionary<StrategyGamePlayData.UnitKey, string> HumankindUnitNames = new Dictionary<StrategyGamePlayData.UnitKey, string>
	{
		// ========== 인류 세력 - 백화휘랑 (2100) ==========
		[ StrategyGamePlayData.UnitKey.인류_병사_전선보병] =  "전선보병" ,
		[ StrategyGamePlayData.UnitKey.인류_병사_중장갑보병] = "중장갑보병",
		[ StrategyGamePlayData.UnitKey.인류_병사_대기갑보병] = "대기갑보병",
		[ StrategyGamePlayData.UnitKey.인류_병사_박격포병] = "박격포병",

		[ StrategyGamePlayData.UnitKey.인류_병사_전투의무병] = "전투의무병",
		[ StrategyGamePlayData.UnitKey.인류_병사_보급병] = "보급병",
		[ StrategyGamePlayData.UnitKey.인류_병사_전투공병] = "전투공병",
		[ StrategyGamePlayData.UnitKey.인류_병사_역장보병] = "역장보병",

		[ StrategyGamePlayData.UnitKey.인류_병사_전술정찰병] = "전술정찰병",
		[ StrategyGamePlayData.UnitKey.인류_병사_전자전병] = "전자전병",
	};
	#endregion
	#region UniqueUnitNames
	[ButtonGroup]
	void SetUniqueNames()
	{
		foreach (var item in UniqueUnitNames)
		{
			AddAsset(item.Key, new(item.Key, item.Value));
		}
	}
	Dictionary<StrategyGamePlayData.UnitKey, string> UniqueUnitNames = new Dictionary<StrategyGamePlayData.UnitKey, string>
	{
		// ========== 기계 고유 (1900) ==========
		[ StrategyGamePlayData.UnitKey.기계_고유_아카이브관리자_므네모시네] = "므네모시네",
	   	// ========== 인류 고유 (2900) ==========
		[ StrategyGamePlayData.UnitKey.인류_고유_주우나] = "주우나",
		[ StrategyGamePlayData.UnitKey.인류_고유_주하로] = "주하로",
		[ StrategyGamePlayData.UnitKey.인류_고유_강도현] = "강도현",
		[ StrategyGamePlayData.UnitKey.인류_고유_이세미유하] = "이세 미유하",
	};
	#endregion

	[ButtonGroup("SetupUnitProfiles", order: -4)]
	private void SetupUnitProfiles()
	{
		if (KeyPairTargetList == null || KeyPairTargetList.Length == 0)
		{
			Debug.LogWarning($"{name}: KeyPairTargetList is empty.");
			return;
		}

		string basePath = $"Assets/Resources/ScriptableObject/UnitProfiles";
		if (!System.IO.Directory.Exists(basePath))
			System.IO.Directory.CreateDirectory(basePath);

		int updatedCount = 0;
		for (int i = 0 ; i < KeyPairTargetList.Length ; i++)
		{
			var pair = KeyPairTargetList[i];
			UnitInfo info = pair.asset;
			UnitKey key = info.UnitKey;
			string name = info.DisplayName;

			// 이미 존재하면 패스
			if (info.UnitProfileObject != null)
				continue;

			// 검색 경로
			string assetName = key.ToString();
			string assetPath = $"{basePath}/{assetName}.asset";

			// 기존 파일이 있는지 검사
			UnitProfileObject profile = null;

			profile = AssetDatabase.LoadAssetAtPath<UnitProfileObject>(assetPath);

			// 없다면 새로 생성
			if (profile == null)
			{
				profile = ScriptableObject.CreateInstance<UnitProfileObject>();
				profile.name = assetName;
				profile.unitKey = key;
				profile.displayName = name;

				AssetDatabase.CreateAsset(profile, assetPath);
				AssetDatabase.SaveAssets();
				Debug.Log($"[SetupUnitProfiles] Created new UnitProfileObject: {assetPath}");
			}

			info = new UnitInfo(key, name, profile);
			KeyPairTargetList[i] = new KeyPairAssetsStruct(key, info);
			updatedCount++;
		}

		EditorUtility.SetDirty(this);
		AssetDatabase.SaveAssets();

		Debug.Log($"[{name}] SetupUnitProfiles completed. Updated {updatedCount} entries.");
	}
#endif

	[Serializable]
	public struct UnitInfo
	{
		[ShowIf("@unitProfileObject == null"), SerializeField]
		private StrategyGamePlayData.UnitKey unitKey;
		[ShowIf("@unitProfileObject == null"), SerializeField]
		private string displayName;
		[SerializeField]
		private UnitProfileObject unitProfileObject;

		public readonly string DisplayName => UnitProfileObject == null ? displayName : UnitProfileObject.displayName;
		public readonly UnitKey UnitKey => UnitProfileObject == null ? unitKey : UnitProfileObject.unitKey;
		public readonly UnitProfileObject UnitProfileObject => unitProfileObject;

		public UnitInfo(UnitKey unitKey, string displayName) : this()
		{
			this.displayName = displayName;
			this.unitKey = unitKey;
			unitProfileObject = null;
		}
		public UnitInfo(UnitKey unitKey, string displayName, UnitProfileObject unitProfileObject) : this()
		{
			this.displayName = displayName;
			this.unitKey = unitKey;
			this.unitProfileObject = unitProfileObject;
		}
	}
}