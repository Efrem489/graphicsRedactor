using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using VectorEditor.Services;
using VectorEditor.Shapes;
using VectorLine = VectorEditor.Shapes.Line;

namespace VectorEditor
{
    /// <summary>
    /// Главное окно векторного редактора.
    /// Предоставляет функциональность для создания, редактирования и управления ломаными линиями.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Figure> _figures = new List<Figure>();
        private readonly IVectorDrawingStorageService _storageService;
        private Figure? _selectedFigure;
        private readonly List<Ellipse> _handles = new List<Ellipse>();
        private bool _isCreatingLine;
        private bool _isDraggingWhole;
        private bool _isDraggingPoint;
        private int _draggingPointIndex = -1;
        private Point _dragStartPosition;
        private Polyline? _previewPolyline;
        private Ellipse? _previewHandle;
        private Point _firstPoint;
        private bool _isInPreviewMode;

        public MainWindow()
        {
            InitializeComponent();
            _storageService = new XmlVectorDrawingStorageService();
            colorComboBox.SelectedIndex = 0;
        }

        private void CreateLineButton_Click(object sender, RoutedEventArgs e)
        {
            _isCreatingLine = true;
            _isInPreviewMode = false;
            DeselectCurrent();
            ClearPreview();
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFigure == null)
                return;

            _figures.Remove(_selectedFigure);
            drawingCanvas.Children.Remove(_selectedFigure.Visual);
            DeselectCurrent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Преобразуем фигуры в их данные
            var data = _figures
                .Select(f => f.GetData())
                .ToList();

            _storageService.Save(data);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var loadedData = _storageService.Load();
            if (loadedData == null)
                return;

            drawingCanvas.Children.Clear();
            _figures.Clear();

