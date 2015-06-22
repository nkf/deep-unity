using System;
using System.IO;

namespace QAI.Utility {
    public struct AIID {
        public string ID { get; private set; }
        public AIID(string id) : this() {
            if(ReferenceEquals(null, id)) 
                throw new ArgumentException("id may not be null");
            if(id.IndexOfAny(Path.GetInvalidFileNameChars()) == -1)
                throw new ArgumentException("id may not contain invalid file name characters. See System.IO.Path.GetInvalidFileNameChars");
            ID = id;
        }

        public bool Equals(AIID other) {
            return string.Equals(ID, other.ID);
        }

        public override bool Equals(object obj) {
            if(ReferenceEquals(null, obj)) return false;
            return obj is AIID && Equals((AIID)obj);
        }

        public override int GetHashCode() {
            return ID.GetHashCode();
        }
    }
}
