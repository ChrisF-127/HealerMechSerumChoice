using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HMSChoice
{
	public class Dialog_HediffSelection : Window
	{
		private string DialogTitle;
		private readonly List<Hediff> Hediffs;
		private Hediff SelectedHediff;

		private readonly Action<Hediff> Execute;

		private Vector2 scrollPosition = Vector2.zero;
		public override Vector2 InitialSize => new Vector2(500f, 500f);

		public static void CreateDialog(Pawn pawn, Action<Hediff> execute)
		{
			if (pawn == null)
			{
				Log.Error($"Attempted creating {nameof(Dialog_HediffSelection)} with null pawn");
				return;
			}

			var hediffs = pawn?.health?.hediffSet?.hediffs?.FindAll((Hediff hediff) => IsValidHediff(pawn, hediff));
			if (hediffs?.Count > 0)
			{
				hediffs.SortByDescending((hediff) => HealthCardUtility.GetListPriority(hediff.Part));
				Find.WindowStack.Add(new Dialog_HediffSelection("SY_HMSC.DialogTitle".Translate(pawn), hediffs, execute));
			}
			else
				Messages.Message("SY_HMSC.NoHediffsToHeal".Translate(pawn), MessageTypeDefOf.RejectInput, false);
		}

		private Dialog_HediffSelection(string title, List<Hediff> hediffs, Action<Hediff> execute)
		{
			DialogTitle = title;

			if (!(hediffs?.Count > 0))
				Log.Error($"{nameof(Dialog_HediffSelection)} created with empty Hediff list (is null: {hediffs == null})");
			Hediffs = hediffs ?? new List<Hediff>();
			SelectedHediff = null;

			Execute = execute;

			forcePause = true;
			absorbInputAroundWindow = true;
			onlyOneOfTypeAllowed = false;
		}

		public override void DoWindowContents(Rect inRect)
		{
			var oriColor = GUI.color;
			var oriFont = Text.Font;

			float y = inRect.y;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, y, inRect.width, 42f), DialogTitle);
			y += 42f;

			Text.Font = GameFont.Small;

			float width = inRect.width - 16f;
			var dialogTextHeight = Text.CalcHeight("foo", width);
			var rect = new Rect(10f, y, width - 10f, dialogTextHeight);
			var x = rect.x;
			var w = GetPartLabelWidth(rect.width);
			Widgets.Label(
				new Rect(x, rect.y, w, rect.height),
				"SY_HMSC.Part".Translate());
			x += w + 4f;
			w = GetHediffLabelWidth(rect.width);
			Widgets.Label(
				new Rect(x, rect.y, w, rect.height),
				"SY_HMSC.HealthCondition".Translate());
			x += w + 4f;
			w = GetSeverityLabelWidth(rect.width);
			Widgets.Label(
				new Rect(x, rect.y, w, rect.height),
				"SY_HMSC.Severity".Translate());
			y += dialogTextHeight;

			GUI.color = Widgets.SeparatorLineColor;
			Widgets.DrawLineHorizontal(0f, y, width);
			GUI.color = oriColor;

			Rect outRect = new Rect(inRect.x, y, inRect.width, inRect.height - 35f - 5f - y);
			Rect viewRect = new Rect(0f, 0f, width, CalcOptionsHeight(width));
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

			var totalHeight = 0f;
			y = 0;
			foreach (var hediff in Hediffs)
			{
				var hediffHeight = CalcHediffHeight(hediff, width);
				rect = new Rect(
					10f,
					y + 4f,
					viewRect.width - 10f,
					hediffHeight);

				if (Mouse.IsOver(rect))
					Widgets.DrawHighlight(rect);

				if (RadioButtonHediff(rect, hediff, SelectedHediff == hediff))
					SelectedHediff = hediff;

				y += hediffHeight + 8f;
				totalHeight += hediffHeight + 8f;
			}
			Widgets.EndScrollView();

			if (Widgets.ButtonText(new Rect(0f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "CancelButton".Translate(), doMouseoverSound: false))
				Close(null);

			if (SelectedHediff != null 
				&& Widgets.ButtonText(new Rect(inRect.width / 2f + 20f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "Confirm".Translate(), doMouseoverSound: false))
				Close(SelectedHediff);

			Text.Font = oriFont;
		}

		public void Close(Hediff hediff)
		{
			Close();
			Execute.Invoke(hediff);
		}

		private bool RadioButtonHediff(Rect rect, Hediff hediff, bool chosen)
		{
			var oriColor = GUI.color;
			var pawn = hediff.pawn;

			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleLeft;
			var x = rect.x;

			// Part
			if (hediff.Part == null)
				GUI.color = Color.red;
			else
				GUI.color = HealthUtility.GetPartConditionLabel(pawn, hediff.Part).second;
			var width = GetPartLabelWidth(rect.width);
			Widgets.Label(
				new Rect(x, rect.y, width, rect.height),
				hediff.Part?.LabelCap ?? "WholeBody".Translate());
			x += width + 4f;

			// Condition
			GUI.color = hediff.LabelColor;
			width = GetHediffLabelWidth(rect.width);
			Widgets.Label(
				new Rect(x, rect.y, width, rect.height),
				hediff.LabelCap);
			x += width + 4f;

			// Severity
			width = GetSeverityLabelWidth(rect.width);
			Widgets.Label(
				new Rect(x, rect.y, width, rect.height),
				hediff.SeverityLabel);
			//x += width + 4f;

			Text.Anchor = anchor;
			GUI.color = oriColor;

			bool num = Widgets.ButtonInvisible(rect);
			if (num && !chosen)
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();

			Widgets.RadioButtonDraw(
				rect.x + rect.width - 24f,
				rect.y + rect.height / 2f - 12f,
				chosen);
			return num;
		}

		private float CalcOptionsHeight(float width)
		{
			float height = 0f;
			foreach (var hediff in Hediffs)
				height += CalcHediffHeight(hediff, width) + 8f;
			return height;
		}

		private float CalcHediffHeight(Hediff hediff, float width) =>
			Mathf.Max(Text.CalcHeight(hediff.Part?.Label, GetPartLabelWidth(width)), Text.CalcHeight(hediff.Label, GetHediffLabelWidth(width)));

		private float GetPartLabelWidth(float width) =>
			(width - 24f) * (2f / 6f) - 4f;
		private float GetHediffLabelWidth(float width) =>
			(width - 24f) * (3f / 6f) - 4f;
		private float GetSeverityLabelWidth(float width) =>
			(width - 24f) * (1f / 6f) - 4f;


		private static bool IsValidHediff(Pawn pawn, Hediff hediff) =>
			hediff.Visible
			&& hediff.def.isBad
			&& hediff.def.everCurableByItem
			&& !hediff.FullyImmune()
			&& !(hediff is Hediff_MissingPart
				&& hediff.Part != null
				&& (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(hediff.Part) || (hediff.Part.parent != null && pawn.health.hediffSet.PartIsMissing(hediff.Part.parent))));
	}
}
