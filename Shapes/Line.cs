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
    public class Line : Figure
    {
        private readonly BrokenLineData _data;
        private readonly Polyline _polyline;

        /// <inheritdoc />
        public override Shape Visual => _polyline;

        /// <inheritdoc />
        public override double Thickness => _data.Thickness;

        /// <inheritdoc />
        public override Color StrokeColor => _data.StrokeColor;

        /// <summary>
        /// Получает данные линии.
        /// </summary>
        public BrokenLineData Data => _data;

        /// <summary>
        /// Инициализирует новый экземпляр класса Line на основе данных.
        /// </summary>
        /// <param name="data">Данные линии.</param>
        public Line(BrokenLineData data)
        {
            _data = data;
            _polyline = CreatePolyline();
        }

        /// <inheritdoc />
        public override IReadOnlyList<Point> GetPoints() => _data.Points;

        /// <inheritdoc />
        public override void SetThickness(double thickness)
        {
            _data.Thickness = thickness;
            _polyline.StrokeThickness = thickness;
        }

        /// <inheritdoc />
        public override void SetColor(Color color)
        {
            _data.StrokeColor = color;
            _polyline.Stroke = new SolidColorBrush(color);
        }

        /// <inheritdoc />
        public override void MovePoint(int pointIndex, Point newPosition)
        {
            if (pointIndex < 0 || pointIndex >= _data.Points.Count)
                return;

            _data.Points[pointIndex] = newPosition;
            _polyline.Points[pointIndex] = newPosition;
        }

        /// <inheritdoc />
        public override void MoveAll(Vector offset)
        {
            for (int i = 0; i < _data.Points.Count; i++)
            {
                var newPoint = _data.Points[i] + offset;
                _data.Points[i] = newPoint;
                _polyline.Points[i] = newPoint;
            }
        }

        /// <inheritdoc />
        public override bool InsertPoint(Point clickPosition, double maxDistance = 10)
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

        /// <inheritdoc />
        public override bool MatchesShape(Shape shape)
        {
            if (shape is not Polyline polyline)
                return false;

            return ReferenceEquals(_polyline, polyline) || _polyline.Points.SequenceEqual(polyline.Points);
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

