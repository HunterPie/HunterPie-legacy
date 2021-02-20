using System;
using System.Windows;
using DiscordRPC;
using DiscordRPC.Message;
using HunterPie.GUI.Widgets.Notifications;
using HunterPie.Logger;


namespace HunterPie.Core.Integrations.Discord
{
    internal class Presence : IDisposable
    {
        const string AppId = "567152028070051859";

        private RichPresence instance;
        private DiscordRpcClient client;
        private Game context;
        private bool disposedValue;
        private bool isOffline;
        private bool failedToRegisterScheme;
        private bool isVisible = ConfigManager.Settings.RichPresence.Enabled;

        /// <summary>
        /// Initializes a new Presence instance
        /// </summary>
        /// <param name="context">Context of the game</param>
        public Presence(Game context)
        {
            this.context = context;

            HookEvents();
        }

        public void Initialize()
        {
            if (isOffline)
                return;

            if (!InitializeDiscordClient())
                return;

            InitializeJoinScheme();

            client.Initialize();

            if (!isVisible)
                client.ClearPresence();
        }

        private bool InitializeDiscordClient()
        {
            instance = new RichPresence();
            Debugger.Discord(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_CONNECTED']"));

            instance.Secrets = new Secrets();

            try
            {
                client = new DiscordRpcClient(AppId, autoEvents: true);
            }
            catch (Exception err)
            {
                Debugger.Error($"Failed to create Rich Presence connection:\n{err}");
                return false;
            }
            return true;
        }

        private void InitializeJoinScheme()
        {
            try
            {
                client.RegisterUriScheme("582010");
            }
            catch (Exception err)
            {
                Debugger.Error(err);
                failedToRegisterScheme = true;
            }

            if (!failedToRegisterScheme)
            {
                client.OnReady += OnReady;
                client.OnJoinRequested += OnJoinRequested;
                client.OnJoin += OnJoin;

                client.SetSubscription(EventType.JoinRequest | EventType.Join);
            }
        }

        private void OnJoin(object sender, JoinMessage args)
        {
            Debugger.Discord(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_JOINING']"));
            System.Diagnostics.Process.Start($"steam://joinlobby/582010/{args.Secret}");
            Debugger.Debug($"steam://joinlobby/582010/{args.Secret}");
        }

        private void OnJoinRequested(object sender, JoinRequestMessage args)
        {
            Debugger.Discord(
                GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_JOIN_REQUEST']")
                .Replace("{Username}", args.User.ToString())
                );

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                DiscordNotify discordNotify = new DiscordNotify(args);
                discordNotify.OnRequestAccepted += (src, args) =>
                {
                    client.Respond(args, true);
                };
                discordNotify.OnRequestRejected += (src, args) =>
                {
                    client.Respond(args, false);
                };
                discordNotify.Show();
            });
        }

        private void OnReady(object sender, ReadyMessage args)
        {
            Debugger.Discord(
                GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_USER_CONNECTED']")
                .Replace("{Username}", args.User.ToString())
                );
        }

        public void SetOfflineMode()
        {
            isOffline = true;
        }

        private void UpdatePresenceInstance()
        {
            if (instance is null || context is null)
                return;

            // Do nothing if RPC is disabled
            if (!isVisible)
                return;

            UpdatePresenceJoin();

            UpdatePresenceDetails();

            client.SetPresence(instance);
        }

        private void UpdatePresenceJoin()
        {
            if (!failedToRegisterScheme)
            {
                if (context.Player.SteamSession != 0 &&
                    ConfigManager.Settings.RichPresence.LetPeopleJoinSession)
                    instance.Secrets.JoinSecret = $"{context.Player.SteamSession}/{context.Player.SteamID}";
                else
                    instance.Secrets.JoinSecret = null;
            }
        }

