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
				prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompUsable_UsedBy_Prefix)));

			harmony.Patch(
				typeof(CompUsable).GetMethod("CanBeUsedBy"),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompUsable_CanBeUsedBy_Transpiler)));


			// compatibility patch for More Faction Interaction's Mystical Shaman healer
			var methodInfo_MysticalShaman_Notify_CaravanArrived = Type.GetType("MoreFactionInteraction.More_Flavour.MysticalShaman, MoreFactionInteraction")?.GetMethod("Notify_CaravanArrived");
			if (methodInfo_MysticalShaman_Notify_CaravanArrived != null)
			{
				harmony.Patch(
					methodInfo_MysticalShaman_Notify_CaravanArrived,
					transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Notify_CaravanArrived_Transpiler)));
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

		internal static IEnumerable<CodeInstruction> CompUsable_CanBeUsedBy_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var list = instructions.ToList();

			var success = false;
			Label? label0 = null;
			var label1 = generator.DefineLabel();
			for (int i = 0; i < list.Count; i++)
			{
				if (label0 == null)
				{
					if (list[i].opcode == OpCodes.Callvirt && list[i].operand is MethodInfo mi0 && mi0.Name == "get_Humanlike"
						&& list[i + 1].opcode == OpCodes.Brtrue_S)
					{
						label0 = list[++i].labels?.FirstOrDefault();

						list.Insert(++i, new CodeInstruction(OpCodes.Ldtoken, typeof(CompUsable_FixHealthConditionChoice)));
						list.Insert(++i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Type), nameof(Type.GetTypeFromHandle))));
						list.Insert(++i, new CodeInstruction(OpCodes.Ldarg_0));
						list.Insert(++i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(object), nameof(GetType))));
						list.Insert(++i, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Type), nameof(Type.IsAssignableFrom))));
						list.Insert(++i, new CodeInstruction(OpCodes.Brtrue_S, label1));
					}
				}
				else
				{
					if (list[i].labels.Contains((Label)label0))
					{
						list[i].labels.Add(label1);
						success = true;
						break;
					}
				}
			}
			if (!success)
				Log.Error($"{nameof(HMSChoice)}: applying CanBeUsedBy patch failed");

			return list;
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
