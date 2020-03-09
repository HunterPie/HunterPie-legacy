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
        string AcceptableType;
        Player Context;

        public AbnormalityContainer(Player context, string Type) {
            InitializeComponent();
            BaseWidth = Width;
            BaseHeight = Height;
            this.AcceptableType = Type;
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
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
            RemoveWindowTransparencyFlag();
        }

        public override void LeaveWidgetDesignMode() {
            base.LeaveWidgetDesignMode();
            this.ResizeMode = ResizeMode.CanResize;
            SizeToContent = SizeToContent.WidthAndHeight;
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
            if (this.AcceptableType != "*" && args.Abnormality.Type != this.AcceptableType) return;
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
                this.BuffTray.Children.Clear();
                if (this.ActiveAbnormalities.Count == 0) {
                    this.WidgetHasContent = false;
                }
                foreach(Parts.AbnormalityControl Abnorm in this.ActiveAbnormalities.Values) {
                    this.BuffTray.Children.Add(Abnorm);
                }
            }));
        }

        #endregion


        #region Window events

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            this.UnhookEvents();
        }

        private void OnMouseEnter(object sender, MouseEventArgs e) {
            this.MouseOver = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                this.MoveWidget();
            } else if (e.RightButton == MouseButtonState.Pressed) {

            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e) {
            if (this.MouseOver) {
                if (e.Delta > 0) {
                    //ScaleWidget(DefaultScaleX + 0.05, DefaultScaleY + 0.05);
                } else {
                    //ScaleWidget(DefaultScaleX - 0.05, DefaultScaleY - 0.05);
                }
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e) {
            this.MouseOver = false;
        }
        #endregion
        private void OnSizeChange(object sender, SizeChangedEventArgs e) {
            
            // This means the user didn't resize the widget
            if (this.BuffTray.ActualWidth + 4 == e.NewSize.Width && this.BuffTray.ActualHeight + 4 == e.NewSize.Height) return;
            // Only resize if in design mode
            if (!this.InDesignMode) return;
            // Resize depending on the orientation
            if (this.BuffTray.Orientation == Orientation.Horizontal) {
                if (e.NewSize.Width < 40) return;
                this.BuffTray.MaxWidth = e.NewSize.Width;
            } else {
                if (e.NewSize.Height < 40) return;
                this.BuffTray.MaxHeight = e.NewSize.Height;
            }
        }
    }
}