        private void UpdatePresenceDetails()
        {
            if (context.Player.ZoneID == 0)
            {
                string description = context.Player.IsLoggedOn ?
                    GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_IN_LOADING_SCREEN']") :
                    GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_IN_MAIN_MENU']");

                instance.WithDetails(description)
                    .WithState(null)
                    .WithParty(null)
                    .WithAssets(new Assets() {
                        LargeImageKey = "main-menu",
                        LargeImageText = null,
                        SmallImageKey = null,
                        SmallImageText = null
                    });
            } else
            {
                if (!context.Player.IsLoggedOn)
                    instance.WithDetails(
                        GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_IN_MAIN_MENU']")
                        )
                        .WithState(null)
                        .WithParty(null)
                        .WithAssets(new Assets()
                        {
                            LargeImageKey = "main-menu",
                            LargeImageText = null,
                            SmallImageKey = null,
                            SmallImageText = null
                        });
                else
                {
                    string description = GetDescriptionBasedOnContext();
                    string state = GetStateBasedOnContext();

                    if (description is null || state is null)
                        return;

                    var party = GetPartyBasedOnContext();

                    instance.WithDetails(description)
                        .WithState(state)
                        .WithParty(party)
                        .WithParty(party)
                        .WithAssets(new Assets()
                        {
                            LargeImageKey = context.Player.ZoneName == null ? "main-menu" : $"st{context.Player.ZoneID}",
                            LargeImageText = context.Player.ZoneID == 0 ? null : context.Player.ZoneName,
                            SmallImageKey = context.Player.WeaponName == null ? "hunter-rank" : $"weap{context.Player.WeaponID}",
                            SmallImageText = $"{context.Player.Name} | HR: {context.Player.Level} | MR: {context.Player.MasterRank}"
                        })
                        .WithTimestamps(GetTimestamps(context.Time));
                }
            }
        }

        private string GetDescriptionBasedOnContext()
        {
            if (context is null || context.Player is null)
                return string.Empty;

            // Special cases
            switch (context.Player.ZoneID)
            {
                case 504:
                    return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_TRAINING']");
            }

            // Chilling in village
            if (context.Player.InPeaceZone)
                return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_IN_TOWN']");

            // Exploring
            if (context.HuntedMonster is null)
                return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_EXPLORING']");

            // Hunting a monster
            if (string.IsNullOrEmpty(context.HuntedMonster.Name))
                return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_EXPLORING']");

            return ConfigManager.Settings.RichPresence.ShowMonsterHealth ?
                GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_HUNTING']")
                .Replace("{Monster}", context.HuntedMonster.Name)
                .Replace("{Health}", $"{(int)(context.HuntedMonster.HPPercentage * 100)}%") :
                GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_HUNTING']")
                .Replace("{Monster}", context.HuntedMonster.Name)
                .Replace("({Health})", null);
        }

        private string GetStateBasedOnContext()
        {
            if (context is null || context.Player is null)
                return string.Empty;

            if (context.Player.PlayerParty.Size > 1 ||
                context.Player.PlayerParty.LobbySize > 1)
            {
                if (context.Player.InPeaceZone)
                    return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_STATE_LOBBY']");
                else
                    return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_STATE_PARTY']");
            }
            return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_STATE_SOLO']");
        }

        private DiscordRPC.Party GetPartyBasedOnContext()
        {
            int nMax = context.Player.InPeaceZone ? context.Player.PlayerParty.MaxLobbySize :
                context.Player.PlayerParty.MaxSize;
            string hash = context.Player.InPeaceZone ? context.Player.SteamSession.ToString() :
                context.Player.PlayerParty.PartyHash;

            return new DiscordRPC.Party()
            {
                Size = context.Player.PlayerParty.Size,
                Max = nMax,
                ID = hash
            };
        }

        private Timestamps GetTimestamps(DateTime? start)
        {
            Timestamps timestamps = new Timestamps();
            try
            {
                timestamps.Start = start;
            } catch
            {
                timestamps.Start = null;
            }
            return timestamps;
        }

        private void HookEvents()
        {
            ConfigManager.OnSettingsUpdate += OnSettingsUpdate;

            context.OnClockChange += OnClockChange;
            context.Player.OnZoneChange += OnZoneChange;
        }

        private void UnhookEvents()
        {
            ConfigManager.OnSettingsUpdate -= OnSettingsUpdate;

            context.OnClockChange -= OnClockChange;
            context.Player.OnZoneChange -= OnZoneChange;
        }

        private void OnZoneChange(object source, EventArgs args)
        {
            UpdatePresenceInstance();
        }

        private void OnClockChange(object source, EventArgs args)
        {
            UpdatePresenceInstance();
        }

        private void OnSettingsUpdate(object sender, EventArgs e)
        {
            isVisible = ConfigManager.Settings.RichPresence.Enabled;

            if (!isVisible)
                try
                {
                    client.ClearPresence();
                } catch { }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    UnhookEvents();
                    client?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
