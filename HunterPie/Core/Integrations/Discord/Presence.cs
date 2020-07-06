using System;
using DiscordRPC;
using HunterPie.Logger;

namespace HunterPie.Core.Integrations.Discord
{
    public class Presence : IDisposable
    {
        public bool IsDisposed { get; private set; }
        private readonly string APP_ID = "567152028070051859";
        private bool FailedToRegisterScheme { get; set; }
        private bool isOffline = false;
        private bool isVisible = true;
        private RichPresence Instance;
        public DiscordRpcClient Client;
        public Game ctx;

        /* Constructor and base functions */

        public Presence(Game context)
        {
            ctx = context;
            HookEvents();
        }

        public void SetOfflineMode() => isOffline = true;

        ~Presence()
        {
            Dispose(false);
        }

        /* Event handlers */

        private void HookEvents()
        {
            UserSettings.OnSettingsUpdate += HandleSettings;
            // Game context
            ctx.OnClockChange += HandlePresence;
            ctx.Player.OnZoneChange += HandlePresence;
        }

        private void UnhookEvents()
        {
            UserSettings.OnSettingsUpdate -= HandleSettings;
            ctx.OnClockChange -= HandlePresence;
            ctx.Player.OnZoneChange -= HandlePresence;
        }

        /* Connection */

        public void StartRPC()
        {
            if (isOffline) return;

            // Check if connection exists to avoid creating multiple connections
            Instance = new RichPresence();
            Debugger.Discord(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_CONNECTED']"));
            Instance.Secrets = new Secrets();
            Client = new DiscordRpcClient(APP_ID, autoEvents: true);

            try
            {
                Client.RegisterUriScheme("582010");

            }
            catch (Exception err)
            {
                Debugger.Error(err);
                FailedToRegisterScheme = true;
            }

            if (!FailedToRegisterScheme)
            {
                // Events
                Client.OnReady += Client_OnReady;
                Client.OnJoinRequested += Client_OnJoinRequested;
                Client.OnJoin += Client_OnJoin;

                Client.SetSubscription(EventType.JoinRequest | EventType.Join);
            }

            Client.Initialize();
            if (!UserSettings.PlayerConfig.RichPresence.Enabled && isVisible)
            {
                Client?.ClearPresence();
                isVisible = false;
            }
        }

        private void Client_OnJoin(object sender, DiscordRPC.Message.JoinMessage args)
        {
            Debugger.Discord(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_JOINING']"));
            System.Diagnostics.Process.Start($"steam://joinlobby/582010/{args.Secret}");
            Debugger.Debug($"steam://joinlobby/582010/{args.Secret}");
        }

        private void Client_OnReady(object sender, DiscordRPC.Message.ReadyMessage args) => Debugger.Discord(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_USER_CONNECTED']").Replace("{Username}", args.User.ToString()));

        private void Client_OnJoinRequested(object sender, DiscordRPC.Message.JoinRequestMessage args)
        {
            Debugger.Discord(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_JOIN_REQUEST']").Replace("{Username}", args.User.ToString()));

            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                GUI.Widgets.Notification_Widget.DiscordNotify DiscordNotification = new GUI.Widgets.Notification_Widget.DiscordNotify(args);

                DiscordNotification.OnRequestAccepted += OnDiscordRequestAccepted;
                DiscordNotification.OnRequestRejected += OnDiscordRequestRejected;

                DiscordNotification.Show();
            }));
        }

        private void OnDiscordRequestRejected(object source, DiscordRPC.Message.JoinRequestMessage args)
        {
            GUI.Widgets.Notification_Widget.DiscordNotify src = (GUI.Widgets.Notification_Widget.DiscordNotify)source;
            src.OnRequestAccepted -= OnDiscordRequestAccepted;
            src.OnRequestRejected -= OnDiscordRequestRejected;

            Client.Respond(args, false);

            App.Current.Dispatcher.Invoke(new Action(() => { src.Close(); }));
        }

        private void OnDiscordRequestAccepted(object source, DiscordRPC.Message.JoinRequestMessage args)
        {
            GUI.Widgets.Notification_Widget.DiscordNotify src = (GUI.Widgets.Notification_Widget.DiscordNotify)source;
            src.OnRequestAccepted -= OnDiscordRequestAccepted;
            src.OnRequestRejected -= OnDiscordRequestRejected;

            Client.Respond(args, true);

            src.Close();
        }

        public void HandleSettings(object source, EventArgs e)
        {
            if (UserSettings.PlayerConfig.RichPresence.Enabled && !isVisible)
            {
                isVisible = true;
            }
            else if (!UserSettings.PlayerConfig.RichPresence.Enabled && isVisible)
            {
                try
                {
                    Client.ClearPresence();
                }
                catch { }
                isVisible = false;
            }
        }

        public void HandlePresence(object source, EventArgs e)
        {
            if (Instance == null) return;

            // Do nothing if RPC is disabled
            if (!isVisible) return;

            if (!FailedToRegisterScheme)
            {
                if (ctx.Player.SteamSession != 0 && ctx.Player.InPeaceZone && UserSettings.PlayerConfig.RichPresence.LetPeopleJoinSession)
                {
                    Instance.Secrets.JoinSecret = $"{ctx.Player.SteamSession}/{ctx.Player.SteamID}";
                }
                else
                {
                    Instance.Secrets.JoinSecret = null;
                }
            }

            // Only update RPC if player isn't in loading screen
            switch (ctx.Player.ZoneID)
            {
                case 0:
                    Instance.Details = ctx.Player.PlayerAddress == 0 ? GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_IN_MAIN_MENU']") : GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_IN_LOADING_SCREEN']");
                    Instance.State = null;
                    GenerateAssets("main-menu", null, null, null);
                    Instance.Party = null;
                    break;
                default:
                    if (ctx.Player.PlayerAddress == 0)
                    {
                        Instance.Details = GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_IN_MAIN_MENU']");
                        Instance.State = null;
                        GenerateAssets("main-menu", null, null, null);
                        Instance.Party = null;
                        break;
                    }
                    Instance.Details = GetDescription();
                    Instance.State = GetState();
                    GenerateAssets(ctx.Player.ZoneName == null ? "main-menu" : $"st{ctx.Player.ZoneID}", ctx.Player.ZoneID == 0 ? null : ctx.Player.ZoneName, ctx.Player.WeaponName == null ? "hunter-rank" : $"weap{ctx.Player.WeaponID}", $"{ctx.Player.Name} | HR: {ctx.Player.Level} | MR: {ctx.Player.MasterRank}");
                    if (!ctx.Player.InPeaceZone)
                    {
                        MakeParty(ctx.Player.PlayerParty.Size, ctx.Player.PlayerParty.MaxSize, ctx.Player.PlayerParty.PartyHash);
                    }
                    else
                    {
                        MakeParty(ctx.Player.PlayerParty.LobbySize, ctx.Player.PlayerParty.MaxLobbySize, ctx.Player.SteamSession.ToString());
                    }
                    Instance.Timestamps = NewTimestamp(ctx.Time);
                    break;
            }
            Client.SetPresence(Instance);
        }

