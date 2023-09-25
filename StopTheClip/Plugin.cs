using Dalamud.Game;
using Dalamud.Game.Gui;
using Dalamud.Game.Command;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System;
using System.Text.RegularExpressions;

using StopTheClip.Structures;
using MemoryManager.Structures;
using Dalamud.Logging;

namespace StopTheClip
{
    public unsafe class StopTheClip : IDalamudPlugin
    {
        [PluginService] public static DalamudPluginInterface? PluginInterface { get; private set; }
        [PluginService] public static Framework? Framework { get; private set; }
        [PluginService] public static CommandManager? CommandManager { get; private set; }
        [PluginService] public static ClientState? ClientState { get; private set; }
        [PluginService] public static PartyList? PartyList { get; private set; }
        [PluginService] public static SigScanner? SigScanner { get; private set; }
        [PluginService] public static ChatGui? ChatGui { get; private set; }
        [PluginService] public static Condition? Condition { get; private set; }

        public StopTheClip Plugin { get; init; }
        public static SharedMemoryManager smm = new SharedMemoryManager();

        public string Name => "StopTheClip";
        private const string CommandName = "/stoptheclip";

        private TargetSystem* targetSystem = TargetSystem.Instance();
        private ControlSystemCameraManager* csCameraManager = null;
        private HookManager hookManager = new HookManager();

        private bool isEnabled = false;
        private bool inCutscene = false;
        private bool isXIVRActive = false;
        private bool origEnabled = false;

        public StopTheClip()
        {
            Plugin = this;

            CommandManager!.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "[enable|disable]"
            });

            ClientState!.Login += OnLogin;
            ClientState!.Logout += OnLogout;
            Framework!.Update += Update;
            PluginInterface!.UiBuilder.Draw += DrawUI;
            PluginInterface!.UiBuilder.OpenConfigUi += ToggleUI;

