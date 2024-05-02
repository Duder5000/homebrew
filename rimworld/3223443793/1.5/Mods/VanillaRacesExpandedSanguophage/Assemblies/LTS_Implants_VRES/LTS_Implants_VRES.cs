using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using VanillaRacesExpandedSanguophage;
using Verse;

namespace LTS_Implants_VRES
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("rimworld.LTS.implants_VRES");
            //Harmony.DEBUG = true;
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(CompDraincasket))]
    [HarmonyPatch(nameof(CompDraincasket.CompTick))]
    class VanillaRacesExpandedSanguophage_CompDraincasket_CompTick_Patch
    {
        [HarmonyPostfix]
        public static void CompTick_Postfix(CompDraincasket __instance)
        {
            //Log.Warning("Working");
            if (__instance.parent.IsHashIntervalTick(60000) && __instance.Occupant != null && __instance.Fuel > 0f && __instance.compResource != null)
            {
                if (__instance.Occupant?.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("Synthmarrow")) != null)
                {
                    __instance.compResource.PipeNet.DistributeAmongStorage(2);
                }
            }
        }
        //[HarmonyTranspiler]
        //static IEnumerable<CodeInstruction> CompTickTranspile(IEnumerable<CodeInstruction> code, ILGenerator il)
        //{
        //    List<CodeInstruction> lines = new List<CodeInstruction>(code);
        //    List<CodeInstruction> newLines = new List<CodeInstruction>();
        //    string referenceLine = "";
        //    int referenceLineNumber = 0;

        //    Log.Message("BEGINNING OF OUTPUT");
        //    for (int lineNumber = 0; lineNumber < lines.Count; lineNumber++) //finds the line number of the line after the one we want to inject 
        //    {
        //        Log.Message("Line " + (lineNumber).ToString() + ": " + lines[lineNumber].ToString());
        //        if (lines[lineNumber].ToString() == referenceLine)
        //        {
        //            referenceLineNumber = lineNumber;
        //        }
        //    }
        //    Log.Message("END OF OUTPUT");
        //    return lines.AsEnumerable();
        //}
    }
}
