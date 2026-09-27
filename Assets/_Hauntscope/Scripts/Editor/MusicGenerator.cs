using System.IO;
using UnityEditor;
using UnityEngine;
using static Hauntscope.Editor.AudioDsp;

namespace Hauntscope.Editor
{
    // Renders the soundtrack into Audio/Music (GDD 5.34): the menu theme, the hunt's five layers and its stingers. The
    // layers are normalised together, so any mix of them the game plays stays under full scale.
    public static class MusicGenerator
    {
        public const string Folder = "Assets/_Hauntscope/Audio/Music";

        private const float LayersPeakDb = -2f;
        private const float SinglePeakDb = -3f;

        // In the order of HuntMusicLayer.
        public static readonly string[] HuntLayers = { "HuntDrone", "HuntPulse", "HuntHeart", "HuntArp", "HuntDrums" };

        [MenuItem("Hauntscope/Build Music")]
        public static void Build()
        {
            var layers = new[] { MusicScores.HuntDrone(), MusicScores.HuntPulse(), MusicScores.HuntHeart(), MusicScores.HuntArp(), MusicScores.HuntDrums() };
            var sum = new MusicTrack(MusicScores.HuntBpm, MusicScores.HuntBars);
            foreach (var layer in layers)
                sum.Mix(layer, 1f);
            var gain = Mathf.Pow(10f, LayersPeakDb / 20f) / Mathf.Max(sum.Peak(), 1e-6f);

            // The heart and the pulse sit in the middle; mono keeps them half the size.
            var stereo = new[] { true, false, false, true, true };
            for (var i = 0; i < layers.Length; i++)
            {
                layers[i].Scale(gain);
                Save(layers[i], HuntLayers[i], stereo[i], true);
            }

            SaveNormalized(MusicScores.MenuTheme(), "MenuTheme", true);
            SaveNormalized(MusicScores.StingerCaptured(), "StingerCaptured", false);
            SaveNormalized(MusicScores.StingerEscaped(), "StingerEscaped", false);
            SaveNormalized(MusicScores.StingerSurge(), "StingerSurge", false);
            AssetDatabase.SaveAssets();
            Debug.Log("Hauntscope: built the soundtrack.");
        }

        public static AudioClip Load(string name)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>($"{Folder}/{name}.wav");
        }

        private static void SaveNormalized(MusicTrack track, string name, bool loop)
        {
            track.Scale(Mathf.Pow(10f, SinglePeakDb / 20f) / Mathf.Max(track.Peak(), 1e-6f));
            if (!loop)
                FadeTail(track);
            Save(track, name, true, loop);
        }

        private static void FadeTail(MusicTrack track)
        {
            var fade = Mathf.Min(track.Length, SampleRate / 20);
            for (var i = 0; i < fade; i++)
            {
                var gain = (float)i / fade;
                track.Left[track.Length - 1 - i] *= gain;
                track.Right[track.Length - 1 - i] *= gain;
            }
        }

        private static void Save(MusicTrack track, string name, bool stereo, bool loop)
        {
            Directory.CreateDirectory(Path.GetFullPath(Folder));
            var path = $"{Folder}/{name}.wav";
            var channels = stereo ? 2 : 1;
            using (var stream = new FileStream(Path.GetFullPath(path), FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                var dataSize = track.Length * channels * 2;
                writer.Write(new[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + dataSize);
                writer.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)channels);
                writer.Write(SampleRate);
                writer.Write(SampleRate * channels * 2);
                writer.Write((short)(channels * 2));
                writer.Write((short)16);
                writer.Write(new[] { 'd', 'a', 't', 'a' });
                writer.Write(dataSize);
                for (var i = 0; i < track.Length; i++)
                {
                    if (stereo)
                    {
                        writer.Write(Pcm(track.Left[i]));
                        writer.Write(Pcm(track.Right[i]));
                    }
                    else
                    {
                        writer.Write(Pcm((track.Left[i] + track.Right[i]) * 0.5f));
                    }
                }
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            importer.forceToMono = !stereo;
            importer.loadInBackground = false;
            var settings = importer.defaultSampleSettings;
            // Compressed in memory, not streamed: the hunt's layers must all be ready at the same instant to start in sync.
            settings.loadType = AudioClipLoadType.CompressedInMemory;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = loop ? 0.45f : 0.6f;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }

        private static short Pcm(float sample)
        {
            return (short)Mathf.Clamp(Mathf.RoundToInt(sample * short.MaxValue), short.MinValue, short.MaxValue);
        }
    }
}
