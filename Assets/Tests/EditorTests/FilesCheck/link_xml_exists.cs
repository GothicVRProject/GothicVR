using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;

public class link_xml_exists
{
    // A Test behaves as an ordinary method
    [Test]
    public void link_xml_existsSimplePasses()
    {
        string filePath = "Assets/unZENity-VR/Dependencies/link.xml";
        Assert.IsTrue(File.Exists(filePath), "File " + filePath + " not found.");

    }

}
