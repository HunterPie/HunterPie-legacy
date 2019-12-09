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
        }

        private void HookEvents() {
            ctx.Player.OnZoneChange += HandleZoneChange;
        }
 
        public void HandleZoneChange(object source, EventArgs e) {
            // Only update RPC if player isn't in loading screen
            
        }

        public void InitializePresence() {
            Client = new DiscordRpcClient(APP_ID);
            Client.Initialize();
            Debugger.Discord("Connecting to Discord...");
        }

        public void UpdatePresenceInfo(string details, string state, Assets PresenceAssets, Party PresenceParty, Timestamps PresenceTimestamps) {
            if (isVisible) {
                Instance.Details = details;
                Instance.State = state;
                Instance.Assets = PresenceAssets;
                Instance.Party = PresenceParty;
                Instance.Timestamps = PresenceTimestamps;
                UpdatePresence();
            }
        }

        private void UpdatePresence() {
            Client.SetPresence(Instance);
        }

        public void ShowPresence() {
            isVisible = true;
        }

        public void HidePresence() {
            if (isVisible) {
                Client.ClearPresence();
            }
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
