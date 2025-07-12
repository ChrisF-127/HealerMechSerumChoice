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

			// compatibility patch for More Faction Interaction's Mystical Shaman healer
			var methodInfo_MysticalShaman_Notify_CaravanArrived = Type.GetType("MoreFactionInteraction.More_Flavour.MysticalShaman, MoreFactionInteraction")?.GetMethod("Notify_CaravanArrived");
			if (methodInfo_MysticalShaman_Notify_CaravanArrived != null)
			{
				harmony.Patch(
					methodInfo_MysticalShaman_Notify_CaravanArrived,
					transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Notify_CaravanArrived_Transpiler)));
			}
		}

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
						instruction.operand = AccessTools.Method(typeof(HarmonyPatches), nameof(MechSerumHealer_FixWorstHealthCondition));
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
