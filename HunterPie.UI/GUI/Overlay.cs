using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.GUI.Widgets;
using HunterPie.Logger;
using HunterPie.Memory;

[assembly:InternalsVisibleTo("HunterPie")]
namespace HunterPie.GUI
{
    public class Overlay
    {

        public bool IsDisposed { get; private set; }

        private static readonly List<Widget> widgets = new List<Widget>();

        public static IReadOnlyList<Widget> Widgets => widgets;

        private Core.Settings.Overlay OverlaySettings => ConfigManager.Settings.Overlay;

        Game Context { get; set; }

        public Overlay(Game ctx)
        {
            Context = ctx;
            SetRenderMode();
            SetAliasingMode();

            CreateWidgets();
        }

        #region Overlay static methods

        /// <summary>
        /// Registers a new widget to the overlay
        /// </summary>
        /// <param name="widget">Widget to register</param>
        /// <returns>True if the widget was registered succesfully.</returns>
        public static bool RegisterWidget(Widget widget)
        {
            try
            {
                widgets.Add(widget);

                widget.OverlayActive = ConfigManager.Settings.Overlay.Enabled;
                widget.OverlayFocusActive = ConfigManager.Settings.Overlay.HideWhenGameIsUnfocused;

                return true;
            } catch (Exception err)
            {
                Debugger.Error(err);
                return false;
            }
        }

        /// <summary>
        /// Unregisters a widget from the overlay
        /// </summary>
        /// <param name="widget">Widget to unregister</param>
        /// <returns>True if the widget was unregistered succesfully.</returns>
        public static bool UnregisterWidget(Widget widget)
        {
            try
            {
                widgets.Remove(widget);
                return true;
            } catch (Exception err)
            {
                Debugger.Error(err);
                return false;
            }
        }

        #endregion

        internal async void ToggleDesignMode()
        {
            foreach (Widget widget in Widgets)
            {
                widget.InDesignMode = !widget.InDesignMode;
            }
            if (!Widgets.First().InDesignMode)
                await ConfigManager.TrySaveSettingsAsync();
        }

        private static void SetRenderMode()
        {
            if (!ConfigManager.Settings.Overlay.EnableHardwareAcceleration)
            {
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            }
        }

        private void SetAliasingMode()
        {
            EdgeMode mode = OverlaySettings.EnableAntiAliasing ? EdgeMode.Unspecified : EdgeMode.Aliased;
            foreach (Widget widget in Widgets)
            {
                RenderOptions.SetEdgeMode(widget, mode);
            }
        }

        private void CreateWidgets()
        {
            if (OverlaySettings.Initialize)
            {
                int counter = 0;
                if (OverlaySettings.HarvestBoxComponent.Initialize)
                {
                    RegisterWidget(new Widgets.HarvestBox(Context.Player));
                    counter++;
                }

                if (OverlaySettings.PrimaryMantle.Initialize)
                {
                    RegisterWidget(new Widgets.MantleTimer(0, Context.Player.PrimaryMantle));
                    counter++;
                }

                if (OverlaySettings.SecondaryMantle.Initialize)
                {
                    RegisterWidget(new Widgets.MantleTimer(1, Context.Player.SecondaryMantle));
                    counter++;
                }

                if (OverlaySettings.MonstersComponent.Initialize)
                {
                    RegisterWidget(new Widgets.MonsterContainer(Context));
                    counter++;
                }

                if (OverlaySettings.DPSMeter.Initialize)
                {
                    RegisterWidget(new Widgets.DPSMeter.Meter(Context));
                    counter++;
                }

                for (int AbnormTrayIndex = 0; AbnormTrayIndex < ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars; AbnormTrayIndex++)
                {
                    if (OverlaySettings.AbnormalitiesWidget.BarPresets[AbnormTrayIndex].Initialize)
                    {
                        RegisterWidget(new Widgets.Abnormality_Widget.AbnormalityContainer(Context.Player, AbnormTrayIndex));
                        counter++;
                    }
                }

                if (OverlaySettings.ClassesWidget.Initialize)
                {
                    RegisterWidget(new Widgets.ClassWidget.ClassWidgetContainer(Context));
                    counter++;
                }

                if (OverlaySettings.PlayerHealthComponent.Initialize)
                {
                    RegisterWidget(new Widgets.HealthWidget.PlayerHealth(Context));
                    counter++;
                }

                if (OverlaySettings.NotificationsWidget.Initialize)
                {
                    RegisterWidget(new NotificationsWidget());
                    counter++;
                }

                Debugger.Warn($"Initialized overlay with {counter} widgets!");
            } else
            {
                Debugger.Warn($"Skipped overlay initialization!");
            }

        }

