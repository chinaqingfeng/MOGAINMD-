using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MEC;
using Utf8Json;
using EXILED;
using EXILED.Extensions;
using CustomPlayerEffects;
using Grenades; 

namespace LoutroopPlugin
{
    partial class EventHandlers
    {
        public EventHandlers(LoutroopPlugin plugin) => this.plugin = plugin;
        internal readonly LoutroopPlugin plugin;
        internal List<CoroutineHandle> roundCoroutines = new List<CoroutineHandle>();
        private static bool isHidden;
        internal bool loaded = false;
        private static bool hasTag;
        private static System.Random rand = new System.Random();
        //回合设置
        private bool IsEnableBlackout = false;
        private bool isRoundStarted = false;
        //投票 来自嘤嘤嘤的smod2插件开源，我暂时没头绪//
        private int 同意;
        private int 拒绝;
        private bool 投票是否发起;
        private Dictionary<int, bool> players = new Dictionary<int, bool>();
        private int 理由;
        private string 理由文本;
        private bool 当前是否在投票;
        internal static ReferenceHub SCP181;
        internal static ReferenceHub ChaosCommander;
        internal static ReferenceHub SCP999;
        internal static ReferenceHub SCP550;
        int luckyboyRandomOpenDoor = new System.Random().Next(1, 2);
        int luckyboyRandomHrut = new System.Random().Next(1, 2);
        //private static Vector3 TTMSpawnPos = Map.GetRandomSpawnPoint(RoleType.FacilityGuard);
        //private static Vector3 GhoulSpawnPos = Map.GetRandomSpawnPoint(RoleType.Scp049);

        public void OnWaitingForPlayers()
        {
            Configs.ReloadConfig();
            Log.Info($"[服务器报道]正在等待玩家...");
        }

        public void OnRoundStart()
        {
            Log.Info($"[服务器报道]回合开始了");
            Map.ClearBroadcasts();
            foreach (var player in Player.GetHubs()) player.SetFriendlyFire(false);
            InfectPlayerOnRoundStart();
            
        }

        public void OnRoundEnd()
        {
            Log.Info($"[服务器报道]回合已结束");
            Map.ClearBroadcasts();
            Map.Broadcast(Configs.RoundEndMessage, Configs.RoundEndMessagebctime);
            Cassie.CassieMessage(Configs.RoundEndCassieMessage, false, false);
            foreach (var player in Player.GetHubs()) player.SetFriendlyFire(true);
        }

        public void OnRoundRestart()
        {
            Log.Info($"[服务器报道]回合重启中...");
        }

        public void OnPlayerjoin(PlayerJoinEvent ev)
        {
            Map.ClearBroadcasts();
        }

        public void OnPlayerHurt(ref PlayerHurtEvent ev)
        {
            if (ev.Attacker.GetRole() == RoleType.Scp0492)
            {
                ev.Amount = 50f;
            }

            if (ev.Attacker.GetRole() == RoleType.NtfCommander)
            {
                ev.Amount = 100f;
            }

            if (ev.Attacker.GetRole() == RoleType.Scp049)
            {
                ev.Amount = 300f;
            }

            if (ev.Attacker.GetRole() == RoleType.Scp096)
            {
                ev.Amount = 300f;
            }

            if (ev.Attacker.GetRole() == RoleType.Scp173)
            {
                ev.Amount = 500f;
            }

            if (ev.Attacker.GetTeam() == Team.RIP)
            {

            }
            else
            {
                if (ev.Player.queryProcessor.PlayerId == SCP181?.queryProcessor.PlayerId)
                {
                    if (luckyboyRandomHrut == 1)
                    {
                        ev.Amount = 0f;
                        ev.Player.Broadcast(Configs.LuckyBoySafeAmountPlayermsg, Configs.LuckyBoySafeAmountPlayermsgbctime);
                        ev.Attacker.Broadcast(Configs.LuckyBoySafeAmountAttackermsg, Configs.LuckyBoySafeAmountAttackermsgbctime);
                    }
                }
                
            }

            if (ev.Attacker.queryProcessor.PlayerId == ChaosCommander?.queryProcessor.PlayerId)
            {
                ev.Amount = 70f;
            }

            if (ev.Attacker.GetRole() == RoleType.NtfCommander)
            {
                ev.Amount = 80f;
            }

            if (ev.Attacker.queryProcessor.PlayerId == SCP999?.queryProcessor.PlayerId)
            {
                ev.Amount = 0f;
                ev.Player.AddHealth(10f);
                ev.Attacker.Broadcast($"{ev.Player.GetHealth()}", 1);
            }

            if (ev.Attacker.queryProcessor.PlayerId == SCP550?.queryProcessor.PlayerId)
            {
                ev.Attacker.AddHealth(ev.Amount / 4);
            }

        }

        public void OnShoot(ref ShootEvent ev)//物品射击设置
        { 

        }

