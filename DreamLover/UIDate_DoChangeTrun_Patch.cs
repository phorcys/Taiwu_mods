using Harmony12;

namespace DreamLover
{
	[HarmonyPatch(typeof(UIDate), "DoChangeTrun")]
	public static class UIDate_DoChangeTrun_Patch
	{
		private static void Postfix()
		{
			if (Main.enabled)
			{
				string actorDate = DateFile.instance.GetActorDate(DateFile.instance.MianActorID(), 11, applyBonus: false);
				Main.TaiwuAge = ((actorDate == null) ? (-1) : int.Parse(actorDate));
				string actorDate2 = DateFile.instance.GetActorDate(DateFile.instance.MianActorID(), 997, applyBonus: false);
				Main.TaiwuSex = ((actorDate2 == null) ? (-1) : int.Parse(actorDate2));
			}
		}
	}
}
