using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AutoFlicker
{
	public class CompProperties_AutoFlicker : CompProperties
	{

        public float flickOnPercent = 0.23f; /* not 0.25 to make pawns plan ahead */

        public float flickOffPercent = 0.8f;

        public CompProperties_AutoFlicker()
		{
			this.compClass = typeof(CompAutoFlicker);
		}
	}
}
