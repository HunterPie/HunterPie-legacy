using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Fertilizer = HunterPie.Core.Fertilizer;
using FertilizerEventArgs = HunterPie.Core.FertilizerEventArgs;

namespace HunterPie.GUI.Widgets.Harvest_Box.Parts
{
    /// <summary>
    /// Interaction logic for FertilizerControl.xaml
    /// </summary>
    public partial class FertilizerControl : UserControl
    {
        // TODO: Finish this

        // Animations
        Storyboard ANIM_FERTILIZER_EXPIRE;

        Fertilizer Context;

        public FertilizerControl() => InitializeComponent();

        public void SetContext(Fertilizer ctx)
        {
            Context = ctx;
            ANIM_FERTILIZER_EXPIRE = FindResource("FertilizerExpiring") as Storyboard;
            HookEvents();
        }

        public void HookEvents()
        {
            Context.OnFertilizerChange += OnFertilizerChange;
            Context.OnAmountUpdate += OnAmountUpdate;
        }

        public void UnhookEvents()
        {
            Context.OnFertilizerChange -= OnFertilizerChange;
            Context.OnAmountUpdate -= OnAmountUpdate;
        }

        private void OnAmountUpdate(object source, FertilizerEventArgs args)
        {
            bool ApplyExpiringAnimation = args.Amount <= 4;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                if (ApplyExpiringAnimation)
                {
                    ANIM_FERTILIZER_EXPIRE.Begin(FertilizerName, true);
                    ANIM_FERTILIZER_EXPIRE.Begin(FertilizerAmount, true);
                }
                else
                {
                    ANIM_FERTILIZER_EXPIRE.Remove(FertilizerName);
                    ANIM_FERTILIZER_EXPIRE.Remove(FertilizerAmount);
                }
                FertilizerAmount.Text = $"x{args.Amount}";
            }));
        }

        private void OnFertilizerChange(object source, FertilizerEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
        {
            FertilizerName.Text = args.Name;
            if (args.ID <= 0 || args.ID > 8) FertilizerIcon.Source = null;
            else { FertilizerIcon.Source = FindResource($"ICON_FERTILIZER_{args.ID}") as ImageSource; }
        }));

        public void SetMode(bool IsCompact)
        {
            if (IsCompact)
            {
                FertilizerName.Visibility = Visibility.Collapsed;
                FertilizerHolder.Width = 55;
            }
            else
            {
                FertilizerName.Visibility = Visibility.Visible;
                FertilizerHolder.Width = 230;
            }
        }
    }
}
