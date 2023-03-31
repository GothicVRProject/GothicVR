using System;
using System.Runtime.InteropServices;
using UZVR.World;

namespace UZVR.Phoenix.Bridge
{
    public static class TextureBridge
    {
        private const string DLLNAME = PhoenixBridge.DLLNAME;
        [DllImport(DLLNAME)] private static extern IntPtr textureLoad(IntPtr vdfContainer, string textureFileName, out uint width, out uint height, out UInt64 size);
        [DllImport(DLLNAME)] private static extern byte textureGetByte(IntPtr texture, int index);
        [DllImport(DLLNAME)] private static extern void textureDispose(IntPtr texture);


        public static BTexture LoadTexture(IntPtr vdfContainer, string name)
        {
            var texture = textureLoad(vdfContainer, name, out uint width, out uint height, out UInt64 size);

            if (texture == IntPtr.Zero)
                return null;

            var retTexture = new BTexture
            {
                width = width,
                height = height,
                data = new((int)size)
            };

            for (int i = 0; i < (int)size; i++)
            {
                retTexture.data.Add(textureGetByte(texture, i));
            }

            textureDispose(texture);

            return retTexture;
        }

    }
}