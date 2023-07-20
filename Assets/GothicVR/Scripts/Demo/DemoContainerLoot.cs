using GVR.Caches;
using GVR.Creator;
using GVR.Phoenix.Interface;
using GVR.Util;
using PxCs.Interface;
using System;
using System.Collections.Generic;
using GVR.Creator.Meshes;
using GVR.Phoenix.Util;
using UnityEngine;

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

		private AssetCache assetCache;

		private void Start()
		{
			assetCache = AssetCache.I;
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
				var pxItem = assetCache.TryGetItemData(item.name);

				var mrm = assetCache.TryGetMrm(pxItem?.visual);
				var itemObj = MeshCreator.I.Create(item.name, mrm, default, default, true, itemsObj);
			}
		}
	}
}
