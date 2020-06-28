using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.Memory;

namespace HunterPie.GUI
{
    class Overlay : IDisposable
    {
        public bool IsDisposed { get; private set; }

        readonly List<Widget> Widgets = new List<Widget>();
        Game ctx;

        public Overlay(Game Context)
        {
            ctx = Context;
            SetRenderMode();
            CreateWidgets();
        }

        public void ToggleDesignMode()
        {
            foreach (Widget widget in Widgets)
            {
                widget.InDesignMode = !widget.InDesignMode;
            }
            if (!Widgets.First().InDesignMode) UserSettings.SaveNewConfig();
        }

        private void SetRenderMode()
        {
            if (!UserSettings.PlayerConfig.Overlay.EnableHardwareAcceleration)
            {
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            }
        }

        private void CreateWidgets()
        {
            Widgets.Add(new Widgets.HarvestBox(ctx.Player));
            Widgets.Add(new Widgets.MantleTimer(0, ctx.Player.PrimaryMantle));
            Widgets.Add(new Widgets.MantleTimer(1, ctx.Player.SecondaryMantle));
            Widgets.Add(new Widgets.MonsterContainer(ctx));
            Widgets.Add(new Widgets.DPSMeter.Meter(ctx));
            for (int AbnormTrayIndex = 0; AbnormTrayIndex < UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars; AbnormTrayIndex++)
            {
                Widgets.Add(new Widgets.Abnormality_Widget.AbnormalityContainer(ctx.Player, AbnormTrayIndex));
            }
            Widgets.Add(new Widgets.ClassWidget.ClassWidgetContainer(ctx));
        }

        private void DestroyWidgets()
        {
            if (!Widgets.First().InDesignMode) UserSettings.SaveNewConfig();
            foreach (Widget widget in Widgets)
            {
                widget.InDesignMode = false;
                widget.Close();
            }
            Widgets.Clear();
        }

        public void HookEvents()
        {
            Scanner.OnGameFocus += OnGameFocus;
            Scanner.OnGameUnfocus += OnGameUnfocus;
        }

        private void UnhookEvents()
        {
            Scanner.OnGameFocus -= OnGameFocus;
            Scanner.OnGameUnfocus -= OnGameUnfocus;
        }

        public void Destroy()
        {
            DestroyWidgets();
            UnhookEvents();
            ctx = null;
        }

        public void GlobalSettingsEventHandler(object source, EventArgs e)
        {
            ToggleOverlay();
            DeleteAbnormWidgetsIfNeeded();
            foreach (Widget widget in Widgets)
            {
                widget.ApplySettings();
            }
        }

        private void DeleteAbnormWidgetsIfNeeded()
        {
            List<int> IndexesToRemove = new List<int>();
            int i = 0;
            foreach (Widget widget in Widgets)
            {
                if (widget.WidgetType == 5)
                {
                    Widgets.Abnormality_Widget.AbnormalityContainer widgetConverted = (Widgets.Abnormality_Widget.AbnormalityContainer)widget;
                    if (widgetConverted.AbnormalityTrayIndex >= UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars)
                    {
                        widgetConverted.Close();
                        IndexesToRemove.Add(i);
                    }
                }
                i++;
            }
            foreach (int index in IndexesToRemove)
            {
                Widgets.RemoveAt(index);
            }

        }

        private void OnGameUnfocus(object source, EventArgs args)
        {
            foreach (Widget widget in Widgets)
            {
                widget.OverlayIsFocused = false;
                if (!widget.InDesignMode)
                {
                    widget.ApplySettings(true);
                }
            }

        }

        private void OnGameFocus(object source, EventArgs args)
        {
            foreach (Widget widget in Widgets)
            {
                widget.OverlayIsFocused = true;
                if (!widget.InDesignMode)
                {
                    widget.ApplySettings(true);
                }
            }
        }

        /* Positions and enable/disable components */

        public void ToggleOverlay()
        {
            foreach (Widget widget in Widgets)
            {
                widget.OverlayActive = UserSettings.PlayerConfig.Overlay.Enabled;
                widget.OverlayFocusActive = UserSettings.PlayerConfig.Overlay.HideWhenGameIsUnfocused;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnhookEvents();
                Destroy();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
