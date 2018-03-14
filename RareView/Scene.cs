/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/6/2018
 * Time: 3:42 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;


namespace RareView
{
	/// <summary>
	/// Description of Scene.
	/// </summary>
	public static class Scene
	{
		public enum KeyStatus{Up, Pressed, Repeated};
		
		public static GLControl control = null;
		public static ArcBallCamera Camera = new ArcBallCamera();
		public static PlaneCamera TextureCamera = new PlaneCamera();
		
		public const int VIEW_MODE_3D = 0;
		public const int VIEW_MODE_UV = 1;
		
		public static int ViewMode = VIEW_MODE_3D;
		
		public static int ViewWidth;
		public static int ViewHeight;
		
		public static Matrix4 ProjectionMatrix = Matrix4.Identity;
		
		//Input
		public static float LastMouseX = 0.0f;
		public static float LastMouseY = 0.0f;
		public static float MouseSensitivity = 0.2f;
		public static float MouseWheelSensitivity = 0.00025f;
		public static float MovementSpeed = 0.005f;
		public static float DeltaTime = 1.0f;
		
		//Keys to handle
		private static KeyStatus MoveForward = KeyStatus.Up;
		private static KeyStatus MoveBackward = KeyStatus.Up;
		private static KeyStatus MoveLeft = KeyStatus.Up;
		private static KeyStatus MoveRight = KeyStatus.Up;
		private static KeyStatus MoveUp = KeyStatus.Up;
		private static KeyStatus MoveDown = KeyStatus.Up;
		private static KeyStatus ZoomIn = KeyStatus.Up;
		private static KeyStatus ZoomOut = KeyStatus.Up;
		private static KeyStatus ShiftModifier = KeyStatus.Up;
		private static KeyStatus AltModifier = KeyStatus.Up;
		private static KeyStatus ControlModifier = KeyStatus.Up;
		private static KeyStatus ResetCamera = KeyStatus.Up;
		
		private static Keys MoveForward_Key = Keys.W;
		private static Keys MoveBackward_Key = Keys.S;
		private static Keys MoveLeft_Key = Keys.A;
		private static Keys MoveRight_Key = Keys.D;
		private static Keys MoveUp_Key = Keys.X;
		private static Keys MoveDown_Key = Keys.Z;
		private static Keys ZoomIn_Key = Keys.Oemplus;
		private static Keys ZoomOut_Key = Keys.OemMinus;
		private static Keys ResetCamera_Key = Keys.R;
		
		private static List<CaffFile> CaffFiles = new List<CaffFile>();
		
		private static Stopwatch Timer = new Stopwatch();
		
		public static TreeNodeTag SelectedItemTag = null;
		public static bool RenderVertexPointOverlay = false;
		public static bool RenderWireframeOverlay = false;
		public static bool BackfaceCulling = false;
		
		public static int LoadProgramFromStrings(string vsSource, string fsSource, string gsSource = "")
		{
			bool result = true;
			int vShaderID = GL.CreateShader(ShaderType.VertexShader);
			int fShaderID = GL.CreateShader(ShaderType.FragmentShader);
			int gShaderID = 0;
			if(gsSource != string.Empty)
			{
				gShaderID = GL.CreateShader(ShaderType.GeometryShader);
			}
			
			if(!LoadShaderFromString(vsSource, vShaderID))
			{
				result = false;
			}
			
			if(!LoadShaderFromString(fsSource, fShaderID))
			{
				result = false;
			}
			
			if(gsSource != string.Empty)
			{
				if(!LoadShaderFromString(gsSource, gShaderID))
				{
					result = false;
				}
			}
			
			int programID = GL.CreateProgram();
			if(result)
			{
				GL.AttachShader(programID, vShaderID);
				GL.AttachShader(programID, fShaderID);
				if(gsSource != string.Empty) GL.AttachShader(programID, gShaderID);
				
				GL.LinkProgram(programID);
				int linkResult = 0;
				GL.GetProgram(programID, GetProgramParameterName.LinkStatus, out linkResult);
				if(linkResult == 0)
				{
					Logger.LogError("Error linking program:");
					Logger.LogError(GL.GetProgramInfoLog(programID));
					result = false;
				}
			}
			
			GL.DeleteShader(vShaderID);
			GL.DeleteShader(fShaderID);
			if(gsSource != string.Empty)
			{
				GL.DeleteShader(gShaderID);
			}
			if(result) return programID;
			
			GL.DeleteProgram(programID);
			return -1;
		}
		
