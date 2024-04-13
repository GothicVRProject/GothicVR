using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Creator.Sounds;
using GVR.Data;
using GVR.Globals;
using JetBrains.Annotations;
using ZenKit;
using ZenKit.Daedalus;
using Font = ZenKit.Font;
using Mesh = ZenKit.Mesh;

namespace GVR.Caches
{
    public static class AssetCache
    {
        private static readonly Dictionary<string, ITexture> TextureCache = new();
        private static readonly Dictionary<string, IMesh> MshCache = new();
        private static readonly Dictionary<string, IModelScript> MdsCache = new();
        private static readonly Dictionary<string, IModelAnimation> AnimCache = new();
        private static readonly Dictionary<string, IModelHierarchy> MdhCache = new();
        private static readonly Dictionary<string, IModel> MdlCache = new();
        private static readonly Dictionary<string, IModelMesh> MdmCache = new();
        private static readonly Dictionary<string, IMultiResolutionMesh> MrmCache = new();
        private static readonly Dictionary<string, IMorphMesh> MmbCache = new();
        private static readonly Dictionary<string, ItemInstance> ItemDataCache = new();
        private static readonly Dictionary<int, SvmInstance> SvmDataCache = new();
        private static readonly Dictionary<string, MusicThemeInstance> MusicThemeCache = new();
        private static readonly Dictionary<string, SoundEffectInstance> SfxDataCache = new();
        private static readonly Dictionary<string, ParticleEffectInstance> PfxDataCache = new();
        private static readonly Dictionary<string, SoundData> SoundCache = new();
        private static readonly Dictionary<string, IFont> FontCache = new();

        public static ITexture TryGetTexture(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (TextureCache.TryGetValue(preparedKey, out var data))
                return data;

            ITexture newData = null;
            try
            {
                newData = new Texture(GameData.Vfs, $"{preparedKey}-C.TEX").Cache();
                TextureCache[preparedKey] = newData;
            }
            catch (Exception)
            {
                // ignored
            }

            return newData;
        }

        public static IModelScript TryGetMds(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MdsCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = new ModelScript(GameData.Vfs, $"{preparedKey}.mds").Cache();
            MdsCache[preparedKey] = newData;

            return newData;
        }

        public static IModelAnimation TryGetAnimation(string mdsKey, string animKey)
        {
            var preparedMdsKey = GetPreparedKey(mdsKey);
            var preparedAnimKey = GetPreparedKey(animKey);
            var preparedKey = $"{preparedMdsKey}-{preparedAnimKey}";
            if (AnimCache.TryGetValue(preparedKey, out var data))
                return data;

            IModelAnimation newData = null;
            try
            {
                newData = new ModelAnimation(GameData.Vfs, $"{preparedKey}.man").Cache();
            }
            catch (Exception)
            {
                // ignored
            }

            AnimCache[preparedKey] = newData;

            return newData;
        }

        [CanBeNull]
        public static IMesh TryGetMsh(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MshCache.TryGetValue(preparedKey, out var data))
                return data;

            IMesh newData = null;
            try
            {
                newData = new Mesh(GameData.Vfs, $"{preparedKey}.msh").Cache();
            }
            catch (Exception)
            {
                // ignored
            }

            MshCache[preparedKey] = newData;

            return newData;
        }

        [CanBeNull]
        public static IModelHierarchy TryGetMdh(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MdhCache.TryGetValue(preparedKey, out var data))
                return data;

            IModelHierarchy newData = null;
            try
            {
                newData = new ModelHierarchy(GameData.Vfs, $"{preparedKey}.mdh").Cache();
            }
            catch (Exception)
            {
                // ignored
            }

            MdhCache[preparedKey] = newData;

