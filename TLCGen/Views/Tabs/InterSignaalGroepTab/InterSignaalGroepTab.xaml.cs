﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
using TLCGen.Extensions;
using TLCGen.ViewModels.Enums;

namespace TLCGen.Views
{
    /// <summary>
    /// Interaction logic for ConflictMatrixView.xaml
    /// </summary>
    public partial class InterSignaalGroepTab : UserControl
    {
        public InterSignaalGroepTab()
        {
            InitializeComponent();
        }
    }

    public class SelectedDisplayTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IntersignaalGroepTypeEnum e1 = (IntersignaalGroepTypeEnum)value;
            IntersignaalGroepTypeEnum e2 = (IntersignaalGroepTypeEnum)parameter;
            if (e2.HasFlag(e1))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
