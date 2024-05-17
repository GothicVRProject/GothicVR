using System.IO;
using UnityEditor;
using UnityEngine;

namespace GVR.Editor.Tools
{
    public class HVRTool
    {
        [MenuItem("GothicVR/HVR/Checkout HVR Adapter")]
        public static void CheckoutHVRAdapter()
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = "submodule update";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            Debug.Log(output);
            if (error != "")
                Debug.LogError(error);
            else
                Debug.Log("HVR Adapter checked out. You can now develop in VR mode if you've also installed HVR plugin itself.");
        }

        [MenuItem("GothicVR/HVR/Remove HVR Adapter")]
        public static void DeleteHVRAdapter()
        {
            var dir = new DirectoryInfo(@Application.dataPath + "/GothicVR-HVR-Adapter");
            dir.Delete(true);

            Debug.Log("HVR Adapter removed. You'll now work with Flat only.");
        }
    }
}
