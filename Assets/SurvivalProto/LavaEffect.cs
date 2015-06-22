using UnityEngine;

namespace SurvivalProto {
    public class LavaEffect : MonoBehaviour {
        public float Speed;
        private Material _material;
        void Start() {
            _material = GetComponent<Renderer>().material;
        }

        private float xOffset = 0;
        void Update () {
            _material.mainTextureOffset = new Vector2(xOffset,0);
            xOffset += Speed * Time.deltaTime;
            if (xOffset > 1f) xOffset = 0;
        }
    }
}
