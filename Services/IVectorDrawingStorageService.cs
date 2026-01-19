using System.Collections.Generic;
using VectorEditor;

namespace VectorEditor.Services
{
    /// <summary>
    /// Определяет интерфейс для операций сохранения и загрузки данных векторного редактора.
    /// </summary>
    public interface IVectorDrawingStorageService
    {
        /// <summary>
        /// Сохраняет коллекцию данных фигур.
        /// </summary>
        bool Save(IEnumerable<IFigureData> data);

        /// <summary>
        /// Загружает коллекцию данных фигур.
        /// </summary>
        List<IFigureData>? Load();
    }
}

