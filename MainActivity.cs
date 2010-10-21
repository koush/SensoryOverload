using System;
using android.app;
using android.widget;
using android.os;
using android.graphics;
using android.opengl;
using android.view;
using OpenGLES;
using System.Collections.Generic;
using android.hardware;

namespace com.koushikdutta.sensoryoverload
{
	unsafe public class MainActivity : Activity, GLSurfaceView.Renderer, SensorEventListener
	{
		int myWidth; 
		int myHeight;
		
		// this is how long the user is given to get away from a brand new asteroid
        const int AsteroidGracePeriod = 5000;

        readonly Vector4f myBulletLight = new Vector4f(1, .5f, 0, 1);
        readonly float myBulletDuration = 1f;
        int myLastSpray = 0;
        const int SprayCooldown = 5000;
        int myLastBullet = System.Environment.TickCount;
		int myLastRender = System.Environment.TickCount;
        PhysicsObject myShip = new PhysicsObject();
        float myShipRotation = 0;
        List<PhysicsObject> myBullets = new List<PhysicsObject>();
        List<Asteroid> myAsteroids = new List<Asteroid>();
        readonly float[] myWhiteColor = new float[] { 1, 1, 1, 1 };
        Texture myShipTexture;
        Texture myBulletTexture;

        struct AsteroidMesh
        {
            public short[] SphereIndices;
            public Vector4f[] SpherePoints;
            public Vector4f[] SphereNormals;
        }
        AsteroidMesh[] myAsteroidMeshes = new AsteroidMesh[4];

        void FireBullet()
        {
            myLastBullet = System.Environment.TickCount;
            myBullets.Add(CreateBullet(myShipRotation));
        }

        void Rollover(ref float value, float max)
        {
            // this function makes asteroids and the ship roll over to the other side of the screen
            // if it goes over the edge
            float half = max / 2;
            if (value < -half)
                value += max;
            else if (value > half)
                value -= max;
        }

        PhysicsObject ApplyPhysics(PhysicsObject po, Vector4f acceleration, float elapsedTime, bool rollOver)
        {
            // update the position and velocity of the object
            po.Location += po.Velocity.Scale(elapsedTime);
            po.Velocity += acceleration.Scale(elapsedTime);

            if (rollOver)
            {
                // roll it over the edge of the screen if necessary
                Rollover(ref po.Location.X, myWidth);
                Rollover(ref po.Location.Y, myHeight);
            }
            return po;
        }

        bool IsInBounds(PhysicsObject po)
        {
            // return whether the object is in screen bounds or not
            return po.Location.X < myWidth / 2 && po.Location.X > -myWidth / 2 && po.Location.Y < myHeight / 2 && po.Location.Y > -myHeight / 2;
        }

        void TranslateToObjectSpace(PhysicsObject po)
        {
            // handy function to move into object space given a PhysicsObject
            gl.Translatef(po.Location.X, po.Location.Y, po.Location.Z);
        }

        Vector4f myShipVelocity;
        Vector4f GetShipVelocityVector()
        {
            return myShipVelocity;
			/*
            if (myGSensor == null)
                return Vector4f.Zero;

            // return the velocity vector of the ship from the G-Sensor
            GVector gv = myGSensor.GetGVector().Scale(50f);
            Vector4f ret = new Vector4f();
            ret.X = (float)gv.X;
            ret.Y = (float)-gv.Y;
            ret.Z = 0;
            return ret;
            */
        }