        private string GetDescription()
        {
            // Custom description for special zones
            switch (ctx.Player.ZoneID)
            {
                case 504:
                    return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_TRAINING']");
            }
            if (ctx.Player.InPeaceZone) return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_IN_TOWN']");
            if (ctx.HuntedMonster == null) return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_EXPLORING']");
            else
            {
                if (string.IsNullOrEmpty(ctx.HuntedMonster.Name)) return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_EXPLORING']");
                return UserSettings.PlayerConfig.RichPresence.ShowMonsterHealth ? GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_HUNTING']").Replace("{Monster}", ctx.HuntedMonster.Name).Replace("{Health}", $"{(int)(ctx.HuntedMonster.HPPercentage * 100)}%") : GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_HUNTING']").Replace("{Monster}", ctx.HuntedMonster.Name).Replace("({Health})", null);
            }
        }

        private string GetState()
        {
            if (ctx.Player.PlayerParty.Size > 1 || ctx.Player.PlayerParty.LobbySize > 1)
            {
                if (ctx.Player.InPeaceZone)
                {
                    return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_STATE_LOBBY']");
                }
                else { return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_STATE_PARTY']"); }
            }
            else { return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_STATE_SOLO']"); }
        }

        /* Helpers */

        public void GenerateAssets(string largeImage, string largeImageText, string smallImage, string smallImageText)
        {
            if (Instance.Assets == null) { Instance.Assets = new Assets(); }
            Instance.Assets.LargeImageKey = largeImage;
            Instance.Assets.LargeImageText = largeImageText;
            Instance.Assets.SmallImageKey = smallImage;
            Instance.Assets.SmallImageText = smallImageText;
        }

        public void MakeParty(int partySize, int maxParty, string partyHash)
        {
            if (Instance.Party == null) { Instance.Party = new DiscordRPC.Party(); }
            Instance.Party.Size = partySize;
            Instance.Party.Max = maxParty;
            Instance.Party.ID = partyHash == "0" ? "USER_IN_OFFLINE_MODE" : partyHash;
        }

        public Timestamps NewTimestamp(DateTime? start)
        {
            Timestamps timestamp = new Timestamps();
            try
            {
                timestamp.Start = start;
            }
            catch
            {
                timestamp.Start = null;
            }
            return timestamp;
        }


        /* Dispose */
        public void Dispose()
        {
            Debugger.Discord(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_DISCONNECTED']"));
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                UnhookEvents();
                Client?.ClearPresence();
                Client?.Dispose();
                Instance = null;
            }
            IsDisposed = true;
        }
    }
}
