using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TOHE.Roles.Neutral;
using UnityEngine;

[assembly: AssemblyFileVersion(TOHE.Main.PluginVersion)]
[assembly: AssemblyInformationalVersion(TOHE.Main.PluginVersion)]
[assembly: AssemblyVersion(TOHE.Main.PluginVersion)]
namespace TOHE;

[BepInPlugin(PluginGuid, "TOHEX", PluginVersion)]
[BepInIncompatibility("jp.ykundesu.supernewroles")]
[BepInProcess("Among Us.exe")]
public class Main : BasePlugin
{
    // == プログラム設定 / Program Config ==
    public static readonly string ModName = "TOHEX";
    public static readonly string ModColor = "#fffcbe";
    public static readonly bool AllowPublicRoom = true;
    public static readonly string ForkId = "TOHEX";
    public const string OriginalForkId = "OriginalTOH";
    public static HashAuth DebugKeyAuth { get; private set; }
    public const string DebugKeyHash = "c0fd562955ba56af3ae20d7ec9e64c664f0facecef4b3e366e109306adeae29d";
    public const string DebugKeySalt = "59687b";
    public static ConfigEntry<string> DebugKeyInput { get; private set; }
    public static readonly string MainMenuText = "<color=#fffcbe>喜！</color><color=#FF0066>瓜！</color><color=#FFFF00>叨！</color>";
    public const string PluginGuid = "com.xi.tohex";
    public const string PluginVersion = "1.4.2";
    public const string CanaryPluginVersion = "裤";
    public const int PluginCreate = 7;

    //没错，那个没树枝的N删了

    public Harmony Harmony { get; } = new Harmony(PluginGuid);
    public static Version version = Version.Parse(PluginVersion);
    public static BepInEx.Logging.ManualLogSource Logger;
    public static bool hasArgumentException = false;
    public static string ExceptionMessage;
    public static bool ExceptionMessageIsShown = false;
    public static bool AlreadyShowMsgBox = false;
    public static string credentialsText;
    public static NormalGameOptionsV07 NormalOptions => GameOptionsManager.Instance.currentNormalGameOptions;
    //Client Options
    public static ConfigEntry<string> HideName { get; private set; }
    public static ConfigEntry<string> HideColor { get; private set; }
    public static ConfigEntry<int> MessageWait { get; private set; }
    public static ConfigEntry<bool> UnlockFPS { get; private set; }
    public static ConfigEntry<bool> AutoStart { get; private set; }
    public static ConfigEntry<bool> ForceOwnLanguage { get; private set; }
    public static ConfigEntry<bool> ForceOwnLanguageRoleName { get; private set; }
    public static ConfigEntry<bool> EnableCustomButton { get; private set; }
    public static ConfigEntry<bool> EnableCustomSoundEffect { get; private set; }
    public static ConfigEntry<bool> SwitchVanilla { get; private set; }
    public static ConfigEntry<bool> QSM { get; private set; }
    //public static ConfigEntry<bool> Devtx { get; private set; }
    //public static ConfigEntry<bool> FastBoot { get; private set; }
    public static ConfigEntry<bool> VersionCheat { get; private set; }
    public static ConfigEntry<bool> GodMode { get; private set; }
    public static ConfigEntry<bool> CanPublic { get; private set; }