		public static bool LoadShaderFromString(string shaderSource, int shaderID)
		{
			GL.ShaderSource(shaderID, shaderSource);
			GL.CompileShader(shaderID);
			int compileResult = 0;
			GL.GetShader(shaderID, ShaderParameter.CompileStatus, out compileResult);
			if(compileResult == 0)
			{
				Logger.LogError("Error compiling shader:");
				Logger.LogError(GL.GetShaderInfoLog(shaderID));
				return false;
			}
			
			return true;
		}
		
		
		public static int LoadProgram(string programName, bool hasGeometryShader = false)
		{
			bool result = true;
			
			int vShaderID = GL.CreateShader(ShaderType.VertexShader);
			int fShaderID = GL.CreateShader(ShaderType.FragmentShader);
			int gShaderID = 0;
			if(hasGeometryShader)
			{
				gShaderID = GL.CreateShader(ShaderType.GeometryShader);
			}
			
			if(!LoadShader("data/shaders/" + programName + ".vsh", vShaderID))
			{
				result = false;
			}
			
			if(!LoadShader("data/shaders/" + programName + ".fsh", fShaderID))
			{
				result = false;
			}
			
			if(hasGeometryShader)
			{
				if(!LoadShader("data/shaders/" + programName + ".gsh", gShaderID))
				{
					result = false;
				}
			}
			
			int programID = GL.CreateProgram();
			if(result)
			{
				GL.AttachShader(programID, vShaderID);
				GL.AttachShader(programID, fShaderID);
				if(hasGeometryShader) GL.AttachShader(programID, gShaderID);
				
				GL.LinkProgram(programID);
				int programLinkResult = 0;
				GL.GetProgram(programID, GetProgramParameterName.LinkStatus, out programLinkResult);
				if(programLinkResult == 0)
				{
					Logger.LogError(string.Format("Error linking program {0}:\n{1}\n", programName, GL.GetProgramInfoLog(programID)));
					result = false;
				}
				
			}
			
			GL.DeleteShader(vShaderID);
			GL.DeleteShader(fShaderID);
			if(hasGeometryShader) GL.DeleteShader(gShaderID);
			if(result)
			{
				return programID;
			}
			
			GL.DeleteProgram(programID);
			return -1;
		}
		
		private static bool LoadShader(string fileName, int shaderID)
		{
			using(var sr = new StreamReader(fileName))
			{
				string shaderCode = sr.ReadToEnd();
				GL.ShaderSource(shaderID, shaderCode);
				GL.CompileShader(shaderID);
				int compileResult = 0;
				GL.GetShader(shaderID, ShaderParameter.CompileStatus, out compileResult);
				if(compileResult == 0)
				{
					Logger.LogError(string.Format("Error compiling shader {0}:\n{1}\n", fileName, GL.GetShaderInfoLog(shaderID)));
					return false;
				}
				return true;
			}
		}
		
		public static void MakeCurrent()
		{
			if(control != null) control.MakeCurrent();
		}
		
		public static void Init(GLControl glCtrl)
		{
			control = glCtrl;
			control.MakeCurrent();
			
			
			
			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.Enable(EnableCap.Blend);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Lequal);
			GL.Enable(EnableCap.AlphaTest);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			
			GL.Enable(EnableCap.Texture2D);
			//GL.Disable(EnableCap.CullFace);
			
			GL.PointSize(1.0f);
			
