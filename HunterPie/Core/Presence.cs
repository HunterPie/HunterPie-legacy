using DiscordRPC;
using System;
using HunterPie.Memory;
using HunterPie.Logger;

namespace HunterPie.Core {
    public class Presence : IDisposable {
        public bool IsDisposed { get; private set; }
        private readonly string APP_ID = "567152028070051859";
        private bool isOffline = false;
        private bool isVisible = true;
        private RichPresence Instance;
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

        ~Presence() {
            Dispose(false);
        }

        /* Event handlers */

        private void HookEvents() {
            UserSettings.OnSettingsUpdate += HandleSettings;
            // Game context
            ctx.OnClockChange += HandlePresence;
            ctx.Player.OnZoneChange += HandlePresence;
        }
        
        private void UnhookEvents() {
            UserSettings.OnSettingsUpdate -= HandleSettings;
            ctx.OnClockChange -= HandlePresence;
            ctx.Player.OnZoneChange -= HandlePresence;
        }

            /* Connection */

        public void StartRPC() {
            if (isOffline) return;
            
            // Check if connection exists to avoid creating multiple connections
            Instance = new RichPresence();
            Debugger.Discord("Starting new RPC connection");
            Client = new DiscordRpcClient(APP_ID, autoEvents: true, pipe: -1);

            Client.RegisterUriScheme("582010");

            // Events
            Client.OnReady += Client_OnReady;
            Client.OnJoinRequested += Client_OnJoinRequested;
            Client.OnJoin += Client_OnJoin;

            Client.SetSubscription(EventType.JoinRequest | EventType.Join);

            Client.Initialize();
            if (!UserSettings.PlayerConfig.RichPresence.Enabled && isVisible) {
                Client?.ClearPresence();
                isVisible = false;
            }
        }

        private void Client_OnJoin(object sender, DiscordRPC.Message.JoinMessage args) {
            Debugger.Discord($"Joining session...");
            System.Diagnostics.Process.Start($"steam://joinlobby/582010/{args.Secret}");
        }

        private void Client_OnReady(object sender, DiscordRPC.Message.ReadyMessage args) {
            Debugger.Discord($"Connected to Discord on user: {args.User}");
        }

        private void Client_OnJoinRequested(object sender, DiscordRPC.Message.JoinRequestMessage args) {
            Debugger.Discord($"{args.User} requested to join session.");
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                GUI.Widgets.Notification_Widget.DiscordNotify DiscordNotification = new GUI.Widgets.Notification_Widget.DiscordNotify((DiscordRpcClient)sender, args);
                DiscordNotification.Show();
            }));
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
            if (Instance == null) return;

            // Do nothing if RPC is disabled
            if (!isVisible) return;

            // TODO: Implement join session?
            if (ctx.Player.SteamSession != 0 && ctx.Player.InPeaceZone) {
                Instance.Secrets = new Secrets() {
                    JoinSecret = $"{ctx.Player.SteamSession}/{ctx.Player.SteamID}"
                };
            }
            // Only update RPC if player isn't in loading screen
            switch (ctx.Player.ZoneID) {
                case 0:
                    Instance.Details = ctx.Player.PlayerAddress == 0 ? "In main menu" : "In loading screen";
                    Instance.State = null;
                    GenerateAssets("main-menu", null, null, null);
                    Instance.Party = null;
                    break;
                default:
                    if (ctx.Player.PlayerAddress == 0) {
                        Instance.Details = "In main menu";
                        Instance.State = null;
                        GenerateAssets("main-menu", null, null, null);
                        Instance.Party = null;
                        break;
                    }
                    Instance.Details = GetDescription();
                    Instance.State = GetState();
                    GenerateAssets(ctx.Player.ZoneName == null ? "main-menu" : $"st{ctx.Player.ZoneID}", ctx.Player.ZoneName == "Main Menu" ? null : ctx.Player.ZoneName, ctx.Player.WeaponName == null ? "hunter-rank" : $"weap{ctx.Player.WeaponID}", $"{ctx.Player.Name} | HR: {ctx.Player.Level} | MR: {ctx.Player.MasterRank}");
                    if (!ctx.Player.InPeaceZone) {
                        MakeParty(ctx.Player.PlayerParty.Size, ctx.Player.PlayerParty.MaxSize, ctx.Player.PlayerParty.PartyHash);
                    } else {
                        MakeParty(ctx.Player.PlayerParty.LobbySize, ctx.Player.PlayerParty.MaxLobbySize, ctx.Player.SteamSession.ToString());
                    }
                    Instance.Timestamps = NewTimestamp(ctx.Time);
                    break;
            }
            Client.SetPresence(Instance);
        }

        private string GetDescription() {
            // Custom description for special zones
            switch(ctx.Player.ZoneID) {
                case 504:
                    return "Training";
            }
            if (ctx.Player.InPeaceZone) return "Idle";
            if (ctx.HuntedMonster == null ) return "Exploring";
            else {
                if (string.IsNullOrEmpty(ctx.HuntedMonster.Name)) return "Exploring";
                return UserSettings.PlayerConfig.RichPresence.ShowMonsterHealth ? $"Hunting {ctx.HuntedMonster.Name} ({(int)(ctx.HuntedMonster.HPPercentage * 100)}%)" : $"Hunting {ctx.HuntedMonster.Name}";
            }
        }

        private string GetState() {
            if (ctx.Player.PlayerParty.Size > 1 || ctx.Player.PlayerParty.LobbySize > 1) {
                if (ctx.Player.InPeaceZone) {
                    return "In Lobby";
                } else { return "In Party"; }
            }
            else { return "Solo"; }
        }

        /* Helpers */

        public void GenerateAssets(string largeImage, string largeImageText, string smallImage, string smallImageText) {
            if (Instance.Assets == null) { Instance.Assets = new Assets(); }
            Instance.Assets.LargeImageKey = largeImage;
            Instance.Assets.LargeImageText = largeImageText;
            Instance.Assets.SmallImageKey = smallImage;
            Instance.Assets.SmallImageText = smallImageText;
        }

        public void MakeParty(int partySize, int maxParty, string partyHash) {
            if (Instance.Party == null) { Instance.Party = new DiscordRPC.Party(); }
            Instance.Party.Size = partySize;
            Instance.Party.Max = maxParty;
            Instance.Party.ID = partyHash;
        }

        public Timestamps NewTimestamp(DateTime? start) {
            Timestamps timestamp = new Timestamps();
            try {
                timestamp.Start = start;
            } catch {
                timestamp.Start = null;
            }
            return timestamp;
        }


        /* Dispose */
        public void Dispose() {
            Debugger.Discord("Closed Connection to discord");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (IsDisposed) return;
            if (disposing) {
                UnhookEvents();
                Client?.ClearPresence();
                Client?.Dispose();
                Instance = null;
            }
            this.IsDisposed = true;
        }
    }
}
