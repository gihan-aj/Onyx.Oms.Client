using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop.Features.Reports;

public sealed partial class LedgerRow : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(LedgerRow),
            new PropertyMetadata(string.Empty, OnPropertyChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(LedgerRow),
            new PropertyMetadata(string.Empty, OnPropertyChanged));

    public static readonly DependencyProperty IsPositiveProperty =
        DependencyProperty.Register(nameof(IsPositive), typeof(bool), typeof(LedgerRow),
            new PropertyMetadata(true, OnPropertyChanged));

    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public string Value { get => (string)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public bool IsPositive { get => (bool)GetValue(IsPositiveProperty); set => SetValue(IsPositiveProperty, value); }

    public LedgerRow()
    {
        InitializeComponent();
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not LedgerRow row) return;
        row.LabelBlock.Text = row.Label;
        string sign = row.IsPositive ? "" : "− ";
        row.ValueBlock.Text = sign + row.Value;
        row.ValueBlock.Foreground = row.IsPositive
            ? (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"]
            : (Brush)Application.Current.Resources["SystemFillColorCautionBrush"];
    }
}
