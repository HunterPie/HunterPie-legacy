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
            throw new NotImplementedException();
        }

        private void OnWeaponChange(object source, EventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
