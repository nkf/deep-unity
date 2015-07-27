namespace QAI.Utility {
    public struct Coordinates {
        public readonly int x, y;

        public Coordinates(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Coordinates other) {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Coordinates && Equals((Coordinates) obj);
        }

        public override int GetHashCode() {
            unchecked { return (x*397) ^ y; }
        }

        public override string ToString() {
            return string.Format("[{0},{1}]", x, y);
        }
    }
}