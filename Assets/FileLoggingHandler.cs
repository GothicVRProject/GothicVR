using System.IO;
using UnityEngine;

namespace GVR.Settings
{
    public class FileLoggingHandler : MonoBehaviour
    {
        private StreamWriter fileWriter;

        private void Awake()
        {
            fileWriter = new StreamWriter(Application.persistentDataPath + "/gothicvr_log.txt", false);
            Application.logMessageReceived += HandleLog;
            Debug.Log("Init file logging handler done");
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
            fileWriter.Close();
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            fileWriter.WriteLine(type + ": " + logString);
            if (type == LogType.Exception)
            {
                fileWriter.WriteLine(stackTrace);
            }
        }
    }
}
