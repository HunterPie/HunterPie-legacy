using DiscordRPC;
using System;
using HunterPie.Memory;
using HunterPie.Logger;

namespace HunterPie.Core {
    class Presence {
        private string APP_ID = "567152028070051859";
        private bool isOffline = false;
        private bool isVisible = true;
        private RichPresence Instance = new RichPresence();
        public DiscordRpcClient Client;
        public Game ctx;

        /* Constructor and base functions */

        public Presence(Game context) {
            ctx = context;
            HookEvents();
        }

        public void SetOfflineMode() {
            isOffline = true;
        }

        /* Event handlers */

        private void HookEvents() {
            UserSettings.OnSettingsUpdate += HandleSettings;
            // Process
            Scanner.OnGameStart += StartRPC;
            Scanner.OnGameClosed += CloseRPCConnection;
            // Game context
            ctx.OnClockChange += HandlePresence;
            ctx.Player.OnZoneChange += HandlePresence;
        }
        
            /* Connection */

        public void StartRPC(object source, EventArgs e) {
            if (isOffline) return;
            // Check if connection exists to avoid creating multiple connections
            if (Client == null || Client.IsDisposed) {
                Debugger.Discord("Starting new RPC connection");
                Client = new DiscordRpcClient(APP_ID);
                Client.Initialize();
            }
        }

        public void CloseRPCConnection(object source, EventArgs e) {
            Debugger.Discord("Closed connection");
            Client.ClearPresence();
            Client.Dispose();
        }

        public void HandleSettings(object source, EventArgs e) {
            if (UserSettings.PlayerConfig.RichPresence.Enabled && !isVisible) {
                isVisible = true;
            } else if (!UserSettings.PlayerConfig.RichPresence.Enabled && isVisible) {
                try {
                    Client.ClearPresence();
                } catch {}
                isVisible = false;
            }
        }

        public void HandlePresence(object source, EventArgs e) {
            // Do nothing if RPC is disabled
            if (!isVisible) return;

            // Only update RPC if player isn't in loading screen
            switch(ctx.Player.ZoneID) {
                case 0:
                    Instance.Details = ctx.Player.PlayerAddress == 0 ? "In main menu" : "In loading screen";
                    Instance.State = null;
                    Instance.Assets = GenerateAssets("main-menu", null, null, null);
                    Instance.Party = null;
                    break;
                default:
                    if (ctx.Player.PlayerAddress == 0) {
                        Instance.Details = "In main menu";
                        Instance.State = null;
                        Instance.Assets = GenerateAssets("main-menu", null, null, null);
                        Instance.Party = null;
                        break;
                    }
                    Instance.Details = ctx.HuntedMonster == null ? ctx.Player.inPeaceZone ? "Idle" : "Exploring" : $"Hunting {ctx.HuntedMonster.Name} ({(int)(ctx.HuntedMonster.HPPercentage * 100)}%)";
                    Instance.State = ctx.Player.PartySize > 1 ? "In Party" : "Solo";
                    Instance.Assets = GenerateAssets(ctx.Player.ZoneName == null ? "main-menu" : $"st{ctx.Player.ZoneID}", ctx.Player.ZoneName == "Main Menu" ? null : ctx.Player.ZoneName, ctx.Player.WeaponName == null ? "hunter-rank" : ctx.Player.WeaponName.Replace(' ', '-').ToLower(), $"{ctx.Player.Name} | HR: {ctx.Player.Level} | MR: {ctx.Player.MasterRank}");
                    // TODO: Generate party hash
                    Instance.Party = MakeParty(ctx.Player.PartySize, ctx.Player.PartyMax, "test");
                    break;
            }
            Client.SetPresence(Instance);
        }

        /* Helpers */

        public void InitializePresence() {
            Client = new DiscordRpcClient(APP_ID);
            Client.Initialize();
            Debugger.Discord("Connecting to Discord...");
        }

        public Assets GenerateAssets(string largeImage, string largeImageText, string smallImage, string smallImageText) {
            Assets assets = new Assets();
            assets.LargeImageKey = largeImage;
            assets.LargeImageText = largeImageText;
            assets.SmallImageKey = smallImage;
            assets.SmallImageText = smallImageText;
            return assets;
        }

        public Party MakeParty(int partySize, int maxParty, string partyHash) {
            Party party = new Party();
            party.Size = partySize;
            party.Max = maxParty;
            party.ID = partyHash;
            return party;
        }

        public Timestamps NewTimestamp(DateTime start) {
            Timestamps timestamp = new Timestamps();
            try {
                timestamp.Start = start;
            } catch {
                timestamp.Start = null;
            }
            return timestamp;
        }

    }
}
