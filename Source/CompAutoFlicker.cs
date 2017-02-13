using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;

namespace AutoFlicker
{
    public class CompAutoFlicker : ThingComp
    {

        private bool enabled = false;

        private int currDaysPassed;

        private bool hasFlickedOnToday;

        private bool hasFlickedOffToday;


        private CompFlickable flickableComp;

        public CompProperties_AutoFlicker Props => (CompProperties_AutoFlicker)this.props;

	    public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            this.flickableComp = this.parent.GetComp<CompFlickable>();
        }

        public override void CompTick()
        {
            base.CompTick();

            CheckDesignateFlick();
        }

        //Make it compatible with both ticker types
        public override void CompTickRare()
        {
            base.CompTickRare();

            CheckDesignateFlick();
        }

        private void CheckDesignateFlick()
        {
            if (!enabled || !parent.Spawned) { return; }

            if (GenDate.DaysPassed > this.currDaysPassed)
            {
                hasFlickedOnToday = false;
                hasFlickedOffToday = false;
                this.currDaysPassed = GenDate.DaysPassed;
            }

            bool? wantsFlickOn = null;

            if (!hasFlickedOnToday && GenLocalDate.DayPercent(this.parent.Map) > this.Props.flickOnPercent)
            {
                wantsFlickOn = true;
                hasFlickedOnToday = true;
            }
            if (!hasFlickedOffToday && GenLocalDate.DayPercent(this.parent.Map) > this.Props.flickOffPercent)
            {
                wantsFlickOn = false;
                hasFlickedOffToday = true;
            }

            if (wantsFlickOn == null)
            {
                return;
            }


            var curState = flickableComp.SwitchIsOn;
            if (flickableComp.WantsFlick())
            {
                curState = !curState;
            }

            if (curState != wantsFlickOn)
            {
                var field = typeof(CompFlickable).GetField("wantSwitchOn", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
                field.SetValue(flickableComp, wantsFlickOn);
                FlickUtility.UpdateFlickDesignation(this.parent);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue(ref this.enabled, nameof(this.enabled), false);
            Scribe_Values.LookValue(ref this.currDaysPassed, nameof(this.currDaysPassed), 0);
            Scribe_Values.LookValue(ref this.hasFlickedOnToday, nameof(this.hasFlickedOnToday), false);
            Scribe_Values.LookValue(ref this.hasFlickedOffToday, nameof(this.hasFlickedOffToday), false);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var c in base.CompGetGizmosExtra())
            {
                yield return c;
            }
            if (this.parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    hotKey = KeyBindingDefOf.CommandItemForbid,
                    icon = TexCommand.Forbidden,
                    defaultLabel = "Day/Night Flicking",
                    defaultDesc = "Automatically instructs Pawns to flick on during the day and off at night",
                    isActive = () => this.enabled,
                    toggleAction = () => this.enabled = !this.enabled
                };
            }
        }
    }
}