    public static Dictionary<byte, PlayerVersion> playerVersion = new();
    //Preset Name Options
    public static ConfigEntry<string> Preset1 { get; private set; }
    public static ConfigEntry<string> Preset2 { get; private set; }
    public static ConfigEntry<string> Preset3 { get; private set; }
    public static ConfigEntry<string> Preset4 { get; private set; }
    public static ConfigEntry<string> Preset5 { get; private set; }
    //Other Configs
    public static ConfigEntry<string> WebhookURL { get; private set; }
    public static ConfigEntry<string> BetaBuildURL { get; private set; }
    public static ConfigEntry<float> LastKillCooldown { get; private set; }
    public static ConfigEntry<float> LastShapeshifterCooldown { get; private set; }
    public static OptionBackupData RealOptionsData;
    public static Dictionary<byte, PlayerState> PlayerStates = new();
    public static Dictionary<byte, string> AllPlayerNames = new();
    public static Dictionary<(byte, byte), string> LastNotifyNames;
    public static Dictionary<byte, Color32> PlayerColors = new();
    public static Dictionary<byte, PlayerState.DeathReason> AfterMeetingDeathPlayers = new();
    public static Dictionary<CustomRoles, string> roleColors;
    public static bool IsFixedCooldown => CustomRoles.Vampire.IsEnable();
    public static float RefixCooldownDelay = 0f;
    public static GameData.PlayerInfo LastVotedPlayerInfo;
    public static string LastVotedPlayer;
    public static List<byte> ResetCamPlayerList = new();
    public static List<byte> winnerList = new();
    public static List<string> winnerNameList = new();
    public static List<int> clientIdList = new();
    public static List<(string, byte, string)> MessagesToSend = new();
    public static bool isChatCommand = false;
    public static List<PlayerControl> LoversPlayers = new();
    public static List<PlayerControl> seniormanagementPlayers = new();
    public static bool isLoversDead = true;
    public static bool isjackalDead = true;
    public static bool isSheriffDead = true; 
    public static bool isseniormanagementDead = true;
    public static Dictionary<byte, float> AllPlayerKillCooldown = new();
    public static Dictionary<byte, Vent> LastEnteredVent = new();
    public static Dictionary<byte, Vector2> LastEnteredVentLocation = new();
    public static List<byte> CyberStarDead = new();
    public static List<byte> CaptainDead = new();
    public static List<byte> BoobyTrapBody = new();
    public static Dictionary<byte, byte> KillerOfBoobyTrapBody = new();
    public static Dictionary<byte, string> DetectiveNotify = new();
    public static List<byte> OverDeadPlayerList = new();
    public static bool DoBlockNameChange = false;
    public static int updateTime;
    public static bool newLobby = false;
    public static Dictionary<int, int> SayStartTimes = new();
    public static Dictionary<int, int> SayBanwordsTimes = new();
    public static Dictionary<byte, float> AllPlayerSpeed = new();
    public const float MinSpeed = 0.0001f;
    public static List<byte> CleanerBodies = new();
    public static List<byte> BrakarVoteFor = new();
    public static Dictionary<byte, (byte, float)> BitPlayers = new();
    public static Dictionary<byte, float> WarlockTimer = new();
    public static Dictionary<byte, float> AssassinTimer = new();
    public static Dictionary<byte, PlayerControl> CursedPlayers = new();
    public static Dictionary<byte, bool> isCurseAndKill = new();
    public static Dictionary<byte, int> MafiaRevenged = new();
    public static Dictionary<byte, int> GuesserGuessed = new();
    public static Dictionary<byte, int> CapitalismAddTask = new();
    public static Dictionary<byte, int> CapitalismAssignTask = new();
    public static Dictionary<(byte, byte), bool> isDoused = new();
    public static Dictionary<(byte, byte), bool> isDraw = new();
    public static Dictionary<byte, (PlayerControl, float)> ArsonistTimer = new();
    public static Dictionary<byte, (PlayerControl, float)> RevolutionistTimer = new();
    public static Dictionary<byte, long> RevolutionistStart = new();
    public static Dictionary<byte, long> RevolutionistLastTime = new();
    public static Dictionary<byte, int> RevolutionistCountdown = new();
    public static Dictionary<byte, byte> PuppeteerList = new();
    public static Dictionary<byte, byte> SpeedBoostTarget = new();
    public static Dictionary<byte, int> MayorUsedButtonCount = new();
    public static Dictionary<byte, int> ParaUsedButtonCount = new();
    public static Dictionary<byte, int> MarioVentCount = new();
    public static Dictionary<byte, long> VeteranInProtect = new();
    public static Dictionary<byte, int> VeteranNumOfUsed = new();
    public static Dictionary<byte, long> GrenadierBlinding = new();
    public static Dictionary<byte, long> MadGrenadierBlinding = new();
    public static Dictionary<byte, int> CursedWolfSpellCount = new();
    public static int AliveImpostorCount;
    public static bool isCursed;
    public static Dictionary<byte, bool> CheckShapeshift = new();
    public static Dictionary<byte, byte> ShapeshiftTarget = new();
    public static Dictionary<(byte, byte), string> targetArrows = new();
    public static Dictionary<byte, Vector2> EscapeeLocation = new();
    public static Dictionary<byte, Vector2> TimeMasterLocation = new();
    public static Dictionary<byte, Vector2> signallerLocation = new();
    public static bool VisibleTasksCount = false;
    public static string nickName = "";
    public static bool introDestroyed = false;
    public static int DiscussionTime;
    public static int VotingTime;
    public static byte currentDousingTarget = byte.MaxValue;
    public static byte currentDrawTarget = byte.MaxValue;
    public static float DefaultCrewmateVision;
    public static float DefaultImpostorVision;
    public static bool IsInitialRelease = DateTime.Now.Month == 1 && DateTime.Now.Day is 17;
    public static bool IsAprilFools = DateTime.Now.Month == 4 && DateTime.Now.Day is 1;
    //public static bool IsTOHEjnr = DateTime.Now.Month == 5 && DateTime.Now.Day is 20;
    public static bool ResetOptions = true;
    public static byte FirstDied = byte.MaxValue;
    public static byte ShieldPlayer = byte.MaxValue;
    public static int MadmateNum = 0;
    public static int BardCreations = 0;
    public static Dictionary<byte, long> sabcatInProtect = new();
    public static Dictionary<byte, int> sabcatNumOfUsed = new();
    public static Dictionary<byte, byte> Provoked = new();
    public static Dictionary<byte, int> DovesOfNeaceNumOfUsed = new();
    public static Dictionary<byte, long> RudepeopleInProtect = new();
    public static Dictionary<byte, int> RudepeopleNumOfUsed = new();
    public static Dictionary<byte, long> TimeStopsInProtect = new();
    //public static Dictionary<byte, int> HypnotistMax = new();
    public static Dictionary<byte, long> TimeMasterInProtect = new();
    public static Dictionary<byte, int> TimeMasterNum = new();
    public static Dictionary<byte, int> VultureEatMax = new();
    public static List<byte> VultureBodies = new();
    public static Dictionary<byte, int> BullKillMax = new();
    public static Dictionary<byte, int> MasochismKillMax = new();
    public static Dictionary<byte, long> DissenterInProtect = new();
    public static Dictionary<byte, int> CultivatorKillMax = new();
    public static Dictionary<byte, int> YinLangKillMax = new();
    public static Dictionary<byte, int> DisorderKillCooldownMax = new();
    public static Dictionary<byte, int> ScoutImpotors = new();
    public static Dictionary<byte, int> ScoutCrewmate = new();
    public static Dictionary<byte, int> ScoutNeutral = new();
    public static Dictionary<byte, int> StinkyAncestorKill = new();
    public static Dictionary<byte, long> NiceShieldsInProtect = new();
    public static List<byte> DeputyInProtect = new();
    public static List<byte> QSRInProtect = new();
    public static List<byte> TimeStopsstop = new();
    public static Dictionary<byte, Vector2> TimeMasterbacktrack = new();
    public static Dictionary<byte, long> GrenadiersInProtect = new();
    public static List<byte> ForGrenadiers = new();
    public static Dictionary<byte, float> AllPlayerLocation = new();
    public static List<byte> DemolitionManiacKill = new();
    public static Dictionary<byte, int> CrushMax = new();
    public static Dictionary<byte, int> SlaveownerMax = new();
    public static List<byte> ForSlaveowner = new();
    public static List<byte> ForSpellmaster = new();
    public static Dictionary<byte, int> SpellmasterMax = new();
    public static Dictionary<byte, int> SoulSeekerDead = new();
    public static Dictionary<byte, int> SoulSeekerCanKill = new();
    public static Dictionary<byte, int> SoulSeekerNotCanKill = new();
    public static Dictionary<byte, int> SoulSeekerCanEat = new();
    public static List<byte> KilledDiseased = new();
    public static Dictionary<byte, int> JealousyMax = new();
    public static List<byte> ForJealousy = new();
    public static List<byte> ForSourcePlague = new();
    public static Dictionary<byte, int> PBC = new();
    public static List<byte> ForET = new();
    public static List<byte> ForKing = new();
    public static List<byte> KingCanpc = new(); 
     public static List<byte> KingCanKill = new();
    public static List<byte> ForHotPotato = new();
    public static Dictionary<byte, int> MaxCowboy = new();
    public static List<byte> ForHotCold = new();
    public static List<byte> KillDeathGhost = new();
    public static List<byte> ForExorcist = new();
    public static Dictionary<byte, int> ExorcistMax = new();
    public static Dictionary<byte, long> InBoom = new();
    public static List<byte> ForDemolition = new();
    public static List<byte> ForDestinyChooser = new();
    public static List<byte> ForTasksDestinyChooser = new();
    public static List<byte> ForLostDeadDestinyChooser = new();
    public static Dictionary<byte, long> HemophobiaInKill = new();
    public static List<byte> ForHemophobia = new();
    public static Dictionary<byte, int> ManipulatorImpotors = new();
    public static Dictionary<byte, int> ManipulatorCrewmate = new();
    public static Dictionary<byte, int> ManipulatorNeutral = new();
    public static Dictionary<byte, string> ManipulatorNotify = new();
    public static List<byte> ForSpiritualizerCrewmate = new();
    public static List<byte> ForSpiritualizerImpostor = new();
    public static Dictionary<byte, int> GuideMax = new();
    public static List<byte> ForKnight = new();
    public static Dictionary<byte, long> ForNnurse = new();
    public static List<byte> NnurseHelep = new();
    public static Dictionary<byte, int> NnurseHelepMax = new(); 

