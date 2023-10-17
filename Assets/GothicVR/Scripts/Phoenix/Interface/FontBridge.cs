using System;
using GVR.Extensions;
using PxCs.Interface;
using TMPro;
using UnityEngine;

namespace GVR.Phoenix.Interface
{
    public static class FontBridge
	{
		public static TMP_FontAsset LoadFont(IntPtr vfsPtr, string fontName)
		{
			var fontData = PxFont.LoadFont(vfsPtr, fontName);
			var textureData = fontData.texture;
			var font = new Font(fontData.name);

			var standardShader = Shader.Find("Standard");
			var material = new Material(standardShader);
			font.material = material;

			var texture = new Texture2D(
				(int)textureData.width,
				(int)textureData.height,
				textureData.format.AsUnityTextureFormat(),
				(int)textureData.mipmapCount,
				false);

			texture.name = fontData.name;

			for (var i = 0u; i < textureData.mipmapCount; i++)
				texture.SetPixelData(textureData.mipmaps[i].mipmap, (int)i);

			texture.Apply();

			return TMP_FontAsset.CreateFontAsset(font);
		}
	}
}