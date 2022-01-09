using RimWorld;
using Verse;

namespace HMSChoice
{
	public class CompUseEffect_DestroySelf_FixHealthConditionChoice : CompUseEffect
	{
		public override float OrderPriority => -1000f;

		public override void DoEffect(Pawn usedBy)
		{
			var hediff = parent.GetComp<CompUsable_FixHealthConditionChoice>()?.SelectedHediff;
			if (hediff == null)
				return;

			base.DoEffect(usedBy);
			parent.SplitOff(1).Destroy();
		}
	}
}