        unsafe void ProcessScene()
        {
            // see how much time has elapsed
            // this value is used by the physics system 
            int tickCount = System.Environment.TickCount;
            float elapsedTime = ((float)tickCount - myLastRender) / 1000f;
            myLastRender = tickCount;

            // clear the screen
            gl.Clear(gl.GL_COLOR_BUFFER_BIT | gl.GL_DEPTH_BUFFER_BIT);

            // calculate the screen depth. this may change when the screen is resized/rotated
            float sceneDepth = -(float)(myWidth / Math.Sin(45d / 180d * Math.PI));

            gl.MatrixMode(gl.GL_PROJECTION);
            gl.LoadIdentity();

            float[] ambientLight = new float[] { .5f, .5f, .5f, 1 };
            float[] diffuseLight = new float[] { 1f, 1f, 1f, 1 };
            float[] lightPosition = new float[] { 0, 0, 1, 0 };
            fixed (float* ap = ambientLight, dp = diffuseLight, pp = lightPosition)
            {
                gl.Enable(gl.GL_LIGHT1);
                gl.Lightfv(gl.GL_LIGHT1, gl.GL_AMBIENT, ap);
                gl.Lightfv(gl.GL_LIGHT1, gl.GL_DIFFUSE, dp);
                gl.Lightfv(gl.GL_LIGHT1, gl.GL_POSITION, pp);
                gl.Disable(gl.GL_LIGHT0);
                //gl.Enable(gl.GL_LIGHT2);
            }

            gluPerspective(45, (float)myWidth / (float)myHeight, -sceneDepth - 150, -sceneDepth + 150);

            // reset the model matrix
            gl.MatrixMode(gl.GL_MODELVIEW);
            gl.LoadIdentity();

            // if we've fired a bullet, then the ship will temporarily have a "glow" emanating from it
            // calculate that light position and glow value
            float bulletLightIntensity = (tickCount - myLastBullet) / 1000f;
            if (bulletLightIntensity > myBulletDuration)
                bulletLightIntensity = 0;
            else
                bulletLightIntensity = 1 - (bulletLightIntensity / myBulletDuration);
            Vector4f bulletLight = myBulletLight.Scale(bulletLightIntensity);
            float[] lightArgs = new float[] { bulletLight.X, bulletLight.Y, bulletLight.Z, 1 };
            float[] posArgs = new float[] { myShip.Location.X, myShip.Location.Y, sceneDepth, 1 };
            // set the light
            fixed (float* bp = lightArgs, pp = posArgs, wp = myWhiteColor)
            {
                gl.Lightfv(gl.GL_LIGHT2, gl.GL_DIFFUSE, bp);
                gl.Lightfv(gl.GL_LIGHT2, gl.GL_POSITION, pp);
                gl.Materialfv(gl.GL_FRONT_AND_BACK, gl.GL_AMBIENT, wp);
                gl.Materialfv(gl.GL_FRONT_AND_BACK, gl.GL_DIFFUSE, wp);
            }

            gl.Enable(gl.GL_BLEND);
            // enable texturing for only the sprite: ships and bullets
            gl.Enable(gl.GL_TEXTURE_2D);
            // draw and move the ships and bullets
            ProcessShip(elapsedTime, sceneDepth);
            ProcessBullets(elapsedTime, sceneDepth);
            gl.Disable(gl.GL_TEXTURE_2D);

            // turn on depth test for only the 3d asteroids
            gl.Enable(gl.GL_DEPTH_TEST);
            // draw and move the asteroids
            ProcessAsteroids(elapsedTime, sceneDepth);
            gl.Disable(gl.GL_DEPTH_TEST);
            gl.Disable(gl.GL_BLEND);
        }

        public override bool onKeyDown(int arg0, KeyEvent arg1)
        {
            if (arg0 == KeyEvent.KEYCODE_DPAD_LEFT || arg0 == KeyEvent.KEYCODE_DPAD_RIGHT || arg0 == KeyEvent.KEYCODE_DPAD_UP || arg0 == KeyEvent.KEYCODE_DPAD_DOWN)
            {
                FireBullet();
                return true;
            }
            if (arg0 == KeyEvent.KEYCODE_DPAD_CENTER)
            {
                FireSpray();
                return true;
            }
            return base.onKeyDown(arg0, arg1);
        }

        void ProcessShip(float elapsedTime, float sceneDepth)
        {
            myShip.Velocity = GetShipVelocityVector();

            // this makes the ship rotate slowly if the user turns it.
            // the ship can do a max turn of 360 degrees in 1 second, so enforce that
            float angle = ZRotationFromVector(myShip.Velocity);
            float maxRotateAllowed = 360f * elapsedTime;
            float delta = angle - myShipRotation;
            if (Math.Abs(delta) > maxRotateAllowed)
            {
                if ((delta > 0 && delta < 180) || delta < -180)
                    myShipRotation += maxRotateAllowed;
                else
                    myShipRotation -= maxRotateAllowed;
            }
            else
            {
                myShipRotation = angle;
            }
            if (myShipRotation < 0)
                myShipRotation += 360;
            else if (myShipRotation > 360)
                myShipRotation -= 360;

            // move the ship
            myShip = ApplyPhysics(myShip, Vector4f.Zero, elapsedTime, true);
            gl.PushMatrix();
            myShip.Location.Z = sceneDepth;
            TranslateToObjectSpace(myShip);
            if (!float.IsNaN(myShipRotation))
                gl.Rotatef(myShipRotation, 0, 0, 1);
            // draw the ship
            myShipTexture.DrawCenteredSprite();
            gl.PopMatrix();
        }

