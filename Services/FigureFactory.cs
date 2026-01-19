using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorEditor.Shapes;

namespace VectorEditor.Services
{
    /// <summary>
    /// Фабрика для создания фигур
    /// </summary>
    public static class FigureFactory
    {
        private static readonly Dictionary<Type, Func<IFigureData, Figure>> _creators =
            new Dictionary<Type, Func<IFigureData, Figure>>();

        static FigureFactory()
        {
            RegisterCreator<BrokenLineData>(data =>
            {
                if (data is BrokenLineData lineData)
                    return new Line(lineData);
                throw new InvalidCastException($"Ожидался BrokenLineData, получен {data.GetType()}");
            });
            // возможно зарегистрировать создателей для других типов фигур
        }

        /// <summary>
        /// Регистрирует создателя для типа данных фигуры
        /// </summary>
        public static void RegisterCreator<T>(Func<IFigureData, Figure> creator) where T : IFigureData
        {
            _creators[typeof(T)] = creator;
        }

        /// <summary>
        /// Создает фигуру на основе данных
        /// </summary>
        public static Figure CreateFigure(IFigureData data)
        {
            var type = data.GetType();
            if (_creators.TryGetValue(type, out var creator))
            {
                return creator(data);
            }

            throw new InvalidOperationException($"Не зарегистрирован создатель для типа {type}");
        }
    }
}
