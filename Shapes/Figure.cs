using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VectorEditor.Services;

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
        /// Возвращает коллекцию опорных точек фигуры.
        /// </summary>
        public abstract IReadOnlyList<Point> GetPoints();

        /// <summary>
        /// Возвращает данные фигуры для сохранения.
        /// </summary>
        public abstract IFigureData GetData();

        /// <summary>
        /// Устанавливает толщину фигуры.
        /// </summary>
        public abstract void SetThickness(double thickness);

        /// <summary>
        /// Устанавливает цвет обводки фигуры.
        /// </summary>
        public abstract void SetColor(Color color);

        /// <summary>
        /// Перемещает отдельную опорную точку фигуры.
        /// </summary>
        public abstract void MovePoint(int pointIndex, Point newPosition);

        /// <summary>
        /// Перемещает всю фигуру на заданный вектор.
        /// </summary>
        public abstract void MoveAll(Vector offset);

        /// <summary>
        /// Вставляет новую опорную точку в фигуру.
        /// </summary>
        public abstract bool InsertPoint(Point clickPosition, double maxDistance = 10);

        /// <summary>
        /// Проверяет, соответствует ли переданный визуальный элемент данной фигуре.
        /// </summary>
        public abstract bool MatchesShape(Shape shape);
    }
}



