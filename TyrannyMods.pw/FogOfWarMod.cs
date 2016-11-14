using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Patchwork;
using SDK;
using UnityEngine;

namespace TyrannyMods.pw
{
	/// <summary>
	/// Alters fog of war by making revealed areas permanently-revealed.
	/// </summary>
	[ModifiesType]
	public class mod_FogOfWar : FogOfWar
	{
		// This keeps NPCs that have been discovered ("seen") appearing in the pseudo fog-of-war but
		// does not show them if they're in undiscovered fog. Somewhat faulty in that your mouse cursor will
		// context change even if the NPC isn't revealed at all, potentially causing tiny spoilers?
		/*[ModifiesMember("PointVisible")]
        public bool mod_PointVisible(Vector3 worldPosition)
        {
            return true;
        }*/

		///Base fog alpha for totally unexplored areas
		[ModifiesMember("FULL_FOG_ALPHA")] public const float mod_FULL_FOG_ALPHA = 1.0f;

		[ModifiesMember("LOS_ATTENUATION_DISTANCE")] public const float mod_LOS_ATTENUATION_DISTANCE = 50.0f;

		[ModifiesMember("REVEAL_ATTENUATION_DISTANCE")] public const float mod_REVEAL_ATTENUATION_DISTANCE = 50.0f;

		[ModifiesMember("MAX_LOS_DISTANCE")] public const float mod_MAX_LOS_DISTANCE = 50.0f;

