using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalProto {
    public class LavaSpawner : MonoBehaviour {

        public readonly string LavaPrefab = "Lava";
        private Lava[][] _lavaGrid;
    
        private const int GridX = 3;
        private const int GridY = 3;

        void Start() {
            _lavaGrid = new Lava[GridX][];
            for (var i = 0; i < _lavaGrid.Length; i++) {
                _lavaGrid[i] = new Lava[GridY];
            }

            StartCoroutine(LavaSpawnCoroutine());
            StartCoroutine(LavaUpdateCoroutine());
        }

        private IEnumerator LavaSpawnCoroutine() {
            while (true) {
                SpawnLava();
                yield return new WaitForSeconds(5);
            }
        }    
        private const float UpdateSpeed = 0.05f;
        private IEnumerator LavaUpdateCoroutine() {
            while (true) {
                for (var x = 0; x < _lavaGrid.Length; x++) {
                    for (var y = 0; y < _lavaGrid[x].Length; y++) {
                        var lava = _lavaGrid[x][y];
                        if(lava == null) continue;
                        lava.Level += UpdateSpeed;
                        if(lava.Level > Lava.MaxLevel) {
                            Destroy(lava.gameObject);
                            _lavaGrid[x][y] = null;
                        }
                    }
                }
                yield return new WaitForSeconds(UpdateSpeed);
            }
        }

        private void SpawnLava() {
            var i = GetFreeIndex(_lavaGrid);
            var x = i / GridX;
            var y = i % GridY;

            var obj = Instantiate(Resources.Load<GameObject>(LavaPrefab));
            obj.transform.localPosition = new Vector3(x*10+4.5f, -0.5f, y*10+4.5f);
            var lava = obj.GetComponent<Lava>();
            lava.Level = 0;

            _lavaGrid[x][y] = lava;
        }

        private int GetFreeIndex<T>(T[][] a) {
            var free = new List<int>();
            for (int x = 0; x < a.Length; x++) {
                for (int y = 0; y < a[x].Length; y++) {
                    if(a[x][y] == null) free.Add(x*a.Length + y);
                }
            }
            if (free.Count == 0) return -1;
            return free[Random.Range(0, free.Count)];
        }
    }
}
