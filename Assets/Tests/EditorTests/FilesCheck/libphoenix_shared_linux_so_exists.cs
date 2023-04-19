using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;

public class libphoenix_shared_linux_so_exists
{
    // A Test behaves as an ordinary method
    [Test]
    public void libphoenix_shared_linux_so_existsSimplePasses()
    {
        string filePath = "Assets/unZENity-VR/Dependencies/libphoenix-shared.linux.so";
        Assert.IsTrue(File.Exists(filePath), "File " + filePath + " not found.");
    }


}
