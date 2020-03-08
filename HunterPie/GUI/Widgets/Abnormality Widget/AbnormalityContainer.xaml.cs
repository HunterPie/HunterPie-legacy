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

namespace HunterPie.GUI.Widgets.Abnormality_Widget {
    /// <summary>
    /// Interaction logic for AbnormalityContainer.xaml
    /// </summary>
    public partial class AbnormalityContainer : Widget {

        Dictionary<string, Parts.AbnormalityControl> ActiveAbnormalities = new Dictionary<string, Parts.AbnormalityControl>();

        Player Context;

        public AbnormalityContainer(Player context) {
            InitializeComponent();
            BaseWidth = Width;
            BaseHeight = Height;
            ApplySettings();
            SetWindowFlags();
            SetContext(context);
        }

        public override void ApplySettings() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                this.WidgetHasContent = true;
                this.WidgetActive = true;
                base.ApplySettings();
            }));
        }

        private void SetContext(Player ctx) {
            Context = ctx;
            HookEvents();
        }

        public override void EnterWidgetDesignMode() {
            base.EnterWidgetDesignMode();
            RemoveWindowTransparencyFlag();
        }

        public override void LeaveWidgetDesignMode() {
            base.LeaveWidgetDesignMode();
            ApplyWindowTransparencyFlag();
            //SaveSettings();
        }

        #region Game events

        private void HookEvents() {
            Context.Abnormalities.OnNewAbnormality += OnPlayerNewAbnormality;
            Context.Abnormalities.OnAbnormalityRemove += OnPlayerAbnormalityEnd;
        }

        private void UnhookEvents() {
            Context.Abnormalities.OnNewAbnormality -= OnPlayerNewAbnormality;
            Context.Abnormalities.OnAbnormalityRemove -= OnPlayerAbnormalityEnd;
        }

        private void OnPlayerAbnormalityEnd(object source, AbnormalityEventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => {
                this.ActiveAbnormalities.Remove(args.Abnormality.InternalID);
                this.RedrawComponent();
            }));
        }

        private void OnPlayerNewAbnormality(object source, AbnormalityEventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => {
                Parts.AbnormalityControl AbnormalityBox = new Parts.AbnormalityControl(args.Abnormality);
                this.ActiveAbnormalities.Add(args.Abnormality.InternalID, AbnormalityBox);
                this.RedrawComponent();
            }));
        }

        #endregion

        #region Rendering

        private void RedrawComponent() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                this.HuntingHornBuffTray.Children.Clear();
                foreach (string key in this.ActiveAbnormalities.Keys) {
                    HuntingHornBuffTray.Children.Add(ActiveAbnormalities[key]);
                }
            }));
        }

        #endregion


        #region Window events

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            this.UnhookEvents();
        }

        private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            this.MouseOver = true;
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) {
                this.MoveWidget();
            }
        }

        private void OnMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (this.MouseOver) {
                if (e.Delta > 0) {
                    //ScaleWidget(DefaultScaleX + 0.05, DefaultScaleY + 0.05);
                } else {
                    //ScaleWidget(DefaultScaleX - 0.05, DefaultScaleY - 0.05);
                }
            }
        }

        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            this.MouseOver = false;
        }
        #endregion

    }
}
