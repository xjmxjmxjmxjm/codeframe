using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using ProtoBuf;

[ProtoContract]
[Serializable]
public class MonsterData : ExcelBase {
#if UNITY_EDITOR
	/// <summary>
	/// 编辑器下 初始类 转 xml
	/// </summary>
	public override void Construction()
	{
		AllMonster = new List<MonsterBase>();
		MonsterBase monster = new MonsterBase();
		monster.Id = 1;
		AllMonster.Add(monster);
		
		AllSuperMonster = new List<SuperMonster>();
		SuperMonster superMonster = new SuperMonster();
		superMonster.Id = 1;
		superMonster.Hp = 100;
		AllSuperMonster.Add(superMonster);
	}
#endif
	/// <summary>
	/// 数据初始化
	/// </summary>
	public override void Init()
	{
		m_AllMonsterDic.Clear();
		foreach (MonsterBase monster in AllMonster)
		{
			if (m_AllMonsterDic.ContainsKey(monster.Id))
			{
				Debug.LogError(monster.Id + "有重复Id");
			}
			else
			{
				m_AllMonsterDic.Add(monster.Id, monster);
			}
		}
	}

	/// <summary>
	/// 根据 id 查找 monster 数据
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public MonsterBase FindMonsterById(int id)
	{
		MonsterBase monster = null;
		m_AllMonsterDic.TryGetValue(id, out monster);
		return monster;
	}

	[ProtoIgnore]
	[XmlIgnore] 
	public Dictionary<int, MonsterBase> m_AllMonsterDic = new Dictionary<int, MonsterBase>();


	[ProtoMember(1)]
	[XmlElement("AllMonster")]
	public List<MonsterBase> AllMonster { get; set; }
	
	
	[ProtoMember(2)]
	[XmlElement("AllSuperMonster")]
	public List<SuperMonster> AllSuperMonster { get; set; }
}

[ProtoContract]
[ProtoInclude(20, typeof(SuperMonster))]
[Serializable]
public class MonsterBase
{
	[ProtoMember(1)]
	[XmlAttribute("Id")]
	public int Id { get; set; }

	[ProtoMember(2)]
	[XmlAttribute("Name")] public string Name { get; set; } = string.Empty;

	[ProtoMember(3)]
	[XmlAttribute("OutLook")] public string OutLook { get; set; } = string.Empty;
	
	[ProtoMember(4)]
	[XmlAttribute("Level")]
	public int Level { get; set; }
	
	[ProtoMember(5)]
	[XmlAttribute("Rare")]
	public int Rare { get; set; }
	
	[ProtoMember(6)]
	[XmlAttribute("Height")]
	public float Height { get; set; }
	
	[ProtoMember(7)]
	[XmlAttribute("Attack")]
	public int Attack { get; set; }
}

[ProtoContract]
[Serializable]
public class SuperMonster : MonsterBase
{
	[ProtoMember(8)]
	[XmlAttribute("Hp")]
	public int Hp { get; set; }
}