/** 
 * CampingSuppliesMod.cs
 * Dylan Bailey
 * 11/14/16
 * License - Do whatever you want with this code as long as no puppies are set aflame
*/
using Patchwork;
using SDK;

namespace TyrannyMods.pw
{
	[ModifiesType]
	public class mod_CampingSupplies : CampingSupplies
	{

		/// <summary>
		/// Example of modifying properties properly. Note the attribute goes above the get, not the declaration
		/// </summary>
		public static int mod_StackMaximum
		{
			[ModifiesMember("get_StackMaximum")]
			get
			{
				int num = 1;
				GameDifficulty difficulty = GameState.Instance.Difficulty;
				switch (difficulty)
				{
					case GameDifficulty.Easy:
					{
						num = 6;
						break;
					}
					case GameDifficulty.Normal:
					{
						num = 6;
						break;
					}
					case GameDifficulty.Hard:
					case GameDifficulty.PathOfTheDamned:
					{
						num = 4;
						break;
					}

					default:
					{
						break;
					}
				}
				return num;
			}
		}
	}
}