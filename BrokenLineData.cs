using System;
using System.Collections.Generic;
using System.Windows;           
using System.Windows.Media;     

namespace VectorEditor
{
    /// <summary>
    /// Представляет данные ломаной линии в векторном редакторе.
    /// Содержит коллекцию точек, толщину линии и цвет обводки.
    /// </summary>
    [Serializable]
    public class BrokenLineData
    {
        /// <summary>
        /// Получает или задает коллекцию точек, определяющих ломаную линию.
        /// </summary>
        public List<Point> Points { get; set; } = new List<Point>();

        /// <summary>
        /// Получает или задает толщину линии в пикселях.
        /// Значение по умолчанию: 1.
        /// </summary>
        public double Thickness { get; set; } = 1;

        /// <summary>
        /// Получает или задает цвет обводки линии.
        /// Значение по умолчанию: черный цвет.
        /// </summary>
        public Color StrokeColor { get; set; } = Colors.Black;
    }
}