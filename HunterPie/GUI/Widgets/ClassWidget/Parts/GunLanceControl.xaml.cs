using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GunLance = HunterPie.Core.LPlayer.Jobs.GunLance;
using GunLanceEventArgs = HunterPie.Core.LPlayer.Jobs.GunLanceEventArgs;
using JobEventArgs = HunterPie.Core.LPlayer.Jobs.JobEventArgs;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for GunLanceControl.xaml
    /// </summary>
    public partial class GunLanceControl : ClassControl
    {

        const string WyvernLoadedColor = "#FFFF8B00";
        const string BigAmmoLoadedColor = "#FF6EB7EB";
        const string BigAmmoNotLoadedColor = "#FFAE0000";

        public string NextWyvernstakeTimer
        {
            get => (string)GetValue(NextWyvernstakeTimerProperty);
            set => SetValue(NextWyvernstakeTimerProperty, value);
        }

        public static readonly DependencyProperty NextWyvernstakeTimerProperty =
            DependencyProperty.Register("NextWyvernstakeTimer", typeof(string), typeof(GunLanceControl));

        public double WyvernstakeTimerPercentage
        {
            get => (double)GetValue(WyvernstakeTimerPercentageProperty);
            set => SetValue(WyvernstakeTimerPercentageProperty, value);
        }

        public static readonly DependencyProperty WyvernstakeTimerPercentageProperty =
            DependencyProperty.Register("WyvernstakeTimerPercentage", typeof(double), typeof(GunLanceControl));

        public string WyvernstakeTimer
        {
            get => (string)GetValue(WyvernstakeTimerProperty);
            set => SetValue(WyvernstakeTimerProperty, value);
        }

        public static readonly DependencyProperty WyvernstakeTimerProperty =
            DependencyProperty.Register("WyvernstakeTimer", typeof(string), typeof(GunLanceControl));

        public double WyvernboomPercentage
        {
            get => (double)GetValue(WyvernboomPercentageProperty);
            set => SetValue(WyvernboomPercentageProperty, value);
        }

        public static readonly DependencyProperty WyvernboomPercentageProperty =
            DependencyProperty.Register("WyvernboomPercentage", typeof(double), typeof(GunLanceControl));


        public bool WyvernsfireReady
        {
            get => (bool)GetValue(WyvernsfireReadyProperty);
            set => SetValue(WyvernsfireReadyProperty, value);
        }

        // Using a DependencyProperty as the backing store for WyvernsfireReady.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WyvernsfireReadyProperty =
            DependencyProperty.Register("WyvernsfireReady", typeof(bool), typeof(GunLanceControl));

        public string WyvernsfireDiamondColor
        {
            get => (string)GetValue(WyvernsfireDiamondColorProperty);
            set => SetValue(WyvernsfireDiamondColorProperty, value);
        }

        public static readonly DependencyProperty WyvernsfireDiamondColorProperty =
            DependencyProperty.Register("WyvernsfireDiamondColor", typeof(string), typeof(GunLanceControl));

        public string BigAmmoShadowColor
        {
            get => (string)GetValue(BigAmmoShadowColorProperty);
            set => SetValue(BigAmmoShadowColorProperty, value);
        }

        public static readonly DependencyProperty BigAmmoShadowColorProperty =
            DependencyProperty.Register("BigAmmoShadowColor", typeof(string), typeof(GunLanceControl));

        public string BigAmmoImage
        {
            get => (string)GetValue(BigAmmoImageProperty);
            set => SetValue(BigAmmoImageProperty, value);
        }

        public static readonly DependencyProperty BigAmmoImageProperty =
            DependencyProperty.Register("BigAmmoImage", typeof(string), typeof(GunLanceControl));

        GunLance Context;

        public GunLanceControl() => InitializeComponent();

        public void SetContext(GunLance ctx)
        {
            Context = ctx;
            UpdateInformation();
            HookEvents();
        }

        private void UpdateInformation()
        {
            GunLanceEventArgs dummyArgs = new GunLanceEventArgs(Context);
            OnAmmoChange(this, dummyArgs);
            OnWyvernsFireTimerUpdate(this, dummyArgs);
            OnWyvernstakeBlastTimerUpdate(this, dummyArgs);
            OnBigAmmoChange(this, dummyArgs);
            OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));
        }

        private void HookEvents()
        {
            Context.OnAmmoChange += OnAmmoChange;
            Context.OnBigAmmoChange += OnBigAmmoChange;
            Context.OnTotalAmmoChange += OnAmmoChange;
            Context.OnTotalBigAmmoChange += OnBigAmmoChange;
            Context.OnWyvernsFireTimerUpdate += OnWyvernsFireTimerUpdate;
            Context.OnWyvernstakeBlastTimerUpdate += OnWyvernstakeBlastTimerUpdate;
            Context.OnWyvernstakeStateChanged += OnBigAmmoChange;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
        }

        public override void UnhookEvents()
        {
            Context.OnAmmoChange -= OnAmmoChange;
            Context.OnBigAmmoChange -= OnBigAmmoChange;
            Context.OnTotalAmmoChange -= OnAmmoChange;
            Context.OnTotalBigAmmoChange -= OnBigAmmoChange;
            Context.OnWyvernsFireTimerUpdate -= OnWyvernsFireTimerUpdate;
            Context.OnWyvernstakeBlastTimerUpdate -= OnWyvernstakeBlastTimerUpdate;
            Context.OnWyvernstakeStateChanged -= OnBigAmmoChange;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
            Context = null;
            AmmoHolder.Children.Clear();
        }


        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     HasSafiBuff = args.SafijiivaRegenCounter != -1;
                                                                                     SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
                                                                                 }));

        private void OnWyvernstakeBlastTimerUpdate(object source, GunLanceEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                           {
                                                                                               WyvernstakeTimerPercentage = args.WyvernstakeBlastTimer / args.WyvernstakeMax;
                                                                                               WyvernstakeTimer = args.WyvernstakeBlastTimer > 60 ? TimeSpan.FromSeconds(args.WyvernstakeBlastTimer).ToString("m\\:ss") :
                                                                                               TimeSpan.FromSeconds(args.WyvernstakeBlastTimer).ToString("ss");

                                                                                               if (args.WyvernstakeBlastTimer <= 0)
                                                                                               {
                                                                                                   WyvernstakeTimerPercentage = args.HasWyvernstakeLoaded ? 1 : 0;
                                                                                                   WyvernstakeTimer = args.HasWyvernstakeLoaded ? TimeSpan.FromSeconds(args.WyvernstakeNextMax).ToString("m\\:ss") : "00";
                                                                                               }
                                                                                           }));

        private void OnWyvernsFireTimerUpdate(object source, GunLanceEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                      {
                                                                                          WyvernsfireReady = args.WyvernsFireTimer <= 0;
                                                                                          WyvernsfireDiamondColor = WyvernsfireReady ? "#FF2FED55" : "#FFED2F2F";
                                                                                          WyvernboomPercentage = 1 - args.WyvernsFireTimer / 120;
                                                                                      }));

        private void OnBigAmmoChange(object source, GunLanceEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                             {
                                                                                 WyvernstakeTimerPercentage = args.HasWyvernstakeLoaded ? 1 : args.WyvernsFireTimer / Math.Max(1, args.WyvernstakeMax);
                                                                                 BigAmmoImage = args.HasWyvernstakeLoaded ? "pack://siteoforigin:,,,/HunterPie.Resources/UI/Class/GLanceWyvernstake.png" :
                                                                                 args.BigAmmo == 0 ? "pack://siteoforigin:,,,/HunterPie.Resources/UI/Class/GLanceBAmmoEmpty.png" : "pack://siteoforigin:,,,/HunterPie.Resources/UI/Class/GLanceBAmmo.png";
                                                                                 BigAmmoShadowColor = args.HasWyvernstakeLoaded ? WyvernLoadedColor : args.BigAmmo == 0 ? BigAmmoNotLoadedColor : BigAmmoLoadedColor;
                                                                                 WyvernstakeTimer = args.HasWyvernstakeLoaded ? TimeSpan.FromSeconds(args.WyvernstakeNextMax).ToString("m\\:ss") : "00";
                                                                                 NextWyvernstakeTimer = args.HasWyvernstakeLoaded ? TimeSpan.FromSeconds(args.WyvernstakeNextMax).ToString("m\\:ss") : null;
                                                                             }));

        private void OnAmmoChange(object source, GunLanceEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                          {
                                                                              DrawAmmo(args.Ammo, args.TotalAmmo - args.Ammo);
                                                                          }));

        private void DrawAmmo(int full, int empty)
        {
            AmmoHolder.Children.Clear();
            for (int i = 0; i < full; i++)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/HunterPie.Resources/UI/Class/GLanceAmmo.png", UriKind.Absolute)),
                    Height = 19
                };
                img.Source.Freeze();
                AmmoHolder.Children.Add(img);
            }
            for (int i = 0; i < empty; i++)
            {
                Image img = new Image()
                {
                    Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/HunterPie.Resources/UI/Class/GLanceAmmoEmpty.png", UriKind.RelativeOrAbsolute)),
                    Height = 19
                };
                img.Source.Freeze();
                AmmoHolder.Children.Add(img);
            }
        }

    }
}
