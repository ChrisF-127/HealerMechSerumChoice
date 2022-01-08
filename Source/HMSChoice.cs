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
		public Hediff SelectedHediff;

		public override void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget)
		{
			if (!pawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly) || !CanBeUsedBy(pawn, out var _))
				return;

			SelectedHediff = null;

			var hediffs = pawn?.health?.hediffSet?.hediffs?.FindAll((Hediff x) => x.def.isBad && x.def.everCurableByItem && x.Visible);
			if (hediffs?.Count > 0)
				Find.WindowStack.Add(new Dialog_HediffSelection(hediffs, this, StartJob));
			else
			{
				Messages.Message("SY_HMSC.NoHediffsToHeal".Translate(), MessageTypeDefOf.RejectInput, false);
				return;
			}

			void StartJob()
			{
				Job job = extraTarget.IsValid ? JobMaker.MakeJob(Props.useJob, parent, extraTarget) : JobMaker.MakeJob(Props.useJob, parent);
				pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		}
	}

	public class CompUseEffect_FixHealthConditionChoice : CompUseEffect
	{
		public override void DoEffect(Pawn usedBy)
		{
			var choice = parent.GetComp<CompUsable_FixHealthConditionChoice>();
			if (choice?.SelectedHediff == null)
				return;

			base.DoEffect(usedBy);
			TaggedString taggedString = HealthUtility.Cure(choice.SelectedHediff);
			if (PawnUtility.ShouldSendNotificationAbout(usedBy))
				Messages.Message(taggedString, usedBy, MessageTypeDefOf.PositiveEvent);
		}
	}

	public class CompUseEffect_DestroySelf_FixHealthConditionChoice : CompUseEffect
	{
		public override float OrderPriority => -1000f;

		public override void DoEffect(Pawn usedBy)
		{
			var choice = parent.GetComp<CompUsable_FixHealthConditionChoice>();
			if (choice?.SelectedHediff == null)
				return;

			base.DoEffect(usedBy);
			parent.SplitOff(1).Destroy();
		}
	}

	public class Dialog_HediffSelection : Window
	{
		private readonly List<Hediff> Hediffs;
		private Hediff SelectedHediff;

		private readonly Action Action;
		private readonly CompUsable_FixHealthConditionChoice Choice;

		private Vector2 scrollPosition = Vector2.zero;
		public override Vector2 InitialSize => new Vector2(400f, 200f);

		public Dialog_HediffSelection(List<Hediff> hediffs, CompUsable_FixHealthConditionChoice choice, Action action)
		{
			if (!(hediffs?.Count > 0))
				Log.Error($"{nameof(Dialog_HediffSelection)} created with empty Hediff list! (null: {hediffs == null})");
			Hediffs = hediffs ?? new List<Hediff>();
			SelectedHediff = null;

			Choice = choice;
			Action = action;

			forcePause = true;
			absorbInputAroundWindow = true;
			onlyOneOfTypeAllowed = false;
		}

		public override void DoWindowContents(Rect inRect)
		{
			var oriFont = Text.Font;
			var dialogTitle = "SY_HMSC.DialogTitle".Translate();
			var dialogText = "SY_HMSC.ChooseHediffToHeal".Translate();

			float y = inRect.y;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, y, inRect.width, 42f), dialogTitle);
			y += 42f;

			Text.Font = GameFont.Small;
			Rect outRect = new Rect(inRect.x, y, inRect.width, inRect.height - 35f - 5f - y);
			float width = outRect.width - 16f;
			Rect viewRect = new Rect(0f, 0f, width, Text.CalcHeight(dialogText, width) + CalcOptionsHeight(width));
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			Widgets.Label(new Rect(0f, 0f, viewRect.width, viewRect.height - CalcOptionsHeight(width)), dialogText);

			for (int i = 0; i < Hediffs.Count; i++)
			{
				var hediff = Hediffs[i];
				var hediffLabel = hediff.Label.CapitalizeFirst();
				if (!string.IsNullOrEmpty(hediffLabel))
				{
					Rect rect = new Rect(24f, viewRect.height - CalcOptionsHeight(width) + (Text.CalcHeight(hediffLabel, width) + 12f) * i + 8f, viewRect.width - 20f, Text.CalcHeight(hediffLabel, width));
					if (Mouse.IsOver(rect))
						Widgets.DrawHighlight(rect);

					if (Widgets.RadioButtonLabeled(rect, hediffLabel, SelectedHediff == hediff))
						SelectedHediff = hediff;
				}
			}
			Widgets.EndScrollView();

			if (Widgets.ButtonText(new Rect(0f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "CancelButton".Translate(), doMouseoverSound: false))
				Close();

			if (Hediffs.Count > 0)
			{
				if (SelectedHediff == null 
					|| !Widgets.ButtonText(new Rect(inRect.width / 2f + 20f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "Confirm".Translate(), doMouseoverSound: false))
					return;

				Choice.SelectedHediff = SelectedHediff;
				Close();
				Action.Invoke();
			}

			Text.Font = oriFont;
		}

		private float CalcOptionsHeight(float width)
		{
			float num = 0f;
			foreach (var hediff in Hediffs)
				num += Text.CalcHeight(hediff.Label, width);
			return num;
		}
	}
}
