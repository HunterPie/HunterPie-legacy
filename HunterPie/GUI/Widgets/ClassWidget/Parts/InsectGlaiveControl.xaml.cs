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
using InsectGlaive = HunterPie.Core.LPlayer.Jobs.InsectGlaive;
using InsectGlaiveEventArgs = HunterPie.Core.LPlayer.Jobs.InsectGlaiveEventArgs;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for InsectGlaiveControl.xaml
    /// </summary>
    public partial class InsectGlaiveControl : ClassControl
    {

        InsectGlaive Context;

        public InsectGlaiveControl()
        {
            InitializeComponent();
        }

        public void SetContext(InsectGlaive ctx)
        {
            Context = ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnRedBuffUpdate += OnRedBuffUpdate;
            Context.OnWhiteBuffUpdate += OnWhiteBuffUpdate;
            Context.OnOrangeBuffUpdate += OnOrangeBuffUpdate;
        }

        private void OnOrangeBuffUpdate(object source, InsectGlaiveEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void OnWhiteBuffUpdate(object source, InsectGlaiveEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void OnRedBuffUpdate(object source, InsectGlaiveEventArgs args)
        {
            throw new NotImplementedException();
        }

        public override void UnhookEvents()
        {
            base.UnhookEvents();
        }
    }
}
