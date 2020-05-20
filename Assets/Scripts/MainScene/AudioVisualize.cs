using System;
using System.Collections;
using System.Linq;
using GameManager;
using UnityEngine;

namespace MainScene
{
    public class AudioVisualize : Singleton<AudioVisualize>
    {
        public float _space;
        public Transform _cubePrefab;
        public GameObject[] _cubes;
        public float _maxScale = 500;
        public Camera _camera;
        public float _cool = 2;
        public float _minFov = 55;
        public float _maxFov = 60;
        public int _band = 3;
        private static readonly int CurScale = Shader.PropertyToID("_CurScale");
        public int _h = 32;
        public int _v = 16;

        protected override bool Awake()
        {
            if (!base.Awake())
                return false;
            
            _cubes = new GameObject[_h * _v];
            var arr = new GameObject[_h, _v];
            var index = _h * _v - 1;
            var h = _h;
            var v = _v;
            var i = -1;
            var j = 0;
            var sub = 1;
            var start = true;
            
            while(true)
            {
                for (var k = 0; k < h; k++) 
                {
                    if (index < 0)
                        break;
                    i += sub;
                    CreateCube(i, j, ref arr);
                    _cubes[index] = arr[i, j];
                    _cubes[index].name = index.ToString();
                    index--;
                }

                h--;
                v--;

                if (h < 0) break;

                for (var k = 0; k < v; k++) 
                {
                    if (index < 0)
                        break;
                    j += sub;
                    CreateCube(i, j, ref arr);
                    _cubes[index] = arr[i, j];
                    _cubes[index].name = index.ToString();
                    index--;
                }

                if (v < 0) break;

                sub = -sub; 
            }

            return true;
        }

        private void CreateCube(int i, int j, ref GameObject[,] arr)
        {
            var cube = Instantiate(_cubePrefab, transform);
            
            var position = Vector3.zero; 
            position += (_space * (i - _h/2) + 8.5f) * Vector3.right + _space * j * Vector3.forward; 
            cube.position = position;
            //cube.name = (i + j * _v).ToString();

            arr[i, j] = cube.gameObject;
        }

        private void Update()
        {
            for (var i = 0; i < _h * _v; i++)
            {
                if (ReferenceEquals(_cubes[i], null)) continue;
                
                var scale = _cubes[i].transform.localScale;
                scale.y = AudioPeer.Instance._samples[i] * _maxScale;
                if (Math.Abs(scale.y) < 0.01f)
                    scale.y = 0.01f;
                _cubes[i].transform.localScale = scale;
                _cubes[i].transform.GetChild(0).GetComponent<MeshRenderer>().material
                    .SetVector(CurScale, new Vector4(1, 0.1f * scale.y, 1, 1));
            }

            if (_cool > 0) 
            {
                _cool -= Time.deltaTime;
                return;
            }
            _camera.fieldOfView = AudioPeer.Instance._audioBandBuffer[_band] * (_maxFov - _minFov) + _minFov;
        }
    }
}
