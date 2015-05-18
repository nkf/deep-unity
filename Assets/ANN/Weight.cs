using System;
using System.Collections.Generic;
using System.Linq;

namespace QNetwork {
	public interface Weight {
        float Magnitude();
        float PrevDelta();
        void Adjust(float delta, float ext);
	}

    public class SimpleWeight : Weight {
        private float w, dw = 0f;
        
        public SimpleWeight(float w) {
            this.w = w;
        }

        public float Magnitude() {
            return w;
        }

        public float PrevDelta() {
            return dw;
        }

        public void Adjust(float delta, float ext) {
            dw = delta + ext;
            w += dw;
        }
    }

    public class SharedWeight : Weight {
        private float w, dw = 0f, temp = 0f;
        private int cap, sat = 0;

        public SharedWeight(float w, int cap) {
            this.w = w;
            this.cap = cap;
        }

        public float Magnitude() {
            return w;
        }

        public float PrevDelta() {
            return dw;
        }

        public void Adjust(float delta, float ext) {
            if (++sat >= cap) {
                dw = temp + delta + ext;
                w += dw;
                temp = 0f;
                sat = 0;
            } else {
                temp += delta;
            }
        }
    }
}
