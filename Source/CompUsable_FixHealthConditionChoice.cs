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
			Dialog_HediffSelection.CreateDialog(pawn, StartJob);

			void StartJob(Hediff hediff)
			{
				SelectedHediff = hediff;
				if (SelectedHediff != null)
				{
					Job job = extraTarget.IsValid ? JobMaker.MakeJob(Props.useJob, parent, extraTarget) : JobMaker.MakeJob(Props.useJob, parent);
					pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				}
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_References.Look(ref SelectedHediff, "SelectedHediff");
		}
	}
}
