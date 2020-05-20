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
using JobEventArgs = HunterPie.Core.LPlayer.Jobs.JobEventArgs;
using Longsword = HunterPie.Core.LPlayer.Jobs.Longsword;
using LongswordEventArgs = HunterPie.Core.LPlayer.Jobs.LongswordEventArgs;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for LongswordControl.xaml
    /// </summary>
    public partial class LongswordControl : ClassControl
    {
        Longsword Context;

        public LongswordControl()
        {
            InitializeComponent();
        }

        public void SetContext(Longsword ctx)
        {
            Context = ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnChargeLevelChange += OnChargeLevelChange;
            Context.OnInnerGaugeChange += OnInnerGaugeChange;
            Context.OnOuterGaugeChange += OnOuterGaugeChange;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
        }

        public override void UnhookEvents()
        {
            Context.OnChargeLevelChange -= OnChargeLevelChange;
            Context.OnInnerGaugeChange -= OnInnerGaugeChange;
            Context.OnOuterGaugeChange -= OnOuterGaugeChange;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
            Context = null;
            base.UnhookEvents();
        }


        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void OnOuterGaugeChange(object source, LongswordEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void OnInnerGaugeChange(object source, LongswordEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void OnChargeLevelChange(object source, LongswordEventArgs args)
        {
            throw new NotImplementedException();
        }

    }
}
