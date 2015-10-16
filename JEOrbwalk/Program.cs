using System;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;
using SharpDX.Direct3D9;

namespace JEOrbwalk
{
    class Program
    {
        private static bool loaded;

        private static Hero me;

        private static Hero target;

        private const Key ChaseKey = Key.Space;

        private const Key FarmKey = Key.V;

        private const Key LastHitKey = Key.C;

        private static ParticleEffect rangeDisplay;
        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.Load();
            if (rangeDisplay == null)
            {
                return;
            }
            rangeDisplay.Dispose();
            rangeDisplay = null;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!loaded)
            {
                me = ObjectMgr.LocalHero;
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                loaded = true;
            }

            if (!Game.IsInGame || me == null)
            {
                loaded = false;
                me = null;
                if (rangeDisplay == null)
                {
                    return;
                }
                rangeDisplay.Dispose();
                rangeDisplay = null;
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }

            if (rangeDisplay == null)
            {
                rangeDisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                // rangeDisplay.SetControlPointEntity(1, me);
            }
            rangeDisplay.SetControlPoint(1, new Vector3(me.GetAttackRange() + me.HullRadius + 25, 0, 0));

            //rangeDisplay.SetControlPoint(1, new Vector3(me.GetAttackRange() + me.HullRadius, 0, 0));
            //rangeDisplay.SetControlPointEntity(1,me);

            var canCancel = Orbwalking.CanCancelAnimation();
            if (canCancel)
            {
                
                if (Game.IsChatOpen)
                {
                    return;
                }
                if (Game.IsKeyDown(ChaseKey))
                {
                    if (target != null && !target.IsVisible)
                    {
                        var closestToMouse = me.ClosestToMouseTarget(128);
                        if (closestToMouse != null)
                        {
                            target = closestToMouse;
                        }
                    }
                    else
                    {
                        target = me.BestAATarget();
                    }

                    Orbwalking.Orbwalk(target, 500);

                }
                if (Game.IsKeyDown(FarmKey))
                {
                    //Orbwalking.Orbwalk(target, (float)(UnitDatabase.GetAttackRate(me) * 1000));
                }

            }
            
            if (Game.IsKeyDown(LastHitKey))
            {
                Creep creep = KillableCreep();
                if (creep == null)
                {
                    Console.WriteLine("null ");
                    if (target != null && !target.IsVisible)
                    {
                        var closestToMouse = me.ClosestToMouseTarget(128);
                        if (closestToMouse != null)
                        {
                            target = closestToMouse;
                        }
                    }
                    else
                    {
                        target = me.BestAATarget();
                    }
                    Orbwalking.Orbwalk(target, 500);
                }
                else
                {
                    Console.WriteLine("found creep ");
                    Orbwalking.Orbwalk(creep, 500);
                }
                
            }

        }

        private static Creep KillableCreep()
        {
            var minion = ObjectMgr.GetEntities<Creep>()
                   .Where(creep => creep.IsAlive && me.Distance2D(creep) <= me.GetAttackRange())
                   .OrderBy(creep => creep.Health).DefaultIfEmpty(null).FirstOrDefault();
      
            var test = 0;
            if (minion != null)
            {
                var time = /*Environment.TickCount / 1000000000 + (Game.Ping / 1000) +*/ UnitDatabase.GetAttackBackswing(me) + (me.Distance2D(minion) / me.AttackSpeedValue);
                test = (int)Math.Round(time / minion.AttacksPerSecond) * minion.DamageAverage;


                Console.WriteLine("test " + test + " time " + time + " distance " + me.Distance2D(minion) * 5 / me.AttackSpeedValue);
                if (minion != null && (minion.Health - test) < GetPhysDamageOnUnit(minion))
                {

                    if (me.CanAttack())
                    {
                        return minion;
                    }
                }
            }
            return null;
        }
      
        private static double GetPhysDamageOnUnit(Unit unit)
        {
            var PhysDamage = me.DamageAverage;

            double _damageMP = 1 - 0.06 * unit.Armor / (1 + 0.06 * Math.Abs(unit.Armor));

            double realDamage = PhysDamage * _damageMP;

            return realDamage;
        }

    }
}