        void ProcessBullets(float elapsedTime, float sceneDepth)
        {
            int i = 0;
            while (i < myBullets.Count)
            {
                PhysicsObject bullet = myBullets[i];
                // move the bullet
                bullet = ApplyPhysics(bullet, Vector4f.Zero, elapsedTime, false);
                bullet.Location.Z = sceneDepth;
                float angle = ZRotationFromVector(bullet.Velocity);

                myBullets[i] = bullet;
                if (IsInBounds(bullet))
                    i++;
                else
                    myBullets.RemoveAt(i);

                gl.PushMatrix();
                TranslateToObjectSpace(bullet);
                gl.Rotatef(angle, 0, 0, 1);
                // draw the bullet
                myBulletTexture.DrawCenteredSprite();
                gl.PopMatrix();
            }
        }

        unsafe void ProcessAsteroids(float elapsedTime, float sceneDepth)
        {
            gl.EnableClientState(gl.GL_VERTEX_ARRAY);
            gl.EnableClientState(gl.GL_NORMAL_ARRAY);
            for (int i = 0; i < myAsteroids.Count; i++)
            {
                Asteroid asteroid = myAsteroids[i];
                // move the asteroid
                asteroid.PhysicsObject = ApplyPhysics(asteroid.PhysicsObject, Vector4f.Zero, elapsedTime, true);
                asteroid.PhysicsObject.Location.Z = sceneDepth;
                // this is the asteroid spin
                asteroid.Rotation += asteroid.RotationSpeed;
                if (asteroid.Rotation > 360)
                    asteroid.Rotation -= 360;
                myAsteroids[i] = asteroid;

                // if the asteroid is new, calculate the asteroid alpha
                float asteroidAlpha = System.Environment.TickCount - asteroid.CreationTime;
                if (asteroidAlpha > AsteroidGracePeriod)
                    asteroidAlpha = 1;
                else
                    asteroidAlpha /= AsteroidGracePeriod;
                float[] color = new float[] { asteroid.RealColor.X, asteroid.RealColor.Y, asteroid.RealColor.Z, asteroidAlpha };

                gl.PushMatrix();
                TranslateToObjectSpace(asteroid.PhysicsObject);
                gl.Rotatef(asteroid.Rotation, asteroid.RotationVector.X, asteroid.RotationVector.Y, asteroid.RotationVector.Z);

                fixed (Vector4f* vp = myAsteroidMeshes[asteroid.Size].SpherePoints, np = myAsteroidMeshes[asteroid.Size].SphereNormals)
                {
                    fixed (short* ip = myAsteroidMeshes[asteroid.Size].SphereIndices)
                    {
                        fixed (float* cp = color)
                        {
                            gl.VertexPointer(4, gl.GL_FLOAT, 0, (IntPtr)vp);
                            gl.NormalPointer(gl.GL_FLOAT, 0, (IntPtr)np);
                            gl.Materialfv(gl.GL_FRONT_AND_BACK, gl.GL_AMBIENT_AND_DIFFUSE, cp);
                            
                            // draw the asteroid
                            gl.DrawElements(gl.GL_TRIANGLES, myAsteroidMeshes[asteroid.Size].SphereIndices.Length, gl.GL_UNSIGNED_SHORT, (IntPtr)ip);
                        }
                    }
                }
                gl.PopMatrix();
            }
            gl.DisableClientState(gl.GL_NORMAL_ARRAY);
            gl.DisableClientState(gl.GL_VERTEX_ARRAY);
        }

