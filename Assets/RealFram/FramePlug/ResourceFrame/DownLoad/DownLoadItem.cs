﻿using System;
using System.Collections;
using System.IO;
using UnityEngine;

public abstract class DownLoadItem
{
	/// <summary>
	/// 网络资源路径
	/// </summary>
	protected string m_Url;
	public string Url
	{
		get { return m_Url; }
	}

	/// <summary>
	/// 资源下载存放路径 不包含文件名
	/// </summary>
	protected string m_SavePath;
	public string SavePath { get { return m_SavePath; } }

	/// <summary>
	/// 文件名 不包含后缀
	/// </summary>
	protected string m_FileNameWithoutExt;
	public string FileNameWithoutExt { get { return m_FileNameWithoutExt; } }

	/// <summary>
	/// 文件后缀
	/// </summary>
	protected string m_FileExt;
	public string FileExt { get { return m_FileExt; } }

	/// <summary>
	/// 文件名 包含后缀
	/// </summary>
	protected string m_FileName;
	public string FileName { get { return m_FileName; } }

	/// <summary>
	/// 下载路径全路径  路径+文件名+后缀
	/// </summary>
	protected string m_SaveFilePath;
	public string SaveFilePath { get { return m_SaveFilePath; } }

	/// <summary>
	/// 源文件大小
	/// </summary>
	protected long m_FileLength;
	public long FileLength { get { return m_FileLength; } }
	
	/// <summary>
	/// 当前下载大小
	/// </summary>
	protected long m_CurLength;
	public long CurLength { get { return m_CurLength; } }

	/// <summary>
	/// 是否开始了下载
	/// </summary>
	protected bool m_StartDownLoad;
	public bool StartDownLoad { get { return m_StartDownLoad; } }


	public DownLoadItem(string url, string path)
	{
		m_Url = url;
		m_SavePath = path;
		m_StartDownLoad = false;
		m_FileNameWithoutExt = Path.GetFileNameWithoutExtension(m_Url);
		m_FileExt = Path.GetExtension(m_Url);
		m_FileName = string.Format("{0}{1}", m_FileNameWithoutExt, m_FileExt);
		m_SaveFilePath = string.Format("{0}/{1}{2}", m_SavePath, m_FileNameWithoutExt, m_FileExt);
	}

	public virtual IEnumerator DownLoad(Action callback = null)
	{
		yield return null;
	}

	/// <summary>
	/// 获取下载进度
	/// </summary>
	/// <returns></returns>
	public abstract float GetProcess();

	/// <summary>
	/// 获取当前下载的文件大小
	/// </summary>
	/// <returns></returns>
	public abstract long GetCurLength();

	/// <summary>
	/// 获取下载的文件大小
	/// </summary>
	/// <returns></returns>
	public abstract long GetLength();

	public abstract void Destory();
}




















