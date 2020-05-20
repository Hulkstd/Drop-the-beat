using UnityEngine;

namespace GameManager.SaveData
{
    public static class VolumeManager
    {
        private abstract class Prefs
        {
            public static void SetVolume_(float volume) => PlayerPrefs.SetFloat("Volume", volume);
            public static float LoadVolume(float defaultVolume) => PlayerPrefs.GetFloat("Volume", defaultVolume);
            public static void Save() => PlayerPrefs.Save();
        }

        public static void SetVolume(float volume)
        {
            Prefs.SetVolume_(volume);
            Prefs.Save();
        }

        public static float GetVolume(float defaultVolume = 0)
            => Prefs.LoadVolume(defaultVolume);
    }
}