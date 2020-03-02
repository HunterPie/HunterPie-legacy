using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Interop;
using HunterPie.Core;
using HunterPie.Memory;
using System.Linq;

namespace HunterPie.GUI {
    class Overlay : IDisposable {
        public bool IsDisposed { get; private set; }
        KeyboardHook KeyHook;
        List<Widget> Widgets = new List<Widget>();
        Game ctx;

        public Overlay(Game Context) {
            ctx = Context;
            SetRenderMode();
            CreateWidgets();
            SetKeyboardHook();
        }

        private void SetKeyboardHook() {
            KeyHook = new KeyboardHook();
            KeyHook.InstallHooks();
            KeyHook.OnKeyboardKeyPress += OnKeyboardKeyPress;
        }

        private void RemoveKeyboardHook() {
            KeyHook.UninstallHooks();
            KeyHook.OnKeyboardKeyPress -= OnKeyboardKeyPress;
        }

        // Implemented IsHoldingKey to avoid toggle spam
        private bool IsHoldingKey = false;
        private void OnKeyboardKeyPress(object sender, KeyboardInputEventArgs e) {
            if (e.Key == UserSettings.PlayerConfig.Overlay.ToggleDesignModeKey && e.KeyMessage == KeyboardHookHelper.KeyboardMessage.WM_KEYDOWN && !IsHoldingKey) {
                ToggleDesignMode();
                IsHoldingKey = true;
            }
            if (e.Key == UserSettings.PlayerConfig.Overlay.ToggleDesignModeKey && e.KeyMessage == KeyboardHookHelper.KeyboardMessage.WM_KEYUP) {
                IsHoldingKey = false;
            }
            
        }

        private void ToggleDesignMode() {
            foreach (Widget widget in Widgets) {
                widget.InDesignMode = !widget.InDesignMode;   
            }
            if (!Widgets.First().InDesignMode) UserSettings.SaveNewConfig();
        }

        private void SetRenderMode() {
            if (!UserSettings.PlayerConfig.Overlay.EnableHardwareAcceleration) {
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            }
        }

        private void CreateWidgets() {
            Widgets.Add(new Widgets.HarvestBox(ctx.Player));
            Widgets.Add(new Widgets.MantleTimer(0, ctx.Player.PrimaryMantle));
            Widgets.Add(new Widgets.MantleTimer(1, ctx.Player.SecondaryMantle));
            Widgets.Add(new Widgets.MonsterContainer(ctx));
            Widgets.Add(new Widgets.DPSMeter.Meter(ctx));
        }

        private void DestroyWidgets() {
            if (!Widgets.First().InDesignMode) UserSettings.SaveNewConfig();
            foreach (Widget widget in Widgets) {
                widget.InDesignMode = false;
                widget.Close();
            }
            Widgets.Clear();
        }

        public void HookEvents() {
            Scanner.OnGameFocus += OnGameFocus;
            Scanner.OnGameUnfocus += OnGameUnfocus;
        }

        private void UnhookEvents() {
            Scanner.OnGameFocus -= OnGameFocus;
            Scanner.OnGameUnfocus -= OnGameUnfocus;
        }

        public void Destroy() {
            this.DestroyWidgets();
            this.UnhookEvents();
            this.ctx = null;
        }

        public void GlobalSettingsEventHandler(object source, EventArgs e) {
            ToggleOverlay();
            foreach (Widget widget in Widgets) {
                widget.ApplySettings();
            }
        }

        private void OnGameUnfocus(object source, EventArgs args) {
            foreach (Widget widget in Widgets) {
                widget.OverlayIsFocused = false;
                if (!widget.InDesignMode) {
                    widget.ApplySettings();
                }
            }
            
        }

        private void OnGameFocus(object source, EventArgs args) {
            foreach (Widget widget in Widgets) {
                widget.OverlayIsFocused = true;
                if (!widget.InDesignMode) {
                    widget.ApplySettings();
                }
            }
        }

        /* Positions and enable/disable components */

        public void ToggleOverlay() {
            foreach (Widget widget in Widgets) {
                widget.OverlayActive = UserSettings.PlayerConfig.Overlay.Enabled;
                widget.OverlayFocusActive = UserSettings.PlayerConfig.Overlay.HideWhenGameIsUnfocused;
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.UnhookEvents();
                this.RemoveKeyboardHook();
                this.Destroy();
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
