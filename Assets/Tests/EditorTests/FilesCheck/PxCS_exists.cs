using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;

public class PxCS_exists
{
    // A Test behaves as an ordinary method
    [Test]
    public void PxCS_existsSimplePasses()
    {
         string filePath = "Assets/unZENity-VR/Dependencies/PxCs.dll";
        Assert.IsTrue(File.Exists(filePath), "File " + filePath + " not found.");
    }
}
