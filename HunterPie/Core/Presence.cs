using DiscordRPC;

namespace HunterPie.Core {
    class Presence {
        private string APP_ID = "567152028070051859";
        private RichPresence Instance = new RichPresence();
        public DiscordRpcClient Client;
        

        public void InitializePresence() {
            Client = new DiscordRpcClient(APP_ID);
            Client.Initialize();
        }

        public void UpdatePresenceInfo(string details, string state, Assets PresenceAssets, Party PresenceParty, Timestamps PresenceTimestamps) {
            Instance.Details = details;
            Instance.State = state;
            Instance.Assets = PresenceAssets;
            Instance.Party = PresenceParty;
            Instance.Timestamps = PresenceTimestamps;
            UpdatePresence();
        }

        private void UpdatePresence() {
            Client.SetPresence(Instance);
        }

    }
}
