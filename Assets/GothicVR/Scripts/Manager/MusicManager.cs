using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DirectMusic;
using GothicVR.Vob;
using GVR.Caches;
using GVR.Debugging;
using GVR.Globals;
using GVR.Manager.Settings;
using UnityEngine;
using ZenKit;
using ZenKit.Daedalus;
using ZenKit.Vobs;
using Logger = DirectMusic.Logger;
using LogLevel = DirectMusic.LogLevel;
using Object = UnityEngine.Object;

namespace GVR.Manager
{
    public static class MusicManager
    {
        [Flags]
        public enum SegmentTags : byte
        {
            Day = 0,
            Ngt = 1 << 0,

            Std = 0,
            Fgt = 1 << 1,
            Thr = 1 << 2
        }

        private static Performance _dxPerformance;
        private static Loader _dxLoader;
        private static AudioSource _audioSourceComp;
        private static AudioReverbFilter _reverbFilterComp;

        private static MusicThemeInstance _currentTheme;
        /// <summary>
        /// Whenever we collide with a musicZoneVobGO, it's entry will be added to the list and the most important theme will be played.
        /// </summary>
        private static readonly List<GameObject> _musicZones = new();

        // Depending on speed of track, 2048 == around less than a second
        // If we cache each call to dxMusic synthesizer, we would skip a lot of transition options as the synthesizer assumes we're already ahead.
        // This is due to the fact, that whenever we ask for data from dxMusic, the handler "moves" forward as it assumes we play to the end until asking for more data.
        // But if we ask for numerous seconds and therefore "cache" music way too long, the transition will take place very late which can be heard by gamers.
        private const int BUFFER_SIZE = 2048;
        private const int FREQUENCY_RATE = 44100;

        public static void Initialize()
        {
            if (!FeatureFlags.I.enableMusic)
                return;

            _dxPerformance = Performance.Create(FREQUENCY_RATE);

            InitializeUnity();
            InitializeZenKit();
            InitializeDxMusic();

            GvrEvents.MainMenuSceneLoaded.AddListener(OnMainMenuLoaded);
            GvrEvents.LoadingSceneLoaded.AddListener(OnLoadingSceneLoaded);
            GvrEvents.GeneralSceneLoaded.AddListener(OnWorldLoaded);
        }

        private static void InitializeUnity()
        {
            var backgroundMusic = GameObject.Find("BackgroundMusic");
            _audioSourceComp = backgroundMusic.GetComponent<AudioSource>();
            _reverbFilterComp = backgroundMusic.GetComponent<AudioReverbFilter>();

            var audioClip = AudioClip.Create("Music", BUFFER_SIZE * 2, 2, FREQUENCY_RATE, true, PCMReaderCallback);

            _audioSourceComp.priority = 0;
            _audioSourceComp.clip = audioClip;
            _audioSourceComp.loop = true;
            _audioSourceComp.Play();
        }

        private static void OnMainMenuLoaded()
        {
            Play("SYS_MENU");
        }

        private static void OnLoadingSceneLoaded()
        {
            Play("SYS_LOADING");
        }

        private static void OnWorldLoaded()
        {
            _musicZones.Clear();

            var zones = Object.FindObjectsOfType<VobMusicProperties>();
            var playerPosition = GameObject.FindWithTag(Constants.PlayerTag).transform.position;

            foreach (var zone in zones)
            {
                // We always set default music as fallback.
                if (zone.musicData.GetType() == typeof(ZoneMusicDefault))
                {
                    AddMusicZone(zone.gameObject);
                    continue;
                }

                // If it's a normal music, we check if we're standing inside.
                if (zone.GetComponent<BoxCollider>().bounds.Contains(playerPosition))
                {
                    AddMusicZone(zone.gameObject);
                }
            }

            Play(SegmentTags.Std);
        }

        public static void AddMusicZone(GameObject newMusicZoneGo)
        {
            // If a collider triggers multiple times or we added the zone manually: Skip as duplicate
            if (_musicZones.Contains(newMusicZoneGo))
                return;

            _musicZones.Add(newMusicZoneGo);
        }

        public static void RemoveMusicZone(GameObject newMusicZoneGo)
        {
            _musicZones.Remove(newMusicZoneGo);
        }

