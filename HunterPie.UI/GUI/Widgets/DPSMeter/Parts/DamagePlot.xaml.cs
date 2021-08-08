﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.DPSMeter.Parts
{
    public partial class DamagePlot : UserControl, IDisposable
    {
        private Game Context;
        private DispatcherTimer Timer;
        private List<MemberPlotModel> Members = new List<MemberPlotModel>();

        public DamagePlot()
        {
            InitializeComponent();
            Timer = new DispatcherTimer
            {

                Interval = TimeSpan.FromMilliseconds(ConfigManager.Settings.Overlay.DPSMeter.DamagePlotPollInterval)
            };
            Timer.Tick += OnTimerTick;

            // removing all user interactions with plot
            Plot.ActualController.UnbindAll();
        }

        public void SetContext(Game ctx)
        {
            Context = ctx;
            ctx.Player.PlayerParty.OnTotalDamageChange += OnTotalDamageChanged;
            ctx.Player.OnPeaceZoneLeave += OnPeaceZoneLeave;
            ctx.Player.OnPeaceZoneEnter += OnPeaceZoneEnter;
            ctx.Player.PlayerParty.OnTimerReset += OnTimerReset;
        }

        private void OnTimerReset(object source, EventArgs args)
        {
            Dispatch(() =>
            {
                // Restart plot chart so we discard damages that happened
                // before we joined the quest
                if (!Context?.Player.InPeaceZone ?? false)
                {
                    DestroyMemberPlots();
                    CreateMemberPlots();
                }
            });
        }

        public void ApplySettings()
        {
            for (int i = 0; i < Members.Count; i++)
            {
                Members[i].ChangeColor(ConfigManager.Settings.Overlay.DPSMeter.PartyMembers[i].Color);
                Members[i].ChangeMode(ConfigManager.Settings.Overlay.DPSMeter.DamagePlotMode);
            }

            Timer.Interval = TimeSpan.FromMilliseconds(ConfigManager.Settings.Overlay.DPSMeter.DamagePlotPollInterval);
            if (Context?.Player != null && !Timer.IsEnabled && Context.Player.InPeaceZone)
            {
                Timer.Start();
            }
            UpdateVisibility();
        }

        public void Dispose()
        {
            Context.Player.PlayerParty.OnTotalDamageChange -= OnTotalDamageChanged;
            Context.Player.OnPeaceZoneLeave -= OnPeaceZoneLeave;
            Context.Player.OnPeaceZoneEnter -= OnPeaceZoneEnter;
            Context.Player.PlayerParty.OnTimerReset -= OnTimerReset;
            DestroyMemberPlots();
            Timer.Stop();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            // this is used to update plot display even when no damage dealt. This is needed to keep plot always up-to-date

            if (!IsVisible)
            {
                // if widget isn't visible we can skip updating this data
                return;
            }

            UpdateDamage();
        }

        private void OnTotalDamageChanged(object source, EventArgs args) => UpdateDamage();
        private void OnPeaceZoneLeave(object source, EventArgs args)
        {
            Timer.Start();
            CreateMemberPlots();
        }

        private void OnPeaceZoneEnter(object source, EventArgs args)
        {
            Timer.Stop();
            DestroyMemberPlots();
        }

        private void UpdateDamage()
        {
            Dispatch(() =>
            {
                if (Context.Player.PlayerParty.TotalDamage == 0)
                {
                    UpdateVisibility();
                    return;
                }

                int now = (int)Context.Player.PlayerParty.Epoch.TotalMilliseconds;
                foreach (MemberPlotModel member in Members)
                {
                    member.UpdateDamage(now);
                }

                UpdateVisibility();
                Plot.UpdateLayout();
            });
        }

        private void CreateMemberPlots()
        {
            Dispatch(() =>
            {
                var plotMode = ConfigManager.Settings.Overlay.DPSMeter.DamagePlotMode;
                Members = Context.Player.PlayerParty.Members
                    .Select((m, idx) =>
                        new MemberPlotModel(Context.Player.PlayerParty, m, ConfigManager.Settings.Overlay.DPSMeter.PartyMembers[idx].Color, plotMode))
                    .ToList();
                foreach (var node in Members)
                {
                    Plot.Series.Add(node.Series);
                }
            });
        }

        private void DestroyMemberPlots()
        {
            Dispatch(() =>
            {
                Plot.Series.Clear();
                Members.Clear();
                UpdateVisibility();
                Plot.UpdateLayout();
            });
        }

        public void UpdateVisibility()
        {
            Dispatch(() =>
            {
                Visibility = Context.Player?.PlayerParty.TotalDamage > 0
                             && ConfigManager.Settings.Overlay.DPSMeter.EnableDamagePlot
                             && Members.Any(m => m.HasData)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            });
        }
        private void Dispatch(Action f) => Dispatcher.BeginInvoke(DispatcherPriority.Render, f);
    }
}

