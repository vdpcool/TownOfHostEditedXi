﻿using Hazel;
using System.Collections.Generic;
using TOHE.Modules;
using UnityEngine;

namespace TOHE.Roles.Crewmate;

public static class Deputy
{
    private static readonly int Id = 1238574;
    private static List<byte> playerIdList = new();
   public static Dictionary<byte, int> DeputyLimit = new();
    public static OptionItem SkillLimitOpt;
    public static OptionItem SkillCooldown;
    public static OptionItem DeputyCanBeSheriff;
    public static OptionItem DeputyKnowWhosSheriff;
    public static OptionItem SheriffKnowWhosDeputy;

    public static void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Deputy);
        Deputy.SkillCooldown = FloatOptionItem.Create(Id + 42, "DeputySkillCooldown", new(2.5f, 900f, 2.5f), 20f, TabGroup.CrewmateRoles, false).SetParent(Sheriff.HasDeputy)
            .SetValueFormat(OptionFormat.Seconds);
        Deputy.SkillLimitOpt = IntegerOptionItem.Create(Id + 44, "DeputySkillLimit", new(1, 990, 1), 3, TabGroup.CrewmateRoles, false).SetParent(Sheriff.HasDeputy)
            .SetValueFormat(OptionFormat.Times);
        Deputy.DeputyCanBeSheriff = BooleanOptionItem.Create(Id + 46, "DCBS", false, TabGroup.CrewmateRoles, false).SetParent(Sheriff.HasDeputy);
        Deputy.DeputyKnowWhosSheriff = BooleanOptionItem.Create(Id + 48, "DeputyKnowWhosSheriff", true, TabGroup.CrewmateRoles, false).SetParent(Deputy.DeputyCanBeSheriff);
        Deputy.SheriffKnowWhosDeputy = BooleanOptionItem.Create(Id + 50, "SheriffKnowWhosDeputy", true, TabGroup.CrewmateRoles, false).SetParent(Deputy.DeputyCanBeSheriff);
    }
    public static void Init()
    {
        playerIdList = new();
        DeputyLimit = new();
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        DeputyLimit.TryAdd(playerId, SkillLimitOpt.GetInt());

        if (Options.CurrentGameMode != CustomGameMode.TOEX || Options.AllModMode.GetBool()) if (!AmongUsClient.Instance.AmHost) return;
       if (!Main.ResetCamPlayerList.Contains(playerId))
            Main.ResetCamPlayerList.Add(playerId);
    }
   
    public static void Remove(byte playerId)
    {
        playerIdList.Remove(playerId);
        DeputyLimit.Remove(playerId);

        if (Options.CurrentGameMode != CustomGameMode.TOEX || Options.AllModMode.GetBool()) if (!AmongUsClient.Instance.AmHost) return;
        if (Main.ResetCamPlayerList.Contains(playerId))
            Main.ResetCamPlayerList.Remove(playerId);
    }
    public static bool IsEnable => playerIdList.Count > 0;
    private static void SendRPC(byte playerId)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetDeputySellLimit, SendOption.Reliable, -1);
        writer.Write(playerId);
        writer.Write(DeputyLimit[playerId]);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void ReceiveRPC(MessageReader reader)
    {
        byte PlayerId = reader.ReadByte();
        int Limit = reader.ReadInt32();
        if (DeputyLimit.ContainsKey(PlayerId))
            DeputyLimit[PlayerId] = Limit;
        else
            DeputyLimit.Add(PlayerId, SkillLimitOpt.GetInt());
    }
    public static bool CanUseKillButton(byte playerId)
        => !Main.PlayerStates[playerId].IsDead
        && (DeputyLimit.TryGetValue(playerId, out var x) ? x : 1) >= 1;
    public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = CanUseKillButton(id) ? SkillCooldown.GetFloat() : 300f;
    public static string GetSkillLimit(byte playerId) => Utils.ColorString(CanUseKillButton(playerId) ? Utils.GetRoleColor(CustomRoles.Deputy) : Color.gray, DeputyLimit.TryGetValue(playerId, out var constableLimit) ? $"({constableLimit})" : "Invalid");
    public static bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        if (DeputyLimit[killer.PlayerId] <= 0)
        {
            NameNotifyManager.Notify(killer, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Deputy), ("你没有手铐了"))); ;
            return false;
        }
       Main.DeputyInProtect.Remove(target.PlayerId);
       Main.DeputyInProtect.Add(target.PlayerId);
      DeputyLimit[killer.PlayerId]--;
        killer.ResetKillCooldown();
        killer.SetKillCooldown();
        killer.RpcGuardAndKill(target);
        target.RpcGuardAndKill(killer);
        return false;
    }
}