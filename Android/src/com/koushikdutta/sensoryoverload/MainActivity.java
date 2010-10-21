package com.koushikdutta.sensoryoverload;

import com.koushikdutta.monojavabridge.MonoBridge;
import com.koushikdutta.monojavabridge.MonoProxy;

public class MainActivity extends android.app.Activity implements MonoProxy, android.opengl.GLSurfaceView.Renderer
{
	static
	{
		MonoBridge.link(MainActivity.class, "onCreate", "(Landroid/os/Bundle;)V", "android.os.Bundle");
		MonoBridge.link(MainActivity.class, "onSurfaceCreated", "(Ljavax/microedition/khronos/opengles/GL10;Ljavax/microedition/khronos/egl/EGLConfig;)V", "javax.microedition.khronos.opengles.GL10,javax.microedition.khronos.egl.EGLConfig");
		MonoBridge.link(MainActivity.class, "onSurfaceChanged", "(Ljavax/microedition/khronos/opengles/GL10;II)V", "javax.microedition.khronos.opengles.GL10,System.Int32,System.Int32");
		MonoBridge.link(MainActivity.class, "onDrawFrame", "(Ljavax/microedition/khronos/opengles/GL10;)V", "javax.microedition.khronos.opengles.GL10");

	}

	@Override
	protected native void onCreate(android.os.Bundle arg0);
	@Override
	public native void onSurfaceCreated(javax.microedition.khronos.opengles.GL10 arg0,javax.microedition.khronos.egl.EGLConfig arg1);
	@Override
	public native void onSurfaceChanged(javax.microedition.khronos.opengles.GL10 arg0,int w,int h);
	@Override
	public native void onDrawFrame(javax.microedition.khronos.opengles.GL10 arg0);


	long myGcHandle;
	public long getGCHandle() {
		return myGcHandle;
	}

	public void setGCHandle(long gcHandle) {
		myGcHandle = gcHandle;
	}
}
