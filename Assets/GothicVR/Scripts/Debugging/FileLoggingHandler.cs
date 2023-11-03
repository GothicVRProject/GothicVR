using System.IO;
using GVR.Manager.Settings;
using UnityEngine;

namespace GVR.Debugging
{
    public class FileLoggingHandler : MonoBehaviour
    {
        private StreamWriter fileWriter;
        private LogLevel logLevel;

        // Levels are aligned with Unity's levels: UnityEngine.LogType
        private enum LogLevel
        {
	        Debug = 0,
	        Warning = 1,
	        Error = 2,
	        Exception = 3
        }

		private void Awake()
        {
			Application.logMessageReceived += HandleLog;

			fileWriter = new(Application.persistentDataPath + "/gothicvr_log.txt", false);
			fileWriter.WriteLine("DeviceModel: " + SystemInfo.deviceModel);
			fileWriter.WriteLine("DeviceType: " + SystemInfo.deviceType);
			fileWriter.WriteLine("OperatingSystem: " + SystemInfo.operatingSystem);
			fileWriter.WriteLine("OperatingSystemFamily: " + SystemInfo.operatingSystemFamily);
			fileWriter.WriteLine("MemorySize: " + SystemInfo.systemMemorySize);
			fileWriter.WriteLine("GVR Version: " + Application.version);
            fileWriter.WriteLine();
            fileWriter.Flush();
		}

		/// <summary>
		/// We need to set LogLevel at this stage to ensure we have SettingsManager initialized.
		/// </summary>
		private void Start()
		{
			if (LogLevel.TryParse(SettingsManager.GameSettings.LogLevel, true, out LogLevel value))
			{
				fileWriter.WriteLine("LogLevel Setting found: " + SettingsManager.GameSettings.LogLevel);
				logLevel = value;
			}
			else
			{
				fileWriter.WriteLine("LogLevel Setting not found. Setting Default to >Warning<.");
				logLevel = LogLevel.Warning;
			}
			fileWriter.Flush();
		}

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
            fileWriter.Close();
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
	        if (!ShallBeLogged(type))
		        return;
	        
	        fileWriter.WriteLine(type + ": " + logString);
	        fileWriter.Flush();
        }

        private bool ShallBeLogged(LogType type)
        {
	        switch (logLevel)
	        {
		        case LogLevel.Debug:
					return true; // Log everything
			    case LogLevel.Warning:
				    return type != LogType.Log; // includes Assert, Warning, Error, Exception
			    case LogLevel.Error:
				    return type == LogType.Error || type == LogType.Exception;
			    case LogLevel.Exception:
				    return type == LogType.Exception;
			    default:
				    return true; // never reached, but compiler demands it.
	        }
        }
    }
}
