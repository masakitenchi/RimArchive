using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimArchive;

public static class HealingUtility
{
    /// <summary>
    /// 尝试治疗伤口
    /// </summary>
    /// <param name="pawn">目标</param>
    /// <param name="woundsToHeal">伤口数量</param>
    /// <param name="message">治疗信息</param>
    /// <returns>如果治疗了任意伤口，返回True</returns>
    public static bool TryHealWounds(Pawn pawn, out TaggedString message, int woundsToHeal = 1)
    {
        bool EverHealed = false;
        StringBuilder sb = new StringBuilder();
        foreach(var hediff in FindInjuries(pawn, woundsToHeal))
        {
            sb.AppendLine(HealthUtility.Cure(hediff));
            EverHealed = true;
        }
        message = sb.ToString();
        return EverHealed;
    }

    private static Hediff_Injury FindInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
    {
        Hediff_Injury hediff_Injury = null;
        List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++)
        {
            Hediff_Injury hediff_Injury2 = hediffs[i] as Hediff_Injury;
            if (hediff_Injury2 != null && hediff_Injury2.Visible && hediff_Injury2.def.everCurableByItem && (allowedBodyParts == null || allowedBodyParts.Contains(hediff_Injury2.Part)) && (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity))
            {
                hediff_Injury = hediff_Injury2;
            }
        }
        return hediff_Injury;
    }

    private static IEnumerable<Hediff> FindInjuries(Pawn pawn, int injuries = 1, IEnumerable<BodyPartRecord> allowedBodyParts = null)
    {
        List<Hediff> hediffs = pawn.health.hediffSet.hediffs.Where(x => x is Hediff_Injury && x.Visible && x.def.everCurableByItem && (allowedBodyParts == null || allowedBodyParts.Contains(x.Part))).ToList();
        hediffs.SortByDescending(x => x.Severity);
        for (int i = 0; i < injuries && i < hediffs.Count; i++)
        {
            yield return hediffs[i];
        }
    }
}





