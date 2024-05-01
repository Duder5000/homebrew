using HarmonyLib;
using Microsoft.SqlServer.Server;
using Mono.Cecil.Cil;
using RimWorld;
using RimWorld.Planet;
using RimWorld.Utility;
using System;
using System.Collections.Generic;
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
using static UnityEngine.Scripting.GarbageCollector;

namespace LTS_Implants
{
    public class LTS_ChangeHediff : Hediff
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
            bool canApplyOnCheck;
            bool bandwidthCheck = (target.Thing as Corpse).InnerPawn.GetStatValue(StatDef.Named("BandwidthCost")) <= this.parent.pawn.mechanitor.TotalBandwidth - this.parent.pawn.mechanitor.UsedBandwidth;
            canApplyOnCheck = (base.CanApplyOn(target, dest) && target.HasThing && (corpse = target.Thing as Corpse) != null && this.CanResurrect(corpse) && bandwidthCheck);
            //Log.Message("CanApplyOn check: " + canApplyOnCheck);
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
            bool canApplyOnCheck;
            bool bandwidthCheck = (target.Thing as Pawn).GetStatValue(StatDef.Named("BandwidthCost")) <= this.parent.pawn.mechanitor.TotalBandwidth - this.parent.pawn.mechanitor.UsedBandwidth;
            canApplyOnCheck = (base.CanApplyOn(target, dest) && target.HasThing && (pawn = target.Thing as Pawn) != null && bandwidthCheck && this.CanDominate(pawn));

            //Log.Message("bandwidthCheck check: " + bandwidthCheck);
            //Log.Message("Base check: " + (base.CanApplyOn(target, dest)));
            //Log.Message("pawn check: " +  (pawn = target.Thing as Pawn));
            //Log.Message("CanDominate check: " + this.CanDominate(pawn));
            //Log.Message("target.HasThing check: " + target.HasThing);


            //Log.Message("CanApplyOn check: " + canApplyOnCheck);
            return canApplyOnCheck;
        }



        private bool CanDominate(Pawn pawn)
        {
            //Log.Message("bandwidthCheck check: " + bandwidthCheck);
            
            
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
            base.Initialize(props);
            ingredientCountRemaining = this.Props.startingIngredientCount;
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
        public int ingredientCountRemaining;
        public int ammoCountToRefill;
        public int ammoCountPerCharge;
        public SoundDef soundReload;

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
        public AbilityReloadable (Pawn pawn, AbilityDef abilityDef) : base(pawn, abilityDef) { }
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
    public class LTS_IModExtension : DefModExtension
    {
        public int LTS_TicksBetweenPulse;
        public HediffDef LTS_Hediff;
        public float LTS_Severity;
        public GeneDef LTS_Gene;
        public float LTS_HemogenMaxOffset;
        //public bool LTS_DoSomethingOnDeath;
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

    [HarmonyPatch(typeof(Pawn_MechanitorTracker))]
    [HarmonyPatch(nameof(Pawn_MechanitorTracker.CanCommandTo))]
    class Pawn_MechanitorTracker_CanCommandTo_Patch //increases the mechanitor's range by MechRemoteControlDistanceOffset
    {
        [HarmonyPostfix]
        public static void CanCommandToPostfix(LocalTargetInfo target, Pawn_MechanitorTracker __instance, ref bool __result)
        {
            float SignalBoosterRange = __instance.Pawn?.GetStatValue(StatDef.Named("MechRemoteControlDistanceOffset")) ?? 0f;
            __result = target.Cell.InBounds(__instance.Pawn.MapHeld) && (float)__instance.Pawn.Position.DistanceToSquared(target.Cell) < (24.9f + SignalBoosterRange) * (24.9f + SignalBoosterRange);
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
                if (!ModsConfig.IsActive("swwu.MechanitorCommandRange"))
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
            if (!__instance.Deathresting && !IsFrozen)
            {
                __instance.CurLevel += (___pawn?.GetStatValue(StatDef.Named("DeathrestIntervalFactor")) ?? 1f - 1f) * 0.2f * -0.033333335f; //time between deathrests multiplied by DeathrestIntervalFactor
            }
            else if (__instance.Deathresting && !IsFrozen)
            {
                __instance.CurLevel += (___pawn?.GetStatValue(StatDef.Named("DeathrestEffectivenessFactor")) ?? 1f - 1f) * 0.2f * 400f; //Deathrest gained while deathresting multiplier
            }
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
                    float num = (__instance.CurLevelPercentage - 0.1f) / (0.033333335f * (1f/ ___pawn?.GetStatValue(StatDef.Named("DeathrestIntervalFactor")) ?? 1f));//multiplies listed time until next deathrest by DeathrestIntervalFactor
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
                            victim.health.hediffSet.GetFirstHediffOfDef(LTS_Hediff).Severity += (LTS_Severity ?? 0.2f)/victim.BodySize;
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