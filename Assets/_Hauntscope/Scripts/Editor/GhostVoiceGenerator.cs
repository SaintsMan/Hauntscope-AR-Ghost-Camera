using System;
using Hauntscope.Gameplay.Config;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    // Writes one ghost's sounds into Audio/Ghosts/<Name>/ and returns them as its GhostVoice.
    internal static class GhostVoiceGenerator
    {
        private const string Folder = "Assets/_Hauntscope/Audio/Ghosts";
        private const float CryPeak = -3f;

        public static GhostVoice Build(string assetName, GhostVoiceRecipe recipe)
        {
            var folder = $"{Folder}/{assetName}";
            return new GhostVoice(
                Write(folder, "Whisper", recipe.Whisper, recipe.WhisperPeak, true),
                Write(folder, "Ability", recipe.Ability, CryPeak, false),
                Write(folder, "Scare", recipe.Scare, CryPeak, false),
                Write(folder, "Capture", recipe.Capture, CryPeak, false),
                Write(folder, "Escape", recipe.Escape, CryPeak, false),
                Write(folder, "Stagger", recipe.Stagger, CryPeak, false));
        }

        private static AudioClip Write(string folder, string name, Func<float[]> synthesise, float peakDb, bool loop)
        {
            var path = $"{folder}/{name}.wav";
            if (synthesise == null)
            {
                AssetDatabase.DeleteAsset(path);
                return null;
            }

            SfxGenerator.Save(folder, name, synthesise(), peakDb, loop);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
