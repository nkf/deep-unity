namespace QAI.Utility {
    public struct Coordinates {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;
        public Coordinates(int x, int y, int z)
            : this() {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(Coordinates other) {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj) {
            if(ReferenceEquals(null, obj)) return false;
            return obj is Coordinates && Equals((Coordinates)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return hashCode;
            }
        }

        public override string ToString() {
            return string.Format("[{0},{1},{2}]", X, Y, Z);
        }
    }
}