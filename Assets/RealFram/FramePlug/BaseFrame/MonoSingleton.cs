using UnityEngine;
public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
	protected static T instance;

	public static T Instance
	{
		get { return instance; }
	}

	protected virtual void Awake()
	{
		if (instance == null)
		{
			instance = (T) this;
		}
		else
		{
			Debug.LogError("get a second instance of this class" + this.GetType());
		}
	}
}
