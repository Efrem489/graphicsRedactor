using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VectorEditor;

namespace VectorEditor.Shapes
{
    /// <summary>
    /// Представляет ломаную линию в векторном редакторе.
    /// Инкапсулирует данные линии и её визуальное представление.
    /// </summary>
    public class Line
    {
        private readonly BrokenLineData _data;
        private Polyline _polyline;

        /// <summary>
        /// Получает данные линии.
        /// </summary>
        public BrokenLineData Data => _data;

        /// <summary>
        /// Получает визуальный элемент линии.
        /// </summary>
        public Polyline Polyline => _polyline;

        /// <summary>
        /// Инициализирует новый экземпляр класса Line на основе данных.
        /// </summary>
        /// <param name="data">Данные линии.</param>
        public Line(BrokenLineData data)
        {
            _data = data;
            _polyline = CreatePolyline();
        }

        /// <summary>
        /// Обновляет толщину линии.
        /// </summary>
        /// <param name="thickness">Новая толщина линии.</param>
        public void SetThickness(double thickness)
        {
            _data.Thickness = thickness;
            _polyline.StrokeThickness = thickness;
        }

        /// <summary>
        /// Обновляет цвет линии.
        /// </summary>
        /// <param name="color">Новый цвет линии.</param>
        public void SetColor(Color color)
        {
            _data.StrokeColor = color;
            _polyline.Stroke = new SolidColorBrush(color);
        }

        /// <summary>
        /// Перемещает точку линии по указанному индексу.
        /// </summary>
        /// <param name="pointIndex">Индекс точки для перемещения.</param>
        /// <param name="newPosition">Новая позиция точки.</param>
        public void MovePoint(int pointIndex, Point newPosition)
        {
            if (pointIndex < 0 || pointIndex >= _data.Points.Count)
                return;

            _data.Points[pointIndex] = newPosition;
            _polyline.Points[pointIndex] = newPosition;
        }

        /// <summary>
        /// Перемещает всю линию на указанное смещение.
        /// </summary>
        /// <param name="offset">Смещение для перемещения всех точек линии.</param>
        public void MoveAll(Vector offset)
        {
            for (int i = 0; i < _data.Points.Count; i++)
            {
                var newPoint = _data.Points[i] + offset;
                _data.Points[i] = newPoint;
                _polyline.Points[i] = newPoint;
            }
        }

        /// <summary>
        /// Вставляет точку в линию в позиции, ближайшей к указанной точке клика.
        /// </summary>
        /// <param name="clickPosition">Позиция клика мыши.</param>
        /// <param name="maxDistance">Максимальное расстояние для вставки точки.</param>
        /// <returns>true, если точка была успешно вставлена; иначе false.</returns>
        public bool InsertPoint(Point clickPosition, double maxDistance = 10)
        {
            double minDistance = double.MaxValue;
            int insertIndex = -1;
            Point insertPoint = default;

            for (int i = 0; i < _data.Points.Count - 1; i++)
            {
                var p1 = _data.Points[i];
                var p2 = _data.Points[i + 1];
                var projection = ProjectPointOnSegment(clickPosition, p1, p2);
                var distance = (clickPosition - projection).Length;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    insertIndex = i + 1;
                    insertPoint = projection;
                }
            }

            if (insertIndex != -1 && minDistance < maxDistance)
            {
                _data.Points.Insert(insertIndex, insertPoint);
                _polyline.Points = new PointCollection(_data.Points);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяет, соответствует ли визуальный элемент данной линии.
        /// </summary>
        /// <param name="polyline">Визуальный элемент для проверки.</param>
        /// <returns>true, если визуальный элемент соответствует данной линии; иначе false.</returns>
        public bool MatchesPolyline(Polyline polyline)
        {
            return _polyline == polyline || _polyline.Points.SequenceEqual(polyline.Points);
        }

        /// <summary>
        /// Создает визуальный элемент Polyline на основе данных линии.
        /// </summary>
        /// <returns>Созданный визуальный элемент Polyline.</returns>
        private Polyline CreatePolyline()
        {
            return new Polyline
            {
                Points = new PointCollection(_data.Points),
                StrokeThickness = _data.Thickness,
                Stroke = new SolidColorBrush(_data.StrokeColor)
            };
        }

        /// <summary>
        /// Проецирует точку на отрезок, находя ближайшую точку на отрезке к заданной точке.
        /// </summary>
        /// <param name="p">Точка для проецирования.</param>
        /// <param name="a">Начальная точка отрезка.</param>
        /// <param name="b">Конечная точка отрезка.</param>
        /// <returns>Точка проекции на отрезке.</returns>
        private static Point ProjectPointOnSegment(Point p, Point a, Point b)
        {
            var ab = b - a;
            var ap = p - a;
            double projLength = Vector.Multiply(ab, ap) / ab.LengthSquared;
            projLength = System.Math.Clamp(projLength, 0, 1);
            return a + ab * projLength;
        }
    }
}

