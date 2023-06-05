using GVR.Caches;
using GVR.Creator;
using GVR.Phoenix.Interface;
using GVR.Util;
using PxCs.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace GVR.Demo
{
	public class DemoContainerLoot : MonoBehaviour
	{
		public bool debugSpawnContentNow = false;

        [Serializable]
        public struct Content
        {
            public string name;
            public int amount;
        }
        public List<Content> content = new();


		private MeshCreator meshCreator;
		private AssetCache assetCache;

		private void Start()
		{
			meshCreator = SingletonBehaviour<MeshCreator>.GetOrCreate();
			assetCache = SingletonBehaviour<AssetCache>.GetOrCreate();
		}

		private void Update()
		{
			if (debugSpawnContentNow)
			{
				debugSpawnContentNow = false;

				SpawnContent();
			}
		}


		public void SetContent(string contents)
		{
			if (contents == string.Empty)
				return;

			var items = contents.Split(',', ';');

			foreach (var item in items)
			{
				var count = 1;
				var nameCountSplit = item.Split(':');

				if (nameCountSplit.Length != 1)
				{
					count = int.Parse(nameCountSplit[1]);
				}

				content.Add(new() {
					name = nameCountSplit[0],
					amount = count
				});
			}
		}


		private void SpawnContent()
		{
			foreach (var item in content)
			{
				// Get instance from name
				// INSTANCE ItKeLockpick(C_Item)

				// Read visual
				// visual = "ItKe_Lockpick_01.3ds";

				// Load mesh
				// assetCache.TryGet*(...);
			}
		}
	}
}