			FloorGrid.Init();
			TexturePreview.Init();
			DefaultTexture.Init();
			//TextureCamera.Size = new Vector2(DefaultTexture.Width * 1.0f, DefaultTexture.Height * 1.0f);
		}
		
		public static void DeInit()
		{
			FloorGrid.DeInit();
			TexturePreview.DeInit();
			DefaultTexture.DeInit();
		}
		
		public static void Render()
		{
			control.MakeCurrent();
			
			if(BackfaceCulling)
			{
				GL.Enable(EnableCap.CullFace);
			}
			else
			{
				GL.Disable(EnableCap.CullFace);
			}
			
			Timer.Stop();
			DeltaTime = (float)Timer.Elapsed.Milliseconds;
			Timer.Reset();
			Timer.Start();
			
			
			bool isTextureCamera = false;			
			if(ViewMode == VIEW_MODE_UV) isTextureCamera = true;
			else if(ViewMode == VIEW_MODE_3D)
			{
				if(SelectedItemTag != null)
				{
					if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_TEXTURE)
					{
						isTextureCamera = true;
					}
				}
			}
			
			/*
			if(ViewMode == VIEW_MODE_3D)
			{
				Vector3 CamTarget = Camera.Target;
				float velocity = MovementSpeed;
				if(ShiftModifier != KeyStatus.Up) velocity *= 0.25f;
				velocity *= DeltaTime;
				
				if(MoveForward != KeyStatus.Up)
				{
					CamTarget.Z -= velocity;
				}
				if(MoveBackward != KeyStatus.Up)
				{
					CamTarget.Z += velocity;
				}
				
				if(MoveLeft != KeyStatus.Up)
				{
					CamTarget.X -= velocity;
				}
				if(MoveRight != KeyStatus.Up)
				{
					CamTarget.X += velocity;
				}
				
				if(MoveUp != KeyStatus.Up)
				{
					CamTarget.Y -= velocity;
				}
				
				if(MoveDown != KeyStatus.Up)
				{
					CamTarget.Y += velocity;
				}
				Camera.Target = CamTarget;
				
				float keyZoom = 0.0075f;
				float distToZoom = DeltaTime * keyZoom;
				if(ShiftModifier != KeyStatus.Up) distToZoom *= 0.25f;
				
				if(ZoomIn != KeyStatus.Up)
				{
					Camera.Distance -= distToZoom;
				}
				
				if(ZoomOut != KeyStatus.Up)
				{
					Camera.Distance += distToZoom;
				}
				
				if(ResetCamera_Key != KeyStatus.Up)
				{
					Camera.Position = Vector3.Zero;
				}
			}
			else if(ViewMode == VIEW_MODE_UV)
			{
				float velocity = MovementSpeed * 0.25f;
				if(ShiftModifier != KeyStatus.Up) velocity *= 0.25f;
				velocity *= DeltaTime;
				
				if(MoveForward != KeyStatus.Up)
				{
					TextureCamera.Position.Y -= velocity;
				}
				if(MoveBackward != KeyStatus.Up)
				{
					TextureCamera.Position.Y += velocity;
				}
				
				if(MoveLeft != KeyStatus.Up)
				{
					TextureCamera.Position.X -= velocity;
				}
				if(MoveRight != KeyStatus.Up)
				{
					TextureCamera.Position.X += velocity;
				}
				
				
				float keyZoom = 0.0000025f;
				float distToZoom = DeltaTime * keyZoom;
				
				if(ShiftModifier != KeyStatus.Up) distToZoom *= 0.25f;
				if(ZoomIn != KeyStatus.Up)
				{
					TextureCamera.Zoom -= distToZoom;
				}
				
				if(ZoomOut != KeyStatus.Up)
				{
					TextureCamera.Zoom += distToZoom;
				}
				
				if(ResetCamera != KeyStatus.Up)
				{
					TextureCamera.Position = Vector3.Zero;
				}
			}
			*/
			
