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
        private void Start()
        {
            _soundSources = new ObjectPooling<AudioSource>(_soundSourcePrefab, _soundSourceParent);
        }

        public void AddPlaySound(float time, AudioClip audioClip)
        {
            StartCoroutine(PlaySound(time, audioClip));
        }

        private IEnumerator PlaySound(float time, AudioClip audioClip)
        {
            yield return GCManager.Waitfor.ContainsKey(time + "wfs")
                ? GCManager.Waitfor[time + "wfs"]
                : GCManager.PushDataOnWaitfor(time + "wfs", new WaitForSeconds(time));

            var obj = _soundSources.PopObject();
            obj.PlayOneShot(audioClip);
            
            StartCoroutine(ReferenceEquals(audioClip, null)
                ? Utility.Utility.SetActive(0.5f, obj.gameObject) 
                : Utility.Utility.SetActive(audioClip.length + 0.5f, obj.gameObject));

            yield return null;
        }
    }
}
