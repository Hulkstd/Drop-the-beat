using System;
using GameManager;
using UnityEngine;

namespace MainScene
{
    public class AudioPeer : Singleton<AudioPeer>
    {
        public AudioSource _audioSource;
        public float[] _samples = new float[512];

        private float[] _bufferDecrease = new float[8];
        private float[] _freqBand = new float[8];
        private float[] _freqBandHighest = new float[8];
        public float[] _bandBuffer = new float[8];
        public float[] _audioBand = new float[8];
        public float[] _audioBandBuffer = new float[8];
        
        private void Update()
        {
            GetSpectrumAudioSource();
            MakeFrequencyBands();
            BandBuffer();
            CreateAudioBands();
        }

        private void GetSpectrumAudioSource()
        {
            _audioSource.GetSpectrumData(_samples, 0, FFTWindow.Blackman);
        }

        private void CreateAudioBands()
        {
            for (var i = 0; i < 8; i++)
            {
                if (_freqBand[i] > _freqBandHighest[i])
                    _freqBandHighest[i] = _freqBand[i];

                _audioBand[i] = (_freqBand[i] / _freqBandHighest[i]);
                _audioBandBuffer[i] = (_bandBuffer[i] / _freqBandHighest[i]);
            }
        }

        private void BandBuffer()
        {
            for (var i = 0; i < 8; i++)
            {
                if (_freqBand[i] > _bandBuffer[i])
                {
                    _bandBuffer[i] = _freqBand[i];
                    _bufferDecrease[i] = 0.005f;
                }

                if (_freqBand[i] < _bandBuffer[i])
                {
                    _bandBuffer[i] -= _bufferDecrease[i];
                    _bufferDecrease[i] *= 1.2f; 
                }
            }
        }

        private void MakeFrequencyBands()
        {
            var count = 0;

            for (var i = 0; i < 8; i++)
            {
                var sampleCount = (int) Mathf.Pow(2, i) * 2;

                if (i == 7)
                    sampleCount += 2;

                var average = 0f;
                for (var j = 0; j < sampleCount; j++)
                {
                    average += _samples[count] * (count + 1);
                    count++;
                }

                average /= count;

                _freqBand[i] = average * 10;
            }
        }
    }
}