			if(isTextureCamera)
			{
				float velocity = MovementSpeed * 0.25f;
				if(ShiftModifier != KeyStatus.Up) velocity *= 0.25f;
				velocity *= DeltaTime;
				
				if(MoveForward != KeyStatus.Up)
				{
					TextureCamera.Position.Y -= velocity;
				}
				if(MoveBackward != KeyStatus.Up)
				{
					TextureCamera.Position.Y += velocity;
				}
				
				if(MoveLeft != KeyStatus.Up)
				{
					TextureCamera.Position.X -= velocity;
				}
				if(MoveRight != KeyStatus.Up)
				{
					TextureCamera.Position.X += velocity;
				}
				
				float keyZoom = 0.0000025f;
				float distToZoom = DeltaTime * keyZoom;
				
				if(ShiftModifier != KeyStatus.Up) distToZoom *= 0.25f;
				if(ZoomIn != KeyStatus.Up)
				{
					TextureCamera.Zoom -= distToZoom;
				}
				
				if(ZoomOut != KeyStatus.Up)
				{
					TextureCamera.Zoom += distToZoom;
				}
				
				
				if(ResetCamera != KeyStatus.Up)
				{
					TextureCamera.Position = Vector3.Zero;
				}
			}
			else
			{
				//Vector3 CamTarget = Camera.Target;
				float velocity = MovementSpeed;
				if(ShiftModifier != KeyStatus.Up) velocity *= 0.25f;
				velocity *= DeltaTime;
				
				/*
				if(MoveForward != KeyStatus.Up)
				{
					CamTarget.Z -= velocity;
				}
				if(MoveBackward != KeyStatus.Up)
				{
					CamTarget.Z += velocity;
				}
				
				if(MoveLeft != KeyStatus.Up)
				{
					CamTarget.X -= velocity;
				}
				if(MoveRight != KeyStatus.Up)
				{
					CamTarget.X += velocity;
				}
				
				if(MoveUp != KeyStatus.Up)
				{
					CamTarget.Y -= velocity;
				}
				if(MoveDown != KeyStatus.Up)
				{
					CamTarget.Y += velocity;
				}
				*/
				
				var camMovement = new Vector3(0.0f, 0.0f, 0.0f);
				
				if(MoveForward != KeyStatus.Up)
				{
					camMovement.Z += velocity;
				}
				if(MoveBackward != KeyStatus.Up)
				{
					camMovement.Z -= velocity;
				}
				
				if(MoveLeft != KeyStatus.Up)
				{
					camMovement.X -= velocity;
				}
				
				if(MoveRight != KeyStatus.Up)
				{
					camMovement.X += velocity;
				}
				
				if(MoveUp != KeyStatus.Up)
				{
					camMovement.Y -= velocity;
				}
				
				if(MoveDown != KeyStatus.Up)
				{
					camMovement.Y += velocity;
				}
				
				
				if(ControlModifier != KeyStatus.Up)
				{
					Camera.MoveAxial(camMovement);
				}
				else
				{
					Camera.MoveDirectional(camMovement);	
				}
				
				
				if(ResetCamera != KeyStatus.Up)
				{
					Camera.Target = Vector3.Zero;
				}
				
				//Camera.Target = CamTarget;
				
				float distToZoom = 0.0075f;
				if(ShiftModifier != KeyStatus.Up) distToZoom *= 0.25f;
				distToZoom *= DeltaTime;
				
				if(ZoomIn != KeyStatus.Up)
				{
					Camera.Distance -= distToZoom;
				}
				if(ZoomOut != KeyStatus.Up)
				{
					Camera.Distance += distToZoom;
				}
				
				
				
			}
			
			
			GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
			
			SetViewport();
			
			Matrix4 CamMtx = Matrix4.Identity;
			if(ViewMode == VIEW_MODE_3D) CamMtx = Camera.GetMatrix();
			else if(ViewMode == VIEW_MODE_UV) CamMtx = TextureCamera.GetMatrix(new Vector2(control.Width * 1.0f, control.Height * 1.0f));
			Matrix4 ProjViewMtx = CamMtx * ProjectionMatrix;
			
