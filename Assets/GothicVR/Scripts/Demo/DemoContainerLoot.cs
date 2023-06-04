using System;
using System.Collections.Generic;
using UnityEngine;

namespace GVR.Demo
{
    public class DemoContainerLoot : MonoBehaviour
	{
        [Serializable]
        public struct Content
        {
            public string name;
            public int amount;
        }
        public List<Content> content = new();


		public void SetContent(string contents)
		{
			if (contents == string.Empty)
				return;

			var items = contents.Split(',');

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
	}
}