		/// This is where all the magic happens.  Tried to rename as many variables as I could discern.
		/// Most of the functionality is understandable now. In particular, exploredFogAlpha is what 
		/// this mod changed.  Forcing it to 0.0f means all explored areas are fully unveiled.
		[ModifiesMember("UpdateVertices")]
		public void UpdateVerticesRevised()
		{
			//IEDebug.Log("In UpdateVertices()");
			if (this.m_revealList.Count <= 0 || this.m_fogVertices == null ||
			    (this.m_fogAlphasStaging == null || this.m_fogDesiredAlphas == null))
				return;
			for (int index = 0; index < this.m_prevUpdateList.Count; ++index)
				this.m_fogUpdateState[this.m_prevUpdateList[index]] = false;
			float mDeltaTime = this.m_deltaTime/0.5f;
			if (this.m_disableFogFading || (double) mDeltaTime > 1.0)
				mDeltaTime = 1f;
			this.m_updateList.Clear();
			this.m_underRevealerList.Clear();
			this.m_candidates.Clear();
			int num2 = 19;
			int num3 = num2*2 + 1;
			int num4 = num3*num3;
			this.m_groundPlane.normal = Vector3.up;
			this.m_groundPlane.distance = 0.0f;
			for (int index1 = 0; index1 < this.m_revealList.Count; ++index1)
			{
				FogOfWar.Revealer revealer1 = this.m_revealList[index1];
				if (!revealer1.RequiresRefresh)
				{
					if (revealer1.HasTrigger)
					{
						if ((double) revealer1.TriggerBoxSize.sqrMagnitude > 0)
						{
							float triggerPos = revealer1.TriggerPos.x - revealer1.TriggerBoxSize.x*0.5f;
							float triggerPos1 = revealer1.TriggerPos.x + revealer1.TriggerBoxSize.x*0.5f;
							float triggerPos2 = revealer1.TriggerPos.z - revealer1.TriggerBoxSize.z*0.5f;
							float triggerPos3 = revealer1.TriggerPos.z + revealer1.TriggerBoxSize.z*0.5f;
							bool flag = false;
							for (int index2 = 0; index2 < this.m_revealList.Count; ++index2)
							{
								FogOfWar.Revealer revealer2 = this.m_revealList[index2];
								if (revealer2.TriggersBoxColliders && (double) revealer2.WorldPos.x > (double) triggerPos &&
								    ((double) revealer2.WorldPos.x < (double) triggerPos1 &&
								     (double) revealer2.WorldPos.z > (double) triggerPos2) &&
								    (double) revealer2.WorldPos.z < (double) triggerPos3)
								{
									flag = true;
									break;
								}
							}
							if (!flag)
								continue;
						}
						else if (this.PointVisible(revealer1.TriggerPos))
						{
							int x = 0;
							int y = 0;
							this.WorldToFogOfWar(revealer1.TriggerPos, out x, out y);
							if (!this.m_fogLOSRevealed[x*this.m_fogVertsPerRow + y])
								continue;
						}
						else
							continue;
					}
					int x1 = 0;
					int y1 = 0;
					this.WorldToFogOfWar(revealer1.WorldPos, out x1, out y1);
					int losDistance1 = (int) (((double) revealer1.LOSDistance + 3.0)*1.0/(double) this.m_fogTileWidth) + 1;
					int losDistance2 = (int) (((double) revealer1.LOSDistance + 3.0)*1.0/(double) this.m_fogTileHeight) + 1;
					int num11 = x1 - losDistance1;
					int num12 = y1 - losDistance2;
					int mFogVertsPerColumn = x1 + losDistance1;
					int mFogVertsPerRow2 = y1 + losDistance2;
					FogOfWar.RevealerCandidate revealerCandidate = new FogOfWar.RevealerCandidate();
					revealerCandidate.Revealer = revealer1;
					revealerCandidate.UseBacksideVerts = false;
					revealerCandidate.OriginX = x1;
					revealerCandidate.OriginY = y1;
					revealerCandidate.OriginIndex = x1*this.m_fogVertsPerRow + y1;
					if (this.m_los == null ||
					    revealerCandidate.OriginIndex >= 0 && revealerCandidate.OriginIndex < this.m_los.Vertices.Length)
					{
						this.m_candidates.Add(revealerCandidate);
						Ray theRay1 = new Ray(revealer1.WorldPos, -this.CameraPlane.normal);
						revealer1.CameraPos = FogOfWar.GetCameraPlaneRayIntersectionPosition(this.CameraPlane, theRay1) +
						                      this.CameraPlane.normal*0.5f;
						Ray theRay2 = new Ray(revealer1.WorldPos - this.m_cameraForward*500f, this.m_cameraForward);
						revealerCandidate.WorldPos = this.m_levelInfo.GetGroundPlaneRayIntersectionPosition(this.m_groundPlane, theRay2);
						if (this.m_los != null && this.m_los.BacksideVertices.ContainsKey(revealerCandidate.OriginIndex))
						{
							int index2 = -1;
							for (int index3 = 0; index3 < this.m_doorLookupGuid.Length; ++index3)
							{
								if (this.m_doorLookupGuid[index3] == this.m_los.BacksideVertices[revealerCandidate.OriginIndex].DoorID)
								{
									index2 = index3;
									break;
								}
							}
							if (index2 >= 0 && this.m_doorLookupValidation[index2])
							{
								Vector2 rhs = GameUtilities.V3Subtract2D(revealer1.WorldPos, this.m_doorLookupPos[index2]);
								rhs.Normalize();
								if ((double) Vector2.Dot(this.m_doorLookupForward[index2], rhs) < 0.0)
									revealerCandidate.UseBacksideVerts = true;
							}
						}
						if (num11 < 0)
							num11 = 0;
						if (num12 < 0)
							num12 = 0;
						if (mFogVertsPerColumn >= this.m_fogVertsPerColumn)
							mFogVertsPerColumn = this.m_fogVertsPerColumn - 1;
						if (mFogVertsPerRow2 >= this.m_fogVertsPerRow)
							mFogVertsPerRow2 = this.m_fogVertsPerRow - 1;
						for (int index2 = num11; index2 <= mFogVertsPerColumn; ++index2)
						{
							for (int index3 = num12; index3 <= mFogVertsPerRow2; ++index3)
							{
								int mFogVertsPerRow3 = index2*this.m_fogVertsPerRow + index3;
								if (!this.m_fogUpdateState[mFogVertsPerRow3])
								{
									this.m_updateList.Add(mFogVertsPerRow3);
									this.m_underRevealerList.Add(mFogVertsPerRow3);
									this.m_fogUpdateState[mFogVertsPerRow3] = true;
									this.m_fogDesiredAlphas[mFogVertsPerRow3] = this.m_fogAlphasStaging[mFogVertsPerRow3].y;
								}
							}
						}
					}
				}
			}
			this.m_fadeOutList.Clear();
			for (int index1 = 0; index1 < this.m_prevUpdateList.Count; ++index1)
			{
				int index2 = this.m_prevUpdateList[index1];
				if (!this.m_fogUpdateState[index2])
				{
					this.m_updateList.Add(index2);
					this.m_fadeOutList.Add(index2);
					this.m_fogDesiredAlphas[index2] = this.m_fogAlphasStaging[index2].y;
				}
			}
			for (int index = 0; index < this.m_prevUpdateList.Count; ++index)
				this.m_fogLOSRevealed[this.m_prevUpdateList[index]] = false;
			this.m_prevUpdateList.Clear();
			this.m_prevUpdateList.AddRange((IEnumerable<int>) this.m_underRevealerList);
			for (int index1 = 0; index1 < this.m_updateList.Count; ++index1)
			{
				int index2 = this.m_updateList[index1];
				float mFogDesiredAlpha = this.m_fogDesiredAlphas[index2];
				float mFogAlphasStaging = this.m_fogAlphasStaging[index2].y;
				float single = mFogAlphasStaging;
				float single1 = mFogAlphasStaging;
				for (int index3 = 0; index3 < this.m_candidates.Count; ++index3)
				{
					FogOfWar.RevealerCandidate revealerCandidate = this.m_candidates[index3];
					float minAlpha = 0.0f;
					if (this.m_los != null && (!revealerCandidate.Revealer.HasTrigger || revealerCandidate.Revealer.RespectLOS))
					{
						FogOfWarLOS.VertexData[] vertexDataArray = this.m_los.Vertices[revealerCandidate.OriginIndex];
						if (revealerCandidate.UseBacksideVerts)
							vertexDataArray = this.m_los.BacksideVertices[revealerCandidate.OriginIndex].VertexData;
						if (vertexDataArray != null)
						{
							int mFogVertsPerRow = index2/this.m_fogVertsPerRow;
							int mFogVertsPerRow1 = index2%this.m_fogVertsPerRow;
							int originX = mFogVertsPerRow - revealerCandidate.OriginX + num2;
							int originY = mFogVertsPerRow1 - revealerCandidate.OriginY + num2;
							int index4 = originX*num3 + originY;
							if (index4 >= 0 && index4 < num4)
							{
								if (vertexDataArray[index4] == null)
								{
									minAlpha = 0.0f;
								}
								else
								{
									minAlpha = vertexDataArray[index4].MinAlpha;
									if (vertexDataArray[index4].DoorIndices != null)
									{
										for (int index5 = 0; index5 < vertexDataArray[index4].DoorIndices.Length; ++index5)
										{
											byte num13 = vertexDataArray[index4].DoorIndices[index5];
											if (this.m_doorLookupValidation[(int) num13] && this.m_doorLookup[(int) num13].CurrentState != OCL.State.Open &&
											    this.m_doorLookup[(int) num13].CurrentState != OCL.State.SealedOpen)
											{
												minAlpha = this.m_fogAlphasStaging[index2].y;
												break;
											}
										}
									}
								}
								if ((double) minAlpha > (double) this.m_fogAlphasStaging[index2].y)
									minAlpha = this.m_fogAlphasStaging[index2].y;
							}
							else
								continue;
						}
					}
					//  #ZEN(Changed) controls the alpha value of explored fog
					float exploredFogAlpha = 0.0f; //Mathf.Max(0.7f, minAlpha); 
					float lOSDistance3 = revealerCandidate.Revealer.LOSDistance + 3f;
					float lOSDistance4 = revealerCandidate.Revealer.LOSDistance - 3f;
					float single2 = GameUtilities.V3Distance2D(this.m_worldVertices[index2], revealerCandidate.WorldPos);
					if ((double) single2 > (double) lOSDistance3) // if single2 > lOSDistance3
						mFogDesiredAlpha = this.m_fogAlphasStaging[index2].y;
					else if ((double) single2 > (double) revealerCandidate.Revealer.LOSDistance)
					{
						float lOSDistance = (float) ((1.0 - ((double) lOSDistance3 - (double) single2)/3.0)*0.300000011920929 + 0.7f);
						if ((double) lOSDistance < (double) minAlpha)
							lOSDistance = minAlpha;
						if ((double) lOSDistance < (double) this.m_fogAlphasStaging[index2].y)
							mFogAlphasStaging = (double) lOSDistance <= (double) exploredFogAlpha ? exploredFogAlpha : lOSDistance;
						if ((double) lOSDistance < (double) this.m_fogDesiredAlphas[index2])
							mFogDesiredAlpha = lOSDistance;
					}
					else if ((double) single2 < (double) lOSDistance4 - 1.0)
					{
						mFogDesiredAlpha = minAlpha;
						mFogAlphasStaging = exploredFogAlpha;
					}
					else
					{
						float lOSDistance =
							(float) ((1.0 - ((double) revealerCandidate.Revealer.LOSDistance - (double) single2)/3.0)*0.699999988079071);
						if ((double) lOSDistance < (double) minAlpha)
							lOSDistance = minAlpha;
						if ((double) lOSDistance < (double) this.m_fogAlphasStaging[index2].y)
							mFogAlphasStaging = (double) lOSDistance <= (double) exploredFogAlpha ? exploredFogAlpha : lOSDistance;
						if ((double) lOSDistance < (double) this.m_fogDesiredAlphas[index2])
							mFogDesiredAlpha = lOSDistance;
					}
					if (!revealerCandidate.Revealer.HasTrigger && (double) mFogDesiredAlpha < 0.699999988079071)
						this.m_fogLOSRevealed[index2] = true;
					if (revealerCandidate.Revealer.RevealOnly && (double) mFogDesiredAlpha < 0.699999988079071)
						mFogDesiredAlpha = 0.0f;
					if ((double) mFogDesiredAlpha < (double) single)
						single = mFogDesiredAlpha;
					if ((double) mFogAlphasStaging < (double) single1)
						single1 = mFogAlphasStaging;
				}
				if ((double) single > (double) single1)
					single = single1;
				this.m_fogDesiredAlphas[index2] = single;
				this.m_fogAlphasStaging[index2].y = single1;
				if (!this.Disabled && (double) this.m_fogAlphasStaging[index2].y < 0.699999988079071)
					this.m_fogAlphasStaging[index2].y = 0.0f;
				float mFogDesiredAlphas1 = this.m_fogDesiredAlphas[index2] - this.m_fogAlphasStaging[index2].x;
				if ((double) Mathf.Abs(mFogDesiredAlphas1) <= (double) mDeltaTime)
				{
					this.m_fogAlphasStaging[index2].x = this.m_fogDesiredAlphas[index2];
				}
				else
				{
					if ((double) mFogDesiredAlphas1 > 0.0)
						this.m_fogAlphasStaging[index2].x += mDeltaTime;
					else
						this.m_fogAlphasStaging[index2].x -= mDeltaTime;
					this.m_fogUpdateState[index2] = true;
				}
			}
			for (int index1 = 0; index1 < this.m_fadeOutList.Count; ++index1)
			{
				int index2 = this.m_fadeOutList[index1];
				if ((double) this.m_fogAlphasStaging[index2].x < 0.699999988079071)
					this.m_prevUpdateList.Add(index2);
			}
		}
	}
}