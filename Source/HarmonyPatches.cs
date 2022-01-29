using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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


			// compatibility patch for More Faction Interaction's Mystical Shaman healer
			var methodInfo_MysticalShaman_Notify_CaravanArrived = Type.GetType("MoreFactionInteraction.More_Flavour.MysticalShaman, MoreFactionInteraction")?.GetMethod("Notify_CaravanArrived");
			if (methodInfo_MysticalShaman_Notify_CaravanArrived != null)
			{
				harmony.Patch(
					methodInfo_MysticalShaman_Notify_CaravanArrived,
					transpiler: new HarmonyMethod(typeof(MFI_MysticalShaman_Patcher), nameof(MFI_MysticalShaman_Patcher.Notify_CaravanArrived_Transpiler)));
			}
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


	// patcher for More Faction Interaction's Mystical Shaman healer
	internal class MFI_MysticalShaman_Patcher
	{
		internal static IEnumerable<CodeInstruction> Notify_CaravanArrived_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			bool replace = false;
			foreach (var instruction in instructions)
			{
				if (!replace)
				{
					if (instruction.opcode == OpCodes.Ldstr
						&& instruction.operand is string operand
						&& operand == "MechSerumHealer")
					{
						//Log.Warning("REMOVE " + instruction.ToString());

						instruction.opcode = OpCodes.Ldloc_0;
						instruction.operand = null;
						replace = true;
					}
				}
				else
				{
					//Log.Warning("REMOVE " + instruction.ToString());

					if (instruction.opcode == OpCodes.Callvirt
						&& instruction.operand is MethodInfo methodInfo
						&& methodInfo.Name == "DoEffect")
					{
						instruction.opcode = OpCodes.Call;
						instruction.operand = typeof(MFI_MysticalShaman_Patcher).GetMethod(nameof(MechSerumHealer_FixWorstHealthCondition), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
						replace = false;
					}
					else // skip instruction
					{
						continue;
					}
				}

				//Log.Message(instruction.ToString());
				yield return instruction;
			}
		}

		internal static void MechSerumHealer_FixWorstHealthCondition(Pawn usedBy)
		{
			TaggedString taggedString = HealthUtility.FixWorstHealthCondition(usedBy);
			if (PawnUtility.ShouldSendNotificationAbout(usedBy))
				Messages.Message(taggedString, usedBy, MessageTypeDefOf.PositiveEvent);
		}
	}
}