			//TODO: Rendering code here.
			
			bool noRenderGrid = false;
			
			if(ViewMode == VIEW_MODE_3D)
			{
				if(SelectedItemTag != null)
				{
					if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_TEXTURE)
					{
						Texture tmpTexture = SelectedItemTag.File.GetTexture(SelectedItemTag.ObjectID);
						if(tmpTexture != null)
						{
							CamMtx = TextureCamera.GetMatrix(new Vector2(control.Width * 1.0f, control.Height * 1.0f));
							float texW = 1.0f;
							float texH = 1.0f;
							
							Texture.GetTextureAspect(tmpTexture.Width, tmpTexture.Height, out texW, out texH);
							
							TexturePreview.Render(CamMtx, tmpTexture.GLID, texW, texH);
							noRenderGrid = true;
						}
					}
					else if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_RENDERGRAPH)
					{
						var tmpRenderGraphItem = SelectedItemTag.File.GetRenderGraph(SelectedItemTag.ObjectID);
						if(tmpRenderGraphItem != null)
						{
							tmpRenderGraphItem.Render(ProjViewMtx, -1);
							//else tmpRenderGraphItem.Render(ProjViewMtx, SelectedItemTag.SubObjectID);
						}
					}
					else if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_RENDERGRAPH_TRIANGLE_STRIP)
					{
						var tmpRenderGraph = SelectedItemTag.File.GetRenderGraph(SelectedItemTag.ObjectID);
						if(tmpRenderGraph != null)
						{
							tmpRenderGraph.Render(ProjViewMtx, SelectedItemTag.SubObjectID);
						}
					}
					else if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_RENDERGRAPH_VERTEX_BATCH)
					{
						var tmpRenderGraph = SelectedItemTag.File.GetRenderGraph(SelectedItemTag.ObjectID);
						if(tmpRenderGraph != null)
						{
							tmpRenderGraph.RenderVertexBatch(ProjViewMtx, SelectedItemTag.SubObjectID);
						}
					}
					else if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_RENDERGRAPH_MATERIAL)
					{
						var tmpRenderGraph = SelectedItemTag.File.GetRenderGraph(SelectedItemTag.ObjectID);
						if(tmpRenderGraph != null)
						{
							tmpRenderGraph.RenderMaterial(ProjViewMtx, SelectedItemTag.SubObjectID);
						}
					}
				}
				
				
			
				if(!noRenderGrid) FloorGrid.Render(Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f) * ProjViewMtx);
					
			}
			else if(ViewMode == VIEW_MODE_UV)
			{
				
				float texW = 1.0f;
				float texH = 1.0f;
				int textureID = DefaultTexture.GLID;
				
				if(SelectedItemTag != null)
				{
					if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_TEXTURE)
					{
						Texture tmpTexture = SelectedItemTag.File.GetTexture(SelectedItemTag.ObjectID);
						if(tmpTexture != null)
						{
							textureID = tmpTexture.GLID;
							Texture.GetTextureAspect(tmpTexture.Width, tmpTexture.Height, out texW, out texH);
						}
					}
					else if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_RENDERGRAPH_TRIANGLE_STRIP)
					{
						//TexturePreview.Render(CamMtx, DefaultTexture.GLID, 1.0f, 1.0f);
						var tmpRG = SelectedItemTag.File.GetRenderGraph(SelectedItemTag.ObjectID);
						if(tmpRG != null)
						{
							var tmpTB = tmpRG.GetTriangleBatch(SelectedItemTag.SubObjectID);
							if(tmpTB != null)
							{
							   var tmpMat = tmpRG.GetMaterial(tmpTB.MaterialID);
							   var tmpTexture = SelectedItemTag.File.GetTexture(tmpMat.textureID);
							   textureID = tmpTexture.GLID;
							}
							
							tmpRG.RenderTriangleStripUVs(CamMtx, SelectedItemTag.SubObjectID);
						}
					}
					else if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_RENDERGRAPH_VERTEX_BATCH)
					{
						//TexturePreview.Render(CamMtx, DefaultTexture.GLID, 1.0f, 1.0f);
						var tmpRG = SelectedItemTag.File.GetRenderGraph(SelectedItemTag.ObjectID);
						if(tmpRG != null)
						{
							tmpRG.RendeVertexBatchUVs(CamMtx, SelectedItemTag.SubObjectID);
						}
					}
					else if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_RENDERGRAPH_MATERIAL)
					{
						var tmpRG = SelectedItemTag.File.GetRenderGraph(SelectedItemTag.ObjectID);
						if(tmpRG != null)
						{
						   var tmpMat = tmpRG.GetMaterial(SelectedItemTag.SubObjectID);
						   var tmpTexture = SelectedItemTag.File.GetTexture(tmpMat.textureID);
						   textureID = tmpTexture.GLID;
						   
						   tmpRG.RenderMaterialUVs(CamMtx, SelectedItemTag.SubObjectID);
						}
					}
				}
				
				TexturePreview.Render(CamMtx, textureID, texW, texH);
			}
			
			
			control.SwapBuffers();
		}
		
		private static void SetViewport()
		{
			int w = control.Width;
			int h = control.Height;
			GL.Viewport(0, 0, w, h);
			ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI * (45.0f / 180.0f)), w / (float)h, 0.01f, 10000.0f);
		}
		
		public static void MouseMove(MouseEventArgs e)
		{
			//Debug.WriteLine("Mouse Move!");
			float mX = (float)e.X;
			float mY = (float)e.Y;
			
			float dX = mX - LastMouseX;
			float dY = mY - LastMouseY;
			
			Vector3 camRot = Camera.Rotation;
			Vector3 camTarget = Camera.Target;
			
			if((e.Button & MouseButtons.Right) != 0)
			{
				float dXm = MouseSensitivity * dX;
				float dYm = MouseSensitivity * dY;
				
				Camera.Rotation = new Vector3(camRot.X + dXm, camRot.Y + dYm, 0.0f);
			}
			
			LastMouseX = mX;
			LastMouseY = mY;
		}
		
		public static void MouseWheel(MouseEventArgs e)
		{			
			
			bool isTextureCamera = false;			
			if(ViewMode == VIEW_MODE_UV) isTextureCamera = true;
			else if(ViewMode == VIEW_MODE_3D)
			{
				if(SelectedItemTag != null)
				{
					if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_TEXTURE)
					{
						isTextureCamera = true;
					}
				}
			}
			
			if(isTextureCamera)
			{
				float distToZoom = ((e.Delta * (MouseWheelSensitivity * 0.0005f)) * DeltaTime);
				if(ShiftModifier != KeyStatus.Up) distToZoom *= 0.25f;
				TextureCamera.Zoom += distToZoom;
			}
			else
			{
				float distToZoom = ((e.Delta * MouseWheelSensitivity) * DeltaTime);
				if(ShiftModifier != KeyStatus.Up) distToZoom *= 0.25f;
				Camera.Distance += distToZoom;
			}
		}
		
		public static void KeyPress(KeyEventArgs e)
		{
			//Debug.WriteLine("Key Pressed!");
			if(e.KeyCode == MoveForward_Key) MoveForward = (MoveForward == KeyStatus.Pressed) ? KeyStatus.Repeated : KeyStatus.Pressed;
			else if(e.KeyCode == MoveBackward_Key) MoveBackward = (MoveBackward == KeyStatus.Pressed) ? KeyStatus.Repeated : KeyStatus.Pressed;
			else if(e.KeyCode == MoveLeft_Key) MoveLeft = (MoveLeft == KeyStatus.Pressed) ? KeyStatus.Repeated : KeyStatus.Pressed;
			else if(e.KeyCode == MoveRight_Key) MoveRight = (MoveRight == KeyStatus.Pressed) ? KeyStatus.Repeated : KeyStatus.Pressed;
			else if(e.KeyCode == MoveUp_Key) MoveUp = (MoveUp == KeyStatus.Pressed) ? KeyStatus.Repeated : KeyStatus.Pressed;
			else if(e.KeyCode == MoveDown_Key) MoveDown = (MoveDown == KeyStatus.Pressed) ? KeyStatus.Repeated : KeyStatus.Pressed;
			else if(e.KeyCode == ZoomIn_Key) ZoomIn = (ZoomIn == KeyStatus.Pressed) ? KeyStatus.Repeated : KeyStatus.Pressed;
			else if(e.KeyCode == ZoomOut_Key) ZoomOut = (ZoomOut == KeyStatus.Pressed) ? KeyStatus.Repeated : KeyStatus.Pressed;
			else if(e.KeyCode == ResetCamera_Key) ResetCamera = (ResetCamera == KeyStatus.Pressed) ? KeyStatus.Repeated : KeyStatus.Pressed;
			
			if(e.Shift) ShiftModifier = (ShiftModifier == KeyStatus.Pressed) ? KeyStatus.Repeated : KeyStatus.Pressed;
			if(e.Alt) AltModifier = (AltModifier == KeyStatus.Pressed) ? KeyStatus.Repeated : KeyStatus.Pressed;
			if(e.Control) ControlModifier = (ControlModifier == KeyStatus.Pressed) ? KeyStatus.Repeated : KeyStatus.Pressed;
		}
		
		public static void KeyRelease(KeyEventArgs e)
		{
			//Debug.WriteLine("Key Released!");
			if(e.KeyCode == MoveForward_Key) MoveForward = KeyStatus.Up;
			else if(e.KeyCode == MoveBackward_Key) MoveBackward = KeyStatus.Up;
			else if(e.KeyCode == MoveLeft_Key) MoveLeft = KeyStatus.Up;
			else if(e.KeyCode == MoveRight_Key) MoveRight = KeyStatus.Up;
			else if(e.KeyCode == MoveUp_Key) MoveUp = KeyStatus.Up;
			else if(e.KeyCode == MoveDown_Key) MoveDown = KeyStatus.Up;
			else if(e.KeyCode == ZoomIn_Key) ZoomIn = KeyStatus.Up;
			else if(e.KeyCode == ZoomOut_Key) ZoomOut = KeyStatus.Up;
			else if(e.KeyCode == ResetCamera_Key) ResetCamera = KeyStatus.Up;
			
			if(!e.Shift) ShiftModifier = KeyStatus.Up;
			if(!e.Alt) AltModifier = KeyStatus.Up;
			if(!e.Control) ControlModifier = KeyStatus.Up;
		}
		
		public static TreeNode LoadCaffFile(string fileName)
		{
			control.MakeCurrent();
			//var tmpCaff = new CaffFile();
			var tmpCaff = new KameoCaff();
			if(tmpCaff.Read(fileName))
			{
				if(tmpCaff.Init()) return tmpCaff.TreeViewNode;
			}
			
			return null;
		}
		
		public static void UnloadCaffFile(CaffFile file)
		{
			file.DeInit();
			file.Close();
			CaffFiles.Remove(file);
		}
		
		public static object GetSelectedObject()
		{
			if(SelectedItemTag == null) return null;
			if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_RENDERGRAPH_VERTEX_BATCH)
			{
				var tmpGraph = SelectedItemTag.File.GetRenderGraph(SelectedItemTag.ObjectID);
				if(tmpGraph != null)
				{
					return tmpGraph.GetVertexBatch(SelectedItemTag.SubObjectID);
				}
			}
			else if(SelectedItemTag.ObjectType == TreeNodeTag.OBJECT_TYPE_RENDERGRAPH_MATERIAL)
			{
				var tmpGraph = SelectedItemTag.File.GetRenderGraph(SelectedItemTag.ObjectID);
				if(tmpGraph != null)
				{
					return tmpGraph.GetMaterial(SelectedItemTag.SubObjectID);
				}
			}
			
			return null;
		}
	}
}
