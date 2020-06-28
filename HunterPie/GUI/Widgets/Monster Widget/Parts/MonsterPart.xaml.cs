using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HunterPie.Core;
using Timer = System.Threading.Timer;

namespace HunterPie.GUI.Widgets.Monster_Widget.Parts
{
    /// <summary>
    /// Interaction logic for MonsterPart.xaml
    /// </summary>
    public partial class MonsterPart : UserControl
    {
        private Part context;
        private Timer visibilityTimer;

        public MonsterPart() => InitializeComponent();

        private static UserSettings.Config.Monsterscomponent ComponentSettings =>
            UserSettings.PlayerConfig.Overlay.MonstersComponent;

        public void SetContext(Part ctx, double MaxHealthBarSize)
        {
            context = ctx;
            SetPartInformation(MaxHealthBarSize);
            HookEvents();
            StartVisibilityTimer();
        }

        private void HookEvents()
        {
            context.OnHealthChange += OnHealthChange;
            context.OnBrokenCounterChange += OnBrokenCounterChange;
        }

        public void UnhookEvents()
        {
            context.OnHealthChange -= OnHealthChange;
            context.OnBrokenCounterChange -= OnBrokenCounterChange;
            visibilityTimer?.Dispose();
            context = null;
        }

        private void Dispatch(Action f) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, f);

        #region Visibility timer
        private void StartVisibilityTimer()
        {
            if (!ComponentSettings.HidePartsAfterSeconds)
            {
                ApplySettings();
                return;
            }
            if (visibilityTimer == null)
            {
                visibilityTimer = new Timer(_ => HideInactiveBar(), null, 10, 0);
            }
            else
            {
                visibilityTimer.Change(ComponentSettings.SecondsToHideParts * 1000, 0);
            }
        }

        private void HideInactiveBar() => Dispatch(() =>
        {
            Visibility = Visibility.Collapsed;
        });

        #endregion

        #region Settings
        public void ApplySettings()
        {
            Visibility visibility = GetVisibility();
            if (ComponentSettings.HidePartsAfterSeconds) visibility = Visibility.Collapsed;
            Dispatch(() => { Visibility = visibility; });
        }

        private Visibility GetVisibility()
        {
            if (context.IsRemovable)
            {
                return ComponentSettings.EnableRemovableParts ? Visibility.Visible : Visibility.Collapsed;
            }

            if (ComponentSettings.EnableMonsterParts && ComponentSettings.EnabledPartGroups.Contains(context.Group))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        #endregion

        #region Events

        private void SetPartInformation(double newSize)
        {
            PartName.Text = $"{context.Name}";
            UpdatePartBrokenCounter();
            UpdateHealthSize(newSize);
            UpdateHealthText();
            ApplySettings();
        }

        private void OnBrokenCounterChange(object source, MonsterPartEventArgs args)
        {
            Visibility visibility = GetVisibility();
            Dispatch(() =>
            {
                UpdatePartBrokenCounter();
                Visibility = visibility;
                StartVisibilityTimer();
            });
        }

        private void OnHealthChange(object source, MonsterPartEventArgs args)
        {
            Visibility visibility = GetVisibility();
            Dispatch(() =>
            {
                UpdateHealthText();
                Visibility = visibility;
                StartVisibilityTimer();
            });
        }

        public void UpdateHealthBarSize(double newSize)
        {
            if (context == null) return;
            UpdateHealthSize(newSize);
            UpdateHealthText();
            ApplySettings();
        }

        private void UpdatePartBrokenCounter()
        {
            string suffix = "";
            for (int i = context.BreakThresholds.Length - 1; i >= 0; i--)
            {
                int threshold = context.BreakThresholds[i];
                if (context.BrokenCounter < threshold || i == context.BreakThresholds.Length - 1)
                {
                    suffix = $"/{threshold}";
                    if (i < context.BreakThresholds.Length - 1)
                    {
                        suffix += "+";
                    }
                }
            }
            PartBrokenCounter.Text = $"{context.BrokenCounter}{suffix}";
        }

        public void UpdateHealthSize(double newSize)
        {
            PartHealth.MaxSize = newSize - 37;
            PartHealth.MaxHealth = context.TotalHealth;
            PartHealth.Health = context.Health;
        }

        private void UpdateHealthText()
        {
            PartHealth.MaxHealth = context.TotalHealth;
            PartHealth.Health = context.Health;
            PartHealthText.Text = $"{context.Health:0}/{context.TotalHealth:0}";
        }

        #endregion
    }
}
