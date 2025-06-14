using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Control
{
    public sealed partial class GridTable : UserControl
    {
        private List<Tuple<string, GridLength>> _columns = [];
        public List<Tuple<string, GridLength>> Columns { get => _columns; set => _columns = value; }
        private int _selectedIndex = -1;
        public int SelectedIndex { get => _selectedIndex; set { ChangeSelection(value, this); } }

        private int orderByColumn = 0;
        private bool ascending = true;

        private readonly List<Dictionary<string, string?>> _content = [];
        private readonly List<Dictionary<string, string?>> _orderedContent = [];

        private readonly Dictionary<Rectangle, int> rowIndices = [];
        private readonly Dictionary<StackPanel, int> headerIndices = [];

        private readonly List<Rectangle> rectangles = [];
        private readonly List<StackPanel> headers = [];

        public delegate void RowEventHandler(object sender, RowEvent e);

        public event RowEventHandler? SelectionChanged;
        public event RowEventHandler? RowDoubleTapped;

        public GridTable()
        {
            InitializeComponent();
        }

        public Dictionary<string, string?> GetRow(int index)
        {
            return _content[index];
        }

        public void AddRow(Dictionary<string, string?> content)
        {
            _content.Add(content);
        }

        public void Clear()
        {
            _content.Clear();
            _orderedContent.Clear();
        }

        public void ShowContent()
        {
            rowIndices.Clear();
            headerIndices.Clear();
            rectangles.Clear();
            headers.Clear();
            HeaderGrid.Children.Clear();
            HeaderGrid.ColumnDefinitions.Clear();
            GridTableGrid.Children.Clear();
            GridTableGrid.ColumnDefinitions.Clear();
            _selectedIndex = -1;
            for (int i = 0; i < _columns.Count; i++)
            {
                GridTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = _columns[i].Item2 });
                HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = _columns[i].Item2 });
            }
            GridTableGrid.RowDefinitions.Clear();
            for (int i = 0; i < _content.Count; i++)
            {
                GridTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int i = 0; i < _columns.Count; i++)
            {
                var header = new StackPanel { Orientation = Orientation.Horizontal };
                var headerText = new TextBlock { Text = _columns[i].Item1, Padding = new Thickness(10) };
                header.PointerEntered += OnHeaderHover;
                header.PointerExited += OnHeaderHoverExit;
                header.Tapped += OnHeaderTap;
                header.Children.Add(headerText);
                if (i == orderByColumn)
                {
                    CreateOrderIcon(header);
                }
                HeaderGrid.Children.Add(header);
                Grid.SetColumn(header, i);
                Grid.SetRow(header, 0);
                headers.Add(header);
                headerIndices.Add(header, i);
            }

            for (int i = 0; i < _content.Count; i++)
            {
                Rectangle rowBackground = new()
                {
                    Margin = new Thickness(2),
                    Fill = (Brush)Application.Current.Resources["SubtleFillColorTransparentBrush"],
                    StrokeThickness = 1,
                    RadiusX = ((CornerRadius)Application.Current.Resources["ControlCornerRadius"]).TopLeft,
                    RadiusY = ((CornerRadius)Application.Current.Resources["ControlCornerRadius"]).TopLeft,
                };

                rowBackground.PointerEntered += OnBackgroundHover;
                rowBackground.PointerExited += OnBackgroundHoverExit;
                rowBackground.Tapped += OnRowTap;
                rowBackground.DoubleTapped += OnRowDoubleTap;

                Grid.SetRow(rowBackground, i);
                Grid.SetColumn(rowBackground, 0);
                Grid.SetColumnSpan(rowBackground, _columns.Count);

                rowIndices[rowBackground] = i;
                rectangles.Add(rowBackground);
                GridTableGrid.Children.Add(rowBackground);
                for (int j = 0; j < _columns.Count; j++)
                {
                    var text = new TextBlock
                    {
                        Text = _content[i].GetValueOrDefault(_columns[j].Item1, null),
                        Padding = new Thickness(10),
                        IsHitTestVisible = false,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    GridTableGrid.Children.Add(text);
                    Grid.SetColumn(text, j);
                    Grid.SetRow(text, i);
                }
            }
        }

        private void CreateOrderIcon(StackPanel header)
        {
            FontIcon icon = new()
            {
                Glyph = ascending ? "\uE70D" : "\uE70E"
            };
            header.Children.Add(icon);
        }

        private void OnHeaderHover(object sender, RoutedEventArgs e)
        {
            if (sender is StackPanel header && orderByColumn != headerIndices[header])
            {
                FontIcon icon = new()
                {
                    Glyph = "\uE70D",
                    Foreground = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100))
                };
                header.Children.Add(icon);
            }
        }

        private void OnHeaderHoverExit(object sender, RoutedEventArgs e)
        {
            if (sender is StackPanel header && orderByColumn != headerIndices[header])
            {
                header.Children.RemoveAt(1);
            }
        }

        private void OnHeaderTap(object sender, RoutedEventArgs e)
        {
            if (sender is StackPanel header)
            {
                headers[orderByColumn].Children.RemoveAt(1);
                if (orderByColumn != headerIndices[header])
                {
                    header.Children.RemoveAt(1);
                    ascending = true;
                    orderByColumn = headerIndices[header];
                    CreateOrderIcon(header);
                }
                else
                {
                    ascending = !ascending;
                    CreateOrderIcon(header);
                }
            }
        }

        private void OnBackgroundHover(object sender, RoutedEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                if (SelectedIndex != rowIndices.GetValueOrDefault(rect, -1))
                {
                    rect.Fill = (Brush)Application.Current.Resources["SubtleFillColorSecondaryBrush"];
                }
            }
        }

        private void OnBackgroundHoverExit(object sender, RoutedEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                if (SelectedIndex != rowIndices.GetValueOrDefault(rect, -1))
                {
                    rect.Fill = (Brush)Application.Current.Resources["SubtleFillColorTransparentBrush"];
                }
            }
        }

        private void OnRowTap(object sender, RoutedEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                ChangeSelection(rowIndices[rect], sender);
            }
        }

        private void ChangeSelection(int index, object sender)
        {
            if (SelectedIndex >= 0)
            {
                rectangles[SelectedIndex].Stroke = null;
                rectangles[SelectedIndex].Fill = (Brush)Application.Current.Resources["SubtleFillColorTransparentBrush"];
            }
            _selectedIndex = index;
            rectangles[SelectedIndex].Stroke = (Brush)Application.Current.Resources["FocusStrokeColorOuterBrush"];
            rectangles[SelectedIndex].Fill = (Brush)Application.Current.Resources["SubtleFillColorSecondaryBrush"];
            SelectionChanged?.Invoke(sender, new RowEvent(SelectedIndex));
        }

        private void OnRowDoubleTap(object sender, RoutedEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                RowDoubleTapped?.Invoke(sender, new RowEvent(rowIndices[rect]));
            }
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.Key == Windows.System.VirtualKey.Down && SelectedIndex < _content.Count - 1)
                {
                    SelectedIndex++;
                    e.Handled = true;
                    rectangles[SelectedIndex].StartBringIntoView();
                }
                else if (e.Key == Windows.System.VirtualKey.Up && SelectedIndex > 0)
                {
                    SelectedIndex--;
                    e.Handled = true;
                    rectangles[SelectedIndex].StartBringIntoView();
                }
                else if (e.Key == Windows.System.VirtualKey.Enter && SelectedIndex >= 0 && SelectedIndex < _content.Count)
                {
                    RowDoubleTapped?.Invoke(sender, new RowEvent(SelectedIndex));
                }
            }
        }
    }

    public class RowEvent(int index)
    {
        public int Index { get; private set; } = index;
    }
}
