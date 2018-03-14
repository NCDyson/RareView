/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 2/23/2018
 * Time: 8:13 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using System.Diagnostics;

namespace RareView
{
	/// <summary>
	/// Description of PlaneCamera.
	/// </summary>
	public class PlaneCamera
	{
		public Vector3 Position = new Vector3(0.0f, 0.0f, 0.0f);
		public float Zoom = 1.0f;
		
		
		public PlaneCamera()
		{
			
		}
		
		public PlaneCamera(Vector3 _position, float _zoom)
		{
			Position = _position;
			Zoom = _zoom;
		}
		
		public float GetMinSize(Vector2 ViewportSize)
		{
			float minSize = 0.0f;
			if(ViewportSize.X <= ViewportSize.Y)
			{
				minSize = 1.01f / ViewportSize.X;
			}
			else
			{
				minSize = 1.01f / ViewportSize.Y;
			}
			
			return minSize;
		}
		
		public Matrix4 GetMatrix(Vector2 ViewportSize)
		{
			float minSize = GetMinSize(ViewportSize);
			float minMinSize = minSize * 5.0f;
			if(Zoom > minMinSize) Zoom = minMinSize;
			const float maxZoom = 0.025f;
			if(Zoom < (minSize * maxZoom)) Zoom = minSize * maxZoom;
			//Debug.WriteLine("Zoom: {0}, minSize: {1}, minSize * 10: {2}", Zoom, minSize, minSize * 10);
			
			return Matrix4.CreateOrthographic(ViewportSize.X * Zoom, ViewportSize.Y * Zoom, -1, 1) * Matrix4.CreateTranslation(Position);
		}
		
		
	}
}
