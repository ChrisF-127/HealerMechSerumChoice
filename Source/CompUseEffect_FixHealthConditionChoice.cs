using RimWorld;
using Verse;

namespace HMSChoice
{
	public class CompUseEffect_FixHealthConditionChoice : CompUseEffect
	{
		public Hediff HediffToHeal = null;

		public override void DoEffect(Pawn usedBy)
		{
			if (HediffToHeal == null)
				return;

			base.DoEffect(usedBy);
			TaggedString taggedString = HediffToHeal is Hediff_MissingPart ? HealthUtility.Cure(HediffToHeal.Part, usedBy) : HealthUtility.Cure(HediffToHeal);
			if (PawnUtility.ShouldSendNotificationAbout(usedBy))
				Messages.Message(taggedString, usedBy, MessageTypeDefOf.PositiveEvent);
		}
	}
}
