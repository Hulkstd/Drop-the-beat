using UnityEngine;
using UnityEngine.Serialization;

namespace Node
{
    public class NoteInfo : MonoBehaviour
    {
        [FormerlySerializedAs("audio")] public AudioClip _audio;
        [FormerlySerializedAs("longNote")] public bool _longNote;
        [FormerlySerializedAs("timing")] public float _timing;

        public static int Comparision(NoteInfo a, NoteInfo b)
        {
            return a._timing.CompareTo(b._timing);
        }
    }
}
