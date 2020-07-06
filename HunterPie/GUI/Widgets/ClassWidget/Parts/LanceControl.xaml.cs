using System;
using JobEventArgs = HunterPie.Core.LPlayer.Jobs.JobEventArgs;
using Lance = HunterPie.Core.LPlayer.Jobs.Lance;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for LanceControl.xaml
    /// </summary>
    public partial class LanceControl : ClassControl
    {

        Lance Context;

        public LanceControl() => InitializeComponent();

        public void SetContext(Lance ctx)
        {
            Context = ctx;
            UpdateInformation();
            HookEvents();
        }

        private void UpdateInformation() => OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));

        private void HookEvents() => Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;

        public override void UnhookEvents() => Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;

        #region Game Events
        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     HasSafiBuff = args.SafijiivaRegenCounter != -1;
                                                                                     SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
                                                                                 }));
        #endregion

    }
}
