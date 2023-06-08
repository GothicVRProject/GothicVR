using GVR.Caches;
using GVR.Creator;
using GVR.Phoenix.Interface;
using GVR.Util;
using PxCs.Interface;
using System;
using System.Collections.Generic;
using GVR.Phoenix.Util;
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
			var itemsObj = new GameObject("Items");
			itemsObj.SetParent(gameObject, true);
			
			foreach (var item in content)
			{
				var pxItem = PxVm.InitializeItem(PhoenixBridge.VmGothicPtr, item.name);

				var mrm = assetCache.TryGetMrm(pxItem?.visual);
				var itemObj = meshCreator.Create(item.name, mrm, default, default, itemsObj);
			}
		}
	}
}
