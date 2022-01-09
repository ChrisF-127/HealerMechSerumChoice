using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace HMSChoice
{
	public class CompUsable_FixHealthConditionChoice : CompUsable
	{
		public void UsedBy_Override(Pawn p)
		{
			if (!CanBeUsedBy(p, out var _))
				return;

			Dialog_HediffSelection.CreateDialog(p, Execute);

			void Execute(Hediff hediff)
			{
				if (hediff == null)
					return;

				try
				{
					var comps = (from x in parent.GetComps<CompUseEffect>() orderby x.OrderPriority descending select x).ToList();
					foreach (var comp in comps)
					{
						if (comp is CompUseEffect_FixHealthConditionChoice choose)
							choose.HediffToHeal = hediff;
						comp.DoEffect(p);
					}
				}
				catch (Exception arg)
				{
					Log.Error($"Error in {nameof(CompUsable_FixHealthConditionChoice)}.{nameof(UsedBy_Override)}: " + arg);
				}
			}
		}
	}
}
