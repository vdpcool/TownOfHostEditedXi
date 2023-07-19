using HarmonyLib;
using UnityEngine;
using System;

namespace TOHE;

//��Դ��https://github.com/tukasa0001/TownOfHost/pull/1265

public class sml
{
    public static int ifQSM = 0;
}
[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
public static class OptionsMenuBehaviourStartPatch
{
    private static ClientOptionItem UnlockFPS;
    private static ClientOptionItem AutoStart;
    private static ClientOptionItem ForceOwnLanguage;
    private static ClientOptionItem ForceOwnLanguageRoleName;
    private static ClientOptionItem EnableCustomButton;
    private static ClientOptionItem EnableCustomSoundEffect;
    private static ClientOptionItem SwitchVanilla;
    private static ClientOptionItem QSM;
    //private static ClientOptionItem Devtx;
    //private static ClientOptionItem FastBoot;
    private static ClientOptionItem VersionCheat;
    private static ClientOptionItem GodMode;
    public static ClientOptionItem CanPublic;
    //int ifQSM = 0;

    public static void Postfix(OptionsMenuBehaviour __instance)
    {
        if (__instance.DisableMouseMovement == null) return;

        Main.SwitchVanilla.Value = false;
        Main.QSM.Value = false;
        
        //Main.Devtx.Value = false;
        if (Main.ResetOptions || !DebugModeManager.AmDebugger)
        {
            Main.ResetOptions = false;
            Main.VersionCheat.Value = false;
            Main.GodMode.Value = false;
        }

        if (UnlockFPS == null || UnlockFPS.ToggleButton == null)
        {
            UnlockFPS = ClientOptionItem.Create("UnlockFPS", Main.UnlockFPS, __instance, UnlockFPSButtonToggle);
            static void UnlockFPSButtonToggle()
            {
                Application.targetFrameRate = Main.UnlockFPS.Value ? 165 : 60;
                Logger.SendInGame(string.Format(Translator.GetString("FPSSetTo"), Application.targetFrameRate));
            }
        }
        if (AutoStart == null || AutoStart.ToggleButton == null)
        {
            AutoStart = ClientOptionItem.Create("AutoStart", Main.AutoStart, __instance, AutoStartButtonToggle);
            static void AutoStartButtonToggle()
            {
                if (Main.AutoStart.Value == false && GameStates.IsCountDown)
                {
                    GameStartManager.Instance.ResetStartState();
                    Logger.SendInGame(Translator.GetString("CancelStartCountDown"));
                }
            }
        }
        if (ForceOwnLanguage == null || ForceOwnLanguage.ToggleButton == null)
        {
            ForceOwnLanguage = ClientOptionItem.Create("ForceOwnLanguage", Main.ForceOwnLanguage, __instance);
        }
        if (ForceOwnLanguageRoleName == null || ForceOwnLanguageRoleName.ToggleButton == null)
        {
            ForceOwnLanguageRoleName = ClientOptionItem.Create("ForceOwnLanguageRoleName", Main.ForceOwnLanguageRoleName, __instance);
        }
        if (EnableCustomButton == null || EnableCustomButton.ToggleButton == null)
        {
            EnableCustomButton = ClientOptionItem.Create("EnableCustomButton", Main.EnableCustomButton, __instance);
        }
        if (EnableCustomSoundEffect == null || EnableCustomSoundEffect.ToggleButton == null)
        {
            EnableCustomSoundEffect = ClientOptionItem.Create("EnableCustomSoundEffect", Main.EnableCustomSoundEffect, __instance);
        }
        if (SwitchVanilla == null || SwitchVanilla.ToggleButton == null)
        {
            SwitchVanilla = ClientOptionItem.Create("SwitchVanilla", Main.SwitchVanilla, __instance, SwitchVanillaButtonToggle);
            static void SwitchVanillaButtonToggle()
            {
                Harmony.UnpatchAll();
                Main.Instance.Unload();
            }
        }

        if (QSM == null || QSM.ToggleButton == null)
        {
            QSM = ClientOptionItem.Create("QSM", Main.QSM, __instance,QSMButtonToggle);
            static void QSMButtonToggle()
            {
                if(sml.ifQSM == 0)
                {
                    Logger.SendInGame(string.Format(Translator.GetString("QSMInfo"), Application.targetFrameRate));
                    sml.ifQSM = 1;
                }
                else
                {
                    Logger.SendInGame(string.Format(Translator.GetString("NoQSMInfo"), Application.targetFrameRate));
                }

            }

        }
     //   if (CanPublic == null || CanPublic.ToggleButton == null)
       // {
         //   CanPublic = ClientOptionItem.Create("CanPublic", Main.CanPublic, __instance);
       //TO }
        //if (Devtx == null || Devtx.ToggleButton == null)
        //{
        //    Devtx = ClientOptionItem.Create("Devtx", Main.Devtx, __instance);
        //}
        //if (FastBoot == null || FastBoot.ToggleButton == null)
        //{
        //    FastBoot = ClientOptionItem.Create("FastBoot", Main.FastBoot, __instance);
        //}
        if ((VersionCheat == null || VersionCheat.ToggleButton == null) && DebugModeManager.AmDebugger)
        {
            VersionCheat = ClientOptionItem.Create("VersionCheat", Main.VersionCheat, __instance);
        }
        if ((GodMode == null || GodMode.ToggleButton == null) && DebugModeManager.AmDebugger)
        {
            GodMode = ClientOptionItem.Create("GodMode", Main.GodMode, __instance);
        }
    }
}

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Close))]
public static class OptionsMenuBehaviourClosePatch
{
    public static void Postfix()
    {
        if (ClientOptionItem.CustomBackground != null)
        {
            ClientOptionItem.CustomBackground.gameObject.SetActive(false);
        }
    }
}