using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace com.koushikdutta.sensoryoverload
{
    [StructLayout(LayoutKind.Sequential, Size = 12), NativeCppClass]
    struct Vector4f
    {
        public Vector4f(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public float X;
        public float Y;
        public float Z;
        public float W;

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(LengthSquare);
            }
        }


        public float LengthSquare
        {
            get
            {
                return X * X + Y * Y + Z * Z;
            }
        }

        public Vector4f Normalize()
        {
            return Scale(1f / Length);
        }

        public Vector4f Scale(float scale)
        {
            Vector4f ret = this;
            ret.X *= scale;
            ret.Y *= scale;
            ret.Z *= scale;
            ret.W = 1;
            return ret;
        }

        public static Vector4f operator +(Vector4f one, Vector4f two)
        {
            one.X += two.X;
            one.Y += two.Y;
            one.Z += two.Z;
            one.W = 1;
            return one;
        }

        public static Vector4f operator -(Vector4f one, Vector4f two)
        {
            one.X -= two.X;
            one.Y -= two.Y;
            one.Z -= two.Z;
            one.W = 1;
            return one;
        }

        public float DotProduct(Vector4f other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        public Vector4f CrossProduct(Vector4f other)
        {
            Vector4f ret = new Vector4f();
            ret.X = Y * other.Z - other.Y * Z;
            ret.Y = Z * other.X - other.Z * X;
            ret.Z = X * other.Y - other.X * Y;
            ret.W = 1;
            return ret;
        }

        public static readonly Vector4f Zero = new Vector4f(0, 0, 0, 0);
    }
}