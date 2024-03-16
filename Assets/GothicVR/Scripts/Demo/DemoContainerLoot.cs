using System;
using System.Collections.Generic;
using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using GVR.Creator.Meshes.V2;
using GVR.Extensions;
using UnityEngine;

namespace GVR.Demo
{
	public class DemoContainerLoot : MonoBehaviour
	{
		public bool debugSpawnContentNow = false;

		private readonly char[] itemNameSeparators = { ';', ',' };
		private readonly char[] itemCountSeparators = { ':', '.' };
		
		
        [Serializable]
        public struct Content
        {
            public string name;
            public int amount;
        }
        public List<Content> content = new();

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

			var items = contents.Split(itemNameSeparators);

			foreach (var item in items)
			{
				var count = 1;
				var nameCountSplit = item.Split(itemCountSeparators);

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
				var itemInstance = AssetCache.TryGetItemData(item.name);

				var mrm = AssetCache.TryGetMrm(itemInstance.Visual);
				var itemObj = MeshFactory.CreateVob(item.name, mrm, default, default, true, itemsObj);
			}
		}
	}
}
