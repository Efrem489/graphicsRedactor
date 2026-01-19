using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VectorEditor.Services
{
    /// <summary>
    /// Интерфейс для данных любой фигуры
    /// </summary>
    public interface IFigureData
    {
        /// <summary>
        /// Толщина линии фигуры
        /// </summary>
        double Thickness { get; set; }

        /// <summary>
        /// Цвет фигуры
        /// </summary>
        Color StrokeColor { get; set; }

        /// <summary>
        /// Создает фигуру на основе данных
        /// </summary>
        Shapes.Figure CreateFigure();
    }
}
