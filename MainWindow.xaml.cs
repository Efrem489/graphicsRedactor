using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VectorEditor.Services;
using VectorLine = VectorEditor.Shapes.Line;

namespace VectorEditor
{
    /// <summary>
    /// Главное окно векторного редактора.
    /// Предоставляет функциональность для создания, редактирования и управления ломаными линиями.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<VectorLine> _lines = new List<VectorLine>();
        private readonly IVectorDrawingStorageService _storageService;
        private VectorLine? _selectedLine;
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

        /// <summary>
        /// Инициализирует новый экземпляр класса MainWindow.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _storageService = new XmlVectorDrawingStorageService();
            colorComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Обработчик события нажатия кнопки создания линии.
        /// Активирует режим создания новой ломаной линии.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void CreateLineButton_Click(object sender, RoutedEventArgs e)
        {
            _isCreatingLine = true;
            _isInPreviewMode = false;
            DeselectCurrent();
            ClearPreview();
        }

        /// <summary>
        /// Обработчик события нажатия кнопки удаления выбранной линии.
        /// Удаляет выбранную линию из коллекции данных и с холста.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedLine == null)
                return;

            _lines.Remove(_selectedLine);
            drawingCanvas.Children.Remove(_selectedLine.Polyline);
            DeselectCurrent();
        }

        /// <summary>
        /// Обработчик события нажатия кнопки сохранения файла.
        /// Сохраняет все линии через сервис хранения данных.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var data = _lines.Select(line => line.Data).ToList();
            _storageService.Save(data);
        }

        /// <summary>
        /// Обработчик события нажатия кнопки загрузки файла.
        /// Загружает линии из файла через сервис хранения данных.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var loadedData = _storageService.Load();
            if (loadedData == null)
                return;

            drawingCanvas.Children.Clear();
            _lines.Clear();
            
            foreach (var data in loadedData)
            {
                var line = new VectorLine(data);
                _lines.Add(line);
                drawingCanvas.Children.Add(line.Polyline);
            }
        }

        /// <summary>
        /// Обработчик события изменения значения слайдера толщины линии.
        /// Обновляет толщину выбранной линии.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события, содержащие новое значение.</param>
        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_selectedLine == null)
                return;

            _selectedLine.SetThickness(e.NewValue);
        }

        /// <summary>
        /// Обработчик события изменения выбора цвета в комбобоксе.
        /// Обновляет цвет выбранной линии.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события изменения выбора.</param>
        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedLine == null)
                return;

            if (colorComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not string colorString)
                return;

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorString);
                _selectedLine.SetColor(color);
            }
            catch
            {
                // Игнорируем ошибку преобразования цвета
            }
        }

        /// <summary>
        /// Начинает режим предпросмотра новой линии.
        /// Создает предварительный вид линии и маркер начальной точки.
        /// </summary>
        /// <param name="position">Позиция начальной точки линии.</param>
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

        /// <summary>
        /// Обновляет предпросмотр линии в соответствии с текущей позицией курсора.
        /// </summary>
        /// <param name="currentPosition">Текущая позиция курсора мыши.</param>
        private void UpdatePreview(Point currentPosition)
        {
            if (_previewPolyline == null || _previewPolyline.Points.Count != 2)
                return;

            _previewPolyline.Points[1] = currentPosition;
        }

        /// <summary>
        /// Очищает предпросмотр линии, удаляя предварительные элементы с холста.
        /// </summary>
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

        /// <summary>
        /// Обработчик события нажатия левой кнопки мыши на холсте.
        /// Обрабатывает создание новой линии, выбор существующей линии, начало перетаскивания точки или линии.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события мыши.</param>
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

                var newData = new BrokenLineData
                {
                    Points = new List<Point> { _firstPoint, position },
                    Thickness = thicknessSlider.Value,
                    StrokeColor = GetSelectedColor()
                };
                var line = new VectorLine(newData);
                _lines.Add(line);
                drawingCanvas.Children.Add(line.Polyline);
                SelectLine(line);
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

            if (hitResult.VisualHit is Polyline polyline)
            {
                var line = _lines.FirstOrDefault(l => l.MatchesPolyline(polyline));
                if (line == null)
                    return;

                if (e.ClickCount == 2)
                {
                    line.InsertPoint(position);
                    UpdateHandles();
                    e.Handled = true;
                    return;
                }

                SelectLine(line);
                _isDraggingWhole = true;
                _dragStartPosition = position;
                polyline.CaptureMouse();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик события перемещения мыши на холсте.
        /// Обрабатывает обновление предпросмотра линии, перетаскивание точки или всей линии.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события мыши.</param>
        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(drawingCanvas);

            if (_isInPreviewMode)
            {
                UpdatePreview(position);
                return;
            }

            if (_isDraggingPoint && _selectedLine != null && _draggingPointIndex >= 0 && _draggingPointIndex < _selectedLine.Data.Points.Count)
            {
                var delta = new Vector(position.X - _dragStartPosition.X, position.Y - _dragStartPosition.Y);
                var newPoint = _selectedLine.Data.Points[_draggingPointIndex] + delta;
                _selectedLine.MovePoint(_draggingPointIndex, newPoint);

                if (_draggingPointIndex < _handles.Count)
                {
                    var handle = _handles[_draggingPointIndex];
                    Canvas.SetLeft(handle, newPoint.X - 4);
                    Canvas.SetTop(handle, newPoint.Y - 4);
                }
                _dragStartPosition = position;
                return;
            }

            if (_isDraggingWhole && _selectedLine != null)
            {
                var delta = new Vector(position.X - _dragStartPosition.X, position.Y - _dragStartPosition.Y);
                _selectedLine.MoveAll(delta);
                UpdateHandlesPositions();
                _dragStartPosition = position;
            }
        }

        /// <summary>
        /// Обработчик события отпускания левой кнопки мыши на холсте.
        /// Завершает операцию перетаскивания точки или линии, освобождая захват мыши.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события мыши.</param>
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
                _selectedLine?.Polyline.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Выбирает линию для редактирования, обновляя UI элементы управления в соответствии с выбранной линией.
        /// </summary>
        /// <param name="line">Линия для выбора.</param>
        private void SelectLine(VectorLine line)
        {
            DeselectCurrent();
            _selectedLine = line;
            thicknessSlider.Value = line.Data.Thickness;

            foreach (ComboBoxItem item in colorComboBox.Items)
            {
                if (item.Tag is not string tagColorStr)
                    continue;

                try
                {
                    var tagColor = (Color)ColorConverter.ConvertFromString(tagColorStr);
                    if (tagColor == line.Data.StrokeColor)
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

        /// <summary>
        /// Снимает выделение с текущей выбранной линии.
        /// </summary>
        private void DeselectCurrent()
        {
            _selectedLine = null;
            ClearHandles();
        }

        /// <summary>
        /// Обновляет маркеры редактирования (ручки) для выбранной линии.
        /// Создает визуальные маркеры для каждой точки линии, позволяющие редактировать её форму.
        /// </summary>
        private void UpdateHandles()
        {
            ClearHandles();
            if (_selectedLine == null)
                return;

            int index = 0;
            foreach (var point in _selectedLine.Data.Points)
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

        /// <summary>
        /// Обновляет позиции маркеров редактирования в соответствии с текущими позициями точек выбранной линии.
        /// </summary>
        private void UpdateHandlesPositions()
        {
            if (_selectedLine == null)
                return;

            for (int i = 0; i < _selectedLine.Data.Points.Count && i < _handles.Count; i++)
            {
                var point = _selectedLine.Data.Points[i];
                var handle = _handles[i];
                Canvas.SetLeft(handle, point.X - 4);
                Canvas.SetTop(handle, point.Y - 4);
            }
        }

        /// <summary>
        /// Удаляет все маркеры редактирования (ручки) с холста.
        /// </summary>
        private void ClearHandles()
        {
            foreach (var handle in _handles)
                drawingCanvas.Children.Remove(handle);
            _handles.Clear();
        }

        /// <summary>
        /// Получает выбранный цвет из комбобокса цветов.
        /// </summary>
        /// <returns>Выбранный цвет или черный цвет по умолчанию, если цвет не может быть определен.</returns>
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
