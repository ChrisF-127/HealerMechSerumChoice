using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HMSChoice
{
	public class Recipe_AdministerUsableItem_FixHealthConditionChoice : Recipe_AdministerUsableItem
	{
		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			Dialog_HediffSelection.CreateDialog(pawn, DoStuff);

			void DoStuff(Hediff hediff)
			{
				if (hediff != null)
				{
					var comp = ingredients[0].TryGetComp<CompUsable_FixHealthConditionChoice>();
					comp.SelectedHediff = hediff;
					comp.UsedBy(pawn);
				}
				else
					pawn.health.surgeryBills.Bills.Remove(bill);
			}
		}

		//public override void CheckForWarnings(Pawn billDoer)
		//{
		//	base.CheckForWarnings(billDoer);
		//}
	}
}
