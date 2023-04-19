using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;

public class I18N_West_exists
{
    // A Test behaves as an ordinary method
    [Test]
    public void I18N_West_existsSimplePasses()
    {
        string filePath = "Assets/unZENity-VR/Dependencies/I18N.West.dll";
        Assert.IsTrue(File.Exists(filePath), "File " + filePath + " not found.");
    }

}