    public static Dictionary<byte, CustomRoles> DevRole = new();
    public static Dictionary<byte, CustomRoles> AllPlayerCustomRoles = new();

    public static IEnumerable<PlayerControl> AllPlayerControls => PlayerControl.AllPlayerControls.ToArray().Where(p => p != null);
    public static IEnumerable<PlayerControl> AllAlivePlayerControls => PlayerControl.AllPlayerControls.ToArray().Where(p => p != null && p.IsAlive() && !p.Data.Disconnected && !Pelican.IsEaten(p.PlayerId));

    public static Main Instance;

    //一些很新的东东

    public static string OverrideWelcomeMsg = "";
    public static int HostClientId;

    public static List<string> TName_Snacks_CN = new() { "CoCo", "喜茶", "奈雪的茶", "蜜雪冰城", "霸王茶姬", "百乐", "斑马", "国誉", "kaco", "晨光", "TOHEX", "TOHE", "TONX", "TOHE-R", "TOH", "TOHEE", "喜", "N", "霸道清风", "BT小叨", "歌姬村民", "Bug世界", "八嘎呀路", "你好", "雪糕", "麦当劳", "肯德基", "汉堡", "蛋挞", "果冻", "果茶", "鲜百香双响奏", "派蒙", "胡桃", "银狼" };
    public static List<string> TName_Snacks_EN = new() { "Ice cream", "Milk tea", "Chocolate", "Cake", "Donut", "Coke", "Lemonade", "Candied haws", "Jelly", "Candy", "Milk", "Matcha", "Burning Grass Jelly", "Pineapple Bun", "Pudding", "Coconut Jelly", "Cookies", "Red Bean Toast", "Three Color Dumplings", "Wormwood Dumplings", "Puffs", "Can be Crepe", "Peach Crisp", "Mochi", "Egg Waffle", "Macaron", "Snow Plum Niang", "Fried Yogurt", "Egg Tart", "Muffin", "Sago Dew", "panna cotta", "soufflé", "croissant", "toffee" };
    public static string Get_TName_Snacks => TranslationController.Instance.currentLanguage.languageID is SupportedLangs.SChinese or SupportedLangs.TChinese ?
        TName_Snacks_CN[IRandom.Instance.Next(0, TName_Snacks_CN.Count)] :
        TName_Snacks_EN[IRandom.Instance.Next(0, TName_Snacks_EN.Count)];

