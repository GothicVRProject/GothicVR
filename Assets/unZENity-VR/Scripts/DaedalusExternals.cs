using System;
using UnityEngine;

namespace UZVR
{
    public static class DaedalusExternals
    {
        public static void NotImplementedCallback(string value)
        {
            throw new NotImplementedException("External >" + value + "< not registered but required by DaedalusVM.");
        }

        public static void Wld_InsertNpc(int npcinstance, string spawnpoint)
        {
            Debug.LogWarning(string.Format("Wld_InsertNpc called with npcinstance={0}, spawnpoint={1}", npcinstance, spawnpoint));
        }
    }
}
