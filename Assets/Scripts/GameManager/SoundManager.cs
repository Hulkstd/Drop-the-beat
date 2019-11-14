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
            yield return _gcManager.Waitfor.ContainsKey(time + "wfs")
                ? (WaitForSeconds) _gcManager.Waitfor[time + "wfs"]
                : (WaitForSeconds) _gcManager.PushDataOnWaitfor(time + "wfs", new WaitForSeconds(time));

            var obj = _soundSources.PopObject();
            obj.PlayOneShot(audioClip);

            try
            {
                StartCoroutine(audioClip == null
                    ? Utility.Utility.SetActive(0.5f, obj.gameObject)
                    : Utility.Utility.SetActive(audioClip.length + 0.5f, obj.gameObject));
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log(obj.gameObject == null);
                Debug.Log(audioClip.length);
            }
        }
    }
}