        public void OnPlayerLeave(PlayerLeaveEvent ev)
        {
            if (ev.Player.IsHost()) return;
            Log.Debug($"[离开服务器] {ev.Player.GetNickname()} ({ev.Player.GetIpAddress()}:{ev.Player.GetUserId()})");
            if (ev.Player.queryProcessor.PlayerId == SCP181?.queryProcessor.PlayerId)
            {
                KillSCP181();
            }
            if (ev.Player.queryProcessor.PlayerId == ChaosCommander?.queryProcessor.PlayerId)
            {
                KillChaosCommander();
            }
            if (ev.Player.queryProcessor.PlayerId == SCP999?.queryProcessor.PlayerId)
            {
                KillSCP999();
            }
            if (ev.Player.queryProcessor.PlayerId == SCP550?.queryProcessor.PlayerId)
            {
                KillSCP550();
            }

        }

        public void On079LvlGain(Scp079LvlGainEvent ev)
        {
            Cassie.CassieMessage(Configs.ComLvlGainCassieMessage, false, false);
            Map.Broadcast(Configs.ComLvlGainMessage, Configs.ComLvlGainMessagebctime);
            Map.TurnOffAllLights(Configs.cpuppf, false);
            Log.Info("79升级了");
        }

        public void OnContain106(Scp106ContainEvent ev)
        {
            Map.ClearBroadcasts();
            Map.Broadcast(Configs.ContainMessage, Configs.ContainMessagebctime);
            Log.Info("老头被献祭");
            if (ev.Player.GetTeam() == Team.CDP)
            {
                ev.Allow = false;
            }
        }

        public void On096Enrage(ref Scp096EnrageEvent ev)
        {
            Map.TurnOffAllLights(Configs.Enragepf, false);
            Cassie.CassieMessage(Configs.SBEnrageCassieMessage, false, false);
            Log.Info("96生气了");
        }

        public void OnPlayerDie(ref PlayerDeathEvent ev)
        {
            if (ev.Player.GetRole() == RoleType.Scp049)//检查他的角色，Get是检测的意思，当然了，Set是放置，如果把Get替换为Set将给玩家一个角色
            {
                Map.Broadcast($"<color=red>[系统消息]</color>\n{ev.Killer.GetNickname()}收容了SCP-049", 10);
                Log.Info("SCP-049已被收容");
            }

            if (ev.Player.GetRole() == RoleType.Scp93989)
            {
                Map.Broadcast($"<color=red>[系统消息]</color>\n{ev.Killer.GetNickname()}收容了SCP-939-89", 10);
                Log.Info("SCP-939-89已被收容");
            }

            if (ev.Player.GetRole() == RoleType.Scp93953)
            {
                Map.Broadcast($"<color=red>[系统消息]</color>\n{ev.Killer.GetNickname()}收容了SCP-939-53", 10);
                Log.Info("SCP-939-53已被收容");
            }

            if (ev.Player.GetRole() == RoleType.Scp106)
            {
                Map.Broadcast($"<color=red>[系统消息]</color>\n{ev.Killer.GetNickname()}收容了SCP-106", 10);
                Log.Info("SCP-106已被收容");
            }

            if (ev.Player.GetRole() == RoleType.Scp096)
            {
                Map.Broadcast($"<color=red>[系统消息]</color>\n{ev.Killer.GetNickname()}收容了SCP-096", 10);
                Log.Info("SCP-096已被收容");
            }

            if (ev.Player.GetRole() == RoleType.Scp079)
            {
                Map.Broadcast($"<color=red>[系统消息]</color>\n{ev.Killer.GetNickname()}收容了SCP-079", 10);
                Log.Info("SCP-079已被收容");
            }

            if (ev.Player.GetRole() == RoleType.Scp173)
            {
                Map.Broadcast($"<color=red>[系统消息]</color>\n{ev.Killer.GetNickname()}收容了SCP-173", 10);
                Log.Info("SCP-173已被收容");
            }
            if (ev.Player.queryProcessor.PlayerId == SCP181?.queryProcessor.PlayerId)
            {
                Cassie.CassieMessage(Configs.LuckyBoyDeathCassiemsg, false, false);
                Map.Broadcast($"<color=red>[系统消息]</color>\n{ev.Killer.GetNickname()}收容了SCP-181", Configs.LuckyBoyDeathmsgbctime);
                KillSCP181();
            }

            if (ev.Player.queryProcessor.PlayerId == ChaosCommander?.queryProcessor.PlayerId)
            {
                KillChaosCommander();
            }

            if (ev.Player.queryProcessor.PlayerId == SCP999?.queryProcessor.PlayerId)
            {
                Cassie.CassieMessage(Configs.SCP999DeathCassieMessage, false, false);
                Map.Broadcast($"<color=red>[系统消息]</color>{ev.Killer.GetNickname()}收容了SCP-999", Configs.SCP999DeathMessagebctime);
                KillSCP999();
            }

            if (ev.Player.queryProcessor.PlayerId == SCP550?.queryProcessor.PlayerId)
            {
                Cassie.CassieMessage(Configs.SCP550DeathCassieMessage, false, false);
                Map.Broadcast($"<color=red>[系统消息]</color>{ev.Killer.GetNickname()}收容了SCP-550", Configs.SCP550DeathMessagebctime);
                KillSCP550();
            }

        }

