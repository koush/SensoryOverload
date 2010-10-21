using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Reflection;

namespace com.koushikdutta.sensoryoverload
{
    static class Utilities
    {
        public static void GenerateSphere(float radius, int subdivisions, out Vector4f[] outPoints, out Vector4f[] outNormals, out short[] outIndices)
        {
            // starting with a octahedron, subdivide the surfaces

            List<Vector4f> points = new List<Vector4f>(new Vector4f[]
            {
                new Vector4f(1, 0, 0, 1),
                new Vector4f(0, 1, 0, 1),
                new Vector4f(0, 0, 1, 1),
                new Vector4f(0, -1 ,0, 1),
                new Vector4f(-1, 0, 0, 1),
                new Vector4f(0, 0, -1, 1),
            }
            );
            List<short> indices = new List<short>(new short[]
            {
                0, 1, 2,
                0, 2, 3,
                4, 3, 2,
                1, 4, 2,
                0, 5, 1,
                3, 5, 0,
                5, 4, 1,
                5, 3, 4,
            }
            );

            for (int i = 0; i < subdivisions; i++)
            {
                List<short> newIndices = new List<short>();
                for (int j = 0; j < indices.Count; j += 3)
                {
                    Vector4f np3 = points[indices[j]] + points[indices[j + 1]];
                    Vector4f np4 = points[indices[j + 1]] + points[indices[j + 2]];
                    Vector4f np5 = points[indices[j]] + points[indices[j + 2]];

                    np3 = np3.Normalize();
                    np4 = np4.Normalize();
                    np5 = np5.Normalize();

                    short i0 = indices[j];
                    short i1 = indices[j + 1];
                    short i2 = indices[j + 2];
                    short i3 = (short)points.Count;
                    points.Add(np3);
                    short i4 = (short)points.Count;
                    points.Add(np4);
                    short i5 = (short)points.Count;
                    points.Add(np5);

                    newIndices.Add(i0);
                    newIndices.Add(i3);
                    newIndices.Add(i5);

                    newIndices.Add(i3);
                    newIndices.Add(i4);
                    newIndices.Add(i5);

                    newIndices.Add(i1);
                    newIndices.Add(i4);
                    newIndices.Add(i3);

                    newIndices.Add(i2);
                    newIndices.Add(i5);
                    newIndices.Add(i4);
                }
                indices = newIndices;
            }

            outPoints = points.ToArray();
            outIndices = indices.ToArray();

            List<Vector4f> normals = new List<Vector4f>();
            for (int i = 0; i < outIndices.Length; i++)
            {
                normals.Add(outPoints[outIndices[i]].Normalize());
            }
            outNormals = normals.ToArray();

            for (int i = 0; i < outPoints.Length; i++)
            {
                outPoints[i] = outPoints[i].Scale(radius);
            }
        }

        public static Random Random = new Random();
        public static Vector4f RandomVector4f()
        {
            Vector4f ret = new Vector4f();
            ret.X = Random.Next(255) - 127;
            ret.Y = Random.Next(255) - 127;
            ret.Z = Random.Next(255) - 127;
            ret = ret.Normalize();
            return ret;
        }

        public static float RandomNormalizedFloat()
        {
            float ret = Random.Next(255) - 127;
            ret /= 128f;
            return ret;
        }
    }
}