        private async Task DestroyWidgets()
        {
            if (!Widgets.First().InDesignMode)
                await ConfigManager.TrySaveSettingsAsync();

            await Application.Current.Dispatcher.InvokeAsync(() => {
                foreach (Widget widget in Widgets)
                {
                    widget.InDesignMode = false;
                    widget.Close();
                }
                widgets.Clear();
            });
        }

        internal void HookEvents()
        {
            Kernel.OnGameFocus += OnGameFocus;
            Kernel.OnGameUnfocus += OnGameUnfocus;
        }

        private void UnhookEvents()
        {
            Kernel.OnGameFocus -= OnGameFocus;
            Kernel.OnGameUnfocus -= OnGameUnfocus;
        }

        public async Task Destroy()
        {
            await DestroyWidgets();
            UnhookEvents();
            Context = null;
        }

        public async void GlobalSettingsEventHandler(object source, EventArgs e)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                {
                    ToggleOverlay();
                    DeleteAbnormWidgetsIfNeeded();
                    foreach (Widget widget in Widgets)
                    {
                        widget.ApplySettings();
                    }
                }));
            } catch(TaskCanceledException) {}


        }

        private void DeleteAbnormWidgetsIfNeeded()
        {
            List<int> IndexesToRemove = new List<int>();
            int i = 0;
            foreach (Widget widget in Widgets)
            {
                if (widget.Type == WidgetType.AbnormalityWidget)
                {
                    Widgets.Abnormality_Widget.AbnormalityContainer widgetConverted = (Widgets.Abnormality_Widget.AbnormalityContainer)widget;
                    if (widgetConverted.AbnormalityTrayIndex >= ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars)
                    {
                        widgetConverted.Close();
                        IndexesToRemove.Add(i);
                    }
                }
                i++;
            }
            foreach (int index in IndexesToRemove)
            {
                widgets.RemoveAt(index);
            }

        }

        private async void OnGameUnfocus(object source, EventArgs args)
        {
            await Application.Current.Dispatcher.InvokeAsync(new Action(() =>
            {
                foreach (Widget widget in Widgets)
                {
                    widget.OverlayIsFocused = false;
                    if (!widget.InDesignMode)
                    {
                        widget.ChangeVisibility();
                    }
                }
            }));
        }

        private async void OnGameFocus(object source, EventArgs args)
        {
            await Application.Current.Dispatcher.InvokeAsync(new Action(() =>
            {
                foreach (Widget widget in Widgets)
                {
                    widget.OverlayIsFocused = true;
                    if (!widget.InDesignMode)
                    {
                        widget.ChangeVisibility();
                    }
                }
            }));
        }

        /* Positions and enable/disable components */

        public void ToggleOverlay()
        {
            foreach (Widget widget in Widgets)
            {
                widget.OverlayActive = ConfigManager.Settings.Overlay.Enabled;
                widget.OverlayFocusActive = ConfigManager.Settings.Overlay.HideWhenGameIsUnfocused;
            }
        }

        protected virtual async Task Dispose(bool disposing)
        {
            if (disposing)
            {
                UnhookEvents();
                await Destroy();
            }
            return;
        }

        public async Task Dispose()
        {
            await Dispose(true);
        }
    }
}