    public override void Load()
    {
        Instance = this;

        //Client Options
        HideName = Config.Bind("Client Options", "Hide Game Code Name", "TOHE");
        HideColor = Config.Bind("Client Options", "Hide Game Code Color", $"{ModColor}");
        DebugKeyInput = Config.Bind("Authentication", "Debug Key", "");
        UnlockFPS = Config.Bind("Client Options", "UnlockFPS", false);
        AutoStart = Config.Bind("Client Options", "AutoStart", false);
        ForceOwnLanguage = Config.Bind("Client Options", "ForceOwnLanguage", false);
        ForceOwnLanguageRoleName = Config.Bind("Client Options", "ForceOwnLanguageRoleName", false);
        EnableCustomButton = Config.Bind("Client Options", "EnableCustomButton", true);
        EnableCustomSoundEffect = Config.Bind("Client Options", "EnableCustomSoundEffect", true);
        SwitchVanilla = Config.Bind("Client Options", "SwitchVanilla", false);
        QSM =  Config.Bind("Client Options", "QSM", false);
        //Devtx = Config.Bind("Client Options", "Devtx", false);
        //FastBoot = Config.Bind("Client Options", "FastBoot", false);
        VersionCheat = Config.Bind("Client Options", "VersionCheat", false);
        GodMode = Config.Bind("Client Options", "GodMode", false);
        CanPublic = Config.Bind("Client Options", "CanPublic", false);


        Logger = BepInEx.Logging.Logger.CreateLogSource("TOHEX");
        TOHE.Logger.Enable();
        TOHE.Logger.Disable("NotifyRoles");
        TOHE.Logger.Disable("SwitchSystem");
        TOHE.Logger.Disable("ModNews");
        if (!DebugModeManager.AmDebugger)
        {
            TOHE.Logger.Disable("2018k");
            //TOHE.Logger.Disable("Github");
            TOHE.Logger.Disable("CustomRpcSender");
            //TOHE.Logger.Disable("ReceiveRPC");
            TOHE.Logger.Disable("SendRPC");
            TOHE.Logger.Disable("SetRole");
            TOHE.Logger.Disable("Info.Role");
            TOHE.Logger.Disable("TaskState.Init");
            //TOHE.Logger.Disable("Vote");
            TOHE.Logger.Disable("RpcSetNamePrivate");
            //TOHE.Logger.Disable("SendChat");
            TOHE.Logger.Disable("SetName");
            //TOHE.Logger.Disable("AssignRoles");
            //TOHE.Logger.Disable("RepairSystem");
            //TOHE.Logger.Disable("MurderPlayer");
            //TOHE.Logger.Disable("CheckMurder");
            TOHE.Logger.Disable("PlayerControl.RpcSetRole");
            TOHE.Logger.Disable("SyncCustomSettings");
        }
        //TOHE.Logger.isDetail = true;

        // 認証関連-初期化
        DebugKeyAuth = new HashAuth(DebugKeyHash, DebugKeySalt);

        // 認証関連-認証
        DebugModeManager.Auth(DebugKeyAuth, DebugKeyInput.Value);

        Preset1 = Config.Bind("Preset Name Options", "Preset1", "Preset_1");
        Preset2 = Config.Bind("Preset Name Options", "Preset2", "Preset_2");
        Preset3 = Config.Bind("Preset Name Options", "Preset3", "Preset_3");
        Preset4 = Config.Bind("Preset Name Options", "Preset4", "Preset_4");
        Preset5 = Config.Bind("Preset Name Options", "Preset5", "Preset_5");
        WebhookURL = Config.Bind("Other", "WebhookURL", "none");
        BetaBuildURL = Config.Bind("Other", "BetaBuildURL", "");
        MessageWait = Config.Bind("Other", "MessageWait", 1);
        LastKillCooldown = Config.Bind("Other", "LastKillCooldown", (float)30);
        LastShapeshifterCooldown = Config.Bind("Other", "LastShapeshifterCooldown", (float)30);

        hasArgumentException = false;
        ExceptionMessage = "";
        try
        {
            roleColors = new Dictionary<CustomRoles, string>()
            {
                //原版职业颜色
                {CustomRoles.Crewmate, "#ffffff"},
                {CustomRoles.Engineer, "#8cffff"},
                {CustomRoles.Scientist, "#8cffff"},
                {CustomRoles.GuardianAngel, "#ffffff"},
                //伪装者颜色设置
                {CustomRoles.Disorder, "#666699" },
                {CustomRoles.ImpostorSchrodingerCat,"#ff1919" },
                //船员颜色设置
                {CustomRoles.Luckey, "#b8d7a3" },
                {CustomRoles.Needy, "#a4dffe"},
                {CustomRoles.SabotageMaster, "#3333ff"},
                {CustomRoles.Snitch, "#b8fb4f"},
                {CustomRoles.Mayor, "#204d42"},
                {CustomRoles.Paranoia, "#c993f5"},
                {CustomRoles.Psychic, "#6F698C"},
                {CustomRoles.Sheriff, "#f8cd46"},
                {CustomRoles.SuperStar, "#f6f657"},
                {CustomRoles.CyberStar, "#ee4a55" },
                {CustomRoles.SpeedBooster, "#00ffff"},
                {CustomRoles.Doctor, "#80ffdd"},
                {CustomRoles.Dictator, "#df9b00"},
                {CustomRoles.Detective, "#663333" },
                {CustomRoles.SwordsMan, "#f0e68c"},
                {CustomRoles.NiceGuesser, "#eede26"},
                {CustomRoles.Transporter, "#42D1FF"},
                {CustomRoles.TimeManager, "#6495ed"},
                {CustomRoles.Veteran, "#a77738"},
                {CustomRoles.Bodyguard, "#185abd"},
                {CustomRoles.Counterfeiter, "#e0e0e0"},
                {CustomRoles.Grenadier, "#3c4a16"},
                {CustomRoles.Medicaler, "#00a4ff"},
                {CustomRoles.Divinator, "#882c83"},
                {CustomRoles.Glitch, "#dcdcdc"},
                {CustomRoles.Judge, "#f8d85a"},
                {CustomRoles.Mortician, "#333c49"},
                {CustomRoles.Mediumshiper, "#7a8c92"},
                {CustomRoles.Observer, "#a8e0fa"},
                {CustomRoles.DovesOfNeace, "#FFFFFF" },
                {CustomRoles.LostCrew, "#666666"},
                {CustomRoles.Rudepeople, "#66CC00"},
                {CustomRoles.HatarakiMan,"#6A5ACD" },
                //{CustomRoles.Hypnotist,"#6495ED" },
                {CustomRoles.XiaoMu,"#FFA500" },
                {CustomRoles.Indomitable,"#808000" },
                {CustomRoles.BadLuck,"#666666" },
                {CustomRoles.Prophet,"#FFCC99" },
                {CustomRoles.Scout,"#6666CC" },
                {CustomRoles.NiceShields,"#009999" },
                {CustomRoles.Deputy,"#f8cd46" },
                {CustomRoles.DemonHunterm,"#9400D3" },
                {CustomRoles.TimeStops,"#0372FB" },
                {CustomRoles.TimeMaster,"#44baff" },
                {CustomRoles.SuperPowers,"#FF6600" },
                {CustomRoles.ET,"#cd44c8" },
                {CustomRoles.CrewSchrodingerCat,"#ffffff" },
                {CustomRoles.GlennQuagmire,"#FF3333" },
                {CustomRoles.EIReverso,"#663366" },
                {CustomRoles.SoulSeeker,"#006666" },
                {CustomRoles.Wronged,"#999999" },
                {CustomRoles.Captain,"#0089ff" },
                {CustomRoles.Solicited, "#0089ff" },
                {CustomRoles.Undercover,"#ff1919" },
                {CustomRoles.Mascot,"#00ff66" },
                {CustomRoles.Cowboy,"#663333" },
                {CustomRoles.BSR,"#00ff55" },
                {CustomRoles.ElectOfficials,"#99CCCC" },
                {CustomRoles.SpeedUp,"#669966" },
                {CustomRoles.ChiefOfPolice,"#f8cd46" },
                {CustomRoles.Fugitive,"#FF9900" },
                {CustomRoles.Manipulator,"#FF3300" },
                {CustomRoles.NiceTracker, "#00cb80" },
                {CustomRoles.Spiritualizer,"#00CCFF"},
                {CustomRoles.Knight,"#CC6600" },
                {CustomRoles.Nurse,"#99CCFF" },
                //中立职业颜色设置
                {CustomRoles.Arsonist, "#ff6633"},
                {CustomRoles.Jester, "#ec62a5"},
                {CustomRoles.Terrorist, "#00e600"},
                {CustomRoles.Executioner, "#611c3a"},
                {CustomRoles.God, "#f96464"},
                {CustomRoles.Opportunist, "#4dff4d"},
                {CustomRoles.Mario, "#ff6201"},
                {CustomRoles.Jackal, "#00b4eb"},
                {CustomRoles.Innocent, "#8f815e"},
                {CustomRoles.Pelican, "#34c84b"},
                {CustomRoles.Revolutionist, "#ba4d06"},
                {CustomRoles.FFF, "#414b66"},
                {CustomRoles.Konan, "#4d4dff"},
                {CustomRoles.Gamer, "#68bc71"},
                {CustomRoles.DarkHide, "#483d8b"},
                {CustomRoles.Workaholic, "#008b8b"},
                {CustomRoles.Collector, "#9d8892"},
                {CustomRoles.Provocateur, "#74ba43"},
                {CustomRoles.Sunnyboy, "#ff9902"},
                {CustomRoles.BloodKnight, "#630000"},
                {CustomRoles.Totocalcio, "#ff9409"},
                {CustomRoles.Succubus, "#ff00ff"},
                {CustomRoles.OpportunistKiller, "#CC6600"},
                {CustomRoles.Vulture, "#663333" },
                {CustomRoles.MengJiangGirl,"#778899" },
                {CustomRoles.Bull ,"#339900" },
                {CustomRoles.Masochism ,"#663366" },
                {CustomRoles.Dissenter ,"#996633" },
                {CustomRoles.FreeMan, "#f6f657" },
                {CustomRoles.StinkyAncestor, "#996600" },
                {CustomRoles.YinLang,"#6A5ACD" },
                {CustomRoles.Amnesiac,"#87CEFA" },
                {CustomRoles.Swapper,"#565656" },
                {CustomRoles.Lawyer, "#949e52"},
                {CustomRoles.QSR, "#949e52"},
                {CustomRoles.Whoops,"#00b4eb" },
                {CustomRoles.Sidekick,"#00b4eb"},
                {CustomRoles.SchrodingerCat,"#666666" },
                {CustomRoles.GamerSchrodingerCat,"#68bc71" },
                {CustomRoles.BloodSchrodingerCat,"#630000" },
                {CustomRoles.JSchrodingerCat,"#00b4eb" },
                {CustomRoles.YLSchrodingerCat,"#6A5ACD" },
                {CustomRoles.PGSchrodingerCat, "#101010"},
                {CustomRoles.DHSchrodingerCat, "#483d8b"},
                {CustomRoles.Crush,"#ff9ace" },
                {CustomRoles.Slaveowner,"#996600" },
                {CustomRoles.Jealousy, "#996666"},
                {CustomRoles.OKSchrodingerCat, "#CC6600"},
                {CustomRoles.SourcePlague, "#CCCC33"},
                  {CustomRoles.PlaguesGod, "#101010"},
                {CustomRoles.King,"#FFCC00" },
                {CustomRoles.Exorcist,"#336666" },
                //管理员
                {CustomRoles.GM, "#ff5b70"},
                //附加职业颜色设置
                {CustomRoles.NotAssigned, "#ffffff"},
                {CustomRoles.LastImpostor, "#ff1919"},
                {CustomRoles.Lovers, "#ff9ace"},
                {CustomRoles.Ntr, "#00a4ff"},
                {CustomRoles.Madmate, "#ff1919"},
                {CustomRoles.Watcher, "#800080"},
                {CustomRoles.Flashman, "#ff8400"},
                {CustomRoles.Lighter, "#eee5be"},
                {CustomRoles.Seer, "#61b26c"},
                {CustomRoles.Brakar, "#1447af"},
                {CustomRoles.Oblivious, "#424242"},
                {CustomRoles.Bewilder, "#c894f5"},
                {CustomRoles.Workhorse, "#00ffff"},
                {CustomRoles.Fool, "#e6e7ff"},
                {CustomRoles.Avanger, "#ffab1b"},
                {CustomRoles.Youtuber, "#fb749b"},
                {CustomRoles.Egoist, "#5600ff"},
                {CustomRoles.TicketsStealer, "#ff1919"},
                {CustomRoles.DualPersonality, "#3a648f"},
                {CustomRoles.Mimic, "#ff1919"},
                {CustomRoles.Reach, "#74ba43"},
                {CustomRoles.Charmed, "#ff00ff"},
                {CustomRoles.Bait, "#00f7ff"},
                {CustomRoles.Trapper, "#5a8fd0"},
                {CustomRoles.Bitch, "#333333"},
                {CustomRoles.Rambler, "#99CCFF"},
                {CustomRoles.Destroyers, "#ff1919"},
                {CustomRoles.Fategiver,"#FFB6C1" },
                {CustomRoles.Wanderers,"#3399FF" },
                {CustomRoles.Attendant,"#00b4eb" },
                {CustomRoles.LostSouls,"#993399" },
                {CustomRoles.OldImpostor, "#ff1919" },
                {CustomRoles.Executor,"#CCCC00" },
                {CustomRoles.OldThousand,"#f6f657" },
                {CustomRoles.involution,"#708090"},
                {CustomRoles.Diseased, "#AAAAAA"},
                {CustomRoles.seniormanagement, "#0089ff"},
                {CustomRoles.Believer, "#000000"},
                {CustomRoles.DeathGhost,"#B22222" },
                {CustomRoles.Energizer,"#9900FF" },
                {CustomRoles.Originator,"#CC9999" },
                {CustomRoles.QL,"#66ff5c" },
                //SoloKombat
                {CustomRoles.KB_Normal, "#f55252"},
                //烫手的山芋
                {CustomRoles.Hotpotato, "#ffa300" },
                {CustomRoles.Coldpotato, "#00efff" },
                //抓捕
                {CustomRoles.captor,"#DC143C" },
                {CustomRoles.runagat,"#0000FF" },
            };
            foreach (var role in Enum.GetValues(typeof(CustomRoles)).Cast<CustomRoles>())
            {
                switch (role.GetCustomRoleTypes())
                {
                    case CustomRoleTypes.Impostor:
                        roleColors.TryAdd(role, "#ff1919");
                        break;
                    default:
                        break;
                }
            }
        }
        catch (ArgumentException ex)
        {
            TOHE.Logger.Error("错误：字典出现重复项", "LoadDictionary");
            TOHE.Logger.Exception(ex, "LoadDictionary");
            hasArgumentException = true;
            ExceptionMessage = ex.Message;
            ExceptionMessageIsShown = false;
        }

        CustomWinnerHolder.Reset();
        ServerAddManager.Init();
        Translator.Init();
        BanManager.Init();
        TemplateManager.Init();
        SpamManager.Init();
        DevManager.Init();
        Cloud.Init();

        IRandom.SetInstance(new NetRandomWrapper());

        TOHE.Logger.Info($"{Application.version}", "AmongUs Version");

        var handler = TOHE.Logger.Handler("GitVersion");
        handler.Info($"{nameof(ThisAssembly.Git.BaseTag)}: {ThisAssembly.Git.BaseTag}");
        handler.Info($"{nameof(ThisAssembly.Git.Commit)}: {ThisAssembly.Git.Commit}");
        handler.Info($"{nameof(ThisAssembly.Git.Commits)}: {ThisAssembly.Git.Commits}");
        handler.Info($"{nameof(ThisAssembly.Git.IsDirty)}: {ThisAssembly.Git.IsDirty}");
        handler.Info($"{nameof(ThisAssembly.Git.Sha)}: {ThisAssembly.Git.Sha}");
        handler.Info($"{nameof(ThisAssembly.Git.Tag)}: {ThisAssembly.Git.Tag}");

        ClassInjector.RegisterTypeInIl2Cpp<ErrorText>();

        Harmony.PatchAll();

        if (!DebugModeManager.AmDebugger) ConsoleManager.DetachConsole();
        else ConsoleManager.CreateConsole();

        TOHE.Logger.Msg("========= TOHE loaded! =========", "Plugin Load");
    }
}
public enum CustomRoles
{
    //Default
    Crewmate = 0,
    //伪装者（原版)
    Impostor,
    Shapeshifter,
    //伪装者
    BountyHunter,
    FireWorks,
    Mafia,
    SerialKiller,
    ShapeMaster,
    EvilGuesser,
    Minimalism,
    Zombie,
    Sniper,
    Vampire,
    Witch,
    Warlock,
    Assassin,
    Hacker,
    Miner,
    Escapee,
    Mare,
    Puppeteer,
    TimeThief,
    EvilTracker,
    AntiAdminer,
    Sans,
    Bomber,
    BoobyTrap,
    Scavenger,
    Capitalism,
    Gangster,
    Cleaner,
    BallLightning,
    Greedier,
    CursedWolf,
    ImperiusCurse,
    QuickShooter,
    Concealer,
    Eraser,
    OverKiller,
    Hangman,
    Bard,
    Swooper,
    Crewpostor,
    Depressed,
    SpecialAgent,
    EvilGambler,
    Fraudster,
    Cultivator,
    Disorder,
    sabcat,
    Grenadiers,
    Vandalism,
    Followers,
    DemolitionManiac,
    ImpostorSchrodingerCat,
    Spellmaster,
    DestinyChooser,
    Hemophobia,
    Guide,
    Anglers,
    //船员（原版）
    Engineer,
    GuardianAngel,
    Scientist,
    //船员
    Luckey,
    Needy,
    SuperStar,
    CyberStar,
    Mayor,
    Paranoia,
    Psychic,
    SabotageMaster,
    Sheriff,
    Snitch,
    SpeedBooster,
    Dictator,
    Doctor,
    //Detective,
    SwordsMan,
    NiceGuesser,
    Transporter,
    TimeManager,
    Veteran,
    Bodyguard,
    Counterfeiter,
    Grenadier,
    Medicaler,
    Divinator,
    Glitch,
    Judge,
    Mortician,
    Mediumshiper,
    Observer,
    DovesOfNeace,
    LostCrew,
    Rudepeople,
    HatarakiMan,
    TimeStops,
    TimeMaster,
    SuperPowers,
    ET,
    CrewSchrodingerCat,
    GlennQuagmire,
    Wronged,
    //Hypnotist,
    XiaoMu,
    Indomitable,
    BadLuck,
    Prophet,
    Scout,
    NiceShields,
    Deputy,
    DemonHunterm,
    EIReverso,
    SoulSeeker,
    Captain,
    Solicited,
    Undercover,
    Mascot,
    Cowboy,
    BSR,
    ElectOfficials,
    SpeedUp,
    ChiefOfPolice,
    Fugitive,
    Manipulator,
    NiceTracker,
    Spiritualizer,
    Knight,
    Nurse,
    //中立
    Arsonist,
    Jester,
    God,
    Opportunist,
    Mario,
    Terrorist,
    Executioner,
    Jackal,
    Innocent,
    Pelican,
    Revolutionist,
    FFF,
    Konan,
    Gamer,
    DarkHide,
    Workaholic,
    Collector,
    Provocateur,
    Sunnyboy,
    BloodKnight,
    Totocalcio,
    Succubus,
    OpportunistKiller,
    Vulture,
    MengJiangGirl,
    Bull,
    Masochism,
    FreeMan,
    Dissenter,
    StinkyAncestor,
    YinLang,
    Swapper,
    Lawyer,
    Whoops,
    Sidekick,
    SchrodingerCat,
    GamerSchrodingerCat,
    BloodSchrodingerCat,
    JSchrodingerCat,
    YLSchrodingerCat,
    PGSchrodingerCat,
    DHSchrodingerCat,
    OKSchrodingerCat,
    Crush,
    Slaveowner,
    QSR,
    Jealousy,
    SourcePlague,
    PlaguesGod,
    King,
    Amnesiac,
    Exorcist,

