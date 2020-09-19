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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HunterPie.Core.Jobs;
using HunterPie.Logger;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for HeavyBowgunControl.xaml
    /// </summary>
    public partial class HeavyBowgunControl : ClassControl
    {

        HeavyBowgun Context { get; set; }

        public HeavyBowgunControl()
        {
            InitializeComponent();
        }

        public void SetContext(HeavyBowgun ctx)
        {
            Context = ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnAmmoCountChange += OnAmmoCountChange;
            Context.OnEquippedAmmoChange += OnEquippedAmmoChange;
            Context.OnScopeMultiplierChange += OnScopeMultiplierChange;
            Context.OnScopeStateChange += OnScopeStateChange;
            Context.OnWyvernheartUpdate += OnWyvernheartUpdate;
            Context.OnWyvernsnipeUpdate += OnWyvernsnipeUpdate;
        }

        public override void UnhookEvents()
        {
            Context.OnAmmoCountChange -= OnAmmoCountChange;
            Context.OnEquippedAmmoChange -= OnEquippedAmmoChange;
            Context.OnScopeMultiplierChange -= OnScopeMultiplierChange;
            Context.OnScopeStateChange -= OnScopeStateChange;
            Context.OnWyvernheartUpdate -= OnWyvernheartUpdate;
            Context.OnWyvernsnipeUpdate -= OnWyvernsnipeUpdate;
            base.UnhookEvents();
        }

        private void OnWyvernsnipeUpdate(object source, HeavyBowgunEventArgs args)
        {
        }

        private void OnWyvernheartUpdate(object source, HeavyBowgunEventArgs args)
        {
            Debugger.Log(args.WyvernheartTimer);
        }

        private void OnScopeStateChange(object source, HeavyBowgunEventArgs args)
        {
        }

        private void OnScopeMultiplierChange(object source, HeavyBowgunEventArgs args)
        {
        }

        private void OnEquippedAmmoChange(object source, HeavyBowgunEventArgs args)
        {
        }

        private void OnAmmoCountChange(object source, HeavyBowgunEventArgs args)
        {
        }
    }
}
