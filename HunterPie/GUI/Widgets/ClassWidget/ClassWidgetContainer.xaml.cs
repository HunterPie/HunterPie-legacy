using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HunterPie.Core;

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
            // Clear this later
            WidgetHasContent = true;
            WidgetActive = true;
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

        private void SaveSettings()
        {
            // TODO: Settings
        }

        public override void ApplySettings(bool FocusTrigger = false)
        {
            // TODO: Apply settings
            base.ApplySettings(FocusTrigger);
        }

        private void SetContext(Game ctx)
        {
            Context = ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.Player.OnWeaponChange += OnWeaponChange;
            Context.Player.OnZoneChange += OnZoneChange;
        }

        private void UnhookEvents()
        {
            Context.Player.OnWeaponChange -= OnWeaponChange;
            Context.Player.OnZoneChange -= OnZoneChange;
            Context = null;
        }

        private void OnZoneChange(object source, EventArgs args)
        {
            //throw new NotImplementedException();
        }

        private void OnWeaponChange(object source, EventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                foreach (Parts.ClassControl control in Container.Children)
                {
                    control.UnhookEvents();
                }
                Container.Children.Clear();
                switch (Context.Player.WeaponID)
                {
                    case 9:
                        var control = new Parts.ChargeBladeControl();
                        control.SetContext(Context.Player.ChargeBlade);
                        Container.Children.Add(control);
                        break;
                }
            }));
            
        }


        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.MoveWidget();
                SaveSettings();
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookEvents();
            IsClosed = true;
        }

    }
}
