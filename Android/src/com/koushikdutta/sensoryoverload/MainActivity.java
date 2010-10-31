package com.koushikdutta.sensoryoverload;

import com.koushikdutta.monojavabridge.MonoBridge;
import com.koushikdutta.monojavabridge.MonoProxy;

public class MainActivity extends android.app.Activity implements MonoProxy, android.opengl.GLSurfaceView.Renderer, android.hardware.SensorEventListener
{
	static
	{
		MonoBridge.link(MainActivity.class, "onKeyDown", "(ILandroid/view/KeyEvent;)Z", "System.Int32,android.view.KeyEvent");
		MonoBridge.link(MainActivity.class, "onCreate", "(Landroid/os/Bundle;)V", "android.os.Bundle");
		MonoBridge.link(MainActivity.class, "onSurfaceCreated", "(Ljavax/microedition/khronos/opengles/GL10;Ljavax/microedition/khronos/egl/EGLConfig;)V", "javax.microedition.khronos.opengles.GL10,javax.microedition.khronos.egl.EGLConfig");
		MonoBridge.link(MainActivity.class, "onSurfaceChanged", "(Ljavax/microedition/khronos/opengles/GL10;II)V", "javax.microedition.khronos.opengles.GL10,System.Int32,System.Int32");
		MonoBridge.link(MainActivity.class, "onDrawFrame", "(Ljavax/microedition/khronos/opengles/GL10;)V", "javax.microedition.khronos.opengles.GL10");
		MonoBridge.link(MainActivity.class, "onSensorChanged", "(Landroid/hardware/SensorEvent;)V", "android.hardware.SensorEvent");
		MonoBridge.link(MainActivity.class, "onAccuracyChanged", "(Landroid/hardware/Sensor;I)V", "android.hardware.Sensor,System.Int32");

	}

	@Override
	public native boolean onKeyDown(int arg0,android.view.KeyEvent arg1);
	@Override
	protected native void onCreate(android.os.Bundle arg0);
	@Override
	public native void onSurfaceCreated(javax.microedition.khronos.opengles.GL10 arg0,javax.microedition.khronos.egl.EGLConfig arg1);
	@Override
	public native void onSurfaceChanged(javax.microedition.khronos.opengles.GL10 arg0,int w,int h);
	@Override
	public native void onDrawFrame(javax.microedition.khronos.opengles.GL10 arg0);
	@Override
	public native void onSensorChanged(android.hardware.SensorEvent arg0);
	@Override
	public native void onAccuracyChanged(android.hardware.Sensor arg0,int arg1);



	long myGCHandle;
	public long getGCHandle() {{
		return myGCHandle;
	}}

	public void setGCHandle(long gcHandle) {{
		myGCHandle = gcHandle;
	}}

	@Override
	protected void finalize() throws Throwable {{
	    super.finalize();
	    MonoBridge.releaseGCHandle(myGCHandle);
	}}

}
