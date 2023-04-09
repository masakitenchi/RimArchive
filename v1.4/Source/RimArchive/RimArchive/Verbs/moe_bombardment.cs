using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

#pragma warning disable CS1591

namespace RimArchive
{
    public class Textures : DefModExtension
    {
        public List<string> projectiletexpath = new List<string>
        {
            "Things/Projectile/Bullet_Big"
        };
    }

    [StaticConstructorOnStartup]
    public class Bombardment : OrbitalStrike
    {
        public class BombardmentProjectile : IExposable
        {
            private int lifeTime;

            private int maxLifeTime;

            private Material material;

            public IntVec3 targetCell;

            private const float StartZ = 60f;

            private const float Scale = 2.5f;

            private const float Angle = 180f;

            public int LifeTime => lifeTime;

            public BombardmentProjectile()
            {
            }

            public BombardmentProjectile(int lifeTime, IntVec3 targetCell, Material material)
            {
                this.material = material;
                this.lifeTime = lifeTime;
                maxLifeTime = lifeTime;
                this.targetCell = targetCell;
            }

            public void Tick()
            {
                lifeTime--;
            }

            public void Draw()
            {
                if (lifeTime > 0)
                {
                    Vector3 pos = targetCell.ToVector3() + Vector3.forward * Mathf.Lerp(60f, 0f, 1f - (float)lifeTime / (float)maxLifeTime);
                    pos.z += 1.25f;
                    pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                    Matrix4x4 matrix = default(Matrix4x4);
                    matrix.SetTRS(pos, Quaternion.Euler(0f, 180f, 0f), new Vector3(2.5f, 1f, 2.5f));
                    Graphics.DrawMesh(MeshPool.plane10, matrix, this.material, 0);
                }
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref lifeTime, "lifeTime", 0);
                Scribe_Values.Look(ref maxLifeTime, "maxLifeTime", 0);
                Scribe_Values.Look(ref targetCell, "targetCell");
            }
        }
        public float impactAreaRadius = 15f;
        public FloatRange explosionRadiusRange = new FloatRange(6f, 8f);
        public int randomFireRadius = 25;
        public int bombIntervalTicks = 18;
        public int warmupTicks = 60;
        public int explosionCount = 30;
        private int ticksToNextEffect;
        private IntVec3 nextExplosionCell = IntVec3.Invalid;
        internal List<Material> materials = new List<Material>();
        private List<BombardmentProjectile> projectiles = new List<BombardmentProjectile>();
        public const int EffectiveAreaRadius = 23;
        private const int StartRandomFireEveryTicks = 20;
        private const int EffectDuration = 60;
        public static readonly SimpleCurve DistanceChanceFactor = new SimpleCurve
        {
            new CurvePoint(0f, 1f),
            new CurvePoint(1f, 0.1f)
        };

        public override void SpawnSetup(Map map, bool respawningAfterReload)
        {
            base.SpawnSetup(map, respawningAfterReload);
            if (!respawningAfterReload)
            {
                GetNextExplosionCell();
            }
        }

        public override void StartStrike()
        {
            duration = bombIntervalTicks * explosionCount;
            base.StartStrike();
        }

        public override void Tick()
        {
            if (base.Destroyed)
            {
                return;
            }

            if (warmupTicks > 0)
            {
                warmupTicks--;
                if (warmupTicks == 0)
                {
                    StartStrike();
                }
            }
            else
            {
                base.Tick();
                if (Find.TickManager.TicksGame % 20 == 0 && base.TicksLeft > 0)
                {
                    StartRandomFire();
                }
            }

            EffectTick();
        }

        private void EffectTick()
        {
            if (!nextExplosionCell.IsValid)
            {
                ticksToNextEffect = warmupTicks - bombIntervalTicks;
                GetNextExplosionCell();
            }

            ticksToNextEffect--;
            if (ticksToNextEffect <= 0 && base.TicksLeft >= bombIntervalTicks)
            {
                SoundDefOf.Bombardment_PreImpact.PlayOneShot(new TargetInfo(nextExplosionCell, base.Map));
                projectiles.Add(new BombardmentProjectile(60, nextExplosionCell, materials.RandomElement()));
                ticksToNextEffect = bombIntervalTicks;
                GetNextExplosionCell();
            }

            for (int num = projectiles.Count - 1; num >= 0; num--)
            {
                projectiles[num].Tick();
                if (projectiles[num].LifeTime <= 0)
                {
                    TryDoExplosion(projectiles[num]);
                    projectiles.RemoveAt(num);
                }
            }
        }

        private void GetNextExplosionCell()
        {
            nextExplosionCell = (from x in GenRadial.RadialCellsAround(base.Position, impactAreaRadius, useCenter: true)
                                 where x.InBounds(base.Map)
                                 select x).RandomElementByWeight((IntVec3 x) => DistanceChanceFactor.Evaluate(x.DistanceTo(base.Position) / impactAreaRadius));
        }

        private void StartRandomFire()
        {
            IntVec3 intVec = (from x in GenRadial.RadialCellsAround(base.Position, randomFireRadius, useCenter: true)
                              where x.InBounds(base.Map)
                              select x).RandomElementByWeight((IntVec3 x) => DistanceChanceFactor.Evaluate(x.DistanceTo(base.Position)));
            List<Thing> list = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].TryGetComp<CompProjectileInterceptor>().BombardmentCanStartFireAt(
                    new RimWorld.Bombardment() { instigator = this.instigator},
                    intVec))
                {
                    return;
                }
            }

            FireUtility.TryStartFireIn(intVec, base.Map, Rand.Range(0.1f, 0.925f));
        }

        private void TryDoExplosion(BombardmentProjectile proj)
        {
            List<Thing> list = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].TryGetComp<CompProjectileInterceptor>().CheckBombardmentIntercept(
                    new RimWorld.Bombardment() { instigator = this.instigator}, 
                    new RimWorld.Bombardment.BombardmentProjectile() { targetCell = proj.targetCell}
                    ))
                {
                    return;
                }
            }

            GenExplosion.DoExplosion(proj.targetCell, base.Map, explosionRadiusRange.RandomInRange, DamageDefOf.Bomb, instigator, -1, -1f, null, projectile: def, weapon: weaponDef);
        }

        public override void Draw()
        {
            base.Draw();
            if (!projectiles.NullOrEmpty())
            {
                for (int i = 0; i < projectiles.Count; i++)
                {
                    projectiles[i].Draw();
                }
            }
        }
    }
    public class Verb_Bombardment : RimWorld.Verb_Bombardment
    {
        new public int DurationTicks = 600;
        protected override bool TryCastShot()
        {
            if(currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }
            Bombardment obj = (Bombardment)GenSpawn.Spawn(ThingDefOf.Bombardment_beacon, currentTarget.Cell, caster.Map);
            obj.duration = DurationTicks;
            obj.instigator = caster;
            obj.weaponDef = base.EquipmentSource?.def;
            foreach (string texpath in base.EquipmentSource?.def.GetModExtension<Textures>().projectiletexpath)
                obj.materials.Add(MaterialPool.MatFrom(texpath));
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return 23f;
        }
    }
}
