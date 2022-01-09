using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HMSChoice
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		static HarmonyPatches()
		{
			Harmony harmony = new Harmony("syrus.hmschoice");

			// you cannot imagine how annoyed I am for having to do this, just because "UsedBy" is not overrideable...
			harmony.Patch(
				typeof(CompUsable).GetMethod("UsedBy"),
				prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.CompUsable_UsedBy_Prefix)));
		}

		static bool CompUsable_UsedBy_Prefix(CompUsable __instance, Pawn p)
		{
			if (__instance is CompUsable_FixHealthConditionChoice choose)
			{
				choose.UsedBy_Override(p);
				return false;
			}
			return true;
		}
	}
}
