using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VectorEditor.Shapes
{
    /// <summary>
    /// Базовый абстрактный класс для всех фигур в векторном редакторе.
    /// Инкапсулирует общие операции работы с фигурой.
    /// </summary>
    public abstract class Figure
    {
        /// <summary>
        /// Визуальный элемент фигуры, который размещается на холсте.
        /// </summary>
        public abstract Shape Visual { get; }

        /// <summary>
        /// Текущая толщина линии/обводки фигуры.
        /// </summary>
        public abstract double Thickness { get; }

        /// <summary>
        /// Текущий цвет обводки фигуры.
        /// </summary>
        public abstract Color StrokeColor { get; }

        /// <summary>
        /// Возвращает коллекцию опорных точек фигуры,
        /// которые используются для отображения и перетаскивания маркеров (ручек).
        /// </summary>
        public abstract IReadOnlyList<Point> GetPoints();

        /// <summary>
        /// Устанавливает толщину фигуры.
        /// </summary>
        /// <param name="thickness">Новая толщина.</param>
        public abstract void SetThickness(double thickness);

        /// <summary>
        /// Устанавливает цвет обводки фигуры.
        /// </summary>
        /// <param name="color">Новый цвет.</param>
        public abstract void SetColor(Color color);

        /// <summary>
        /// Перемещает отдельную опорную точку фигуры.
        /// </summary>
        /// <param name="pointIndex">Индекс точки.</param>
        /// <param name="newPosition">Новая позиция точки.</param>
        public abstract void MovePoint(int pointIndex, Point newPosition);

        /// <summary>
        /// Перемещает всю фигуру на заданный вектор.
        /// </summary>
        /// <param name="offset">Вектор смещения.</param>
        public abstract void MoveAll(Vector offset);

        /// <summary>
        /// Вставляет новую опорную точку в фигуру в позиции, ближайшей к указанной точке.
        /// </summary>
        /// <param name="clickPosition">Позиция клика.</param>
        /// <param name="maxDistance">Максимальное расстояние до фигуры.</param>
        /// <returns>true, если точка вставлена; иначе false.</returns>
        public abstract bool InsertPoint(Point clickPosition, double maxDistance = 10);

        /// <summary>
        /// Проверяет, соответствует ли переданный визуальный элемент данной фигуре.
        /// Используется для поиска фигуры по результатам HitTest.
        /// </summary>
        /// <param name="shape">Визуальный элемент.</param>
        /// <returns>true, если элемент относится к фигуре; иначе false.</returns>
        public abstract bool MatchesShape(Shape shape);
    }
}