            foreach (var data in loadedData)
            {
                // Используем фабрику для создания фигуры
                var figure = FigureFactory.CreateFigure(data);
                _figures.Add(figure);
                drawingCanvas.Children.Add(figure.Visual);
            }
        }

        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _selectedFigure?.SetThickness(e.NewValue);
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedFigure == null)
                return;

            if (colorComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not string colorString)
                return;

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorString);
                _selectedFigure.SetColor(color);
            }
            catch
            {
                // Игнорируем ошибку преобразования цвета
            }
        }

        private void StartPreview(Point position)
        {
            _isInPreviewMode = true;
            _firstPoint = position;

            _previewHandle = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.Blue
            };
            Canvas.SetLeft(_previewHandle, position.X - 4);
            Canvas.SetTop(_previewHandle, position.Y - 4);
            drawingCanvas.Children.Add(_previewHandle);

            _previewPolyline = new Polyline
            {
                Stroke = new SolidColorBrush(GetSelectedColor()),
                StrokeThickness = thicknessSlider.Value,
                Points = new PointCollection { position, position }
            };
            drawingCanvas.Children.Add(_previewPolyline);
        }

        private void UpdatePreview(Point currentPosition)
        {
            if (_previewPolyline == null || _previewPolyline.Points.Count != 2)
                return;

            _previewPolyline.Points[1] = currentPosition;
        }

        private void ClearPreview()
        {
            if (_previewHandle != null && drawingCanvas.Children.Contains(_previewHandle))
            {
                drawingCanvas.Children.Remove(_previewHandle);
                _previewHandle = null;
            }

            if (_previewPolyline != null && drawingCanvas.Children.Contains(_previewPolyline))
            {
                drawingCanvas.Children.Remove(_previewPolyline);
                _previewPolyline = null;
            }

            _isInPreviewMode = false;
        }

        private void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(drawingCanvas);

            if (_isCreatingLine)
            {
                if (!_isInPreviewMode)
                {
                    StartPreview(position);
                    e.Handled = true;
                    return;
                }

                // Создаем данные для линии
                var lineData = new BrokenLineData
                {
                    Points = new List<Point> { _firstPoint, position },
                    Thickness = thicknessSlider.Value,
                    StrokeColor = GetSelectedColor()
                };

                // Создаем фигуру через фабрику
                var line = FigureFactory.CreateFigure(lineData);
                _figures.Add(line);
                drawingCanvas.Children.Add(line.Visual);
                SelectFigure(line);
                ClearPreview();
                _isCreatingLine = false;
                e.Handled = true;
                return;
            }

            var hitResult = VisualTreeHelper.HitTest(drawingCanvas, position);
            if (hitResult?.VisualHit == null)
            {
                DeselectCurrent();
                if (_isCreatingLine)
                {
                    ClearPreview();
                    _isCreatingLine = false;
                }
                return;
            }

            if (hitResult.VisualHit is Ellipse handle)
            {
                _isDraggingPoint = true;
                _draggingPointIndex = _handles.IndexOf(handle);
                _dragStartPosition = position;
                handle.CaptureMouse();
                e.Handled = true;
                return;
            }

            if (hitResult.VisualHit is Shape shape)
            {
                var figure = _figures.FirstOrDefault(f => f.MatchesShape(shape));
                if (figure == null)
                    return;

                if (e.ClickCount == 2)
                {
                    figure.InsertPoint(position);
                    UpdateHandles();
                    e.Handled = true;
                    return;
                }

                SelectFigure(figure);
                _isDraggingWhole = true;
                _dragStartPosition = position;
                shape.CaptureMouse();
                e.Handled = true;
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(drawingCanvas);

            if (_isInPreviewMode)
            {
                UpdatePreview(position);
                return;
            }

            if (_isDraggingPoint && _selectedFigure != null && _draggingPointIndex >= 0 &&
                _draggingPointIndex < _selectedFigure.GetPoints().Count)
            {
                var delta = new Vector(position.X - _dragStartPosition.X, position.Y - _dragStartPosition.Y);
                var currentPoints = _selectedFigure.GetPoints();
                var newPoint = currentPoints[_draggingPointIndex] + delta;
                _selectedFigure.MovePoint(_draggingPointIndex, newPoint);

                if (_draggingPointIndex < _handles.Count)
                {
                    var handle = _handles[_draggingPointIndex];
                    Canvas.SetLeft(handle, newPoint.X - 4);
                    Canvas.SetTop(handle, newPoint.Y - 4);
                }
                _dragStartPosition = position;
                return;
            }

            if (_isDraggingWhole && _selectedFigure != null)
            {
                var delta = new Vector(position.X - _dragStartPosition.X, position.Y - _dragStartPosition.Y);
                _selectedFigure.MoveAll(delta);
                UpdateHandlesPositions();
                _dragStartPosition = position;
            }
        }

        private void DrawingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingPoint)
            {
                _isDraggingPoint = false;
                if (_draggingPointIndex >= 0 && _draggingPointIndex < _handles.Count)
                    _handles[_draggingPointIndex].ReleaseMouseCapture();
                _draggingPointIndex = -1;
                return;
            }

            if (_isDraggingWhole)
            {
                _isDraggingWhole = false;
                if (_selectedFigure?.Visual is UIElement element)
                {
                    element.ReleaseMouseCapture();
                }
            }
        }

        private void SelectFigure(Figure figure)
        {
            DeselectCurrent();
            _selectedFigure = figure;
            thicknessSlider.Value = figure.Thickness;

            foreach (ComboBoxItem item in colorComboBox.Items)
            {
                if (item.Tag is not string tagColorStr)
                    continue;

                try
                {
                    var tagColor = (Color)ColorConverter.ConvertFromString(tagColorStr);
                    if (tagColor == figure.StrokeColor)
                    {
                        colorComboBox.SelectedItem = item;
                        break;
                    }
                }
                catch
                {
                    // Игнорируем ошибку преобразования цвета
                }
            }
            UpdateHandles();
        }

        private void DeselectCurrent()
        {
            _selectedFigure = null;
            ClearHandles();
        }

        private void UpdateHandles()
        {
            ClearHandles();
            if (_selectedFigure == null)
                return;

            int index = 0;
            foreach (var point in _selectedFigure.GetPoints())
            {
                var handle = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.Blue,
                    Tag = index++
                };
                Canvas.SetLeft(handle, point.X - 4);
                Canvas.SetTop(handle, point.Y - 4);
                drawingCanvas.Children.Add(handle);
                _handles.Add(handle);
            }
        }

        private void UpdateHandlesPositions()
        {
            if (_selectedFigure == null)
                return;

            var points = _selectedFigure.GetPoints();
            for (int i = 0; i < points.Count && i < _handles.Count; i++)
            {
                var point = points[i];
                var handle = _handles[i];
                Canvas.SetLeft(handle, point.X - 4);
                Canvas.SetTop(handle, point.Y - 4);
            }
        }

        private void ClearHandles()
        {
            foreach (var handle in _handles)
                drawingCanvas.Children.Remove(handle);
            _handles.Clear();
        }

        private Color GetSelectedColor()
        {
            if (colorComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not string colorString)
                return Colors.Black;

            try
            {
                return (Color)ColorConverter.ConvertFromString(colorString);
            }
            catch
            {
                return Colors.Black;
            }
        }
    }
}
