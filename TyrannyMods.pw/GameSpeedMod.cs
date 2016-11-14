/** 
 * GameSpeedMod.cs
 * Dylan Bailey
 * 11/14/16
 * License - Do whatever you want with this code as long as no puppies are set aflame
*/

using Patchwork;
using SDK;

namespace TyrannyMods.pw
{
	/// <summary>
	/// Modifies the game speed toggles to add a new 6x speed.
	/// </summary>
	[ModifiesType]
	public class mod_TimeController : TimeController
	{
		[ModifiesMember("ToggleFast")]
		public void ToggleFastNew()
		{
			if (this.TimeScale < 0.3f)
				this.TimeScale = 1.0f;
			else if (this.TimeScale < 1.9f)
				this.TimeScale = 2.0f;
			else if (this.TimeScale < 5.5f)
				this.TimeScale = 6.0f;
			else this.TimeScale = 2.0f;
			
			this.UpdateTimeScale();
		}

		[ModifiesMember("ToggleSlow")]
		public void ToggleSlowNew()
		{
			if (this.TimeScale > 1.0f)
				this.TimeScale = 1.0f;
			else this.TimeScale = 0.2f;
			this.UpdateTimeScale();
		}

		[ModifiesMember("Update")]
		private void UpdateNew()
		{
			if (!GameState.IsTransitionInProgress)
			{
				this.UpdateTimeScale();
			}
			if (GameUtilities.Instance.UIHelper_KeyInputAvailable)
			{
				if (GameInput.GetControlDown(MappedControl.RESTORE_SPEED, true))
				{
					this.TimeScale = this.NormalTime;
				}
				else if (GameInput.GetControlDown(MappedControl.SLOW_TOGGLE, true))
				{
					this.ToggleSlow();
				}
				else if (GameInput.GetControlDown(MappedControl.FAST_TOGGLE, true))
				{
					this.ToggleFast();
				}
			}
		}
	}
}