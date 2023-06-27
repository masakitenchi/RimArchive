using System;
#pragma warning disable CS1591

namespace RimArchive
{
    [Obsolete("Use CompProperties_Healing instead")]
    public class CompProperties_ContactPot : CompProperties
    {
        public CompProperties_ContactPot() => compClass = typeof(CompContactPot);
        public int HealPawnCount = 4;
        public int WoundsToHealEach = 2;
    }

    [Obsolete("Use CompProperties_Healing instead")]
    public class CompContactPot : ThingComp
    {
        private int _healsleft;
        public CompProperties_ContactPot Props => this.props as CompProperties_ContactPot;
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this._healsleft = Props.HealPawnCount;
        }
        public void DoEffect(Pawn pawn)
        {
            if (HealingUtility.TryHealWounds(pawn, out var Message, Props.WoundsToHealEach))
            {
                Messages.Message(Message, MessageTypeDefOf.PositiveEvent);
                _healsleft--;
            }
            if (_healsleft <= 0)
            {
                this.parent.Destroy();
            }
        }
    }



}
