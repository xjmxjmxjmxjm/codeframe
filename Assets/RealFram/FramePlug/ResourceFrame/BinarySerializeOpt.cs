using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using Object = System.Object;
using ProtoBuf;

public class BinarySerializeOpt
{

	public static bool ProtoSerialize(string path, System.Object obj)
	{
		try
		{
			using (Stream stream = File.Create(path))
			{
				Serializer.Serialize(stream, obj);
				return true;
			}
		}
		catch (Exception e)
		{
			Debug.Log("protobuf  Serialize error path =" + path);
		}
		return false;
	}

	public static T ProtoDeSerialize<T>(string path) where T : class
	{
		try
		{
			using (Stream stream = File.OpenRead(path))
			{
				return Serializer.Deserialize<T>(stream);
			}
		}
		catch (Exception e)
		{
			Debug.Log("protobuf  DeSerialize error path =" + path);
		}

		return null;
	}

	public static byte[] ProtoSerialize(System.Object obj)
	{
		try
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Serializer.Serialize(ms, obj);
				byte[] result = new byte[ms.Length];
				ms.Position = 0;
				ms.Read(result, 0, result.Length);
				return result;
			}
		}
		catch (Exception e)
		{
			Debug.Log("error" + e);
		}

		return null;
	}
	
	public static T ProtoDeSerialize<T>(byte[] msg) where T : class
	{
		try
		{
			using (MemoryStream ms = new MemoryStream())
			{
				ms.Write(msg, 0, msg.Length);
				ms.Position = 0;
				return Serializer.Deserialize<T>(ms);
			}
		}
		catch (Exception e)
		{
			Debug.Log("error" + e);
		}

		return null;
	}
	
	
	/// <summary>
	/// 类序列化成xml
	/// </summary>
	/// <param name="path"></param>
	/// <param name="obj"></param>
	/// <returns></returns>
	public static bool Xmlserialize(string path, System.Object obj)
	{
		try
		{
			using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
				{
					//XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
					//namespaces.Add(string.Empty, string.Empty);
					XmlSerializer xs = new XmlSerializer(obj.GetType());
					xs.Serialize(sw, obj);
				}
			}

			return true;
		}
		catch (Exception e)
		{
			Debug.LogError("此类无法转换成 xml " + obj.GetType() + "," + e);
		}

		return false;
	}

	/// <summary>
	/// 编辑器时读取 xml
	/// </summary>
	/// <param name="path"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static T XmlDeserialize<T>(string path) where T : class
	{
		T t = default(T);
		
		try
		{
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				XmlSerializer xs = new XmlSerializer(typeof(T));
				t = (T)xs.Deserialize(fs);
			}
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogError("xml cant changeto binary: " + path + "," + e);
		}

		return t;
	}

	/// <summary>
	/// xml 反序列化
	/// </summary>
	/// <param name="path"></param>
	/// <param name="type"></param>
	/// <returns></returns>
	public static Object XmlDeserialize(string path, Type type)
	{
		Object obj = null;
		
		try
		{
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				XmlSerializer xs = new XmlSerializer(type);
				obj = xs.Deserialize(fs);
			}
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogError("xml cant changeto binary: " + path + "," + e);
		}

		return obj;
	}

	/// <summary>
	/// 运行时使读取 xml
	/// </summary>
	/// <param name="path"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static T XmlDeserializeRun<T>(string path)where T : class 
	{
		T t = default(T);
		TextAsset textAsset = ResourceManager.Instance.LoadResource<TextAsset>(path);

		if (textAsset == null)
		{
			UnityEngine.Debug.LogError("cant load TextAsset: " + path);
			return null;
		}

		try
		{
			using (MemoryStream stream = new MemoryStream(textAsset.bytes))
			{
				XmlSerializer xs = new XmlSerializer(typeof(T));
				t = (T)xs.Deserialize(stream);
			}

			ResourceManager.Instance.ReleaseResource(path, true);
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogError("load TextAsset exception: " + path + "," + e);
		}

		return t;
	}

	
	/// <summary>
	/// 类转成 二进制
	/// </summary>
	/// <param name="path"></param>
	/// <param name="obj"></param>
	/// <returns></returns>
	public static bool BinarySerialize(string path, System.Object obj)
	{
		try
		{
			using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(fs, obj);
			}

			return true;
		}
		catch (Exception e)
		{
			Debug.LogError("此类无法转换成 二进制 " + obj.GetType() + "," + e);
		}

		return false;
	}
	
	/// <summary>
	/// 运行时使读取 binary
	/// </summary>
	/// <param name="path"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static T BinaryDeserialize<T>(string path)where T : class 
	{
		T t = default(T);
		TextAsset textAsset = ResourceManager.Instance.LoadResource<TextAsset>(path);

		if (textAsset == null)
		{
			UnityEngine.Debug.LogError("cant load TextAsset: " + path);
			return null;
		}

		try
		{
			using (MemoryStream stream = new MemoryStream(textAsset.bytes))
			{
				BinaryFormatter bf = new BinaryFormatter();
				t = (T) bf.Deserialize(stream);
			}

			ResourceManager.Instance.ReleaseResource(path, true);
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogError("load TextAsset exception: " + path + "," + e);
		}

		return t;
	}
	
}