            return newData;
        }

        [CanBeNull]
        public static IModel TryGetMdl(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MdlCache.TryGetValue(preparedKey, out var data))
                return data;

            IModel newData = null;
            try
            {
                newData = new Model(GameData.Vfs, $"{preparedKey}.mdl").Cache();
            }
            catch (Exception)
            {
                // ignored
            }

            MdlCache[preparedKey] = newData;

            return newData;
        }

        [CanBeNull]
        public static IModelMesh TryGetMdm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MdmCache.TryGetValue(preparedKey, out var data))
                return data;

            IModelMesh newData = null;
            try
            {
                newData = new ModelMesh(GameData.Vfs, $"{preparedKey}.mdm").Cache();
            }
            catch (Exception)
            {
                // ignored
            }

            MdmCache[preparedKey] = newData;

            return newData;
        }

        public static IMultiResolutionMesh TryGetMrm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MrmCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = new MultiResolutionMesh(GameData.Vfs, $"{preparedKey}.mrm").Cache();
            MrmCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// MMS == MorphMesh
        /// e.g. face animations during dialogs.
        /// </summary>
        public static IMorphMesh TryGetMmb(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MmbCache.TryGetValue(preparedKey, out var data))
                return data;

            IMorphMesh newData = null;
            try
            {
                newData = new MorphMesh(GameData.Vfs, $"{preparedKey}.mmb").Cache();
            }
            catch (Exception)
            {
                // ignored
            }
            MmbCache[preparedKey] = newData;

            return newData;
        }

        public static MusicThemeInstance TryGetMusic(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MusicThemeCache.TryGetValue(preparedKey, out var data))
                return data;

            MusicThemeInstance newData = null;
            try
            {
                newData = GameData.MusicVm.InitInstance<MusicThemeInstance>(preparedKey);
            }
            catch (Exception)
            {
                // ignored
            }
            MusicThemeCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once in ZenKit.
        /// There are two ways of getting Item data. Via INSTANCE name or symbolIndex inside VM.
        /// </summary>
        public static ItemInstance TryGetItemData(int instanceId)
        {
            var symbol = GameData.GothicVm.GetSymbolByIndex(instanceId);

            if (symbol == null)
                return null;

            return TryGetItemData(symbol.Name);
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once in ZenKit.
        /// There are two ways of getting Item data. Via INSTANCE name or symbolIndex inside VM.
        /// </summary>
        [CanBeNull]
        public static ItemInstance TryGetItemData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (ItemDataCache.TryGetValue(preparedKey, out var data))
                return data;

            ItemInstance newData = null;
            try
            {
                newData = GameData.GothicVm.InitInstance<ItemInstance>(preparedKey);
            }
            catch (Exception)
            {
                // ignored
            }
            ItemDataCache[preparedKey] = newData;

            return newData;
        }
        
        /// <summary>
        /// Hint: Instances only need to be initialized once in ZenKit.
        /// </summary>
        [CanBeNull]
        public static SvmInstance TryGetSvmData(int voiceId)
        {
            if (SvmDataCache.TryGetValue(voiceId, out var data))
                return data;

            SvmInstance newData = null;
            try
            {
                newData = GameData.GothicVm.InitInstance<SvmInstance>($"SVM_{voiceId}");
            }
            catch (Exception)
            {
                // ignored
            }
            SvmDataCache[voiceId] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once in ZenKit and don't need to be deleted during runtime.
        /// </summary>
        [CanBeNull]
        public static SoundEffectInstance TryGetSfxData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (SfxDataCache.TryGetValue(preparedKey, out var data))
                return data;

            SoundEffectInstance newData = null;
            try
            {
                newData = GameData.SfxVm.InitInstance<SoundEffectInstance>(preparedKey);
            }
            catch (Exception)
            {
                // ignored
            }
            SfxDataCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once in ZenKit and don't need to be deleted during runtime.
        /// </summary>
        public static ParticleEffectInstance TryGetPfxData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (PfxDataCache.TryGetValue(preparedKey, out var data))
                return data;

            ParticleEffectInstance newData = null;
            try
            {
                newData = GameData.PfxVm.InitInstance<ParticleEffectInstance>(preparedKey);
            }
            catch (Exception)
            {
                // ignored
            }
            PfxDataCache[preparedKey] = newData;

            return newData;
        }

        public static SoundData TryGetSound(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (SoundCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = SoundCreator.GetSoundArrayFromVfs($"{preparedKey}.wav");
            SoundCache[preparedKey] = newData;

            return newData;
        }

        public static IFont TryGetFont(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (FontCache.TryGetValue(preparedKey, out var data))
                return data;

            var fontData = new Font(GameData.Vfs, $"{preparedKey}.fnt").Cache();
            FontCache[preparedKey] = fontData;

            return fontData;
        }

        private static string GetPreparedKey(string key)
        {
            var lowerKey = key.ToLower();
            var extension = Path.GetExtension(lowerKey);

            if (extension == string.Empty)
                return lowerKey;
            else
                return lowerKey.Replace(extension, "");
        }

        public static void Dispose()
        {
            TextureCache.Clear();
            MdsCache.Clear();
            AnimCache.Clear();
            MdhCache.Clear();
            MdlCache.Clear();
            MdmCache.Clear();
            MrmCache.Clear();
            MmbCache.Clear();
            ItemDataCache.Clear();
            SvmDataCache.Clear();
            MusicThemeCache.Clear();
            SfxDataCache.Clear();
            PfxDataCache.Clear();
            SoundCache.Clear();
            FontCache.Clear();
        }
    }
}
