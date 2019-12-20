using DiscordRPC;
using HunterPie;
using System;

namespace HunterPie.Core {
    class Presence {
        private string APP_ID = "567152028070051859";
        private bool isVisible = true;
        private RichPresence Instance = new RichPresence();
        public DiscordRpcClient Client;
        public Game ctx;

        public Presence(Game context) {
            ctx = context;
            HookEvents();
        }

        private void HookEvents() {
            UserSettings.OnSettingsUpdate += HandleSettings;
            ctx.OnClockChange += HandlePresence;
            ctx.Player.OnZoneChange += HandlePresence;
        }
 
        public void HandleSettings(object source, EventArgs e) {
            if (UserSettings.PlayerConfig.RichPresence.Enabled && !isVisible) {
                isVisible = true;
            } else if (!UserSettings.PlayerConfig.RichPresence.Enabled && isVisible) {
                Client.ClearPresence();
                isVisible = false;
            }
        }

        public void HandlePresence(object source, EventArgs e) {
            // Do nothing if RPC is disabled
            if (!isVisible) return;

            // Only update RPC if player isn't in loading screen
            switch(ctx.Player.ZoneID) {
                case 0:
                    Instance.Details = ctx.Player.Slot == 999 ? "In main menu" : "In loading screen";
                    Instance.State = null;
                    Instance.Assets = GenerateAssets("main-menu", null, null, null);
                    Instance.Party = null;
                    break;
                default:
                    if (ctx.Player.Slot == 999) {
                        Instance.Details = "In main menu";
                        Instance.State = null;
                        Instance.Assets = GenerateAssets("main-menu", null, null, null);
                        Instance.Party = null;
                        break;
                    }
                    Instance.Details = ctx.HuntedMonster == null ? ctx.Player.inPeaceZone ? "Idle" : "Exploring" : $"Hunting {ctx.HuntedMonster.Name} ({(int)(ctx.HuntedMonster.HPPercentage * 100)}%)";
                    Instance.State = ctx.Player.PartySize > 1 ? "In Party" : "Solo";
                    Instance.Assets = GenerateAssets(ctx.Player.ZoneName == null ? "main-menu" : ctx.Player.ZoneName.Replace(' ', '-').Replace("'", string.Empty).ToLower(), ctx.Player.ZoneName == "Main Menu" ? null : ctx.Player.ZoneName, ctx.Player.WeaponName == null ? "hunter-rank" : ctx.Player.WeaponName.Replace(' ', '-').ToLower(), $"{ctx.Player.Name} | Lvl: {ctx.Player.Level}");
                    // TODO: Generate party hash
                    Instance.Party = MakeParty(ctx.Player.PartySize, ctx.Player.PartyMax, "test");
                    break;
            }
            Client.SetPresence(Instance);
        }

        public void InitializePresence() {
            Client = new DiscordRpcClient(APP_ID);
            Client.Initialize();
            Debugger.Discord("Connecting to Discord...");
        }

        public void DisconnectPresence() {
            Client.ClearPresence();
            Client.Dispose();
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
