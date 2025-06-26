using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<int, int> _orderedToRawIndeces = [];
        private readonly Dictionary<int, int> _rawToOrderedIndeces = [];

        private readonly Dictionary<Rectangle, int> rowIndices = [];
        private readonly Dictionary<StackPanel, int> headerIndices = [];

        private readonly List<Rectangle> rectangles = [];
        public Dictionary<Rectangle, bool> RectangleColors { get; private set; } = [];
        public Dictionary<bool, IGridTableRowBrush> Colors { get; private set; } = new()
        {
            { false, new NormalBrush() },
            { true, new AccentBrush() }
        };
        private readonly List<StackPanel> headers = [];

        public delegate void RowEventHandler(object sender, RowEvent e);
        public delegate CommandBarFlyout RowRightTapEventHandler(object sender, RowEvent e);
        public delegate void RowAddedHandler(Rectangle row, RowEvent e);

        public event RowEventHandler? SelectionChanged;
        public event RowEventHandler? RowDoubleTapped;
        public event RowRightTapEventHandler? RowRightTapped;
        public event RowAddedHandler? RowAdded;

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
            _selectedIndex = -1;
        }

        public void ShowContent()
        {
            rowIndices.Clear();
            headerIndices.Clear();
            rectangles.Clear();
            RectangleColors.Clear();
            headers.Clear();
            HeaderGrid.Children.Clear();
            HeaderGrid.ColumnDefinitions.Clear();
            GridTableGrid.Children.Clear();
            GridTableGrid.ColumnDefinitions.Clear();
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

            OrderContent();

            for (int i = 0; i < _orderedContent.Count; i++)
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
                rowBackground.RightTapped += OnRowRightTap;

                Grid.SetRow(rowBackground, i);
                Grid.SetColumn(rowBackground, 0);
                Grid.SetColumnSpan(rowBackground, _columns.Count);

                rowIndices[rowBackground] = i;
                rectangles.Add(rowBackground);
                RectangleColors.Add(rowBackground, false);
                GridTableGrid.Children.Add(rowBackground);
                for (int j = 0; j < _columns.Count; j++)
                {
                    var text = new TextBlock
                    {
                        Text = _orderedContent[i].GetValueOrDefault(_columns[j].Item1, null),
                        Padding = new Thickness(10),
                        IsHitTestVisible = false,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    GridTableGrid.Children.Add(text);
                    Grid.SetColumn(text, j);
                    Grid.SetRow(text, i);
                }
                RowAdded?.Invoke(rowBackground, new RowEvent(_orderedToRawIndeces[i]));
            }

            if (SelectedIndex >= 0)
            {
                ChangeSelection(SelectedIndex, this);
                DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
                {
                    rectangles[_rawToOrderedIndeces[SelectedIndex]].StartBringIntoView();
                });
            }
        }

        public Rectangle GetRectangle(int index)
        {
            return rectangles[_rawToOrderedIndeces[index]];
        }

        private void OrderContent()
        {
            _orderedContent.Clear();
            if (ascending)
            {
                foreach (var item in _content.OrderBy(x => x[_columns[orderByColumn].Item1]))
                {
                    _orderedContent.Add(item);
                }
            }
            else
            {
                foreach (var item in _content.OrderByDescending(x => x[_columns[orderByColumn].Item1]))
                {
                    _orderedContent.Add(item);
                }
            }
            SetupOrderedToRaw();
        }

        private void SetupOrderedToRaw()
        {
            _orderedToRawIndeces.Clear();
            _rawToOrderedIndeces.Clear();
            for (int i = 0; i < _orderedContent.Count; i++)
            {
                for (int j = 0; j < _content.Count; j++)
                {
                    if (_orderedContent[i] == _content[j])
                    {
                        _orderedToRawIndeces.Add(i, j);
                        _rawToOrderedIndeces.Add(j, i);
                        break;
                    }
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
            if (sender is StackPanel header && orderByColumn != headerIndices[header] && header.Children.Count > 1)
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
                ShowContent();
            }
        }

        private void OnBackgroundHover(object sender, RoutedEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                if (SelectedIndex != _orderedToRawIndeces.GetValueOrDefault(rowIndices.GetValueOrDefault(rect, -1), -1))
                {
                    rect.Fill = Colors[RectangleColors[rect]].HoverFill;
                }
            }
        }

        private void OnBackgroundHoverExit(object sender, RoutedEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                if (SelectedIndex != _orderedToRawIndeces.GetValueOrDefault(rowIndices.GetValueOrDefault(rect, -1), -1))
                {
                    rect.Fill = Colors[RectangleColors[rect]].Fill;
                }
            }
        }

        private void OnRowTap(object sender, RoutedEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                ChangeSelection(_orderedToRawIndeces[rowIndices[rect]], sender);
            }
        }

        private void ChangeSelection(int index, object sender)
        {
            if (SelectedIndex >= 0)
            {
                rectangles[_rawToOrderedIndeces[SelectedIndex]].Stroke = null;
                rectangles[_rawToOrderedIndeces[SelectedIndex]].Fill = Colors[RectangleColors[rectangles[_rawToOrderedIndeces[SelectedIndex]]]].Fill;
            }
            _selectedIndex = index;
            rectangles[_rawToOrderedIndeces[SelectedIndex]].Stroke = Colors[RectangleColors[rectangles[_rawToOrderedIndeces[SelectedIndex]]]].Stroke;
            rectangles[_rawToOrderedIndeces[SelectedIndex]].Fill = Colors[RectangleColors[rectangles[_rawToOrderedIndeces[SelectedIndex]]]].HoverFill;
            SelectionChanged?.Invoke(sender, new RowEvent(SelectedIndex));
        }

        private void OnRowDoubleTap(object sender, RoutedEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                RowDoubleTapped?.Invoke(sender, new RowEvent(_orderedToRawIndeces[rowIndices[rect]]));
            }
        }

        private void OnRowRightTap(object sender, RightTappedRoutedEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                var flyout = RowRightTapped?.Invoke(sender, new RowEvent(_orderedToRawIndeces[rowIndices[rect]]));
                if (flyout != null)
                {
                    FlyoutShowOptions option = new()
                    {
                        ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway,
                        Position = e.GetPosition(rect),
                        Placement = FlyoutPlacementMode.RightEdgeAlignedTop
                    };
                    flyout.ShowAt(rect, option);
                }
            }
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.Key == Windows.System.VirtualKey.Down && _rawToOrderedIndeces[SelectedIndex] < _content.Count - 1)
                {
                    SelectedIndex = _orderedToRawIndeces[_rawToOrderedIndeces[SelectedIndex] + 1];
                    e.Handled = true;
                    rectangles[_rawToOrderedIndeces[SelectedIndex]].StartBringIntoView();
                }
                else if (e.Key == Windows.System.VirtualKey.Up && _rawToOrderedIndeces[SelectedIndex] > 0)
                {
                    SelectedIndex = _orderedToRawIndeces[_rawToOrderedIndeces[SelectedIndex] - 1];
                    e.Handled = true;
                    rectangles[_rawToOrderedIndeces[SelectedIndex]].StartBringIntoView();
                }
                else if (e.Key == Windows.System.VirtualKey.Enter && _rawToOrderedIndeces[SelectedIndex] >= 0 && _rawToOrderedIndeces[SelectedIndex] < _content.Count)
                {
                    RowDoubleTapped?.Invoke(sender, new RowEvent(SelectedIndex));
                }
            }
        }
    }

    public sealed class RowEvent(int index)
    {
        public int Index { get; private set; } = index;
    }

    public interface IGridTableRowBrush
    {
        Brush Fill { get; }
        Brush HoverFill { get; }
        Brush Stroke { get; }
    }

    public sealed class NormalBrush : IGridTableRowBrush
    {
        public Brush Fill { get; } = (Brush)Application.Current.Resources["SubtleFillColorTransparentBrush"];
        public Brush HoverFill { get; } = (Brush)Application.Current.Resources["SubtleFillColorSecondaryBrush"];
        public Brush Stroke { get; } = (Brush)Application.Current.Resources["FocusStrokeColorOuterBrush"];
    }

    public sealed class AccentBrush : IGridTableRowBrush
    {
        public Brush Fill { get => new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColorDark2"]); }
        public Brush HoverFill { get => new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColorDark1"]); }
        public Brush Stroke { get => (Brush)Application.Current.Resources["FocusStrokeColorOuterBrush"]; }
    }
}
