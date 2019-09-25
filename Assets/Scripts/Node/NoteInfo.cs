using UnityEngine;
using UnityEngine.Serialization;

namespace Node
{
    public class NoteInfo : MonoBehaviour
    {
        [FormerlySerializedAs("audio")] public AudioClip _audio;
    }
}
