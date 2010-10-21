using System;
using System.Collections.Generic;
using System.Text;
using OpenGLES;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using android.graphics;
using android.opengl;
using android.content;

namespace com.koushikdutta.sensoryoverload
{
    class Texture
    {
        private Texture()
        {
        }

        uint myName;

        public uint Name
        {
            get { return myName; }
        }

        int myWidth;

        public int Width
        {
            get { return myWidth; }
        }
        int myHeight;

        public int Height
        {
            get { return myHeight; }
        }

        unsafe public static Texture LoadResource(Context context, int resource)
        {
            Texture ret = new Texture();

            uint tex;
            gl.GenTextures(1, &tex);
            ret.myName = tex;

            gl.BindTexture(gl.GL_TEXTURE_2D, ret.myName);

			// do stuff to load it
			var inputStream = context.getResources().openRawResource(resource);
            try
            {
                var bitmap = BitmapFactory.decodeStream(inputStream);
                ret.myWidth = bitmap.Width;
                ret.myHeight = bitmap.Height;
                GLUtils.texImage2D(GLES20.GL_TEXTURE_2D, 0, bitmap, 0);
                Console.WriteLine(gl.GetError());
            }
            finally
            {
                inputStream.close();
            }

            gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MIN_FILTER, gl.GL_LINEAR);
            gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MAG_FILTER, gl.GL_LINEAR);

            ret.myPositionCoords = new float[] 
            { 
                0, -ret.Height, 0, 
                0, 0, 0, 
                ret.myWidth, -ret.myHeight, 0, 
                ret.myWidth, 0, 0 
            };

            return ret;
        }

        static readonly float[] mySpriteCoords = new float[] 
        { 
            0, 1,
            0, 0,
            1, 1,
            1, 0
        };

        float[] myPositionCoords;

        unsafe public void DrawCenteredSprite()
        {
            gl.Color4f(1, 1, 1, 0);
            gl.BindTexture(gl.GL_TEXTURE_2D, myName);
            gl.Translatef(-myWidth / 2, myHeight / 2, 0);
            gl.EnableClientState(gl.GL_TEXTURE_COORD_ARRAY);
            gl.EnableClientState(gl.GL_VERTEX_ARRAY);

            fixed (float* spritePointer = mySpriteCoords, positionPointer = myPositionCoords)
            {
                gl.VertexPointer(3, gl.GL_FLOAT, 0, (IntPtr)positionPointer);
                gl.TexCoordPointer(2, gl.GL_FLOAT, 0, (IntPtr)spritePointer);
                gl.DrawArrays(gl.GL_TRIANGLE_STRIP, 0, 4);
            }

            gl.DisableClientState(gl.GL_TEXTURE_COORD_ARRAY);
            gl.DisableClientState(gl.GL_VERTEX_ARRAY);
        }
    }
}
