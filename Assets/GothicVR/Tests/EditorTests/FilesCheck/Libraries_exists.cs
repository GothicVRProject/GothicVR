using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;

public class Libraries_exists
{
    // A Test behaves as an ordinary method
    [Test]
    public void Libraries_existsSimplePasses()
    {
        string filePath = "Assets/GothicVR/Tests/EditorTests/FilesCheck/dll-List.txt";
        Assert.IsTrue(File.Exists(filePath), "File " + filePath + " not found.");

        string[] dllFiles = File.ReadAllLines(filePath);
        foreach (string dllFile in dllFiles) {
            if (!File.Exists(Path.Combine("Assets/GothicVR/Dependencies/", dllFile))) {
                Assert.Fail("File " + dllFile + " not found.");
            }
        }
    }
}
