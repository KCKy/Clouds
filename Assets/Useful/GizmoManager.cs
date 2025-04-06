using System.Collections.Generic;
using UnityEngine;

namespace Useful
{
    /// <summary>
    /// Provides a simple, thread safe way to register gizmos to be drawn for some period of time.
    /// </summary>
    public class GizmoManager : MonoBehaviour
    {
        [SerializeField] bool draw;
        [SerializeField] bool onlyDrawWhenSelected;
        readonly object _lock = new();
        // holds all the gizmos to keep drawing
        readonly Dictionary<object, List<GizmoObject>> _objects = new();

        void Awake()
        {
            lock (_lock)
            {
                _objects.Clear();
            }
        }

        void Draw()
        {
            lock (_lock)
            {
                foreach (var (_, list) in _objects)
                {
                    foreach (var gizmoObject in list)
                    {
                        if (Gizmos.color != gizmoObject.Color)
                            Gizmos.color = gizmoObject.Color;
                        gizmoObject.Draw();
                    }
                }
            }
        }
        /// <summary>
        /// Adds a gizmo to be drawn, until <see cref="Expire"/> with the same duration is called.
        /// </summary>
        public void Add(object duration, GizmoObject obj)
        {
            lock (_lock)
            {
                if (!_objects.ContainsKey(duration))
                    _objects.Add(duration, new());
                _objects[duration].Add(obj);
            }
        }
        /// <summary>
        /// Adds a multiple gizmos to be drawn, until <see cref="Expire"/> with the same duration is called.
        /// </summary>
        public void Add(object duration, IEnumerable<GizmoObject> obj)
        {
            lock (_lock)
            {
                if (!_objects.ContainsKey(duration))
                    _objects.Add(duration, new());
                _objects[duration].AddRange(obj);
            }
        }
        /// <summary>
        /// Stops drawing all gizmos with the given duration.
        /// </summary>
        public void Expire(object duration)
        {
            lock (_lock)
            {
                if (_objects.TryGetValue(duration, out var list))
                    list.Clear();
            }
        }

        void OnDrawGizmos()
        {
            if (draw && !onlyDrawWhenSelected)
            {
                Draw();
            }
        }

        void OnDrawGizmosSelected()
        {
            if (draw && onlyDrawWhenSelected)
            {
                Draw();
            }
        }

        public abstract class GizmoObject
        {
            public readonly Color Color;

            protected GizmoObject(Color color)
            {
                Color = color;
            }
            public abstract void Draw();
        }

        public class Line : GizmoObject
        {
            readonly Vector3 _from;
            readonly Vector3 _to;
            /// <summary>
            /// A line from 'from' to 'to'.
            /// </summary>
            public Line(Color color, Vector3 from, Vector3 to) : base(color)
            {
                _from = from;
                _to = to;
            }
            public override void Draw()
            {
                Gizmos.DrawLine(_from, _to);
            }
        }

        public class Cube : GizmoObject
        {
            readonly Vector3 _pos;
            readonly Vector3 _size;
            /// <summary>
            /// A wireframe cube centered at 'pos' with width, height and depth equal to the x, y and z components of 'size'.
            /// </summary>
            public Cube(Color color, Vector3 pos, Vector3 size) : base(color)
            {
                _pos = pos;
                _size = size;
            }
            /// <summary>
            /// A wireframe cube centered at 'pos' with width, height and depth all equal to 'size'.
            /// </summary>
            public Cube(Color color, Vector3 pos, float size) : base(color)
            {
                _pos = pos;
                _size = Vector3.one * size;
            }
            public override void Draw()
            {
                Gizmos.DrawWireCube(_pos, _size);
            }
        }
        public class Sphere : GizmoObject
        {
            readonly Vector3 _pos;
            readonly float _radius;
            /// <summary>
            /// A wireframe sphere centered at 'pos' with radius 'radius'.
            /// </summary>
            public Sphere(Color color, Vector3 pos, float radius) : base(color)
            {
                _pos = pos;
                _radius = radius;
            }
            public override void Draw()
            {
                Gizmos.DrawWireSphere(_pos, _radius);
            }
        }
        public class Mesh : GizmoObject
        {
            readonly UnityEngine.Mesh _mesh;
            readonly Vector3 _pos;
            readonly Vector3 _scale;
            readonly Quaternion _rotation;
            /// <summary>
            /// A wireframe mesh with origin at 'pos'.
            /// </summary>
            public Mesh(Color color, UnityEngine.Mesh mesh, Vector3 pos) : base(color)
            {
                _mesh = mesh;
                _pos = pos;
                _scale = Vector3.one;
                _rotation = Quaternion.identity;
            }
            /// <summary>
            /// A wireframe mesh with origin at 'pos', with given scale and rotation.
            /// </summary>
            public Mesh(Color color, UnityEngine.Mesh mesh, Vector3 pos, Vector3 scale, Quaternion rotation) : base(color)
            {
                _mesh = mesh;
                _pos = pos;
                _scale = scale;
                _rotation = rotation;
            }

            public override void Draw()
            {
                Gizmos.DrawWireMesh(_mesh, _pos, _rotation, _scale);
            }
        }
    }
}
