/** 
 * CameraControlMod.cs
 * Dylan Bailey
 * 11/14/16
 * License - Do whatever you want with this code as long as no puppies are set aflame
*/

using System;
using Patchwork;
using SDK;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TyrannyMods.pw
{
	[ModifiesType]
	class CameraControlMod : CameraControl
	{
		///No actual changes here, used it to test basic functionality
		[ModifiesMember("DoUpdate")]
		public void DoUpdateRevised()
		{
			RaycastHit raycastHit;
			if (Application.isPlaying)
			{
				Camera camera = Camera.main;
				if (!this.m_forceReset)
				{
					this.m_testLeft = false;
					this.m_testRight = false;
					this.m_testTop = false;
					this.m_testBottom = false;
				}
				else
				{
					this.m_testLeft = true;
					this.m_testRight = true;
					this.m_testTop = true;
					this.m_testBottom = true;
					this.m_forceReset = false;
				}
				if (this.PlayerControlEnabled && GameState.ApplicationIsFocused && !CameraControl.UIWindowVisible)
				{
					if (this.PlayerScrollEnabled)
					{
						float axisRaw = Input.GetAxisRaw("Mouse ScrollWheel");
						if (axisRaw != 0f)
						{
							this.OrthoSettings.SetZoomLevelDelta(axisRaw);
							this.ResetAtEdges();
						}
					}
					if (GameInput.GetDoublePressed(KeyCode.Mouse0, true) && (!CameraControl.MouseOverUI || GameCursor.CharacterUnderCursor != null))
					{
						Vector3 worldMousePosition = GameInput.WorldMousePosition;
						if (GameCursor.CharacterUnderCursor)
						{
							worldMousePosition = GameCursor.CharacterUnderCursor.transform.position;
							this.ResetAtEdges();
						}
						if (CameraControl.Instance)
						{
							CameraControl.Instance.FocusOnPoint(worldMousePosition, 0.4f);
							this.ResetAtEdges();
						}
					}
					if (GameInput.GetControlDown(MappedControl.ZOOM_IN))
					{
						this.OrthoSettings.SetZoomLevelDelta(this.m_zoomRes);
						this.ResetAtEdges();
					}
					if (GameInput.GetControlDown(MappedControl.ZOOM_OUT))
					{
						this.OrthoSettings.SetZoomLevelDelta(-this.m_zoomRes);
						this.ResetAtEdges();
					}
					if (GameInput.GetControlDown(MappedControl.RESET_ZOOM))
					{
						this.OrthoSettings.SetZoomLevel(1f, false);
						this.ResetAtEdges();
					}
					if (GameInput.GetControlDown(MappedControl.PAN_CAMERA))
					{
						this.m_mouseDrag_lastMousePos = GameInput.MousePosition;
						float single = (float)camera.pixelWidth * 0.5f;
						float single1 = (float)camera.pixelHeight * 0.5f;
						Vector3 groundPlane = this.ProjectScreenCoordsToGroundPlane(camera, new Vector3(single + 1f, single1, camera.nearClipPlane));
						Vector3 vector3 = this.ProjectScreenCoordsToGroundPlane(camera, new Vector3(single, single1, camera.nearClipPlane));
						this.CameraPanDeltaX = vector3 - groundPlane;
						groundPlane = this.ProjectScreenCoordsToGroundPlane(camera, new Vector3(single, single1 + 1f, camera.nearClipPlane));
						vector3 = this.ProjectScreenCoordsToGroundPlane(camera, new Vector3(single, single1, camera.nearClipPlane));
						this.CameraPanDeltaY = vector3 - groundPlane;
					}
					else if (!GameInput.GetControl(MappedControl.PAN_CAMERA))
					{
						if (GameUtilities.Instance == null)
						{
							return;
						}
						bool flag = GameUtilities.Instance.CameraHelper_UseEdgeScrolling();
						Vector3 vector31 = (Screen.fullScreen || GameUtilities.Instance.UIHelper_ShouldClipCursor() ? GameInput.MousePosition : GameInput.GlobalMousePosition);
						if (GameInput.GetControl(MappedControl.PAN_CAMERA_LEFT) || flag && vector31.x < this.m_mouseScrollBuffer && vector31.x > this.m_mouseScrollBufferOuter)
						{
							this.m_atRight = false;
							if (!this.m_atLeft)
							{
								CameraControl positionOffset = this;
								positionOffset.position_offset = positionOffset.position_offset - (Camera.main.transform.right * this.CameraMoveDelta);
								this.m_testLeft = true;
							}
						}
						else if (GameInput.GetControl(MappedControl.PAN_CAMERA_RIGHT) || flag && vector31.x > (float)Screen.width - this.m_mouseScrollBuffer && vector31.x < (float)Screen.width - this.m_mouseScrollBufferOuter)
						{
							this.m_atLeft = false;
							if (!this.m_atRight)
							{
								CameraControl cameraControl = this;
								cameraControl.position_offset = cameraControl.position_offset + (Camera.main.transform.right * this.CameraMoveDelta);
								this.m_testRight = true;
							}
						}
						if (GameInput.GetControl(MappedControl.PAN_CAMERA_DOWN) || flag && vector31.y < this.m_mouseScrollBuffer && vector31.y > this.m_mouseScrollBufferOuter)
						{
							this.m_atTop = false;
							if (!this.m_atBottom)
							{
								CameraControl positionOffset1 = this;
								positionOffset1.position_offset = positionOffset1.position_offset - (Camera.main.transform.up * this.CameraMoveDelta);
								this.m_testBottom = true;
							}
						}
						else if (GameInput.GetControl(MappedControl.PAN_CAMERA_UP) || flag && vector31.y > (float)Screen.height - this.m_mouseScrollBuffer && vector31.y < (float)Screen.height - this.m_mouseScrollBufferOuter)
						{
							this.m_atBottom = false;
							if (!this.m_atTop)
							{
								CameraControl cameraControl1 = this;
								cameraControl1.position_offset = cameraControl1.position_offset + (Camera.main.transform.up * this.CameraMoveDelta);
								this.m_testTop = true;
							}
						}
					}
					else
					{
						Vector3 mousePosition = GameInput.MousePosition - this.m_mouseDrag_lastMousePos;
						this.m_mouseDrag_lastMousePos = GameInput.MousePosition;
						if (mousePosition.x < 0f)
						{
							this.m_atLeft = false;
						}
						else if (mousePosition.x > 0f)
						{
							this.m_atRight = false;
						}
						if (mousePosition.y < 0f)
						{
							this.m_atBottom = false;
						}
						else if (mousePosition.y > 0f)
						{
							this.m_atTop = false;
						}
						if (this.m_atRight && mousePosition.x < 0f)
						{
							mousePosition.x = 0f;
						}
						else if (this.m_atLeft && mousePosition.x > 0f)
						{
							mousePosition.x = 0f;
						}
						if (this.m_atTop && mousePosition.y < 0f)
						{
							mousePosition.y = 0f;
						}
						else if (this.m_atBottom && mousePosition.y > 0f)
						{
							mousePosition.y = 0f;
						}
						if (mousePosition.x < 0f)
						{
							this.m_testRight = true;
						}
						else if (mousePosition.x > 0f)
						{
							this.m_testLeft = true;
						}
						if (mousePosition.y < 0f)
						{
							this.m_testTop = true;
						}
						else if (mousePosition.y > 0f)
						{
							this.m_testBottom = true;
						}
						CameraControl cameraControl2 = this;
						Vector3 positionOffset2 = cameraControl2.position_offset;
						Vector3 vector32 = -camera.transform.right;
						Vector3 cameraPanDeltaX = this.CameraPanDeltaX;
						cameraControl2.position_offset = positionOffset2 + ((vector32 * cameraPanDeltaX.magnitude) * mousePosition.x);
						CameraControl positionOffset3 = this;
						positionOffset3.position_offset = positionOffset3.position_offset + ((-camera.transform.up * Vector3.Dot(-camera.transform.up, this.CameraPanDeltaY)) * mousePosition.y);
					}
				}
				if (this.InterpolatingToTarget)
				{
					if (GameState.s_playerCharacter)
					{
						CameraControl moveTime = this;
						moveTime.MoveTime = moveTime.MoveTime - CameraControl.GetDeltaTime();
					}
					Vector3 moveToPointDest = Vector3.zero;
					if (this.MoveTime > 0f)
					{
						float moveTime1 = this.MoveTime / this.MoveTotalTime;
						moveToPointDest.x = Mathf.SmoothStep(this.MoveToPointDest.x, this.MoveToPointSrc.x, moveTime1);
						moveToPointDest.y = Mathf.SmoothStep(this.MoveToPointDest.y, this.MoveToPointSrc.y, moveTime1);
						moveToPointDest.z = Mathf.SmoothStep(this.MoveToPointDest.z, this.MoveToPointSrc.z, moveTime1);
						this.OrthoSettings.SetZoomLevel(Mathf.SmoothStep(this.ZoomDest, this.ZoomSrc, moveTime1), false);
					}
					else
					{
						this.MoveTime = 0f;
						this.InterpolatingToTarget = false;
						moveToPointDest = this.MoveToPointDest;
						this.OrthoSettings.SetZoomLevel(this.ZoomDest, false);
					}
					this.lastPosition = moveToPointDest;
					base.transform.position = moveToPointDest;
					this.position_offset = Vector3.zero;
					this.ResetAtEdges();
				}
				Vector3 vector33 = Vector3.zero;
				if (this.m_screenShakeTimer > 0f && GameState.s_playerCharacter != null)
				{
					CameraControl mScreenShakeTimer = this;
					mScreenShakeTimer.m_screenShakeTimer = mScreenShakeTimer.m_screenShakeTimer - Time.unscaledDeltaTime;
					Vector3 mScreenShakeStrength = (Random.onUnitSphere * this.m_screenShakeStrength) * (this.m_screenShakeTimer / this.m_screenShakeTotalTime);
					vector33 = vector33 + (mScreenShakeStrength.x * -camera.transform.right);
					vector33 = vector33 + (mScreenShakeStrength.y * -camera.transform.up);
				}
				Vector3 vector34 = Vector3.zero;
				base.transform.position = (this.lastPosition + this.position_offset) + vector33;
				if (!this.m_blockoutMode)
				{
					Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, Camera.main.nearClipPlane));
					Vector3 mWorldBoundsOrigin = Camera.main.ScreenToWorldPoint(new Vector3((float)Camera.main.pixelWidth, (float)Camera.main.pixelHeight, Camera.main.nearClipPlane));
					Vector3 worldPoint1 = Camera.main.ScreenToWorldPoint(new Vector3((float)Camera.main.pixelWidth, 0f, Camera.main.nearClipPlane)) - worldPoint;
					Vector3 worldPoint2 = Camera.main.ScreenToWorldPoint(new Vector3(0f, (float)Camera.main.pixelHeight, Camera.main.nearClipPlane)) - worldPoint;
					float single2 = worldPoint1.magnitude;
					float single3 = worldPoint2.magnitude;
					worldPoint = worldPoint - this.m_worldBoundsOrigin;
					mWorldBoundsOrigin = mWorldBoundsOrigin - this.m_worldBoundsOrigin;
					Vector3 mWorldBoundsX = this.m_worldBoundsX;
					Vector3 mWorldBoundsY = this.m_worldBoundsY;
					mWorldBoundsX.Normalize();
					mWorldBoundsY.Normalize();
					float single4 = Vector3.Dot(worldPoint, mWorldBoundsX);
					float single5 = Vector3.Dot(worldPoint, mWorldBoundsY);
					float single6 = Vector3.Dot(mWorldBoundsOrigin, mWorldBoundsX);
					float single7 = Vector3.Dot(mWorldBoundsOrigin, mWorldBoundsY);
					float mWorldBoundsX1 = this.m_worldBoundsX.magnitude;
					float mWorldBoundsY1 = this.m_worldBoundsY.magnitude;
					float bufferLeft = this.BufferLeft;
					float bufferRight = this.BufferRight;
					float bufferTop = this.BufferTop;
					float bufferBottom = this.BufferBottom;
					if (single2 > mWorldBoundsX1)
					{
						float single8 = (single2 - mWorldBoundsX1) / 2f;
						bufferLeft = bufferLeft + single8;
						bufferRight = bufferRight + single8;
					}
					if (single3 > mWorldBoundsY1)
					{
						float single9 = (single3 - mWorldBoundsY1) / 2f;
						bufferTop = bufferTop + single9;
						bufferBottom = bufferBottom + single9;
					}
					if (this.m_testLeft && single4 < -bufferLeft)
					{
						vector34 = vector34 + ((-single4 - bufferLeft) * mWorldBoundsX);
						this.m_atLeft = true;
						this.m_atRight = false;
					}
					else if (this.m_testRight && single6 > mWorldBoundsX1 + bufferRight)
					{
						vector34 = vector34 - ((single6 - (mWorldBoundsX1 + bufferRight)) * mWorldBoundsX);
						this.m_atRight = true;
						this.m_atLeft = false;
					}
					if (this.m_testBottom && single5 < -bufferBottom)
					{
						vector34 = vector34 + ((-single5 - bufferBottom) * mWorldBoundsY);
						this.m_atBottom = true;
						this.m_atTop = false;
					}
					else if (this.m_testTop && single7 > mWorldBoundsY1 + bufferTop)
					{
						vector34 = vector34 - ((single7 - (mWorldBoundsY1 + bufferTop)) * mWorldBoundsY);
						this.m_atTop = true;
						this.m_atBottom = false;
					}
					base.transform.position = (this.lastPosition + this.position_offset) + vector34;
					this.lastPosition = base.transform.position;
					Transform transforms = base.transform;
					transforms.position = transforms.position + vector33;
					this.position_offset = Vector3.zero;
				}
				if (this.Audio != null)
				{
					Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
					if (!Physics.Raycast(ray, out raycastHit, Single.PositiveInfinity, 1 << (LayerMask.NameToLayer("Walkable") & 31)))
					{
						Plane plane = new Plane(Vector3.up, new Vector3(0f, this.m_lastAudioY, 0f));
						this.Audio.position = this.GetPlaneRayIntersectionPosition(plane, ray);
					}
					else
					{
						this.Audio.position = raycastHit.point;
						this.m_lastAudioY = this.Audio.position.y;
					}
				}
			}
		}

	}
}
