using System;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace QAI.Utility {
    public class QGrid {
        public int GridSize { get; private set; }
        public Transform Transform { get; private set; }
        public readonly float ResolutionX, ResolutionY, ResolutionZ;
        public Vector3 Offset { get; private set; }
        public Vector3 Center { get { return Transform.position + Offset; } }
        public Vector3 Size { get; private set; }
        public BoxBounds Bounds { get { return new BoxBounds(Center, Size, Transform.rotation, Transform.position); } }
        public Axis NormalAxis { get; private set; }
        private readonly Matrix<float> _matrix;
        public Matrix<float> Matrix {
            get { return _matrix.Clone(); }
        }

        public QGrid(int size, Transform transform, GridSettings gs = null) {
            if(gs == null) gs = new GridSettings();
            _matrix = Matrix<float>.Build.Dense(size, size);
            GridSize = size;
            Transform = transform;
            Offset = gs.Offset;
            Size = new Vector3(size * gs.ResolutionX, size * gs.ResolutionY, size * gs.ResolutionZ);
            NormalAxis = gs.NormalAxis;
            ResolutionX = gs.ResolutionX;
            ResolutionY = gs.ResolutionY;
            ResolutionZ = gs.ResolutionZ;
        }

        public QGrid(QGrid other) {
            _matrix = other._matrix.Clone();
            GridSize = other.GridSize;
            Transform = other.Transform;
            Offset = other.Offset;
            Size = other.Size;
            NormalAxis = other.NormalAxis;
            ResolutionX = other.ResolutionX;
            ResolutionY = other.ResolutionY;
            ResolutionZ = other.ResolutionZ;
        }

        public float this[int x, int y] {
            get { return _matrix[x, y]; }
            set { _matrix[x, y] = value; }
        }

        public float this[Coordinates c] {
            get { return _matrix[c.x, c.y]; }
            set { _matrix[c.x, c.y] = value; }
        }

        public void Populate(Func<BoxBounds, float> populator) {
            Iterate((c, b) => this[c] = populator(b));
        }

        public void Populate(Func<BoxBounds, Coordinates, float> populator) {
            Iterate((c, b) => this[c] = populator(b,c));
        }

        /// <summary>
        /// Locate a point's position in the grid
        /// </summary>
        /// <param name="p">The point, which position will be located in the grid</param>
        /// <returns>If the point is within the bounds of the grid the coordinates to the grid cell where in the point is located will be returned</returns>
        public Coordinates? Locate(Vector3 p) {
            var b = Bounds;

            //Since lower bound is exclusive we subtract a small amount to make the upper bound exclusive aswell. 
            b.size -= new Vector3(0.01f, 0.01f, 0.01f);
            if(b.Contains(p)) {
				p = p.RotatePoint(Transform.position, Quaternion.Inverse(Transform.rotation));
                var d = p - Center;
                d = new Vector3(d.x / ResolutionX, d.y / ResolutionY, d.z / ResolutionZ);
                var halfSize = (GridSize - 1)/2f;
                var c = new Vector3(halfSize,halfSize,halfSize);
                var r = c + d;
                if(NormalAxis == Axis.Y) return new Coordinates(Mathf.RoundToInt(r.x), Mathf.RoundToInt(r.z));
                if(NormalAxis == Axis.Z) return new Coordinates(Mathf.RoundToInt(r.x), Mathf.RoundToInt(r.y));
            }
            return null;
        }

        public void SetAll(float value) {
            for (int x = 0; x < _matrix.RowCount; x++) {
                for (int y = 0; y < _matrix.ColumnCount; y++) {
                    _matrix[x, y] = value;
                }
            }
        }

        public void DebugDraw(Func<float, Color> colorFunc = null) {
            Iterate((coor, b) => {
                var color = colorFunc == null ? Color.white : colorFunc(this[coor]);
                if (NormalAxis == Axis.Z) {
                    b.DebugDrawXY(color);
                } else {
                    b.DebugDrawXZ(color);
                }
            });
        }

        private void Iterate(Action<Coordinates, BoxBounds> f) {
            var left = Center.x -((GridSize/2f)*ResolutionX - ResolutionX/2);
            var resolutionUp = NormalAxis == Axis.Z ? ResolutionY : ResolutionZ;
            var center = NormalAxis == Axis.Z ? Center.y : Center.z;
            var top = center - ((GridSize/2f)*resolutionUp - resolutionUp/2);
            var cellsize = new Vector3(ResolutionX, ResolutionY, ResolutionZ);
            var c = Vector3.zero;
            for(var x = 0; x < GridSize; x++) {
                for(var y = 0; y < GridSize; y++) {
                    if (NormalAxis == Axis.Z)
                        c = new Vector3(left + x*ResolutionX, top + y*resolutionUp, 0);
                    if (NormalAxis == Axis.Y)
                        c = new Vector3(left + x*ResolutionX, 0, top + y*resolutionUp);
                    c = c.RotatePoint(Transform.position, Transform.rotation);
                    f(new Coordinates(x, y), new BoxBounds(c, cellsize, Transform.rotation));
                }
            }
        }
    }

    public enum Axis {
        Z, Y
    }

    public class GridSettings {
        public Vector3 Offset;
        public Axis NormalAxis;
        public float ResolutionX, ResolutionY, ResolutionZ;
        public GridSettings() {
            //Do not set the value for normal axis in the constructor (or at the declaration) because it bugs the object initializer, by always overriding it.
            //By not setting the value it will default to the first declared value in the Axis enum.
            Offset = Vector3.zero;
            ResolutionX = 1;
            ResolutionY = 1;
            ResolutionZ = 1;
        }
    }
}