using System;
using System.Reflection;
using System.Windows;

namespace HunterPie.Utils
{
    public static class DesignUtils
    {
        public static bool IsInDesignMode => System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
    }
}
