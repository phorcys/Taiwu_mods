using GameData;
using UnityEngine;

namespace DreamLover
{
	public static class ExpandUtils
	{
		public static bool RemoveAllLove()
		{
			if ((Object)(object)DateFile.instance == (Object)null || !Characters.HasChar(DateFile.instance.MianActorID()))
			{
				return false;
			}
			int num = DateFile.instance.MianActorID();
			int[] allCharIds = Characters.GetAllCharIds();
			int[] array = allCharIds;
			foreach (int num2 in array)
			{
				if (DateFile.instance.GetActorSocial(num2, 312).Contains(num) && !DateFile.instance.GetActorSocial(num, 312).Contains(num2))
				{
					DateFile.instance.RemoveActorSocial(num2, num, 312);
				}
			}
			return true;
		}
	}
}
