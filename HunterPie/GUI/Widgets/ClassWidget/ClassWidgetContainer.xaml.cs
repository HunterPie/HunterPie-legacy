using System;
using System.Windows.Input;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.GUI.Widgets.ClassWidget.Parts;
using Classes = HunterPie.Core.Enums.Classes;

namespace HunterPie.GUI.Widgets.ClassWidget
{
    /// <summary>
    /// Interaction logic for ClassWidgetContainer.xaml
    /// </summary>
    public partial class ClassWidgetContainer : Widget
    {
        Game Context;

        public ClassWidgetContainer(Game ctx)
        {
            WidgetType = 6;
            InitializeComponent();
            SetContext(ctx);
            SetWindowFlags();
        }

        public override void EnterWidgetDesignMode()
        {
            base.EnterWidgetDesignMode();
            RemoveWindowTransparencyFlag();
        }

        public override void LeaveWidgetDesignMode()
        {
            base.LeaveWidgetDesignMode();
            ApplyWindowTransparencyFlag();
            SaveSettings();
        }

        private void SaveSettings() => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                     {
                                         SaveSettingsBasedOnClass();
                                     }));

        public override void ApplySettings(bool FocusTrigger = false) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                       {
                                                                           if (IsClosed) return;
                                                                           ApplySettingsBasedOnClass();
                                                                           base.ApplySettings(FocusTrigger);
                                                                       }));

        private void ApplySettingsBasedOnClass()
        {
            UserSettings.Config.ClassesWidget classesConfig = UserSettings.PlayerConfig.Overlay.ClassesWidget;
            UserSettings.Config.WeaponHelperStructure config;
            switch ((Classes)Context.Player.WeaponID)
            {
                case Classes.DualBlades:
                    config = classesConfig.DualBladesHelper;
                    break;
                case Classes.LongSword:
                    config = classesConfig.LongSwordHelper;
                    break;
                case Classes.Hammer:
                    config = classesConfig.HammerHelper;
                    break;
                case Classes.Lance:
                    config = classesConfig.LanceHelper;
                    break;
                case Classes.GunLance:
                    config = classesConfig.GunLanceHelper;
                    break;
                case Classes.SwitchAxe:
                    config = classesConfig.SwitchAxeHelper;
                    break;
                case Classes.ChargeBlade:
                    config = classesConfig.ChargeBladeHelper;
                    break;
                case Classes.InsectGlaive:
                    config = classesConfig.InsectGlaiveHelper;
                    break;
                case Classes.Bow:
                    config = classesConfig.BowHelper;
                    break;
                default:
                    return;
            }
            WidgetActive = config.Enabled;
            Left = config.Position[0];
            Top = config.Position[1];
            ScaleWidget(config.Scale, config.Scale);
        }

        private void SaveSettingsBasedOnClass()
        {
            if (Context == null) return;
            UserSettings.Config.ClassesWidget classesConfig = UserSettings.PlayerConfig.Overlay.ClassesWidget;
            UserSettings.Config.WeaponHelperStructure config;
            switch ((Classes)Context.Player.WeaponID)
            {
                case Classes.DualBlades:
                    config = classesConfig.DualBladesHelper;
                    break;
                case Classes.LongSword:
                    config = classesConfig.LongSwordHelper;
                    break;
                case Classes.Hammer:
                    config = classesConfig.HammerHelper;
                    break;
                case Classes.Lance:
                    config = classesConfig.LanceHelper;
                    break;
                case Classes.GunLance:
                    config = classesConfig.GunLanceHelper;
                    break;
                case Classes.SwitchAxe:
                    config = classesConfig.SwitchAxeHelper;
                    break;
                case Classes.ChargeBlade:
                    config = classesConfig.ChargeBladeHelper;
                    break;
                case Classes.InsectGlaive:
                    config = classesConfig.InsectGlaiveHelper;
                    break;
                case Classes.Bow:
                    config = classesConfig.BowHelper;
                    break;
                default:
                    return;
            }
            config.Position[0] = (int)Left;
            config.Position[1] = (int)Top;
            config.Scale = (float)DefaultScaleX;
        }

        private void SetContext(Game ctx)
        {
            Context = ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.Player.OnClassChange += OnClassChange;
            Context.Player.OnZoneChange += OnZoneChange;
        }

        private void UnhookEvents()
        {
            foreach (ClassControl control in Container.Children)
            {
                control.UnhookEvents();
            }
            Container.Children.Clear();
            Context.Player.OnWeaponChange -= OnClassChange;
            Context.Player.OnZoneChange -= OnZoneChange;
            Context = null;
        }

        private void ScaleWidget(double newScaleX, double newScaleY)
        {
            if (newScaleX <= 0.2) return;
            Container.LayoutTransform = new ScaleTransform(newScaleX, newScaleY);
            DefaultScaleX = newScaleX;
            DefaultScaleY = newScaleY;
        }

        private void OnZoneChange(object source, EventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                  {
                                                                      WidgetHasContent = !Context.Player.InHarvestZone && !(Context.Player.ZoneID == 0);
                                                                      ChangeVisibility(false);
                                                                  }));

        private void OnClassChange(object source, EventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                   {
                                                                       foreach (ClassControl control in Container.Children)
                                                                       {
                                                                           control.UnhookEvents();
                                                                       }
                                                                       Container.Children.Clear();
                                                                       WidgetHasContent = !Context.Player.InHarvestZone;
                                                                       switch ((Classes)Context.Player.WeaponID)
                                                                       {
                                                                           case Classes.DualBlades:
                                                                               SetClassToDualBlades();
                                                                               break;
                                                                           case Classes.LongSword:
                                                                               SetClassLongSword();
                                                                               break;
                                                                           case Classes.Hammer:
                                                                               SetClassToHammer();
                                                                               break;
                                                                           case Classes.Lance:
                                                                               SetClassToLance();
                                                                               break;
                                                                           case Classes.GunLance:
                                                                               SetClassToGunLance();
                                                                               break;
                                                                           case Classes.SwitchAxe:
                                                                               SetClassToSwitchAxe();
                                                                               break;
                                                                           case Classes.ChargeBlade:
                                                                               SetClassToChargeBlade();
                                                                               break;
                                                                           case Classes.InsectGlaive:
                                                                               SetClassToInsectGlaive();
                                                                               break;
                                                                           case Classes.Bow:
                                                                               SetClassToBow();
                                                                               break;
                                                                           default:
                                                                               WidgetHasContent = false;
                                                                               break;
                                                                       }
                                                                   }));

        private void SetClassToLance()
        {
            var control = new LanceControl();
            control.SetContext(Context.Player.Lance);
            Container.Children.Add(control);

            ApplySettings();
        }

        private void SetClassToDualBlades()
        {
            var control = new DualBladeControl();
            control.SetContext(Context.Player.DualBlades);
            Container.Children.Add(control);

            ApplySettings();
        }

        private void SetClassToHammer()
        {
            var control = new HammerControl();
            control.SetContext(Context.Player.Hammer);
            Container.Children.Add(control);

            ApplySettings();
        }

        private void SetClassToBow()
        {
            var control = new BowControl();
            control.SetContext(Context.Player.Bow);
            Container.Children.Add(control);

            ApplySettings();
        }

        private void SetClassLongSword()
        {
            var control = new LongswordControl();
            control.SetContext(Context.Player.Longsword);
            Container.Children.Add(control);

            ApplySettings();
        }

        private void SetClassToGunLance()
        {
            var control = new GunLanceControl();
            control.SetContext(Context.Player.GunLance);
            Container.Children.Add(control);

            ApplySettings();
        }

        private void SetClassToSwitchAxe()
        {
            var control = new SwitchAxeControl();
            control.SetContext(Context.Player.SwitchAxe);
            Container.Children.Add(control);

            ApplySettings();
        }

        private void SetClassToChargeBlade()
        {
            // Add the control to the container
            var control = new ChargeBladeControl();
            control.SetContext(Context.Player.ChargeBlade);
            Container.Children.Add(control);

            // Apply charge blade widget settings
            ApplySettings();
        }

        private void SetClassToInsectGlaive()
        {
            var InsectGlaiveControl = new InsectGlaiveControl();
            InsectGlaiveControl.SetContext(Context.Player.InsectGlaive);
            Container.Children.Add(InsectGlaiveControl);

            ApplySettings();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                MoveWidget();
                SaveSettings();
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ScaleWidget(DefaultScaleX + 0.05, DefaultScaleY + 0.05);
            }
            else
            {
                ScaleWidget(DefaultScaleX - 0.05, DefaultScaleY - 0.05);
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookEvents();
            IsClosed = true;
        }

    }
}
