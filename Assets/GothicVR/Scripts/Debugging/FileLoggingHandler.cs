using System.IO;
using UnityEngine;

namespace GVR.Debugging
{
    public class FileLoggingHandler : MonoBehaviour
    {
        private StreamWriter fileWriter;
        int currLogLevel = 0;


		private void Awake()
        {
			//currLogLevel = SingletonBehaviour<SettingsManager>.GetOrCreate().GameSettings.LogLevel;

			fileWriter = new StreamWriter(Application.persistentDataPath + "/gothicvr_log.txt", false);
			fileWriter.WriteLine("DeviceModel: " + SystemInfo.deviceModel);
			fileWriter.WriteLine("DeviceType: " + SystemInfo.deviceType);
			fileWriter.WriteLine("OperatingSystem: " + SystemInfo.operatingSystem);
			fileWriter.WriteLine("OperatingSystemFamily: " + SystemInfo.operatingSystemFamily);
			fileWriter.WriteLine("MemorySize: " + SystemInfo.systemMemorySize);
			fileWriter.WriteLine("GVR Version: " + Application.version);
            fileWriter.WriteLine();
			Application.logMessageReceived += HandleLog;
            Debug.Log("Init file logging handler done");
			fileWriter.Flush();
		}

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
            fileWriter.Close();
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            var level = (int)type;

			if (level >= currLogLevel)
			{
				switch (level)
				{
					case (int)LogLevel.Debug:
						fileWriter.WriteLine(type + ": " + logString);
						fileWriter.Flush();
						break;
					case (int)LogLevel.Info:
						fileWriter.WriteLine(type + ": " + logString);
						fileWriter.Flush();
						break;
					case (int)LogLevel.Warn:
						fileWriter.WriteLine(type + ": " + logString);
						fileWriter.Flush();
						break;
					case (int)LogLevel.Error:
						fileWriter.WriteLine(type + ": " + logString);
						fileWriter.WriteLine(stackTrace);
						fileWriter.Flush();
						break;
					case (int)LogLevel.Fatal:
						fileWriter.WriteLine(type + ": " + logString);
						fileWriter.WriteLine(stackTrace);
						fileWriter.Flush();
						break;
				}
			}

		}
    }

    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        Fatal = 4,
    }
}
