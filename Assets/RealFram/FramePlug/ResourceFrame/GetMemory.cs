using UnityEngine;
public class GetMemory : Singleton<GetMemory> {

	/*public static int GetUseMemory()
	{
		int memory = -1;
#if UNITY_ANDROID
        try
        {
            Debug.Log("GetUseMemory******************************内存使用情况");
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var unityPluginLoader = new AndroidJavaClass("包");
            float tempMemory = unityPluginLoader.CallStatic<float>("GetMemory", currentActivity);
            memory = (int)tempMemory;
        }
        catch (System.Exception e)
        {
            Debug.Log("抛异常了************************GetMemory: " + e.Message);
        }
#elif UNITY_IOS
        memory = (int)_IOS_GetTaskUsedMemeory();
#endif
		return memory;
	}*/
	
	/*//Android
	
	public static float GetMemory(Activity currentActivity)
	{
		float memory = -1;
		try
		{
undefined
			int pid = android.os.Process.myPid();
			ActivityManager mActivityManager = (ActivityManager)currentActivity
				.getSystemService(Context.ACTIVITY_SERVICE);
			Debug.MemoryInfo[] memoryInfoArray = mActivityManager.getProcessMemoryInfo(new int[] { pid });
			memory = (float)memoryInfoArray[0].getTotalPrivateDirty() / 1024;
		}
		catch (Exception e)
		{undefined
			if (Utile.isDebug())
				Utile.LogError(e.toString());
		}
		return memory;
	}

	//ios

	long _IOS_GetTaskUsedMemeory()
	{
	undefined
		task_basic_info_data_t taskInfo;
		mach_msg_type_number_t infoCount = TASK_BASIC_INFO_COUNT;
		kern_return_t kernReturn = task_info(mach_task_self(),
			TASK_BASIC_INFO,
			(task_info_t) & taskInfo,
			&infoCount);
		if (kernReturn == KERN_SUCCESS)
		{
		undefined
			long usedMemory = taskInfo.resident_size / 1024.0 / 1024.0;
			return usedMemory;
		}
		return 0;
	}*/
}
