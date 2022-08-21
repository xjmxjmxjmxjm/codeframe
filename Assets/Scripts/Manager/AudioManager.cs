using UnityEngine;
public class AudioManager : Singleton<AudioManager> {

	// bg audioSource
	private AudioSource m_BGAudioSource;

	public void Init(AudioSource bgAudioSource)
	{
		m_BGAudioSource = bgAudioSource;
	}
	/// <summary>
	/// 音乐加载部分
	/// </summary>
	/// <param name="path"></param>
	/// <param name="clip"></param>
	/// <param name="clearOldSound"></param>
	public void PlayBGAudioSource(string path, bool clearOldSound = true)
	{
		if (m_BGAudioSource.clip != null)
		{
			ResourceManager.Instance.ReleaseResource(m_BGAudioSource.clip, clearOldSound);
			m_BGAudioSource.clip = null;
		}
		AudioClip tClip = ResourceManager.Instance.LoadResource<AudioClip>(path);
		m_BGAudioSource.clip = tClip;
		m_BGAudioSource.Play();
	}

	public void ReleaseCurrentBGAudioSource(string path, bool clearOldSound = true)
	{
		if (m_BGAudioSource.clip != null)
		{
			ResourceManager.Instance.ReleaseResource(m_BGAudioSource.clip, clearOldSound);
			m_BGAudioSource.clip = null;
		}
	}
}