    //SoloKombat
    KB_Normal,

    //烫手的山芋
    Hotpotato,
    Coldpotato,

    //抓捕
    captor,
    runagat,

    //管理员
    GM,

    //附加职业 after 500
    NotAssigned = 500,
    LastImpostor,
    Lovers,
    Ntr,
    Madmate,
    Watcher,
    Flashman,
    Lighter,
    Seer,
    Brakar,
    Oblivious,
    Bewilder,
    Workhorse,
    Fool,
    Avanger,
    Youtuber,
    Egoist,
    TicketsStealer,
    DualPersonality,
    Mimic,
    Reach,
    Charmed,
    Bait,
    Trapper,
    Bitch,
    Rambler,
    Destroyers,
    UnluckyEggs,
    Fategiver,
    Wanderers,
    Attendant,
    LostSouls,
    Detective,
    OldImpostor,
    Executor,
    OldThousand,
    involution,//一个小小的船附
    Diseased,
    seniormanagement,
    Believer,
    DeathGhost,
    Energizer,
    Originator,
    QL,
}
//胜利设置
public enum CustomWinner
{
    //夺取胜利或者独自胜利
    Draw = -1,
    Default = -2,
    None = -3,
    Error = -4,
    Impostor = CustomRoles.Impostor,
    Crewmate = CustomRoles.Crewmate,
    Jester = CustomRoles.Jester,
    Terrorist = CustomRoles.Terrorist,
    Lovers = CustomRoles.Lovers,
    Executioner = CustomRoles.Executioner,
    Arsonist = CustomRoles.Arsonist,
    Revolutionist = CustomRoles.Revolutionist,
    Jackal = CustomRoles.Jackal,
    God = CustomRoles.God,
    Mario = CustomRoles.Mario,
    Innocent = CustomRoles.Innocent,
    Pelican = CustomRoles.Pelican,
    Youtuber = CustomRoles.Youtuber,
    Egoist = CustomRoles.Egoist,
    Gamer = CustomRoles.Gamer,
    DarkHide = CustomRoles.DarkHide,
    Workaholic = CustomRoles.Workaholic,
    Collector = CustomRoles.Collector,
    BloodKnight = CustomRoles.BloodKnight,
    Succubus = CustomRoles.Succubus,
    Vulture = CustomRoles.Vulture,
    MengJiangGirl = CustomRoles.MengJiangGirl,
    Bull = CustomRoles.Bull,
    Masochism = CustomRoles.Masochism,
    Dissenter = CustomRoles.Dissenter,
    StinkyAncestor = CustomRoles.StinkyAncestor,
    YinLang = CustomRoles.YinLang,
    Jealousy = CustomRoles.Jealousy,
    PlaguesGod = CustomRoles.PlaguesGod,
    King = CustomRoles.King,
    CP = CustomRoles.Coldpotato,
    captor = CustomRoles.captor,
    runagat = CustomRoles.runagat,
        Exorcist = CustomRoles.Exorcist
}
public enum AdditionalWinners
{
    //跟随胜利
    None = -1,
    Lovers = CustomRoles.Lovers,
    Opportunist = CustomRoles.Opportunist,
    Executioner = CustomRoles.Executioner,
    FFF = CustomRoles.FFF,
    Provocateur = CustomRoles.Provocateur,
    Sunnyboy = CustomRoles.Sunnyboy,
    Totocalcio = CustomRoles.Totocalcio,
    OpportunistKiller = CustomRoles.OpportunistKiller,
    FreeMan = CustomRoles.FreeMan,
    Lawyer = CustomRoles.Lawyer,
    Slaveowner = CustomRoles.Slaveowner, 
    QSR = CustomRoles.QSR,
    OKC = CustomRoles.OKSchrodingerCat,
    DHCat = CustomRoles.DHSchrodingerCat
}
public enum SuffixModes
{
    None = 0,
    TOHE,
    Streaming,
    Recording,
    RoomHost,
    OriginalName,
    DoNotKillMe,
    NoAndroidPlz,
    Test
}
public enum VoteMode
{
    Default,
    Suicide,
    SelfVote,
    Skip
}
public enum TieMode
{
    Default,
    All,
    Random
}