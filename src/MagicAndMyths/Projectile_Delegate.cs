﻿using System;
using Verse;

namespace MagicAndMyths
{
    public class Projectile_Delegate : Projectile
    {
        public Action<Projectile_Delegate, Thing, bool> OnImpact;

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            OnImpact?.Invoke(this, hitThing, blockedByShield);
            base.Impact(hitThing, blockedByShield);
        }
    }
}