        private static void InitializeZenKit()
        {
            // Load all music files into vfs.
            GameData.Vfs.Mount(Path.Combine(SettingsManager.GameSettings.GothicIPath, "_work"), "/", VfsOverwriteBehavior.All);
        }

        private static void InitializeDxMusic()
        {
            Logger.Set(FeatureFlags.I.dxMusicLogLevel, LoggerCallback);

            _dxLoader = Loader.Create(LoaderOptions.Download);
            _dxLoader.AddResolver(name =>
            {
                try
                {
                    return GameData.Vfs.Find(name).Buffer.Bytes;
                }
                catch (Exception)
                {
                    // No audio file found. Return null for now as it seems sufficient.
                    return null;
                }
            });
        }

        private static void LoggerCallback(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Fatal:
                case LogLevel.Error:
                    Debug.LogError(message);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogLevel.Info:
                case LogLevel.Debug:
                case LogLevel.Trace:
                    Debug.Log(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        private static void PCMReaderCallback(float[] data)
        {
            _dxPerformance.RenderPcm(data, true);
        }

        public static void Play(SegmentTags tags)
        {
            var zoneName = _musicZones
                .OrderBy(i => i.GetComponent<VobMusicProperties>().musicData.Priority)
                .Last()
                .GetComponent<VobMusicProperties>().musicData.Name;

            bool isDay = (tags & SegmentTags.Ngt) == 0;
            string result = zoneName.Substring(zoneName.IndexOf("_") + 1);
            var musicTag = "STD";

            if ((tags & SegmentTags.Fgt) != 0)
                musicTag = "FGT";

            if ((tags & SegmentTags.Thr) != 0)
                musicTag = "THR";

            var musicThemeInstanceName = $"{result}_{(isDay ? "DAY" : "NGT")}_{musicTag}";

            Play(musicThemeInstanceName);
        }

        public static void Play(string musicInstanceName)
        {
            var music = AssetCache.TryGetMusic(musicInstanceName);
            Play(music);
        }

        public static void Play(MusicThemeInstance theme)
        {
            if (!FeatureFlags.I.enableMusic)
                return;

            // Do not restart the current theme if already playing.
            // Multiple MusicThemeInstances can reference the same audio. Therefore checking actual files only.
            if (_currentTheme != null && theme.File == _currentTheme.File)
                return;

            var segment = _dxLoader.GetSegment(theme.File);

            var timing = ToTiming(theme.TransSubType);
            var embellishment = ToEmbellishment(theme.TransType);

            if (FeatureFlags.I.dxMusicLogLevel >= LogLevel.Info)
                Debug.Log($"Changing music theme to: {theme.File}");

            _dxPerformance.PlayTransition(segment, embellishment, timing);

            // Tests sounded feasible like when you stop the music you get somme afterglow hall.
            // TODO - But I have no clue if decayTime is the right timer to set here. Alter if you have better ears than I have. ;-)
            _reverbFilterComp.decayTime = theme.ReverbTime / 1000; // ms in seconds

            _currentTheme = theme;
        }

        private static Timing ToTiming(MusicTransitionType type)
        {
            return type switch
            {
                MusicTransitionType.Measure or MusicTransitionType.Unknown => Timing.Measure,
                MusicTransitionType.Immediate => Timing.Instant,
                MusicTransitionType.Beat => Timing.Beat,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private static Embellishment ToEmbellishment(MusicTransitionEffect effect)
        {
            return effect switch
            {
                // None or Unknown needs to be set to End - otherwise normal transitions won't happen in G1 music.
                MusicTransitionEffect.Unknown or MusicTransitionEffect.None => Embellishment.End,
                MusicTransitionEffect.Groove => Embellishment.Groove,
                MusicTransitionEffect.Fill => Embellishment.Fill,
                MusicTransitionEffect.Break => Embellishment.Break,
                MusicTransitionEffect.Intro => Embellishment.Intro,
                MusicTransitionEffect.End => Embellishment.End,
                MusicTransitionEffect.EndAndInto => Embellishment.EndAndIntro,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect, null)
            };
        }
    }
}
