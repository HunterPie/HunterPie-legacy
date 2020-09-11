using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HunterPie.Logger;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts.Components
{
    /// <summary>
    /// Interaction logic for NoteComponent.xaml
    /// </summary>
    public partial class NoteComponent : UserControl, IDisposable
    {

        private byte noteId { get; set; }
        public byte NoteId
        {
            get => noteId;
            set
            {
                if (value != noteId)
                {
                    
                    DrawingImage icon = null;
                    switch (value)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            icon = FindResource($"ICON_NOTE_{value}") as DrawingImage;
                            break;
                    }
                    NoteIcon = icon;
                }
            }
        }

        public DrawingImage NoteIcon
        {
            get { return (DrawingImage)GetValue(NoteIconProperty); }
            set { SetValue(NoteIconProperty, value); }
        }
        public static readonly DependencyProperty NoteIconProperty =
            DependencyProperty.Register("NoteIcon", typeof(DrawingImage), typeof(NoteComponent));

        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set {
                SetValue(ColorProperty, value);
                if (NoteId != 4)
                {
                    UpdateColorFromDrawingCopy();
                }               
            }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(NoteComponent));

        public bool Destroy
        {
            get { return (bool)GetValue(DestroyProperty); }
            set { SetValue(DestroyProperty, value); }
        }
        public static readonly DependencyProperty DestroyProperty =
            DependencyProperty.Register("Destroy", typeof(bool), typeof(NoteComponent));

        public NoteComponent()
        {
            InitializeComponent();
        }

        private void UpdateColorFromDrawingCopy()
        {
            // I'm sorry for this hacky way, bUT APPARENTLY DYNAMICRESOURCE IS DUMB and didn't work properly
            var newDrawingImg = NoteIcon.Clone();
            var newDrawingGroup = (DrawingGroup)newDrawingImg.Drawing;
            ((GeometryDrawing)newDrawingGroup.Children[0]).Brush = Color;
            if (newDrawingImg.CanFreeze)
            {
                newDrawingImg.Freeze();
            }
            NoteIcon = newDrawingImg;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Destroy = true;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        private void OnDestroyAnimationComplete(object sender, EventArgs e)
        {
            ((Panel)Parent).Children.Remove(this);
        }
    }
}
