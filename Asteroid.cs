using System;
using OpenGLES;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Reflection;

namespace com.koushikdutta.sensoryoverload
{
    struct Asteroid
    {
        public PhysicsObject PhysicsObject;
        public Vector4f RotationVector;
        public float Rotation;
        public float RotationSpeed;
        public int CreationTime;
        public int Size;
        public Vector4f RealColor;
    }
}