        void FireSpray()
        {
            if (System.Environment.TickCount - myLastSpray > SprayCooldown)
            {
                myLastSpray = System.Environment.TickCount;
                for (float angle = myShipRotation - 45; angle <= myShipRotation + 45; angle += 5)
                {
                    myBullets.Add(CreateBullet(angle));
                }
            }
        }

        // get the z axis rotation amount for a 2d vector
        float ZRotationFromVector(Vector4f v)
        {
            float angle = (float)(Math.Acos(v.Normalize().DotProduct(new Vector4f(1, 0, 0, 1))) / Math.PI * 180f);
            if (v.Y < 0)
                angle = 360 - angle;
            return angle;
        }

		// create a bullet flying at the given angle
        // the starting point is the ship
        PhysicsObject CreateBullet(float angle)
        {
            PhysicsObject bullet = new PhysicsObject();
            bullet.Location = myShip.Location;
            bullet.Velocity.X = (float)Math.Cos(angle / 180 * Math.PI);
            bullet.Velocity.Y = (float)Math.Sin(angle / 180 * Math.PI);
            bullet.Velocity = bullet.Velocity.Normalize().Scale(500);
            return bullet;
        }
		
        Asteroid CreateAsteroid()
        {
            // create a large asteroid
            Asteroid newAsteroid = new Asteroid();
            newAsteroid.RealColor.X = Math.Abs(Utilities.RandomNormalizedFloat());
            newAsteroid.RealColor.Y = Math.Abs(Utilities.RandomNormalizedFloat());
            newAsteroid.RealColor.Z = Math.Abs(Utilities.RandomNormalizedFloat());
            newAsteroid.RotationVector = Utilities.RandomVector4f();
            newAsteroid.RotationSpeed = Utilities.RandomNormalizedFloat() * 10;
            newAsteroid.PhysicsObject.Location = Utilities.RandomVector4f();
			newAsteroid.PhysicsObject.Location.Z = 0;
            newAsteroid.PhysicsObject.Velocity = Utilities.RandomVector4f().Scale(50);
            newAsteroid.PhysicsObject.Velocity.Z = (float)Math.Abs(newAsteroid.PhysicsObject.Velocity.Z);
            newAsteroid.CreationTime = System.Environment.TickCount;
            newAsteroid.Size = 3;

            return newAsteroid;
        }

		// reset the game state
        void ResetGame()
        {
            myAsteroids.Clear();
            myAsteroids.Add(CreateAsteroid());
            myAsteroids.Add(CreateAsteroid());
            myAsteroids.Add(CreateAsteroid());
            myShip.Location = Vector4f.Zero;
            myShip.Velocity = Vector4f.Zero;
        }
		
		protected override void onCreate (Bundle arg0)
		{
			base.onCreate (arg0);

            // create the 4 asteroid sizes
            for (int i = 0; i < myAsteroidMeshes.Length; i++)
            {
                Utilities.GenerateSphere(16 * (1 << i), 2, out myAsteroidMeshes[i].SpherePoints, out myAsteroidMeshes[i].SphereNormals, out myAsteroidMeshes[i].SphereIndices);
            }
			
			var surfaceView = new GLSurfaceView(this);
			surfaceView.setRenderer(this);
			setContentView(surfaceView);

            var sm = getSystemService(SENSOR_SERVICE) as SensorManager;
            sm.registerListener(this, sm.getSensorList(SensorManager.SENSOR_ACCELEROMETER).@get(0) as Sensor, SensorManager.SENSOR_DELAY_GAME);
		}

		// This constructor is a requirement for all CLR classes that inherit from java.lang.Object
		protected MainActivity (MonoJavaBridge.JNIEnv env) : base(env)
		{
		}