            Initialize();
            Start();
        }

        public void Dispose()
        {
            Stop();
            smm.SetClose(SharedMemoryPlugins.StopTheClip);
            smm.Dispose();
            //smm.OutputStatus();

            ClientState!.Login -= OnLogin;
            ClientState!.Logout -= OnLogout;
            Framework!.Update -= Update;
            PluginInterface!.UiBuilder.Draw -= DrawUI;
            PluginInterface!.UiBuilder.OpenConfigUi -= ToggleUI;

            CommandManager!.RemoveHandler(CommandName);
        }

        public void ToggleUI() => PluginUI.isVisible ^= true;
        private void OnCommand(string command, string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                ToggleUI();
                return;
            }
            var regex = Regex.Match(argument, "^(\\w+) ?(.*)");
            var subcommand = regex.Success && regex.Groups.Count > 1 ? regex.Groups[1].Value : string.Empty;

            switch (subcommand.ToLower())
            {
                case "enable":
                    {
                        Start();
                        break;
                    }
                case "disable":
                    {
                        Stop();
                        break;
                    }
            }

        }

        private void DrawUI()
        {
            bool curEnabled = isEnabled;
            PluginUI.Draw(ref curEnabled);
            if (curEnabled != isEnabled)
                Toggle();
        }

        private void OnLogin(object? sender, EventArgs e)
        {
            if(isEnabled)
                ClipManagerSetNearClip();

        }
        private void OnLogout(object? sender, EventArgs e)
        {
            //----
            // Sets the lengths of the TargetSystem to 0 as they keep their size
            // even though the data is reset
            //----
            targetSystem->ObjectFilterArray0.Length = 0;
            targetSystem->ObjectFilterArray1.Length = 0;
            targetSystem->ObjectFilterArray2.Length = 0;
            targetSystem->ObjectFilterArray3.Length = 0;

            if (isEnabled)
                ClipManagerResetNearClip();
        }

        public static void PrintEcho(string message) => ChatGui!.Print($"[StopTheClip] {message}");
        public static void PrintError(string message) => ChatGui!.PrintError($"[StopTheClip] {message}");

        private void Update(Framework framework)
        {
            //----
            // if xivr is open check to see if its active
            // if so save the current active value and disable it
            //----
            if (smm.CheckOpen(SharedMemoryPlugins.XIVR))
            {
                bool xivrActive = smm.CheckActive(SharedMemoryPlugins.XIVR);
                if(isXIVRActive != xivrActive)
                {
                    if(xivrActive)
                    {
                        origEnabled = isEnabled;
                        if (isEnabled)
                            Stop();
                    }
                    else
                    {
                        isEnabled = origEnabled;
                        if (isEnabled)
                            Start();
                    }
                }
            }

            if (isEnabled)
                inCutscene = Condition![ConditionFlag.OccupiedInCutSceneEvent] || Condition![ConditionFlag.WatchingCutscene] || Condition![ConditionFlag.WatchingCutscene78];
        }
        

        private void ClipManagerSetNearClip()
        {
            //----
            // Set the near clip
            //----
            if (csCameraManager != null && csCameraManager->ActiveCameraIndex == 0 && csCameraManager->GameCamera != null)
                csCameraManager->GameCamera->Camera.BufferData->NearClip = 0.05f;
        }

        private void ClipManagerResetNearClip()
        {
            //----
            // Set the near clip
            //----
            if (csCameraManager != null && csCameraManager->ActiveCameraIndex == 0 && csCameraManager->GameCamera != null)
                csCameraManager->GameCamera->Camera.BufferData->NearClip = 0.1f;
        }



        private static class Signatures
        {
            internal const string g_ControlSystemCameraManager = "48 8D 0D ?? ?? ?? ?? F3 0F 10 4B ??";
            internal const string RunGameTasks = "E8 ?? ?? ?? ?? 48 8B 8B B8 35 00 00";
        }

        private void Initialize()
        {
            SignatureHelper.Initialise(this);
            hookManager.SetFunctionHandles(this);
            smm.SetOpen(SharedMemoryPlugins.StopTheClip);
            csCameraManager = (ControlSystemCameraManager*)SigScanner!.GetStaticAddressFromSig(Signatures.g_ControlSystemCameraManager);
        }

        public void Start()
        {
            hookManager.EnableFunctionHandles();
            smm.SetActive(SharedMemoryPlugins.StopTheClip);
            ClipManagerSetNearClip();
            isEnabled = true;
            //PrintEcho("Clipping Enabled");
            //PluginLog.Log($"STC Stop: {isEnabled} {origEnabled}");
            //smm.OutputStatus();
        }

        public void Stop()
        {
            //PrintEcho("Clipping Disabled");
            isEnabled = false;
            ClipManagerResetNearClip();
            smm.SetInactive(SharedMemoryPlugins.StopTheClip);
            hookManager.DisableFunctionHandles();
            //PluginLog.Log($"STC Stop: {isEnabled} {origEnabled}");
            //smm.OutputStatus();
        }

        public void Toggle()
        {
            if (isEnabled)
                Stop();
            else
                Start();
        }







        //----
        // RunGameTasks
        //----
        private delegate void RunGameTasksDg(UInt64 a, float* frameTiming);
        [Signature(Signatures.RunGameTasks, DetourName = nameof(RunGameTasksFn))]
        private Hook<RunGameTasksDg>? RunGameTasksHook = null;

        [HandleStatus("RunGameTasks")]
        public void RunGameTasksStatus(bool status)
        {
            if (status == true)
                RunGameTasksHook?.Enable();
            else
                RunGameTasksHook?.Disable();
        }

        public unsafe void RunGameTasksFn(UInt64 a, float* frameTiming)
        {
            if (isEnabled)
            { 
                for (int i = 0; i < 40; i++)
                {
                    if (i == 18)
                        CheckVisibility();

                    GameTask* task = (GameTask*)((UInt64)(i * 0x78) + *(UInt64*)(a + 0x58)); 
                    task->vf1(frameTiming);
                }
            }
            else
                RunGameTasksHook!.Original(a, frameTiming);
        }


        private void CheckVisibilityInner(Character* character)
        {
            if (character == null)
                return;

            if ((ObjectKind)character->GameObject.ObjectKind == ObjectKind.Pc ||
                (ObjectKind)character->GameObject.ObjectKind == ObjectKind.BattleNpc ||
                (ObjectKind)character->GameObject.ObjectKind == ObjectKind.EventNpc ||
                (ObjectKind)character->GameObject.ObjectKind == ObjectKind.Mount ||
                (ObjectKind)character->GameObject.ObjectKind == ObjectKind.Companion ||
                (ObjectKind)character->GameObject.ObjectKind == ObjectKind.Retainer)
            {
                Structures.Model* model = (Structures.Model*)character->GameObject.DrawObject;
                if (model == null)
                    return;

                if (model->CullType == ModelCullTypes.InsideCamera && (byte)character->GameObject.TargetableStatus == 255)
                    model->CullType = ModelCullTypes.Visible;

                DrawDataContainer* drawData = &character->DrawData;
                if (drawData != null && !drawData->IsWeaponHidden)
                {
                    UInt64 mhOffset = (UInt64)(&drawData->MainHand);
                    if (mhOffset != 0)
                    {
                        Structures.Model* mhWeap = *(Structures.Model**)(mhOffset + 0x8);
                        if (mhWeap != null)
                            mhWeap->CullType = ModelCullTypes.Visible;
                    }

                    UInt64 ohOffset = (UInt64)(&drawData->OffHand);
                    if (ohOffset != 0)
                    {
                        Structures.Model* ohWeap = *(Structures.Model**)(ohOffset + 0x8);
                        if (ohWeap != null)
                            ohWeap->CullType = ModelCullTypes.Visible;
                    }

                    UInt64 fOffset = (UInt64)(&drawData->UnkF0);
                    if (fOffset != 0)
                    {
                        Structures.Model* fWeap = *(Structures.Model**)(fOffset + 0x8);
                        if (fWeap != null)
                            fWeap->CullType = ModelCullTypes.Visible;
                    }
                }

                Structures.Model* mount = (Structures.Model*)model->mountedObject;
                if (mount != null)
                    mount->CullType = ModelCullTypes.Visible;

                Character.OrnamentContainer* oCont = &character->Ornament;
                if (oCont != null)
                {
                    GameObject* bonedOrnament = (GameObject*)oCont->OrnamentObject;
                    if (bonedOrnament != null)
                    {
                        Structures.Model* ornament = (Structures.Model*)bonedOrnament->DrawObject;
                        if (ornament != null)
                            ornament->CullType = ModelCullTypes.Visible;
                    }
                }
            }
        }
        private void CheckVisibility()
        {
            PlayerCharacter? player = ClientState!.LocalPlayer;
            if (player == null)
                return;

            if (inCutscene)
                return;

            //----
            // Check the player
            //----
            Character* character = (Character*)player.Address;
            if (character != null && character != targetSystem->ObjectFilterArray0[0])
                CheckVisibilityInner(character);

            //----
            // Check anyone in your party (used for multi seater mounts)
            //----
            for (int i = 0; i < PartyList!.Length; i++)
            {
                Dalamud.Game.ClientState.Objects.Types.GameObject partyMember = PartyList[i]!.GameObject!;
                if (partyMember != null)
                {
                    Character* partyCharacter = (Character*)partyMember.Address;
                    if (character != null)
                        CheckVisibilityInner(partyCharacter);
                }
            }

            //----
            // Check anyone in sight
            //----
            for (int i = 0; i < targetSystem->ObjectFilterArray1.Length; i++)
                CheckVisibilityInner((Character*)targetSystem->ObjectFilterArray1[i]);
        }
    }
}
