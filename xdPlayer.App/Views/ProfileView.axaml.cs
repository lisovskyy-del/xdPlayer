using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Specialized;
using System.Linq;
using xdPlayer.App.ViewModels;

namespace xdPlayer.App.Views;

public partial class ProfileView : UserControl
{
    private ProfileViewModel? _vm;
    private Canvas? _chartCanvas;

    public ProfileView()
    {
        if (!Design.IsDesignMode)
            DataContext = App.Services?.GetRequiredService<ProfileViewModel>();

        InitializeComponent();

        if (Design.IsDesignMode)
            DataContext = new ProfileViewModel();

        DataContextChanged += (_, _) => AttachChartHandler();

        _chartCanvas = this.FindControl<Canvas>("ChartCanvas");
        if (_chartCanvas != null)
            _chartCanvas.SizeChanged += (_, _) => DrawChart();

        AttachChartHandler();
    }

    private void AttachChartHandler()
    {
        if (_vm != null)
            _vm.ChartData.CollectionChanged -= OnChartDataChanged;

        _vm = DataContext as ProfileViewModel;

        if (_vm != null)
            _vm.ChartData.CollectionChanged += OnChartDataChanged;

        DrawChart();
    }

    private void OnChartDataChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(DrawChart);
    }

    private void DrawChart()
    {
        var canvas = _chartCanvas ?? this.FindControl<Canvas>("ChartCanvas");
        if (canvas == null || _vm == null) return;

        canvas.Children.Clear();

        var data = _vm.ChartData.ToList();
        if (data.Count < 2) return;

        double width = canvas.Bounds.Width;
        double height = canvas.Bounds.Height;

        if (width <= 0 || height <= 0) return;

        var maxValue = Math.Max(data.Max(d => d.Count), 1);
        var stepX = width / (data.Count - 1);

        var linePoints = new Points();
        var areaPoints = new Points();

        areaPoints.Add(new Point(0, height));

        for (int i = 0; i < data.Count; i++)
        {
            var x = i * stepX;
            var y = height - (data[i].Count / (double)maxValue) * (height - 20);

            linePoints.Add(new Point(x, y));
            areaPoints.Add(new Point(x, y));
        }

        areaPoints.Add(new Point(width, height));

        var area = new Polygon
        {
            Points = areaPoints,
            Fill = new SolidColorBrush(Color.Parse("#1FFFFFFF"))
        };

        var line = new Polyline
        {
            Points = linePoints,
            Stroke = new SolidColorBrush(Color.Parse("#CCCCCC")),
            StrokeThickness = 2
        };

        canvas.Children.Add(area);
        canvas.Children.Add(line);

        var labelCount = Math.Min(data.Count, 6);
        var labelStep = Math.Max(1, data.Count / labelCount);

        for (int i = 0; i < data.Count; i += labelStep)
        {
            var x = i * stepX;
            var label = new TextBlock
            {
                Text = data[i].Date.ToString("MMM d"),
                FontSize = 9,
                Opacity = 0.5,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(label, Math.Max(0, x - 15));
            Canvas.SetTop(label, height + 4);
            canvas.Children.Add(label);
        }
    }
}