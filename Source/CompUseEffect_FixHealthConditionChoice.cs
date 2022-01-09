using RimWorld;
using Verse;

namespace HMSChoice
{
	public class CompUseEffect_FixHealthConditionChoice : CompUseEffect
	{
		public override void DoEffect(Pawn usedBy)
		{
			var hediff = parent.GetComp<CompUsable_FixHealthConditionChoice>()?.SelectedHediff;
			if (hediff == null)
				return;

			base.DoEffect(usedBy);
			TaggedString taggedString = hediff is Hediff_MissingPart ? HealthUtility.Cure(hediff.Part, usedBy) : HealthUtility.Cure(hediff);
			if (PawnUtility.ShouldSendNotificationAbout(usedBy))
				Messages.Message(taggedString, usedBy, MessageTypeDefOf.PositiveEvent);
		}
	}
}
