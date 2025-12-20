using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using VectorEditor.Services;

namespace VectorEditor
{
    /// <summary>
    /// Главное окно векторного редактора.
    /// Предоставляет функциональность для создания, редактирования и управления ломаными линиями.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<BrokenLineData> _drawingData = new List<BrokenLineData>();
        private readonly IFileService _fileService;
        private BrokenLineData? _selectedData;
        private Polyline? _selectedPolyline;
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
            _fileService = new XmlFileService();
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
            if (_selectedData != null && _selectedPolyline != null)
            {
                _drawingData.Remove(_selectedData);
                drawingCanvas.Children.Remove(_selectedPolyline);
                DeselectCurrent();
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки сохранения файла.
        /// Открывает диалог сохранения и сохраняет все линии в XML файл через файловый сервис.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog { Filter = "XML Files (*.xml)|*.xml" };
            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    _fileService.Save(_drawingData, saveDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки загрузки файла.
        /// Открывает диалог выбора файла и загружает линии из XML файла через файловый сервис.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml" };
            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    var loadedData = _fileService.Load(openDialog.FileName);
                    drawingCanvas.Children.Clear();
                    _drawingData.Clear();
                    _drawingData.AddRange(loadedData);
                    foreach (var data in _drawingData)
                    {
                        CreatePolylineUI(data);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Обработчик события изменения значения слайдера толщины линии.
        /// Обновляет толщину выбранной линии в данных и на холсте.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события, содержащие новое значение.</param>
        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_selectedData != null && _selectedPolyline != null)
            {
                _selectedData.Thickness = e.NewValue;
                _selectedPolyline.StrokeThickness = e.NewValue;
            }
        }

        /// <summary>
        /// Обработчик события изменения выбора цвета в комбобоксе.
        /// Обновляет цвет выбранной линии в данных и на холсте.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события изменения выбора.</param>
        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedData != null && _selectedPolyline != null && colorComboBox.SelectedItem is ComboBoxItem item && item.Tag is string colorString)
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorString);
                    _selectedData.StrokeColor = color;
                    _selectedPolyline.Stroke = new SolidColorBrush(color);
                }
                catch
                {
                    // Игнорируем ошибку преобразования цвета
                }
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
            if (_previewPolyline != null && _previewPolyline.Points.Count == 2)
            {
                _previewPolyline.Points[1] = currentPosition;
            }
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
                else
                {
                    var newData = new BrokenLineData
                    {
                        Points = new List<Point> { _firstPoint, position },
                        Thickness = thicknessSlider.Value,
                        StrokeColor = GetSelectedColor()
                    };
                    _drawingData.Add(newData);
                    var polyline = CreatePolylineUI(newData);
                    SelectLine(newData, polyline);
                    ClearPreview();
                    _isCreatingLine = false;
                    e.Handled = true;
                    return;
                }
            }

            var hitResult = VisualTreeHelper.HitTest(drawingCanvas, position);
            if (hitResult?.VisualHit != null)
            {
                if (hitResult.VisualHit is Ellipse handle)
                {
                    _isDraggingPoint = true;
                    _draggingPointIndex = _handles.IndexOf(handle);
                    _dragStartPosition = position;
                    handle.CaptureMouse();
                    e.Handled = true;
                    return;
                }
                else if (hitResult.VisualHit is Polyline polyline)
                {
                    var data = _drawingData.FirstOrDefault(d => polyline.Points.SequenceEqual(d.Points));
                    if (data != null)
                    {
                        if (e.ClickCount == 2)
                        {
                            InsertPointOnLine(data, polyline, position);
                            UpdateHandles();
                            e.Handled = true;
                            return;
                        }
                        else
                        {
                            SelectLine(data, polyline);
                            _isDraggingWhole = true;
                            _dragStartPosition = position;
                            polyline.CaptureMouse();
                            e.Handled = true;
                        }
                    }
                }
            }
            else
            {
                DeselectCurrent();
                if (_isCreatingLine)
                {
                    ClearPreview();
                    _isCreatingLine = false;
                }
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
            }

            if (_isDraggingPoint && _selectedData != null && _selectedPolyline != null && _draggingPointIndex >= 0 && _draggingPointIndex < _selectedData.Points.Count)
            {
                var delta = new Vector(position.X - _dragStartPosition.X, position.Y - _dragStartPosition.Y);
                var point = _selectedData.Points[_draggingPointIndex] + delta;
                _selectedData.Points[_draggingPointIndex] = point;
                _selectedPolyline.Points[_draggingPointIndex] = point;
                if (_draggingPointIndex < _handles.Count)
                {
                    var handle = _handles[_draggingPointIndex];
                    Canvas.SetLeft(handle, point.X - 4);
                    Canvas.SetTop(handle, point.Y - 4);
                }
                _dragStartPosition = position;
            }
            else if (_isDraggingWhole && _selectedData != null && _selectedPolyline != null)
            {
                var delta = new Vector(position.X - _dragStartPosition.X, position.Y - _dragStartPosition.Y);
                int count = Math.Min(_selectedData.Points.Count, Math.Min(_selectedPolyline.Points.Count, _handles.Count));
                for (int i = 0; i < count; i++)
                {
                    var point = _selectedData.Points[i] + delta;
                    _selectedData.Points[i] = point;
                    _selectedPolyline.Points[i] = point;
                    var handle = _handles[i];
                    Canvas.SetLeft(handle, point.X - 4);
                    Canvas.SetTop(handle, point.Y - 4);
                }
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
                if (_draggingPointIndex >= 0 && _handles.Count > _draggingPointIndex)
                    _handles[_draggingPointIndex].ReleaseMouseCapture();
                _draggingPointIndex = -1;
            }
            else if (_isDraggingWhole)
            {
                _isDraggingWhole = false;
                _selectedPolyline?.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Создает визуальный элемент Polyline на основе данных линии и добавляет его на холст.
        /// </summary>
        /// <param name="data">Данные ломаной линии для визуализации.</param>
        /// <returns>Созданный визуальный элемент Polyline.</returns>
        private Polyline CreatePolylineUI(BrokenLineData data)
        {
            var polyline = new Polyline
            {
                Points = new PointCollection(data.Points),
                StrokeThickness = data.Thickness,
                Stroke = new SolidColorBrush(data.StrokeColor)
            };
            drawingCanvas.Children.Add(polyline);
            return polyline;
        }

        /// <summary>
        /// Выбирает линию для редактирования, обновляя UI элементы управления в соответствии с выбранной линией.
        /// </summary>
        /// <param name="data">Данные выбранной линии.</param>
        /// <param name="polyline">Визуальный элемент выбранной линии.</param>
        private void SelectLine(BrokenLineData data, Polyline polyline)
        {
            DeselectCurrent();
            _selectedData = data;
            _selectedPolyline = polyline;
            thicknessSlider.Value = data.Thickness;

            foreach (ComboBoxItem item in colorComboBox.Items)
            {
                if (item.Tag is string tagColorStr)
                {
                    try
                    {
                        var tagColor = (Color)ColorConverter.ConvertFromString(tagColorStr);
                        if (tagColor == data.StrokeColor)
                        {
                            colorComboBox.SelectedItem = item;
                            break;
                        }
                    }
                    catch { /* игнорируем ошибку преобразования цвета */ }
                }
            }
            UpdateHandles();
        }

        /// <summary>
        /// Снимает выделение с текущей выбранной линии.
        /// </summary>
        private void DeselectCurrent()
        {
            _selectedData = null;
            _selectedPolyline = null;
            ClearHandles();
        }

        /// <summary>
        /// Обновляет маркеры редактирования (ручки) для выбранной линии.
        /// Создает визуальные маркеры для каждой точки линии, позволяющие редактировать её форму.
        /// </summary>
        private void UpdateHandles()
        {
            ClearHandles();
            if (_selectedData == null) return;
            int index = 0;
            foreach (var point in _selectedData.Points)
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
        /// Удаляет все маркеры редактирования (ручки) с холста.
        /// </summary>
        private void ClearHandles()
        {
            foreach (var handle in _handles)
                drawingCanvas.Children.Remove(handle);
            _handles.Clear();
        }

        /// <summary>
        /// Вставляет новую точку в линию в позиции, ближайшей к точке клика.
        /// Ищет ближайший сегмент линии и вставляет точку проекции клика на этот сегмент.
        /// </summary>
        /// <param name="data">Данные линии, в которую вставляется точка.</param>
        /// <param name="polyline">Визуальный элемент линии.</param>
        /// <param name="clickPosition">Позиция клика мыши.</param>
        private void InsertPointOnLine(BrokenLineData data, Polyline polyline, Point clickPosition)
        {
            double minDistance = double.MaxValue;
            int insertIndex = -1;
            Point insertPoint = default;

            for (int i = 0; i < data.Points.Count - 1; i++)
            {
                var p1 = data.Points[i];
                var p2 = data.Points[i + 1];
                var projection = ProjectPointOnSegment(clickPosition, p1, p2);
                var distance = (clickPosition - projection).Length;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    insertIndex = i + 1;
                    insertPoint = projection;
                }
            }

            if (insertIndex != -1 && minDistance < 10)
            {
                data.Points.Insert(insertIndex, insertPoint);
                polyline.Points = new PointCollection(data.Points);

                if (_selectedData == data && _selectedPolyline == polyline)
                {
                    UpdateHandles();
                }
            }
        }

        /// <summary>
        /// Проецирует точку на отрезок, находя ближайшую точку на отрезке к заданной точке.
        /// </summary>
        /// <param name="p">Точка для проецирования.</param>
        /// <param name="a">Начальная точка отрезка.</param>
        /// <param name="b">Конечная точка отрезка.</param>
        /// <returns>Точка проекции на отрезке.</returns>
        private Point ProjectPointOnSegment(Point p, Point a, Point b)
        {
            var ab = b - a;
            var ap = p - a;
            double projLength = Vector.Multiply(ab, ap) / ab.LengthSquared;
            projLength = Math.Clamp(projLength, 0, 1);
            return a + ab * projLength;
        }

        /// <summary>
        /// Получает выбранный цвет из комбобокса цветов.
        /// </summary>
        /// <returns>Выбранный цвет или черный цвет по умолчанию, если цвет не может быть определен.</returns>
        private Color GetSelectedColor()
        {
            if (colorComboBox.SelectedItem is ComboBoxItem item && item.Tag is string colorString)
            {
                try
                {
                    return (Color)ColorConverter.ConvertFromString(colorString);
                }
                catch
                {
                    return Colors.Black;
                }
            }
            return Colors.Black;
        }
    }
}