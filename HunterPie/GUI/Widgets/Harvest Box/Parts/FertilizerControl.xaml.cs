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
using System.Windows.Media.Animation;
using Fertilizer = HunterPie.Core.Fertilizer;
using FertilizerEventArgs = HunterPie.Core.FertilizerEventArgs;

namespace HunterPie.GUI.Widgets.Harvest_Box.Parts {
    /// <summary>
    /// Interaction logic for FertilizerControl.xaml
    /// </summary>
    public partial class FertilizerControl : UserControl {
        // TODO: Finish this

        // Animations
        Storyboard ANIM_FERTILIZER_EXPIRE;

        Fertilizer Context;

        public FertilizerControl() {
            InitializeComponent();
        }

        public void SetContext(Fertilizer ctx) {
            Context = ctx;
        }

        public void HookEvents() {
            Context.OnFertilizerChange += OnFertilizerChange;
            Context.OnAmountUpdate += OnAmountUpdate;
        }

        public void UnhookEvents() {
            Context.OnFertilizerChange -= OnFertilizerChange;
            Context.OnAmountUpdate -= OnAmountUpdate;
        }

        private void OnAmountUpdate(object source, FertilizerEventArgs args) {
            bool ApplyExpiringAnimation = args.Amount <= 4;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                if (ApplyExpiringAnimation) {
                    ANIM_FERTILIZER_EXPIRE.Begin(FertilizerName, true);
                    ANIM_FERTILIZER_EXPIRE.Begin(FertilizerAmount, true);
                } else {
                    ANIM_FERTILIZER_EXPIRE.Remove(FertilizerName);
                    ANIM_FERTILIZER_EXPIRE.Remove(FertilizerAmount);
                }
            }));
        }

        private void OnFertilizerChange(object source, FertilizerEventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                this.FertilizerName.Text = args.Name;
                this.FertilizerIcon.Source = FindResource($"ICON_FERTILIZER{args.ID}") as ImageSource;
            }));
        }
    }
}