        public void OnSetRole(SetClassEvent ev)
        {
           if (ev.Player.GetTeam() == Team.SCP)
            {
                ev.Player.SetRankName($"{ev.Player.GetRole()}");
                ev.Player.SetRankColor("red");
            }

           if (ev.Player.GetRole() == RoleType.NtfCommander)
            {
                ev.Player.SetHealth(350);
                ev.Player.SetRankName("九尾狐指挥官");
                ev.Player.SetRankColor("cyan");
            }

        }

        public void OnTriggerTesla(ref TriggerTeslaEvent ev)//GetTeam，这是检查团队的方式，而不是检查个人
        {
            if (ev.Player.GetTeam() == Team.MTF)
            {
                ev.Triggerable = false;
            }

            if (ev.Player.GetTeam() == Team.RSC)
            {
                ev.Triggerable = false;
            }
        }

        public void OnCheckEscape(ref CheckEscapeEvent ev)
        {
            if (ev.Player.GetTeam() == Team.CDP)
            {
                Map.Broadcast("有一名D级人员逃出了", 3);
                Cassie.CassieMessage(Configs.ClassDEscapeCassieMessage, false, false);
            }

            if (ev.Player.GetTeam() == Team.RSC)
            {
                Map.Broadcast("有一名科学家逃出了", 3);
                Cassie.CassieMessage(Configs.ScientistEscapeCassieMessage, false, false);
            }

        }

        public void OnTeamRespawn(ref TeamRespawnEvent ev)
        {
            if (ev.IsChaos)
            {
                Map.Broadcast(Configs.ChaosEnterMessage, Configs.ChaosEnterMessagebctime);
                Cassie.CassieMessage(Configs.ChaosEnterCassieMessage, false, false);
                InfectPlayerOnTeamRespawn();
            }
        }

        public void OnSpawnRagdoll(SpawnRagdollEvent ev)
        {
            if (ev.Player.GetRole() == RoleType.Scp049)
            {
                ev.Player.AddHealth(20f);
            }
        }

        public void OnFemurEnter(FemurEnterEvent ev)
        {
            Map.Broadcast(Configs.FemurEnterMessage, Configs.FemurEnterMessagebctime);
            Cassie.CassieMessage(Configs.FemurEnterCassieMessage, false, false);
        }

        public void OnPocketDimDamage(PocketDimDamageEvent ev)
        {
            ev.Player.AddHealth(1f);
        }

        public void OnAnnounceNtfEntrance(AnnounceNtfEntranceEvent ev)
        {
            int SCPCount = 0;
            foreach (var i in Player.GetHubs())
            {
                if (i.GetTeam() == Team.SCP && i.GetRole() != RoleType.Scp0492)
                {
                    SCPCount++;
                }
            }

            if (SCPCount > 0)
            {
                Map.Broadcast(Configs.MTFRespawnSCPMessage, Configs.MTFRespawnSCPMessagebctime);
            }
            else
            {
                Map.Broadcast(Configs.MTFRespawnNOSCPMessage, Configs.MTFRespawnNOSCPMessagebctime);
            }
            
        }

        public void OnAnnounceDecontamination(AnnounceDecontaminationEvent ev)
        {
            Map.Broadcast(Configs.AnnounceDecontaminationMessage, Configs.AnnounceDecontaminationMessagebctime);
        }

        public void OnUseMedicalItem(UsedMedicalItemEvent ev)
        {
            
        }

        public void OnPickupItem(ref PickupItemEvent ev)
        {
            if (ev.Item.ItemId == ItemType.GunUSP)
            {
                Map.Broadcast($"有{ev.Player.GetTeam()}捡起了MicroHID", Configs.PickupMHIDmsgbctime);
            }
            
        }

        public void OnDoorInteraction(DoorInteractionEvent ev)
        {
            if (ev.Player.GetRole() == RoleType.NtfCommander)
            {
                ev.Allow = true;
            }

            if (ev.Player.queryProcessor.PlayerId == SCP181?.queryProcessor.PlayerId)
            {
                if (luckyboyRandomOpenDoor == 1)
                {
                    ev.Allow = true;
                }

                if (luckyboyRandomOpenDoor == 2)
                {
                    ev.Allow = false;
                }

            }
        }

        public static IEnumerator<float> DelayAction(float delay, Action x)
        {
            yield return Timing.WaitForSeconds(delay);
            x();
        }

        public static void OnTeleport106(Scp106TeleportEvent ev)
        {
            if (ev.Player.queryProcessor.PlayerId == SCP550?.queryProcessor.PlayerId)
            {
                ev.Allow = false;
            }
        }

    }
    


}
