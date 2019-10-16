using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;

namespace GameManager
{
    public class SoundManager : Singleton<SoundManager>
    {
        [FormerlySerializedAs("soundSourcePrefab")] public AudioSource _soundSourcePrefab;
        [FormerlySerializedAs("soundSourceParent")] public Transform _soundSourceParent;
        private ObjectPooling<AudioSource> _soundSources;
        private GCManager _gcManager;

        private void Start()
        {
            _soundSources = new ObjectPooling<AudioSource>(_soundSourcePrefab, _soundSourceParent);
        }

        public void AddPlaySound(float time, AudioClip audioClip)
        {
            if (_gcManager == null) _gcManager = GCManager.Instance;
            StartCoroutine(PlaySound(time, audioClip));
        }

        private IEnumerator PlaySound(float time, AudioClip audioClip)
        {
            yield return _gcManager.Waitfor.ContainsKey(time)
                ? (WaitForSeconds)_gcManager.Waitfor[time]
                : (WaitForSeconds)_gcManager.PushDataOnWaitfor(time, new WaitForSeconds(time));

            var obj = _soundSources.PopObject();
            obj.PlayOneShot(audioClip);
            StartCoroutine(Utility.Utility.SetActive(audioClip.length + 0.5f, obj.gameObject));
        }
    }
}
