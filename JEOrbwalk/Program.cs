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

        private const Key FarmKey = Key.Z;

        private const Key LastHitKey = Key.X;

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
                    Creep creep = KillableCreep(true);
                    if (creep == null)
                    {
                        //Console.WriteLine("null ");
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
                        //Console.WriteLine("found creep ");
                        Orbwalking.Orbwalk(creep, 500);
                    }
                }

            }
            
            if (Game.IsKeyDown(LastHitKey))
            {
                Creep creep = KillableCreep(false);
                if (creep == null)
                {
                    //Console.WriteLine("null ");
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
                    //Console.WriteLine("found creep ");
                    Orbwalking.Orbwalk(creep, 500);
                }
                
            }

        }


        private static Creep KillableCreep(bool islaneclear)
        {
            var minion = ObjectMgr.GetEntities<Creep>()
                   .Where(creep => creep.IsAlive && me.Distance2D(creep) <= me.GetAttackRange())
                   .OrderBy(creep => creep.Health).DefaultIfEmpty(null).FirstOrDefault();
      
            var test = 0;
            if (minion != null)
            {
                var missilespeed =me.AttackRange/ Missilespeed(me);
                var time = me.IsRanged == false ? 0 : /*Environment.TickCount / 1000000000 + (Game.Ping / 1000) +*/  UnitDatabase.GetAttackBackswing(me) + (me.Distance2D(minion) * missilespeed / 1000);
                test = (int)Math.Round(time * minion.AttacksPerSecond) * minion.DamageAverage;

               
                //Console.WriteLine("test " + test + " time " + time + " distance " + me.Distance2D(minion) / missilespeed);
                if (minion != null && (minion.Health) < GetPhysDamageOnUnit(minion, test))
                {

                    if (me.CanAttack())
                    {
                        return minion;
                    }
                }
            }
            return islaneclear == true ? minion : null;
        }
      
        private static double GetPhysDamageOnUnit(Unit unit, double bonusdamage)
        {
            var PhysDamage = me.DamageAverage;

            double _damageMP = 1 - 0.06 * unit.Armor / (1 + 0.06 * Math.Abs(unit.Armor));

            double realDamage = (bonusdamage+PhysDamage) * _damageMP;

            return realDamage;
        }


        private static float Missilespeed(Hero hero)
        {
            var missilespeed = 0f;

            switch (hero.Name.ToLowerInvariant())
            {
                case "npc_dota_hero_ancientapparition":
                    missilespeed = 1250;
                    break;
                case "npc_dota_hero_bane":
                case "npc_dota_hero_batrider":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_chen":
                    missilespeed = 1100;
                    break;
                case "npc_dota_hero_clinkz":
                case "npc_dota_hero_crystalmaiden":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_dazzle":
                    missilespeed = 1200;
                    break;
                case "npc_dota_hero_deathprophet":
                    missilespeed = 1000;
                    break;
                case "npc_dota_hero_disruptor":
                    missilespeed = 1200;
                    break;
                case "npc_dota_hero_drow_ranger":
                    missilespeed = 1250;
                    break;
                case "npc_dota_hero_enchantress":
                case "npc_dota_hero_enigma":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_gyrocopter":
                    missilespeed = 3000;
                    break;
                case "npc_dota_hero_huskar":
                    missilespeed = 1400;
                    break;
                case "npc_dota_hero_invoker":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_io":
                    missilespeed = 1200;
                    break;
                case "npc_dota_hero_jakiro":
                    missilespeed = 1100;
                    break;
                case "npc_dota_hero_keeperofthelight":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_leshrac":
                case "npc_dota_hero_lich":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_lina":
                case "npc_dota_hero_lion":
                    missilespeed = 1000;
                    break;
                case "npc_dota_hero_lonedruid":
                case "npc_dota_hero_luna":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_medusa":
                    missilespeed = 1200;
                    break;
                case "npc_dota_hero_mirana":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_morphling":
                    missilespeed = 1300;
                    break;
                case "npc_dota_hero_naturesprophet":
                    missilespeed = 1125;
                    break;
                case "npc_dota_hero_necrophos":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_oracle":
                case "npc_dota_hero_outworlddevourer":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_phoenix":
                    missilespeed = 1100;
                    break;
                case "npc_dota_hero_puck":
                case "npc_dota_hero_pugna":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_queenofpain":
                    missilespeed = 1500;
                    break;
                case "npc_dota_hero_razor":
                    missilespeed = 2000;
                    break;
                case "npc_dota_hero_rubick":
                    missilespeed = 1125;
                    break;
                case "npc_dota_hero_shadowdemon":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_shadowfiend":
                    missilespeed = 1200;
                    break;
                case "npc_dota_hero_shadowshaman":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_silencer":
                case "npc_dota_hero_skywrathmage":
                    missilespeed = 1000;
                    break;
                case "npc_dota_hero_sniper":
                    missilespeed = 3000;
                    break;
                case "npc_dota_hero_stormspirit":
                    missilespeed = 1100;
                    break;
                case "npc_dota_hero_techies":
                case "npc_dota_hero_templarassassin":
                case "npc_dota_hero_tinker":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_trollwarlord":
                    missilespeed = 1200;
                    break;
                case "npc_dota_hero_vengefulspirit":
                    missilespeed = 1500;
                    break;
                case "npc_dota_hero_venomancer":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_viper":
                    missilespeed = 1200;
                    break;
                case "npc_dota_hero_visage":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_warlock":
                    missilespeed = 1200;
                    break;
                case "npc_dota_hero_weaver":
                    missilespeed = 900;
                    break;
                case "npc_dota_hero_windranger":
                    missilespeed = 1200;
                    break;
                case "npc_dota_hero_winterwyvern":
                    missilespeed = 700;
                    break;
                case "npc_dota_hero_witchdoctor":
                    missilespeed = 1200;
                    break;
                case "npc_dota_hero_zeus":
                    missilespeed = 1100;
                    break;
                default:
                    missilespeed = float.PositiveInfinity;
                    break;
            }

            return missilespeed;
        }
    }
}
