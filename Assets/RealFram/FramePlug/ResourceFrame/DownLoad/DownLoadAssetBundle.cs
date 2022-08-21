using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DownLoadAssetBundle : DownLoadItem
{
	private UnityWebRequest m_WebRequest;
	
	public DownLoadAssetBundle(string url, string path) : base(url, path)
	{
		
	}

	public override IEnumerator DownLoad(Action callback = null)
	{
		m_WebRequest = UnityWebRequest.Get(m_Url);
		m_StartDownLoad = true;
		m_WebRequest.timeout = 30;
		yield return m_WebRequest.SendWebRequest();
		m_StartDownLoad = false;

		if (m_WebRequest.isNetworkError)
		{
			Debug.LogError("download error" + m_WebRequest.error);
		}
		else
		{
			byte[] bytes = m_WebRequest.downloadHandler.data;
			FileTool.CreateFile(m_SaveFilePath, bytes);
			if (callback != null)
			{
				callback();
			}
		}

	}

	public override float GetProcess()
	{
		if (m_WebRequest != null)
		{
			return m_WebRequest.downloadProgress;
		}
		else
		{
			return 0;
		}
	}

	public override long GetCurLength()
	{
		if (m_WebRequest != null)
		{
			return (long)m_WebRequest.downloadedBytes;
		}
		else
		{
			return 0;
		}
	}

	public override long GetLength()
	{
		return 0;
	}

	public override void Destory()
	{
		if (m_WebRequest != null)
		{
			m_WebRequest.Dispose();
			m_WebRequest = null;
		}
	}
}
