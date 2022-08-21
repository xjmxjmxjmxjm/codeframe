using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class AssetBundleConfig {

	[XmlElement("m_oAllABBase")]
	public List<ABBase> m_oAllABBase { get; set; }
}

[Serializable]
public class ABBase
{
	[XmlAttribute("Path")]
	public string Path { get; set; }
	
	[XmlAttribute("Crc")]
	public uint Crc { get; set; }
	
	[XmlAttribute("ABName")]
	public string ABName { get; set; }
	
	[XmlAttribute("AssetName")]
	public string AssetName { get; set; }
	
	[XmlElement("ABDependce")]
	public List<string> ABDependce { get; set; }
	
}
