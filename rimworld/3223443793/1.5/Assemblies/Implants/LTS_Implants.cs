using HarmonyLib;
using Microsoft.SqlServer.Server;
using Mono.Cecil.Cil;
using RimWorld;
using RimWorld.Planet;
using RimWorld.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Noise;
using Verse.Sound;
using static HarmonyLib.Code;
using static RimWorld.FoodUtility;
using static UnityEngine.Scripting.GarbageCollector;


namespace LTS_Implants
{
    public class LTS_ChangeHediff : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % (def.GetModExtension<LTS_IModExtension>()?.LTS_TicksBetweenPulse ?? 900) == 0)//pulses every 15 seconds, or LTS_TicksBetweenPulse
            {
                Hediff hediff = this.pawn.health?.hediffSet?.GetFirstHediffOfDef(def.GetModExtension<LTS_IModExtension>()?.LTS_Hediff);//LTS_Hediff is the effected hediff
                if (hediff != null)
                {
                    hediff.Severity += (def.GetModExtension<LTS_IModExtension>()?.LTS_Severity ?? -0.005f);//heal by 0.005/0.5%, or LTS_Severity
                    //Log.Message("I am affecting " + hediff + " By " + (def.GetModExtension<LTS_IModExtension>()?.LTS_Severity ?? -0.005f));
                }
                else
                {
                    //Log.Message("I can't find the hediff");
                }
            }
        }
    }

    public class RitualOutcomeComp_PawnHasHediff : RitualOutcomeComp_Quality
    {
        public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
        {
            Hediff pawn = this.TargetHasHediff(assignments);

            if (pawn == null)//this makes it not show up in the list if the pawn doesn't have the hediff
            {
                return null;
            }


            bool flag = pawn != null;
            float num = (flag ? this.qualityOffset : 0f);


            return new QualityFactor
            {
                label = labelMet,
                present = flag,
                qualityChange = this.ExpectedOffsetDesc(true, num),
                quality = num,
                positive = true,
                priority = 4f,
                count = specialMessage,
                toolTip = tipMessage,
            };
        }


        private Hediff TargetHasHediff(RitualRoleAssignments assignments)//returns true if the target have the hediff
        {
            Pawn mother = assignments.FirstAssignedPawn(this.role);
            return mother.health?.hediffSet?.GetFirstHediffOfDef(this.hediff);
        }


        public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
        {
            string text = ((this.qualityOffset < 0f) ? "" : "+");
            return ((this.TargetHasHediff(ritual.assignments) != null) ? this.label : this.labelNotMet).CapitalizeFirst().Formatted(Array.Empty<NamedArgument>()) + ": " + "OutcomeBonusDesc_QualitySingleOffset".Translate(text + this.qualityOffset.ToStringPercent()) + ".";
        }


        public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
        {
            if (this.TargetHasHediff(ritual.assignments) == null)
            {
                return 0f;
            }
            return this.qualityOffset;
        }


        protected override string ExpectedOffsetDesc(bool positive, float quality = 0f)
        {
            if (!positive)
            {
                return "";
            }
            return quality.ToStringWithSign("0.#%");
        }
        public class test
        {
            public string chance = "1";
        }


        public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
        {
            return (float)((this.TargetHasHediff(ritual.assignments) != null) ? 1 : 0);
        }

        public string tipMessage;

        public string labelNotMet;

        public string specialMessage;

        public string labelMet;

        [NoTranslate]
        public string role;

        [NoTranslate]
        public HediffDef hediff;
    }

    public class Recipe_InstallImplantNeedingGene : Recipe_InstallImplant
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            Pawn pawn;
            //return (pawn = thing as Pawn) != null && (this.recipe.genderPrerequisite ?? pawn.gender) == pawn.gender && (!this.recipe.mustBeFertile || !pawn.Sterile(false)) && (this.recipe.allowedForQuestLodgers || !pawn.IsQuestLodger()) && (this.recipe.minAllowedAge <= 0 || pawn.ageTracker.AgeBiologicalYears >= this.recipe.minAllowedAge) && (this.recipe.developmentalStageFilter == null || this.recipe.developmentalStageFilter.Value.Has(pawn.DevelopmentalStage)) && pawn.genes.HasGene(recipe.GetModExtension<LTS_IModExtension>()?.LTS_Gene);
            if ((pawn = thing as Pawn) == null)
            {
                return false;
            }
            if ((this.recipe.genderPrerequisite ?? pawn.gender) != pawn.gender)
            {
                return false;
            }
            if (this.recipe.mustBeFertile && pawn.Sterile())
            {
                return false;
            }
            if (!this.recipe.allowedForQuestLodgers && pawn.IsQuestLodger())
            {
                return false;
            }
            if (this.recipe.minAllowedAge > 0 && pawn.ageTracker.AgeBiologicalYears < this.recipe.minAllowedAge)
            {
                return false;
            }
            if (this.recipe.developmentalStageFilter != null && !this.recipe.developmentalStageFilter.Value.Has(pawn.DevelopmentalStage))
            {
                return false;
            }
            if (ModsConfig.AnomalyActive)
            {
                if (this.recipe.mutantBlacklist != null && pawn.IsMutant && this.recipe.mutantBlacklist.Contains(pawn.mutant.Def))
                {
                    return false;
                }
                if (this.recipe.mutantPrerequisite != null && (!pawn.IsMutant || !this.recipe.mutantPrerequisite.Contains(pawn.mutant.Def)))
                {
                    return false;
                }
            }
            if (!pawn.genes.HasGene(recipe.GetModExtension<LTS_IModExtension>()?.LTS_Gene))
            {
                return false;
            }
            return true;
        }
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[] { billDoer, pawn });
            }
            pawn.health.AddHediff(this.recipe.addsHediff, part, null, null);
            if (pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>() != null)
            {
                pawn.genes.GetFirstGeneOfType<Gene_Hemogen>().SetMax(pawn.genes.GetFirstGeneOfType<Gene_Hemogen>().Max + recipe.GetModExtension<LTS_IModExtension>()?.LTS_HemogenMaxOffset ?? 0); //Offsets max hemogen by LTS_HemogenMaxOffset, or 0 if LTS_HemogenMaxOffset is null.
            }
        }
    }

    public class Recipe_RemoveImplantWithMaxHemogenEffect : Recipe_RemoveImplant
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            MedicalRecipesUtility.IsClean(pawn, part);
            bool flag = this.IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            if (billDoer != null)
            {
                if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[] { billDoer, pawn });
                if (!pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).Contains(part))
                {
                    return;
                }
                Hediff hediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.def == this.recipe.removesHediff);
                if (hediff != null)
                {
                    if (hediff.def.spawnThingOnRemoved != null)
                    {
                        GenSpawn.Spawn(hediff.def.spawnThingOnRemoved, billDoer.Position, billDoer.Map, WipeMode.Vanish);
                    }
                    pawn.health.RemoveHediff(hediff);
                    pawn.genes.GetFirstGeneOfType<Gene_Hemogen>().SetMax(pawn.genes.GetFirstGeneOfType<Gene_Hemogen>().Max + recipe.GetModExtension<LTS_IModExtension>()?.LTS_HemogenMaxOffset ?? 0); //Offsets max hemogen by LTS_HemogenMaxOffset, or 0 if LTS_HemogenMaxOffset is null.
                }
            }
            if (flag)
            {
                base.ReportViolation(pawn, billDoer, pawn.HomeFaction, -70, null);
            }
        }
    }

    public class CompProperties_AbilityMechCluster : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityMechCluster()
        {
            this.compClass = typeof(CompAbilityEffect_MechCluster);
        }

        public float displayRadius;
    }

    public class CompAbilityEffect_MechCluster : CompAbilityEffect
    {
        public new CompProperties_AbilityMechCluster Props
        {
            get
            {
                return (CompProperties_AbilityMechCluster)this.props;
            }
        }

        public bool ShouldHaveInspectString
        {
            get
            {
                return ModsConfig.BiotechActive && this.parent.pawn.RaceProps.IsMechanoid;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (Faction.OfMechanoids == null)
            {
                Messages.Message("MessageNoFactionForVerbMechCluster".Translate(), this.parent.pawn, MessageTypeDefOf.RejectInput, null, false);
            }
            else
            {
                MechClusterUtility.SpawnCluster(target.Cell, this.parent.pawn.MapHeld, MechClusterGenerator.GenerateClusterSketch(2500f, this.parent.pawn.MapHeld, true, true), true, false, null);

            }
        }

        public override void PostApplied(List<LocalTargetInfo> targets, Map map)
        {
            base.PostApplied(targets, map);
            if (this.parent.def.defName == "MechhiveSatelliteUplink") //add field for cooldownFactorStat. Change this if statement to if it's not null. change the contents of getstatvalue on the next line to that field.
            {
                this.parent.StartCooldown(Mathf.RoundToInt(this.parent.def.cooldownTicksRange.RandomInRange * this.parent.pawn?.GetStatValue(StatDef.Named("MechhiveSatelliteUplinkCooldownFactor")) ?? 1f));
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawRadiusRing(target.Cell, this.Props.displayRadius);
        }

        public override string CompInspectStringExtra()
        {
            if (!this.ShouldHaveInspectString)
            {
                return null;
            }
            if (this.parent.CanCast)
            {
                return "AbilityMechSmokepopCharged".Translate();
            }
            return "AbilityMechSmokepopRecharging".Translate(this.parent.CooldownTicksRemaining.ToStringTicksToPeriod(true, false, true, true, false));
        }
    }

    public class CompProperties_MechanitorResurrectMech : CompProperties_AbilityEffect
    {
        public CompProperties_MechanitorResurrectMech()
        {
            this.compClass = typeof(CompAbilityEffect_MechanitorResurrectMech);
        }

        public int maxCorpseAgeTicks = int.MaxValue;

        public EffecterDef appliedEffecterDef;

        public EffecterDef resolveEffecterDef;

        //public EffecterDef centerEffecterDef;
    }

    public class CompAbilityEffect_MechanitorResurrectMech : CompAbilityEffect
    {
        public new CompProperties_MechanitorResurrectMech Props
        {
            get
            {
                return (CompProperties_MechanitorResurrectMech)this.props;
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Corpse corpse;
            bool bandwidthCheck = (target.Thing as Corpse).InnerPawn.GetStatValue(StatDef.Named("BandwidthCost")) <= this.parent.pawn.mechanitor.TotalBandwidth - this.parent.pawn.mechanitor.UsedBandwidth;
            bool canApplyOnCheck = (base.CanApplyOn(target, dest) && target.HasThing && (corpse = target.Thing as Corpse) != null && this.CanResurrect(corpse) && bandwidthCheck);
            //Log.Message("CanApplyOn check: " + canApplyOnCheck);



            if ((target.Thing as Corpse).InnerPawn.Faction == this.parent.pawn.Faction)
            {
                Messages.Message("Can only resurrect allied mechs", (target.Thing as Pawn), MessageTypeDefOf.NegativeEvent);
            }
            else if ((target.Thing as Corpse).timeOfDeath >= Find.TickManager.TicksGame - this.Props.maxCorpseAgeTicks)
            {
                Messages.Message("Target has been dead too long", (target.Thing as Pawn), MessageTypeDefOf.NegativeEvent);
            }
            else if (!bandwidthCheck)
            {
                Messages.Message("Insufficient bandwidth", (target.Thing as Pawn), MessageTypeDefOf.NegativeEvent);
            }




            return canApplyOnCheck;
        }



        private bool CanResurrect(Corpse corpse)
        {
            //return corpse.InnerPawn.RaceProps.IsMechanoid && corpse.InnerPawn.RaceProps.mechWeightClass < MechWeightClass.UltraHeavy && corpse.InnerPawn.Faction == this.parent.pawn.Faction && (corpse.InnerPawn.kindDef.abilities == null || !corpse.InnerPawn.kindDef.abilities.Contains(AbilityDefOf.ResurrectionMech)) && corpse.timeOfDeath >= Find.TickManager.TicksGame - this.Props.maxCorpseAgeTicks;
            return corpse.InnerPawn.RaceProps.IsMechanoid && corpse.InnerPawn.Faction == this.parent.pawn.Faction && (corpse.InnerPawn.kindDef.abilities == null || !corpse.InnerPawn.kindDef.abilities.Contains(AbilityDefOf.ResurrectionMech)) && corpse.timeOfDeath >= Find.TickManager.TicksGame - this.Props.maxCorpseAgeTicks;
        }



        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            //Log.Message("Apply called");
            base.Apply(target, dest);
            Corpse corpse = (Corpse)target.Thing;
            if (!this.CanResurrect(corpse))
            {
                return;
            }
            Pawn innerPawn = corpse.InnerPawn;
            ResurrectionUtility.TryResurrect(innerPawn, null);
            if (this.Props.appliedEffecterDef != null)
            {
                Effecter effecter = this.Props.appliedEffecterDef.SpawnAttached(innerPawn, innerPawn.MapHeld, 1f);
                effecter.Trigger(innerPawn, innerPawn, -1);
                effecter.Cleanup();
                this.parent.pawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, innerPawn);//if resurrection successful, immediately takes control of resurrected mech.

            }
            innerPawn.stances.stagger.StaggerFor(60, 0.17f);
        }
        public override bool GizmoDisabled(out string reason)
        {
            reason = null;
            return false;
        }

        public override IEnumerable<Mote> CustomWarmupMotes(LocalTargetInfo target)
        {
            foreach (LocalTargetInfo localTargetInfo in this.parent.GetAffectedTargets(target))
            {
                Thing thing = localTargetInfo.Thing;
                yield return MoteMaker.MakeAttachedOverlay(thing, ThingDefOf.Mote_MechResurrectWarmupOnTarget, Vector3.zero, 1f, -1f);
            }
            IEnumerator<LocalTargetInfo> enumerator = null;
            yield break;
        }

        public override void PostApplied(List<LocalTargetInfo> targets, Map map)
        {
            //Log.Message("PostApplied called");
            Vector3 vector = Vector3.zero;
            foreach (LocalTargetInfo localTargetInfo in targets)
            {
                vector += localTargetInfo.Cell.ToVector3Shifted();
            }
            vector /= (float)targets.Count<LocalTargetInfo>();
            IntVec3 intVec = vector.ToIntVec3();
            this.Props.resolveEffecterDef.Spawn(intVec, map, 1f).EffectTick(new TargetInfo(intVec, map, false), new TargetInfo(intVec, map, false));

        }
    }

    public class CompProperties_MechanitorDominateMech : CompProperties_AbilityEffect
    {
        public CompProperties_MechanitorDominateMech()
        {
            this.compClass = typeof(CompAbilityEffect_MechanitorDominateMech);
        }
    }

    public class CompAbilityEffect_MechanitorDominateMech : CompAbilityEffect
    {
        public new CompProperties_MechanitorDominateMech Props
        {
            get
            {
                return (CompProperties_MechanitorDominateMech)this.props;
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn pawn;
            
            bool bandwidthCheck = (target.Thing as Pawn).GetStatValue(StatDef.Named("BandwidthCost")) <= this.parent.pawn.mechanitor.TotalBandwidth - this.parent.pawn.mechanitor.UsedBandwidth;
            bool notTempMech = target.Thing.TryGetComp<CompMechPowerCell>() == null;
            bool canApplyOnCheck = (base.CanApplyOn(target, dest) && target.HasThing && (pawn = target.Thing as Pawn) != null && bandwidthCheck && this.CanDominate(pawn) && notTempMech);

            
            if (!notTempMech)
            {
                Messages.Message("Cannot target temporary mech", (target.Thing as Pawn), MessageTypeDefOf.NegativeEvent);
            }
            else if ((target.Thing as Pawn).RaceProps.mechWeightClass >= MechWeightClass.UltraHeavy)
            {
                Messages.Message("Cannot target superheavy mech", (target.Thing as Pawn), MessageTypeDefOf.NegativeEvent);
            }
            else if(!bandwidthCheck)
            {
                Messages.Message("Insufficient bandwidth", (target.Thing as Pawn), MessageTypeDefOf.NegativeEvent);
            }

            return canApplyOnCheck;
        }



        private bool CanDominate(Pawn pawn)
        {
            


            return pawn.RaceProps.IsMechanoid && pawn.RaceProps.mechWeightClass < MechWeightClass.UltraHeavy && (pawn.kindDef.abilities == null || !pawn.kindDef.abilities.Contains(AbilityDefOf.ResurrectionMech));
        }



        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            //Log.Message("Apply called");
            base.Apply(target, dest);
            Pawn pawn = (Pawn)target;
            if (!this.CanDominate(pawn))
            {
                return;
            }


            pawn.SetFaction(this.parent.pawn.Faction);//convert pawn

            this.parent.pawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
            pawn.stances.stagger.StaggerFor(60, 0.17f);
        }
        public override bool GizmoDisabled(out string reason)
        {
            reason = null;
            return false;
        }

        //public override IEnumerable<Mote> CustomWarmupMotes(LocalTargetInfo target)
        //{
        //    foreach (LocalTargetInfo localTargetInfo in this.parent.GetAffectedTargets(target))
        //    {
        //        Thing thing = localTargetInfo.Thing;
        //        yield return MoteMaker.MakeAttachedOverlay(thing, ThingDefOf.Mote_MechResurrectWarmupOnTarget, Vector3.zero, 1f, -1f);
        //    }
        //    IEnumerator<LocalTargetInfo> enumerator = null;
        //    yield break;
        //    yield break;
        //}

        public override void PostApplied(List<LocalTargetInfo> targets, Map map)
        {
            //Log.Message("PostApplied called");
            //Vector3 vector = Vector3.zero;
            //foreach (LocalTargetInfo localTargetInfo in targets)
            //{
            //    vector += localTargetInfo.Cell.ToVector3Shifted();
            //}
            //vector /= (float)targets.Count<LocalTargetInfo>();
            //IntVec3 intVec = vector.ToIntVec3();
            //this.Props.resolveEffecterDef.Spawn(intVec, map, 1f).EffectTick(new TargetInfo(intVec, map, false), new TargetInfo(intVec, map, false));

        }
    }

    public class CompProperties_MechanitorMechCarrier : CompProperties_AbilityEffect
    {
        public CompProperties_MechanitorMechCarrier()
        {
            this.compClass = typeof(CompAbilityEffect_MechanitorMechCarrier);
        }
        public ThingDef fixedIngredient;
        public int costPerPawn;
        //public int maxIngredientCount;
        public StatDef maxIngredientStat;
        public SoundDef soundReload;

        public int startingIngredientCount;
        public PawnKindDef spawnPawnKind;
        public int cooldownTicks = 900;
        public int maxPawnsToSpawn = 2;
        public EffecterDef spawnEffecter;
        public EffecterDef spawnedMechEffecter;
        public bool attachSpawnedEffecter;
        public bool attachSpawnedMechEffecter;
    }

    public class CompAbilityEffect_MechanitorMechCarrier : CompAbilityEffect
    {
        public new CompProperties_MechanitorMechCarrier Props
        {
            get
            {
                return (CompProperties_MechanitorMechCarrier)this.props;
            }
        }


        //public int remainingCharges
        //{
        //    get
        //    {
        //        return this.ingredientCountRemaining / this.Props.costPerPawn;
        //    }
        //}


        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % 3000 == 0 && this.ingredientCountRemaining != MaxIngredients)//60,000 ticks in a day. 20 steel a day would be 1/3000 //temporary steel regeneration for untill I can work up the will to figure out jobdrivers.
            {
                this.ingredientCountRemaining++;
            }
            if (Find.Selector.IsSelected(parent.pawn) && Find.TickManager.TicksGame % (int)Find.TickManager.CurTimeSpeed == 0)//ifthe mechanitor is selected, and once erry 60/1 irl seconds
            {
                for (int i = 0; i < spawnedPawns.Count; i++)
                {
                    if (!spawnedPawns[i].Dead)
                    {
                        GenDraw.DrawLineBetween(this.parent.pawn.TrueCenter(), spawnedPawns[i].TrueCenter());
                    }
                }
            }
            //Log.Message("ingredients: " + ingredientCountRemaining);
        }
        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
        }

        //public override void CompTick()
        //{
        //    if (Find.TickManager.TicksGame % 3000 == 0 && this.ingredientCountRemaining != MaxIngredients)//60,000 ticks in a day. 20 steel a day would be 1/3000 //temporary steel regeneration for untill I can work up the will to figure out jobdrivers.
        //    {
        //        this.ingredientCountRemaining++;
        //    }
        //    if (!Find.Selector.IsSelected(parent))
        //    {
        //        return;
        //    }
        //    for (int i = 0; i < spawnedPawns.Count; i++)
        //    {
        //        if (!spawnedPawns[i].Dead)
        //        {
        //            GenDraw.DrawLineBetween(this.parent.pawn.TrueCenter(), spawnedPawns[i].TrueCenter());
        //        }
        //    }
        //}
        public override void Initialize(AbilityCompProperties props)
        {
            //Log.Error("Mechwomb ");
            base.Initialize(props);
            if (ingredientCountRemaining == -1)
            {
                //ingredientCountRemaining = this.Props.startingIngredientCount;
            }


        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            //Log.Message("Apply called");
            base.Apply(target, dest);

            TrySpawnPawns();
        }
        public int maxspawn()
        {
            int max = (int)this.ingredientCountRemaining / this.Props.costPerPawn;
            if (max > 2) { return 2; }
            else { return max; }
        }
        public void TrySpawnPawns()
        {
            int maxCanSpawn = maxspawn();
            if (maxCanSpawn <= 0)
            {
                return;
            }
            PawnGenerationRequest pawnGenerationRequest = new PawnGenerationRequest(this.Props.spawnPawnKind, this.parent.pawn.Faction, PawnGenerationContext.NonPlayer, -1, true, false, false, true, false, 1f, false, true, false, true, true, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, false, false, false, false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn, null, null, null, false, false, false, -1, 0, false);
            Pawn pawn;
            Lord lord = (((pawn = this.parent.pawn as Pawn) != null) ? pawn.GetLord() : null);
            for (int i = 0; i < maxCanSpawn; i++)
            {
                Pawn pawn2 = PawnGenerator.GeneratePawn(pawnGenerationRequest);
                GenSpawn.Spawn(pawn2, this.parent.pawn.Position, this.parent.pawn.Map, WipeMode.Vanish);
                this.spawnedPawns.Add(pawn2);
                if (lord != null)
                {
                    lord.AddPawn(pawn2);
                }
                ingredientCountRemaining -= this.Props.costPerPawn; // removes the amount of steel needed to make a mech from the stored steel
                if (this.Props.spawnedMechEffecter != null)
                {
                    Effecter effecter = new Effecter(this.Props.spawnedMechEffecter);
                    effecter.Trigger(this.Props.attachSpawnedMechEffecter ? pawn2 : new TargetInfo(pawn2.Position, pawn2.Map, false), TargetInfo.Invalid, -1);
                    effecter.Cleanup();
                }
            }
            this.cooldownTicksRemaining = this.Props.cooldownTicks;
            if (this.Props.spawnEffecter != null)
            {
                Effecter effecter2 = new Effecter(this.Props.spawnEffecter);
                effecter2.Trigger(this.Props.attachSpawnedEffecter ? this.parent.pawn : new TargetInfo(this.parent.pawn.Position, this.parent.pawn.Map, false), TargetInfo.Invalid, -1);
                effecter2.Cleanup();
            }
        }

        //go reload this

        public bool NeedsReload(bool allowForcedReload)
        {
            if (this.Props.fixedIngredient == null) //if no ingredient is set
            {
                return false;
            }
            if (MaxIngredients - ingredientCountRemaining == 0) // if it's full
            {
                return remainingCharges != MaxCharges;
            }
            if (!allowForcedReload)
            {
                return this.remainingCharges == 0;
            }
            return remainingCharges != MaxCharges;
        }
        public void ReloadFrom(Thing ammo)//I think that represents the specific stack, not the type
        {
            if (this.NeedsReload(false))
            {
                return;
            }

            if (ammo.stackCount < this.Props.costPerPawn)
            {
                return;
            }
            int num = Mathf.Clamp(ammo.stackCount / this.Props.costPerPawn, 0, MaxCharges - remainingCharges);
            ammo.SplitOff(num * this.Props.costPerPawn).Destroy(DestroyMode.Vanish);
            //this.remainingCharges += num;
            this.ingredientCountRemaining += num * this.Props.costPerPawn;

            if (this.Props.soundReload != null)
            {
                this.Props.soundReload.PlayOneShot(new TargetInfo(this.parent.pawn.Position, this.parent.pawn.Map, false));
            }
        }














        public int remainingCharges
        {
            get
            {
                return this.ingredientCountRemaining / this.Props.costPerPawn;
            }
        }
        public int MaxCharges
        {
            get
            {
                return MaxIngredients / this.Props.costPerPawn;
            }
        }
        public int MaxIngredients
        {
            get
            {
                return (int)this.parent.pawn.GetStatValue(this.Props.maxIngredientStat);// Current max ingredients
            }
        }
        public string LabelRemaining
        {
            get
            {
                return string.Format("{0} / {1}", remainingCharges, MaxCharges);
            }
        } // string of current/max spawnable grubs.

        public List<Pawn> GetSpawnedPawns()
        {
            return spawnedPawns;
        }


        private int replenishInTicks = -1;
        private int cooldownTicksRemaining;
        //private ThingOwner innerContainer;
        private List<Pawn> spawnedPawns = new List<Pawn>();


        //public ThingDef ammoDef;
        //public int ingredientCountRemaining = -1;
        public int ingredientCountRemaining;
        public int ammoCountToRefill;
        public int ammoCountPerCharge;
        public SoundDef soundReload;

        public override void PostExposeData()//used for saving data between game loads
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ingredientCountRemaining, "ingredientCountRemaining", this.Props.startingIngredientCount);
        }
    }

    public class HediffCompProperties_KillSpawnedPawns : HediffCompProperties
    {
        public AbilityDef abilityDef;
        public HediffCompProperties_KillSpawnedPawns()
        {
            compClass = typeof(HediffComp_KillSpawnedPawns);
        }
    }

    public class HediffComp_KillSpawnedPawns : HediffComp
    {
        public HediffCompProperties_KillSpawnedPawns Props => (HediffCompProperties_KillSpawnedPawns)props;

        public override void Notify_PawnKilled()
        {
            foreach (Pawn i in base.Pawn.abilities.GetAbility(Props.abilityDef).CompOfType<CompAbilityEffect_MechanitorMechCarrier>().GetSpawnedPawns())
            {
                if (!i.Dead)
                {
                    i.Kill(null, null);
                }
            }
        }
    }

    public class Command_AbilityReloadable : Command_Ability
    {
        public Command_AbilityReloadable(Ability ability, Pawn pawn) : base(ability, pawn)
        {
        }
        public override string TopRightLabel
        {
            get
            {
                //AbilityDef def = this.ability.def;
                //string text = "";
                //if (def.EntropyGain > 1E-45f)
                //{
                //    text += "NeuralHeatLetter".Translate() + ": " + def.EntropyGain.ToString() + "\n";
                //}
                return this.ability.CompOfType<CompAbilityEffect_MechanitorMechCarrier>().LabelRemaining;


                //return "Test";
            }
        }
    }

    public class AbilityReloadable : Ability
    {
        public AbilityReloadable() : base() { }
        public AbilityReloadable(Pawn pawn) : base(pawn) { }
        public AbilityReloadable(Pawn pawn, AbilityDef abilityDef) : base(pawn, abilityDef) { }
        public AbilityReloadable(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public AbilityReloadable(Pawn pawn, Precept sourcePrecept, AbilityDef abilityDef) : base(pawn, sourcePrecept, abilityDef) { }
        //public AbilityReloadable() : base() { }






        public override IEnumerable<Command> GetGizmos() // sets the gismo to my editable one.
        {
            //if (!ModLister.RoyaltyInstalled)
            //{
            //    yield break;
            //}
            if (this.gizmo == null)
            {
                this.gizmo = new Command_AbilityReloadable(this, this.pawn);
            }
            yield return this.gizmo;
        }
        public override bool CanCast // checks if the ability can cast.
        {
            get
            {
                if (!base.CanCast)
                {
                    return false;
                }
                return this.CompOfType<CompAbilityEffect_MechanitorMechCarrier>().maxspawn() > 0; //ability needs to be able to spawn at least 1 grub.
            }
        }
    }

    public class LTS_ShieldHediff : Hediff_Implant
    {
        public float PsychicShieldCurrentHealth;
        public float PsychicShieldMaxHealth;
        public int PsychicShieldRegenInteruptedTicks;
        public string Mode;//full, recharging, broken, steady

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            UpdatePsychicShieldMaxHealth();
            PsychicShieldCurrentHealth = PsychicShieldMaxHealth;
            //PsychicShieldCurrentHealth = 0;//for testing
        }

        public void UpdatePsychicShieldMaxHealth()
        {
            PsychicShieldMaxHealth = pawn?.GetStatValue(StatDef.Named("PsychicShieldMaxHealth")) ?? 0f;
        }


        public override void Tick()
        {
            base.Tick();
            //if (Find.TickManager.TicksGame % 10 == 0 && PsychicShieldCurrentHealth != PsychicShieldMaxHealth && PsychicShieldRegenInteruptedTicks <= 0)
            if (PsychicShieldCurrentHealth != PsychicShieldMaxHealth && PsychicShieldRegenInteruptedTicks <= 0)
            {
                //PsychicShieldCurrentHealth++;

                PsychicShieldCurrentHealth += PsychicShieldMaxHealth / 300; //full regen in 5 seconds

                if (PsychicShieldCurrentHealth > PsychicShieldMaxHealth)
                {
                    PsychicShieldCurrentHealth = PsychicShieldMaxHealth;
                }
            }
            else
            {
                PsychicShieldRegenInteruptedTicks--;
            }
            UpdatePsychicShieldMaxHealth();


            //60 tick/second
            //regenerate in 5 seconds
            //regenerate in 300 ticks.



        }

        public override string LabelInBrackets //add (x/y) health information next to hediff
        {
            get
            {
                return ((int)PsychicShieldCurrentHealth).ToString() + "/" + ((int)PsychicShieldMaxHealth).ToString();
            }
        }

        public bool CheckForShield()
        {
            return true;
        }

        public override void ExposeData()//used for saving data between game loads
        {
            base.ExposeData();
            Scribe_Values.Look(ref PsychicShieldCurrentHealth, "PsychicShieldCurrentHealth", 0f);
            Scribe_Values.Look(ref PsychicShieldMaxHealth, "PsychicShieldMaxHealth", 0f); 
            Scribe_Values.Look(ref PsychicShieldRegenInteruptedTicks, "PsychicShieldRegenInteruptedTicks", 0);

        }
    }

    public class LTS_HediffCompProperties_Mote : HediffCompProperties
    {
        //public AbilityDef abilityDef;
        public LTS_HediffCompProperties_Mote()
        {
            compClass = typeof(LTS_HediffComp_Mote);
        }
    }

    public class LTS_HediffComp_Mote : HediffComp
    {
        public LTS_HediffCompProperties_Mote Props => (LTS_HediffCompProperties_Mote)props;

        //public override void Notify_PawnKilled()
        //{
        //    foreach (Pawn i in base.Pawn.abilities.GetAbility(Props.abilityDef).CompOfType<CompAbilityEffect_MechanitorMechCarrier>().GetSpawnedPawns())
        //    {
        //        if (!i.Dead)
        //        {
        //            i.Kill(null, null);
        //        }
        //    }
        //}
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);



            if (this.I_ArchicHaloMote != null && (parent.pawn.Rotation != PreviousRotation))
            {
                if (parent.pawn.Rotation == Rot4.East) RotationOffset = new Vector3(0.1f, 0, 0.04f); //horizontal, depth, vertical //nevermind, it changes per direction
                else if (parent.pawn.Rotation == Rot4.West) RotationOffset = -new Vector3(0.1f, 0, -0.04f);
                else if (parent.pawn.Rotation == Rot4.North) RotationOffset = new Vector3(0, 0, 0.08f);
                else RotationOffset = new Vector3(0, 0, 0);

            }

            //Log.Message(HorizontalOffset);

            if (((this.I_ArchicHaloMote == null) || (this.I_ArchicHaloMote.Destroyed) || (parent.pawn.Rotation != PreviousRotation)) && (!parent.pawn.InBed() && parent.pawn.Awake() && !parent.pawn.Downed))
            {
                ThingDef I_ArchicHaloMoteDef = I_DefOf.I_ArchicHaloMote;

                if (this.I_ArchicHaloMote != null) // if a halo exists
                {
                    exactRotation = I_ArchicHaloMote.exactRotation;
                    
                    if (parent.pawn.Rotation != PreviousRotation) //if the pawn's changed direction
                    {
                        
                        
                        I_ArchicHaloMoteDef.mote.fadeInTime = 0.2f;
                        //this.I_ArchicHaloMote.Destroy();
                        I_ArchicHaloMote.def.mote.fadeOutTime = 0.2f;

                    }
                }


                this.I_ArchicHaloMote = MoteMaker.MakeAttachedOverlay(parent.pawn, I_ArchicHaloMoteDef, Vector3.zero - parent.pawn.Rotation.FacingCell.ToVector3() * 0.05f + RotationOffset, 1f, -1f); //Vec3(vetrtical), scale, existence override.
                




                I_ArchicHaloMote.exactRotation = exactRotation;

            }


            if (!parent.pawn.InBed() && parent.pawn.Awake() && !parent.pawn.Downed)
            {
                this.I_ArchicHaloMote.Maintain();
            }

            
            //this.I_ArchicHaloMote.rotationRate = 1f;
            //this.I_ArchicHaloMote.Rotation.Rotate(RotationDirection.Clockwise);
            I_ArchicHaloMote.exactRotation += 0.2f;
            //I_ArchicHaloMote.exactPosition += new Vector3(1, 0.01f, 0);

            //Log.Message(this.I_ArchicHaloMote.exactRotation);
            PreviousRotation = parent.pawn.Rotation;
        }

        private Mote I_ArchicHaloMote;
        private Vector3 RotationOffset = new Vector3(0, 0, 0);
        private Rot4 PreviousRotation;
        private float exactRotation = 0;
        private bool appearInstantly = false;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref I_ArchicHaloMote, "I_ArchicHaloMote");
        }
    }

    public class LTS_CompProperties_ToggleHediff : CompProperties_AbilityEffect 
    {
        public LTS_CompProperties_ToggleHediff()
        {
            this.compClass = typeof(LTS_CompAbilityEffect_ToggleHediff);
        }
        public HediffDef ToggleHediff;
        public float StartSeverity;
    }

    public class LTS_CompAbilityEffect_ToggleHediff : CompAbilityEffect //if the pawn doesn't have the hediff, add it. If the pawn does have the hediff, remove it. If this is removed and the pawn has the hediff, remove it.
    {
        public new LTS_CompProperties_ToggleHediff Props
        {
            get
            {
                return (LTS_CompProperties_ToggleHediff)this.props;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (target.Pawn != null)
            {
                if (target.Pawn.health?.hediffSet?.GetFirstHediffOfDef(this.Props.ToggleHediff) != null)
                {
                    //target.Pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.ToggleHediff).;
                    //target.Pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.ToggleHediff).PreRemoved();
                    target.Pawn.health.RemoveHediff(target.Pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.ToggleHediff));
                }
                else
                {
                    target.Pawn.health.AddHediff(this.Props.ToggleHediff);
                    //if (this.Props?.StartSeverity != null)
                    //{
                    //    target.Pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.ToggleHediff).Severity = this.Props.StartSeverity;
                    //}

                    if (target.Pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.ToggleHediff).TryGetComp<HediffComp_Lactating>() != null)
                    {
                        target.Pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.ToggleHediff).TryGetComp<HediffComp_Lactating>().TryCharge(-0.124f);
                        
                    }
                }
            }
        }
    }

    public class Recipe_RemoveImplantWithToggleHediff : Recipe_RemoveImplant
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            MedicalRecipesUtility.IsClean(pawn, part);
            bool flag = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
                if (!pawn.health.hediffSet.GetNotMissingParts().Contains(part))
                {
                    return;
                }
                Hediff hediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.def == recipe.removesHediff);
                if (hediff != null)
                {
                    if (hediff.def.spawnThingOnRemoved != null)
                    {
                        GenSpawn.Spawn(hediff.def.spawnThingOnRemoved, billDoer.Position, billDoer.Map);
                    }
                    pawn.health.RemoveHediff(hediff);
                }

                if (recipe.GetModExtension<LTS_IModExtension>().LTS_Hediff != null)
                {

                }

                Hediff toggleableHediff = pawn.health.hediffSet.GetFirstHediffOfDef(recipe.GetModExtension<LTS_IModExtension>().LTS_Hediff); //if the optional hediff is there, remove it.
                if (toggleableHediff != null)
                {
                    pawn.health.RemoveHediff(toggleableHediff);
                }



            }
            if (flag)
            {
                ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
            }



            

        }
    }

    //



    public class LTS_IModExtension : DefModExtension
    {
        public int LTS_TicksBetweenPulse;
        public HediffDef LTS_Hediff;
        public float LTS_Severity;
        public GeneDef LTS_Gene;
        public float LTS_HemogenMaxOffset;
        public bool LTS_MakesShield;
        //public bool LTS_DoSomethingOnDeath;
    }

    [DefOf]
    public static class I_DefOf
    {
        public static ThingDef I_ArchicHaloMote;
        static I_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(I_DefOf));
        }
    }

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("rimworld.LTS.implants");
            //Harmony.DEBUG = true;
            harmony.PatchAll();
        }
    }



    [HarmonyPatch(typeof(Toils_Ingest))]
    [HarmonyPatch(nameof(Toils_Ingest.FinalizeIngest))]
    class Toils_Ingest_FinalizeIngest_Patch //add gourmand tongue mood buff if user had gourmand tongue 
    {
        [HarmonyPrefix]
        static bool FinalizeIngest_Prefix(Pawn ingester, TargetIndex ingestibleInd, ref Toil __result)
        {
            Toil toil = ToilMaker.MakeToil("FinalizeIngest");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(ingestibleInd).Thing;
                if (ingester.needs.mood != null && thing.def.IsNutritionGivingIngestible && thing.def.ingestible.chairSearchRadius > 10f)
                {
                    if (!(ingester.Position + ingester.Rotation.FacingCell).HasEatSurface(actor.Map) && ingester.GetPosture() == PawnPosture.Standing && !ingester.IsWildMan() && thing.def.ingestible.tableDesired)
                    {
                        ingester.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.AteWithoutTable);
                    }
                    if (ingester.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("GourmandTongue")) != null) // if the pawn has the tongue, give the thought
                    {
                        ingester.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("AteMealWithGourmandTongue"));
                    }
                    Room room = ingester.GetRoom();
                    if (room != null)
                    {
                        int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
                        if (ThoughtDefOf.AteInImpressiveDiningRoom.stages[scoreStageIndex] != null)
                        {
                            ingester.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(ThoughtDefOf.AteInImpressiveDiningRoom, scoreStageIndex));
                        }
                    }
                }
                float num = ingester.needs?.food?.NutritionWanted ?? (thing.GetStatValue(StatDefOf.Nutrition) * (float)thing.stackCount);
                if (curJob.ingestTotalCount)
                {
                    num = thing.GetStatValue(StatDefOf.Nutrition) * (float)thing.stackCount;
                }
                else if (curJob.overeat)
                {
                    num = Mathf.Max(num, 0.75f);
                }
                float num2 = thing.Ingested(ingester, num);
                if (!ingester.Dead && ingester.needs?.food != null)
                {
                    ingester.needs.food.CurLevel += num2;
                    ingester.records.AddTo(RecordDefOf.NutritionEaten, num2);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            __result = toil;


            return false;
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch(nameof(Pawn.PreApplyDamage))]
    class Pawn_PreApplyDamage_Patch //remove remaining shield health damage. reduce shield health by damage it absorbed. \\currently stops all damage if PsychicShieldMaxHealth > 0.
    {
        //[HarmonyPrefix]
        //public static bool PreApplyDamage_Prefix()
        //{


        //    return false;
        //}


    //    [HarmonyPostfix]
    //    public static void CanCommandToPostfix(ref DamageInfo dinfo, out bool absorbed, Pawn __instance)
    //    {

    //        float PsychicShieldMaxHealth = __instance.GetStatValue(StatDef.Named("PsychicShieldMaxHealth"));

    //        float damageMultiplier = 1f;
    //        float damage = dinfo.Amount;

    //        if (PsychicShieldMaxHealth > 0 && __instance.Awake())
    //        {
    //            if (__instance.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("PsychokeneticShield")) as LTS_ShieldHediff != null)
    //            {
    //                LTS_ShieldHediff shieldHediff = __instance.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("PsychokeneticShield")) as LTS_ShieldHediff;
    //                if (shieldHediff.PsychicShieldCurrentHealth > (dinfo).Amount) //if our shield has more health than the damage
    //                {
    //                    damage = 0;
    //                    shieldHediff.PsychicShieldCurrentHealth -= ((DamageInfo)dinfo).Amount;//
    //                    shieldHediff.PsychicShieldRegenInteruptedTicks = 300;
    //                    SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(__instance.Position, __instance.Map));
    //                }
    //                else //remove all the damage it can.
    //                {
    //                    damage -= shieldHediff.PsychicShieldCurrentHealth;

    //                    if (shieldHediff.PsychicShieldCurrentHealth != 0)
    //                    {
    //                        SoundDef.Named("EnergyShield_Broken").PlayOneShot(new TargetInfo(__instance.Position, __instance.Map));
    //                    }

    //                    shieldHediff.PsychicShieldCurrentHealth = 0;
    //                    shieldHediff.PsychicShieldRegenInteruptedTicks = 1200;//20 seconds
    //                }
    //            }
    //        }



    //        if (ModsConfig.BiotechActive && __instance.genes != null)
    //        {
    //            damageMultiplier *= __instance.genes.FactorForDamage(dinfo);
    //        }
    //        damageMultiplier *= __instance.health.FactorForDamage(dinfo);
    //        dinfo.SetAmount(damage * damageMultiplier);
    //        new ThingWithComps().PreApplyDamage(ref dinfo, out absorbed);
    //        if (!absorbed)
    //        {
    //            __instance.health.PreApplyDamage(dinfo, out absorbed);
    //        }
    //    }
    }

    [HarmonyPatch(typeof(Hediff))]
    [HarmonyPatch(nameof(Hediff.PostAdd))]
    class Hediff_PostAdd_Patch //remove remaining shield health damage. reduce shield health by damage it absorbed. \\currently stops all damage if PsychicShieldMaxHealth > 0.
    {
        [HarmonyPostfix]
        public static void Hediff_PostAdd_Postfix(DamageInfo? dinfo, Hediff __instance)//, ref bool __result)
        {

            float PsychicShieldMaxHealth = __instance.pawn?.GetStatValue(StatDef.Named("PsychicShieldMaxHealth")) ?? 0f;
            //PsychicShieldMaxHealth = 0;PsychicShieldCurrentHealth




            if (dinfo != null)
            {
                if (((DamageInfo)dinfo).Def.harmsHealth)
                {
                    if (PsychicShieldMaxHealth > 0 && __instance.pawn.Awake())
                    {
                        //LTS_ShieldHediff shieldHediff = __instance.pawn?.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("PsychokeneticShield")) as LTS_ShieldHediff;

                        if (__instance.pawn?.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("PsychokeneticShield")) as LTS_ShieldHediff != null)
                        {
                            LTS_ShieldHediff shieldHediff = __instance.pawn?.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("PsychokeneticShield")) as LTS_ShieldHediff;
                            if (shieldHediff.PsychicShieldCurrentHealth > ((DamageInfo)dinfo).Amount) //if our shield has more health than the damage
                            {

                                __instance.pawn.health.RemoveHediff(__instance);//remove the damage
                                shieldHediff.PsychicShieldCurrentHealth -= ((DamageInfo)dinfo).Amount;//
                                shieldHediff.PsychicShieldRegenInteruptedTicks = 300;
                                SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(__instance.pawn.Position, __instance.pawn.Map));
                            }
                            else //remove all the damage it can.
                            {

                                //DamageInfo reducedDamage = (DamageInfo)dinfo;
                                //DamageInfo reducedDamage = new DamageInfo((DamageInfo)dinfo);
                                DamageInfo reducedDamage = new DamageInfo();

                                //reducedDamage

                                reducedDamage.SetAmount(((DamageInfo)dinfo).Amount - shieldHediff.PsychicShieldCurrentHealth);

                                //reducedDamage.

                                //Ok, lets try changing things one by one to see what's breaking it.




                                reducedDamage.SetAllowDamagePropagation(false);
                                //reducedDamage.Def = ((DamageInfo)dinfo).Def;

                                DamageDef reducedDamageDef = new DamageDef();

                                reducedDamage.Def = reducedDamageDef;
                                //reducedDamageDef.


                                Hediff newWound = HediffMaker.MakeHediff(__instance.def, __instance.pawn);

                                DamageWorker.DamageResult reducedDamageResult = new DamageWorker.DamageResult();




                                //__instance.pawn.health.AddHediff(newWound, ((DamageInfo)dinfo).HitPart, null);//having the third argument be 'reducedDamage' is the problem.
                                //__instance.pawn.health.hediffSet.AddDirect(newWound, reducedDamage);

                                //__instance.pawn.health.RemoveHediff(__instance);

                                //__instance.pawn.health.hediffSet.HasHediff;


                                __instance.Heal(shieldHediff.PsychicShieldCurrentHealth);


                                if (shieldHediff.PsychicShieldCurrentHealth != 0)
                                {
                                    SoundDef.Named("EnergyShield_Broken").PlayOneShot(new TargetInfo(__instance.pawn.Position, __instance.pawn.Map));
                                }


                                shieldHediff.PsychicShieldCurrentHealth = 0;
                                shieldHediff.PsychicShieldRegenInteruptedTicks = 1200;//20 seconds


                            }
                        }
                    }
                    //else Log.Error("Shield on 0 HP");
                }
                //else Log.Error("Doesn't harm health");
            }
            //else Log.Error("No damageInfo");
        }
    }

    [HarmonyPatch(typeof(Pawn_MechanitorTracker))]
    [HarmonyPatch(nameof(Pawn_MechanitorTracker.CanCommandTo))]
    class Pawn_MechanitorTracker_CanCommandTo_Patch //increases the mechanitor's range by MechRemoteControlDistanceOffset
    {
        [HarmonyPostfix]
        public static void CanCommandToPostfix(LocalTargetInfo target, Pawn_MechanitorTracker __instance, ref bool __result)
        {
            float SignalBoosterRange = __instance.Pawn?.GetStatValue(StatDef.Named("MechRemoteControlDistanceOffset")) ?? 0f;
            __result = target.Cell.InBounds(__instance.Pawn.MapHeld) && (float)__instance.Pawn.Position.DistanceToSquared(target.Cell) < (24.9f + SignalBoosterRange) * (24.9f + SignalBoosterRange) || __result;//last line should mean that if something else makes it true, then it is(?)
        }
    }

    [HarmonyPatch(typeof(Pawn_MechanitorTracker))]
    [HarmonyPatch(nameof(Pawn_MechanitorTracker.DrawCommandRadius))]
    class Pawn_MechanitorTracker_DrawCommandRadius_Patch //increases the displayed mechanitor range by MechRemoteControlDistanceOffset
    {
        [HarmonyPrefix]
        static bool DrawCommandRadiusPrefix()
        {
            return false;
        }
        [HarmonyPostfix]
        public static void DrawCommandRadiusPostfix(Pawn_MechanitorTracker __instance)
        {

            if (__instance.Pawn.Spawned && __instance.AnySelectedDraftedMechs)
            {
                //GenDraw.DrawRadiusRing(___pawn.Position, 24.9f + (3f*___pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("SignalBoosterImplant"))?.Severity ?? 0f), Color.white, (IntVec3 c) => __instance.CanCommandTo(c));
                if (!ModsConfig.IsActive("swwu.MechanitorCommandRange") && !ModsConfig.IsActive("Neronix17.TweaksGalore")) //for tweaks galore, it'd be better to try to find the setting specifically, with an inverted result and a null check true
                {
                    IntVec3 position = __instance.Pawn.Position;
                    float radius = 24.9f + (__instance.Pawn?.GetStatValue(StatDef.Named("MechRemoteControlDistanceOffset")) ?? 0f);
                    GenDraw.DrawRadiusRing(position, radius, Color.white);
                }

            }
        }
    }

    [HarmonyPatch(typeof(Gene_Deathrest))]
    [HarmonyPatch(nameof(Gene_Deathrest.RemoveOldDeathrestBonuses))]
    class Gene_Deathrest_RemoveOldDeathrestBonuses_Patch //offsets the pawn's hemogen capacity by their BaseHemogenOffset stat after reset
    {
        [HarmonyPostfix]
        public static void RemoveOldDeathrestBonusesPostfix(Gene_Deathrest __instance)
        {
            __instance.pawn.genes.GetFirstGeneOfType<Gene_Hemogen>().SetMax(__instance.pawn.genes.GetFirstGeneOfType<Gene_Hemogen>().Max + __instance.pawn.GetStatValue(StatDef.Named("BaseHemogenOffset")));
        }
    }

    [HarmonyPatch(typeof(Need_Deathrest))]
    [HarmonyPatch(nameof(Need_Deathrest.NeedInterval))]
    class Need_Deathrest_NeedInterval_Patch//reduces and increases the time left for and between death rests for the DeathrestApparatus and DeathrestCapacitor respectively
    {
        [HarmonyPostfix]
        public static void NeedIntervalPostfix(Need_Deathrest __instance, Pawn ___pawn)
        {
            bool IsFrozen = ___pawn.Suspended || (__instance.def.freezeWhileSleeping && !___pawn.Awake()) || (__instance.def.freezeInMentalState && ___pawn.InMentalState);

            if (!IsFrozen)
            {
                //float deathrestingOffset = 40f;
                //float notDeathrestingOffset = -40f;


                //alright lets do some maths
                //base function: when deathresting: +0.2 * efficiency%                                    when not deathresting: -(1/30)                              all /400f
                //we want:                          +0.04(0.2*120% - 100%) * DeathrestEfficiency%                                +(1/30) - (1/30)*(1/120%)            all /400f

                float deathrestingOffset = 0.2f * ((___pawn?.GetStatValue(StatDef.Named("DeathrestEffectivenessFactor")) ?? 1f) - 1f);
                float notDeathrestingOffset = (1f / 30f) - ((1 / 30) * (1 / (___pawn?.GetStatValue(StatDef.Named("DeathrestIntervalFactor")) ?? 1f)));

                deathrestingOffset = -(0.2f);
                notDeathrestingOffset = (1f / 30f);


                deathrestingOffset = 0.1f * ((___pawn?.GetStatValue(StatDef.Named("DeathrestEffectivenessFactor")) ?? 1f) - 1f);

                deathrestingOffset = ((___pawn?.GetStatValue(StatDef.Named("DeathrestEffectivenessFactor")) ?? 1f) - 1f) * 5 * 40;
                notDeathrestingOffset = -((___pawn?.GetStatValue(StatDef.Named("DeathrestIntervalFactor")) ?? 1f) - 1) * 5 * 40;


                deathrestingOffset = ((___pawn?.GetStatValue(StatDef.Named("DeathrestEffectivenessFactor")) ?? 1f) - 1f) * 0.2f; // +   1.2 0r 1   -1   *0.2

                //deathrestingOffset = ((___pawn?.GetStatValue(StatDef.Named("DeathrestEffectivenessFactor")) ?? 1f) - 1f) * 5 * 40; // +   1.2 0r 1   -1   *0.2

                notDeathrestingOffset = (1f / 30f); // cancels out base function

                notDeathrestingOffset += (1 / (___pawn?.GetStatValue(StatDef.Named("DeathrestIntervalFactor")) ?? 1f)) * (-1f / 30f); // 1/stat*base, so 200% = decrease at half the rate and 50% = decrease at double

                Gene_Deathrest firstGeneOfType = ___pawn.genes.GetFirstGeneOfType<Gene_Deathrest>();
                float deathrestPercent = firstGeneOfType.DeathrestPercent;


                __instance.CurLevel += (__instance.Deathresting ? deathrestingOffset : notDeathrestingOffset) / 400f;
                //__instance.CurLevel += (__instance.Deathresting ? (0.2f * ((___pawn?.GetStatValue(StatDef.Named("DeathrestEffectivenessFactor")) ?? 1f) - 1f)) : (((___pawn?.GetStatValue(StatDef.Named("DeathrestIntervalFactor")) ?? 1f) - 1f) / 30f)) / 400f;





                //Log.Message(__instance.CurLevel);
                //Log.Message(___pawn + " is offset by " + ((__instance.Deathresting ? (0.2f * (___pawn?.GetStatValue(StatDef.Named("DeathrestEffectivenessFactor")) ?? 1f)) : ((___pawn?.GetStatValue(StatDef.Named("DeathrestIntervalFactor")) ?? 1f) / 30f)) / 400f) + " making it's curlevel "+ __instance.CurLevel);
            }
        }
    }

    [HarmonyPatch(typeof(Gene_Deathrest))]
    [HarmonyPatch(nameof(Gene_Deathrest.TickDeathresting))]
    class Gene_Deathrest_TickDeathresting_patch//makes death hediff go up.
    {
        [HarmonyPostfix]
        public static void TickDeathrestingPostfix(bool paused, Gene_Deathrest __instance, Pawn ___pawn)
        {

            //

            int everyXTicks;

            //
            //
            //
            // 100% = +0/tick = once every infinity ticks
            //
            // 120% = +0.2/tick = once every 5 ticks
            // 150% = +0.5/tick = once every 2 ticks
            // 200% = +1/tick = once every tick
            // 400% = +3/tick = 3 times every tick


            // 1/(120% - 100%)


            everyXTicks = (int)(1 / ((___pawn?.GetStatValue(StatDef.Named("DeathrestEffectivenessFactor")) ?? 1f) - 1f));


            if (Find.TickManager.TicksGame % everyXTicks == 0)
            {
                __instance.deathrestTicks++;
            }

            //this seems like it should either trigger every turn or just break if the stat is unmodified (when 0 divided by 0 has a remainder of 0), but, for some reason, it seems to work. Well, thank the mechine spirits, I suppose.

            //if (Find.TickManager.TicksGame % 5 == 0 && ((___pawn?.GetStatValue(StatDef.Named("DeathrestEffectivenessFactor")) ?? 1f) - 1f) == 0.2)
            //{
            //    __instance.deathrestTicks++;
            //}



        }
    }

    [HarmonyPatch(typeof(Need_Deathrest))]
    [HarmonyPatch(nameof(Need_Deathrest.GetTipString))]
    class Need_Deathrest_GetTipString_Patch //increases the displayed time left between deathresting if the pawn has a DeathrestCapacitor
    {
        [HarmonyPostfix]
        public static void GetTipStringPostfix(Need_Deathrest __instance, Pawn ___pawn, ref string __result)
        {
            string text = (__instance.LabelCap + ": " + __instance.CurLevelPercentage.ToStringPercent()).Colorize(ColoredText.TipSectionTitleColor) + "\n";
            if (!__instance.Deathresting)
            {
                if (__instance.CurLevelPercentage > 0.1f)
                {
                    float num = (__instance.CurLevelPercentage - 0.1f) / (0.033333335f * (1f / ___pawn?.GetStatValue(StatDef.Named("DeathrestIntervalFactor")) ?? 1f));//multiplies listed time until next deathrest by DeathrestIntervalFactor
                    text += "NextDeathrestNeed".Translate(___pawn.Named("PAWN"), "PeriodDays".Translate(num.ToString("F1")).Named("DURATION")).Resolve().CapitalizeFirst();
                }
                else
                {
                    text += "PawnShouldDeathrestNow".Translate(___pawn.Named("PAWN")).CapitalizeFirst().Colorize(ColorLibrary.RedReadable);
                }
                text += "\n\n";
            }
            __result = text + __instance.def.description;
        }
    }

    [HarmonyPatch(typeof(SanguophageUtility))]
    [HarmonyPatch(nameof(SanguophageUtility.DeathrestJobReport))]
    class SanguophageUtility_DeathrestJobReport_Patch //reduces the displayed time left while deathresting if the pawn has a DeathrestApparatus
    {
        [HarmonyPostfix]
        public static void DeathrestJobReportPostfix(Pawn pawn, ref string __result)
        {
            Hediff_Deathrest hediff_Deathrest = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Deathrest, false) as Hediff_Deathrest;
            if (hediff_Deathrest != null && hediff_Deathrest.Paused)
            {
                __result = "DeathrestPaused".Translate() + ": " + "LethalInjuries".Translate();
                return;
            }
            Gene_Deathrest firstGeneOfType = pawn.genes.GetFirstGeneOfType<Gene_Deathrest>();
            TaggedString taggedString = "Deathresting".Translate().CapitalizeFirst() + ": ";
            float deathrestPercent = firstGeneOfType.DeathrestPercent;
            if (deathrestPercent < 1f)
            {
                taggedString += Mathf.Min(deathrestPercent, 0.99f).ToStringPercent("F0");
            }
            else
            {
                taggedString += string.Format("{0} - {1}", "Complete".Translate().CapitalizeFirst(), "CanWakeSafely".Translate());
            }
            if (deathrestPercent < 1f)
            {
                taggedString += ", " + "DurationLeft".Translate((Mathf.RoundToInt(firstGeneOfType.MinDeathrestTicks * (1f / pawn?.GetStatValue(StatDef.Named("DeathrestEffectivenessFactor")) ?? 1f) - firstGeneOfType.deathrestTicks)).ToStringTicksToPeriod(true, false, true, true, false));
                //Log.Message(pawn?.GetStatValue(StatDef.Named("DeathrestEffectivenessFactor")));
                //Log.Message(firstGeneOfType.deathrestTicks);
            }
            __result = taggedString.Resolve();
        }
    }

    [HarmonyPatch(typeof(DamageWorker_AddInjury))] //change to bite?
    [HarmonyPatch(nameof(DamageWorker_AddInjury.Apply))]
    class DamageWorker_AddInjury_Apply_Patch
    {
        [HarmonyPostfix]
        public static void ApplyPostfix(DamageInfo dinfo, Thing thing, DamageWorker_AddInjury __instance)
        {
            //Log.Message("ApplyPostfix triggered");

            HediffDef LTS_Hediff = __instance.def.GetModExtension<LTS_IModExtension>()?.LTS_Hediff;
            float? LTS_Severity = __instance.def.GetModExtension<LTS_IModExtension>()?.LTS_Severity;
            GeneDef LTS_Gene = __instance.def.GetModExtension<LTS_IModExtension>()?.LTS_Gene;
            Pawn victim = thing as Pawn;
            Pawn instigator = dinfo.Instigator as Pawn;
            if (victim != null && instigator != null) //if the target is a pawn and the attacker is attacking their target (don't ask)
            {
                //Log.Message("There's a victim and perpetrator");
                if (LTS_Hediff != null && LTS_Severity != null && LTS_Gene != null)//if we have a hediff and severity to inflict
                {
                    //Log.Message("Mod extensions detected");
                    if (victim.RaceProps.IsFlesh)//if target is fleshy
                    {
                        //Log.Message("Pawn is fleshy");
                        if (victim.health?.hediffSet?.GetFirstHediffOfDef(LTS_Hediff) != null) //if victim already has hediff, increase it by severity
                        {
                            //Log.Message("Increased hediff");
                            victim.health.hediffSet.GetFirstHediffOfDef(LTS_Hediff).Severity += (LTS_Severity ?? 0.2f) / victim.BodySize;
                        }
                        else //else add hediff then set severity
                        {
                            //Log.Message("Added hediff");
                            victim.health.AddHediff(__instance.def.GetModExtension<LTS_IModExtension>()?.LTS_Hediff, null, null, null);
                            victim.health.hediffSet.GetFirstHediffOfDef(LTS_Hediff).Severity = (LTS_Severity ?? 0.2f) / victim.BodySize;
                        }
                        if (instigator.genes.HasGene(LTS_Gene))//if perp is hemogenic
                        {
                            if (victim.genes?.HasGene(LTS_Gene) != null)
                            {
                                //Log.Message("Genes detected.");
                                if (!victim.genes.HasGene(LTS_Gene) && victim.RaceProps.Humanlike)//if target isn't hemogenic but is humanoid
                                {
                                    //Log.Message("Normal humanlike detected.");
                                    instigator.needs.food.CurLevel += (LTS_Severity ?? 0.2f) / 2;
                                    GeneUtility.OffsetHemogen(instigator, (LTS_Severity ?? 0.2f) / 2);
                                    if (ModsConfig.IsActive("vanillaracesexpanded.sanguophage")) //set hemogen to normal if VRES
                                    {
                                        if (instigator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedSanguophageHemogen")) != null) { instigator.health.RemoveHediff(instigator.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedSanguophageHemogen"))); }
                                        else if (instigator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedAnimalHemogen")) != null) { instigator.health.RemoveHediff(instigator.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedAnimalHemogen"))); }
                                        else if (instigator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedCorpseHemogen")) != null) { instigator.health.RemoveHediff(instigator.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedCorpseHemogen"))); }
                                    }
                                }
                                else if (victim.genes.HasGene(LTS_Gene) && ModsConfig.IsActive("vanillaracesexpanded.sanguophage"))//victim is hemogenic and VRES is active
                                {
                                    //if (victim.genes.GetFirstGeneOfType<Gene_Hemogen>().Value >= 0)//if they've got any hemogen left
                                    //Log.Message("Sangophage detected.");
                                    instigator.needs.food.CurLevel += (LTS_Severity ?? 0.2f) / 2;
                                    GeneUtility.OffsetHemogen(instigator, (LTS_Severity ?? 0.2f) / 2);
                                    //GeneResourceDrainUtility.OffsetResource(gene_HemogenDrain, offset)
                                    GeneUtility.OffsetHemogen(victim, -(LTS_Severity ?? 0.2f) / 2, false);
                                    {
                                        if (instigator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedSanguophageHemogen")) == null) { instigator.health.AddHediff(HediffDef.Named("VRE_ConsumedSanguophageHemogen"), null, null, null); }
                                        if (instigator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedAnimalHemogen")) != null) { instigator.health.RemoveHediff(instigator.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedAnimalHemogen"))); }
                                        else if (instigator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedCorpseHemogen")) != null) { instigator.health.RemoveHediff(instigator.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedCorpseHemogen"))); }
                                    }
                                }
                            }
                            else if (victim.RaceProps.Animal && ModsConfig.IsActive("vanillaracesexpanded.sanguophage"))//victim is animal
                            {
                                //Log.Message("Animal detected.");
                                instigator.needs.food.CurLevel += (LTS_Severity ?? 0.2f) / 2;
                                GeneUtility.OffsetHemogen(instigator, (LTS_Severity ?? 0.2f) / 2);
                                if (instigator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedAnimalHemogen")) == null) { instigator.health.AddHediff(HediffDef.Named("VRE_ConsumedAnimalHemogen"), null, null, null); }
                                if (instigator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedSanguophageHemogen")) != null) { instigator.health.RemoveHediff(instigator.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedSanguophageHemogen"))); }
                                else if (instigator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedCorpseHemogen")) != null) { instigator.health.RemoveHediff(instigator.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedCorpseHemogen"))); }
                            }
                            else if (false && ModsConfig.IsActive("vanillaracesexpanded.sanguophage"))//victim is zombie/rotting? Lets wait for ANOMALY for this.
                            {
                                //Log.Message("Corpse detected.");
                                instigator.needs.food.CurLevel += (LTS_Severity ?? 0.2f) / 2;
                                GeneUtility.OffsetHemogen(instigator, (LTS_Severity ?? 0.2f) / 2);
                                if (instigator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedCorpseHemogen")) == null) { instigator.health.AddHediff(HediffDef.Named("VRE_ConsumedCorpseHemogen"), null, null, null); }
                                if (instigator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedSanguophageHemogen")) != null) { instigator.health.RemoveHediff(instigator.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedSanguophageHemogen"))); }
                                else if (instigator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedAnimalHemogen")) != null) { instigator.health.RemoveHediff(instigator.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("VRE_ConsumedAnimalHemogen"))); }
                            }
                        }
                    }
                }
            }
        }
    } //hemo-fangs

    [HarmonyPatch(typeof(PregnancyUtility))]
    [HarmonyPatch(nameof(PregnancyUtility.ApplyBirthOutcome_NewTemp))]
    class PregnancyUtility_ApplyBirthOutcome_Patch
    {
        [HarmonyPostfix]
        public static void ApplyBirthOutcomePostfix(RitualOutcomePossibility outcome, float quality, Precept_Ritual ritual, List<GeneDef> genes, Pawn geneticMother, Thing birtherThing, Pawn father = null, Pawn doctor = null, LordJob_Ritual lordJobRitual = null, RitualRoleAssignments assignments = null)
        {

            //if (birtherThing == geneticMother)//if the mother is the thing giving birth (i.e. not a tube)
            if (birtherThing as Pawn != null)//if a pawn is the thing giving birth (i.e. not a tube)
            {
                if (PawnHasArchowomb(birtherThing as Pawn))//if the one giving birth has an archowomb 
                {
                    geneticMother.relations.Children.Last().health.AddHediff(HediffDef.Named("Archoborn"), null, null, null);//give the newest child the hediff
                }
            }
        }

        static public bool PawnHasArchowomb(Pawn pawn)
        {
            return (pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("Archowomb")) != null);
        }

        static public bool PawnHasArchowombAndIsGivingBirth(Pawn pawn, Thing thing)
        {
            //Log.Message("PawnHasArchowombAndIsGivingBirth HAS BEEN CALLED AND RETURNED: " + ((thing == pawn && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Archowomb")) != null) || Find.Storyteller.difficulty.babiesAreHealthy));
            //return (thing == pawn && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Archowomb")) != null) || Find.Storyteller.difficulty.babiesAreHealthy;//if the mother is the one giving birth and the mother has an archowomb or babiesAreHealthy is on, return true.

            if (thing as Pawn != null)
            {
                return (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Archowomb")) != null) || Find.Storyteller.difficulty.babiesAreHealthy;
            }
            return false;
        }

        static public int PawnHasArchowombAndIsGivingBirthInt(RitualOutcomePossibility outcome, Pawn pawn, Thing thing)//if the mother is the one giving birth and the mother has an archowomb or babiesAreHealthy is on, return true.
        {
            //if ((thing == pawn && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Archowomb")) != null) || Find.Storyteller.difficulty.babiesAreHealthy)
            //    return 1;
            //else
            //{
            //    return outcome.positivityIndex;
            //}
            if (thing as Pawn != null)
            {
                if ((thing as Pawn).health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Archowomb")) != null || Find.Storyteller.difficulty.babiesAreHealthy)
                {
                    return 1;
                }
            }
            return outcome.positivityIndex;

        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AddLineToApplyBirthOutcome(IEnumerable<CodeInstruction> code, ILGenerator il)
        {
            List<CodeInstruction> lines = new List<CodeInstruction>(code);
            List<CodeInstruction> newLines = new List<CodeInstruction>();
            string referenceLine = "call static System.Single RimWorld.PregnancyUtility::ChanceMomDiesDuringBirth(System.Single quality)";//well, actually it's the line after the line after where we want our code to go, but we've added '-1's to the 'lineAfterLineNumber' to account for that.
            int referenceLineNumber = 0;

            //Log.Message("BEGINNING OF OUTPUT");
            for (int lineNumber = 0; lineNumber < lines.Count; lineNumber++) //finds the line number of the line after the one we want to inject 
            {
                //if (lineNumber >= 0 && lineNumber < 100)
                //{
                //    Log.Message("Line " + (lineNumber).ToString() + ": " + lines[lineNumber].ToString());
                //}
                if (lines[lineNumber].ToString() == referenceLine)
                {
                    referenceLineNumber = lineNumber;
                }
            }
            //Log.Message("END OF OUTPUT");



            if (referenceLineNumber != 0)//if we found the lineAfterLineNumber
            {
                for (int lineNumber = 0; lineNumber < referenceLineNumber - 4; lineNumber++)//add all the lines before our injection to the empty newLines
                {
                    newLines.Add(lines[lineNumber]);
                }
                //stloc.2   //evaluation stack to babiesAreHealthy to, incase i want to
                //stloc.3   //evaluation stack to positivity index

                //ldarg.s   geneticMother
                //ldarg.s birtherThing
                //call      bool LTS_Implants.PregnancyUtility_ApplyBirthOutcome_Patch::PawnHasArchowombAndIsGivingBirth(class ['Assembly-CSharp'] Verse.Pawn, class ['Assembly-CSharp'] Verse.Thing)
                //stloc.3

                newLines.Add(new CodeInstruction(OpCodes.Ldarg_S, 4));//feeds geneticMother
                newLines.Add(new CodeInstruction(OpCodes.Ldarg_S, 5));//feeds birtherThing
                newLines.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PregnancyUtility_ApplyBirthOutcome_Patch), nameof(PregnancyUtility_ApplyBirthOutcome_Patch.PawnHasArchowombAndIsGivingBirth))));
                newLines.Add(new CodeInstruction(OpCodes.Stloc_2));//sets babiesAreHealthy 

                newLines.Add(new CodeInstruction(OpCodes.Ldarg_S, 0));//feeds outcome
                newLines.Add(new CodeInstruction(OpCodes.Ldarg_S, 4));//feeds geneticMother
                newLines.Add(new CodeInstruction(OpCodes.Ldarg_S, 5));//feeds birtherThing
                newLines.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PregnancyUtility_ApplyBirthOutcome_Patch), nameof(PregnancyUtility_ApplyBirthOutcome_Patch.PawnHasArchowombAndIsGivingBirthInt))));
                newLines.Add(new CodeInstruction(OpCodes.Stloc_3));//sets positivityIndex



                for (int lineNumber = referenceLineNumber - 4; lineNumber < lines.Count; lineNumber++)//add all the lines after our injection to the newLines
                {
                    newLines.Add(lines[lineNumber]);
                }

                //Log.Message("RETURNING NEWLINES");
                return newLines.AsEnumerable();
                //Log.Message("HERE!");
                //Log.Message(lines[lineAfterLineNumber-4].ToString());
                //Log.Message(lines[lineAfterLineNumber-3].ToString());
                //Log.Message(lines[lineAfterLineNumber-2].ToString());
                //Log.Message("Here's where things get added");
                //Log.Message(lines[lineAfterLineNumber-1].ToString());
                //Log.Message(lines[lineAfterLineNumber].ToString());

                //Log.Message(newLines[referenceLineNumber -4].ToString());
                //Log.Message(newLines[referenceLineNumber -3].ToString());
                //Log.Message(newLines[referenceLineNumber -2].ToString());
                //Log.Message(newLines[referenceLineNumber -1].ToString());
                //Log.Message(newLines[referenceLineNumber].ToString());
                //Log.Message(newLines[referenceLineNumber +1].ToString());
                //Log.Message(newLines[referenceLineNumber +2].ToString());
                //Log.Message(newLines[referenceLineNumber +3].ToString());
                //Log.Message(newLines[referenceLineNumber + 4].ToString());
                //Log.Message(newLines[referenceLineNumber + 5].ToString());
                //Log.Message(newLines[referenceLineNumber + 6].ToString());
            }
            else
            {
                //Log.Message("LTS Implants: Failed to apply transpile, returning base");
                Log.Error("LTS Implants: Failed to apply transpile, returning base");
            }
            return lines.AsEnumerable();

        }
    }//Apply hediff to child and force healthy birth with archowomb
}