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
                int mainActorId = DateFile.instance.MianActorID();
                Main.TaiwuLovers = DateFile.instance.GetActorSocial(mainActorId, 312);
			}
		}
	}
}