		public void onSurfaceCreated (javax.microedition.khronos.opengles.GL10 arg0, javax.microedition.khronos.egl.EGLConfig arg1)
		{
            //gl.Enable(gl.GL_COLOR_MATERIAL);
            //gl.Enable(gl.GL_CULL_FACE);
            //gl.CullFace(gl.GL_BACK);
            gl.Disable(gl.GL_CULL_FACE);
            gl.ClearColor(0, 0, 0, 1);
            gl.ShadeModel(gl.GL_SMOOTH);
            gl.ClearDepthf(1.0f);
            gl.DepthFunc(gl.GL_LEQUAL);
            gl.BlendFunc(gl.GL_SRC_ALPHA, gl.GL_ONE_MINUS_SRC_ALPHA); 
            gl.Hint(gl.GL_PERSPECTIVE_CORRECTION_HINT, gl.GL_NICEST);

            gl.MatrixMode(gl.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.Enable(gl.GL_LIGHTING);

            myShipTexture = Texture.LoadResource(this, R.drawable.ship);
            myBulletTexture = Texture.LoadResource(this, R.drawable.bullet);
    	}

		public void onSurfaceChanged (javax.microedition.khronos.opengles.GL10 arg0, int w, int h)
		{
			myWidth = w;
			myHeight = h;
            gl.Viewport(0, 0, w, h);
            gl.MatrixMode(gl.GL_PROJECTION);
            gl.LoadIdentity();

            float sceneDepth = (float)(w / Math.Sin(45d / 180d * Math.PI));
            gluPerspective(45, (float)w / (float)h, sceneDepth - 150, sceneDepth + 150);
		}

        void gluPerspective(float fovy, float aspect, float zNear, float zFar)
        {
            float xmin, xmax, ymin, ymax;

            ymax = zNear * (float)Math.Tan(fovy * 3.1415962f / 360.0);
            ymin = -ymax;
            xmin = ymin * aspect;
            xmax = ymax * aspect;

            gl.Frustumf(xmin, xmax, ymin, ymax, zNear, zFar);
        }
		
		public void onDrawFrame (javax.microedition.khronos.opengles.GL10 arg0)
		{
            ProcessScene();
            ProcessCollisions();
		}

        void ProcessCollisions()
        {
            bool gameOver = false;
            int ai = 0;
            // check which asteroids are colliding with bullets
            while (ai < myAsteroids.Count)
            {
                Asteroid asteroid = myAsteroids[ai];
                int asteroidRadiusSquare = 16 * (1 << asteroid.Size);
                asteroidRadiusSquare *= asteroidRadiusSquare;
                bool collisionFound = false;
                for (int bi = 0; bi < myBullets.Count; bi++)
                {
                    PhysicsObject bullet = myBullets[bi];
                    Vector4f v = bullet.Location - asteroid.PhysicsObject.Location;
                    if (v.LengthSquare < asteroidRadiusSquare)
                    {
                        // remove the asteroid and bullet
                        collisionFound = true;
                        myAsteroids.RemoveAt(ai);
                        myBullets.RemoveAt(bi);

                        // split the asteroid into two if it's not tiny
                        if (asteroid.Size > 0)
                        {
                            Asteroid one = CreateAsteroid();
                            Asteroid two = CreateAsteroid();
                            one.Size = two.Size = asteroid.Size - 1;
                            one.CreationTime = two.CreationTime = asteroid.CreationTime;
                            one.PhysicsObject.Location = two.PhysicsObject.Location = asteroid.PhysicsObject.Location;
                            myAsteroids.Add(one);
                            myAsteroids.Add(two);
                        }
                        break;
                    }
                }

                // see if the asteroid is hitting the ship
                Vector4f sv = myShip.Location - asteroid.PhysicsObject.Location;
                if (sv.LengthSquare < asteroidRadiusSquare && System.Environment.TickCount - asteroid.CreationTime > AsteroidGracePeriod)
                    gameOver = true;

                // dont increment if we removed the asteroid
                if (!collisionFound)
                    ai++;
            }

            // if there zero asteroids, add one
            if (myAsteroids.Count == 0)
            {
                // reset the timer
                //myAsteroidTimer.Enabled = false;
                //myAsteroidTimer.Enabled = true;
                myAsteroids.Add(CreateAsteroid());
            }

            // show the gamer over form if the ship exploded
            if (gameOver)
            {
				/*
                if (MessageBox.Show("Game Over! Try again?", "Sensory Overload", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    ResetGame();
                }
                else
                {
                    Close();
                }
                */
            }
        }

        public void onSensorChanged(SensorEvent arg0)
        {
            myShipVelocity.X = arg0.values[0];
            myShipVelocity.Y = arg0.values[1];
            myShipVelocity.Z = 0;
            myShipVelocity = myShipVelocity.Normalize().Scale(200f);
        }

        public void onAccuracyChanged(Sensor arg0, int arg1)
        {
        }
	}
}   

