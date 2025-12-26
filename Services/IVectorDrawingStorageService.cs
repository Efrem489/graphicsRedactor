using System.Collections.Generic;
using VectorEditor;

namespace VectorEditor.Services
{
    /// <summary>
    /// Определяет интерфейс для операций сохранения и загрузки данных векторного редактора с использованием диалоговых окон.
    /// </summary>
    public interface IVectorDrawingStorageService
    {
        /// <summary>
        /// Сохраняет коллекцию данных ломаных линий, открывая диалог выбора файла для сохранения.
        /// </summary>
        /// <param name="data">Коллекция данных ломаных линий для сохранения.</param>
        /// <returns>true, если сохранение выполнено успешно; иначе false.</returns>
        bool Save(IEnumerable<BrokenLineData> data);

        /// <summary>
        /// Загружает коллекцию данных ломаных линий, открывая диалог выбора файла для загрузки.
        /// </summary>
        /// <returns>Коллекция данных ломаных линий, загруженных из файла, или null, если операция была отменена.</returns>
        List<BrokenLineData>? Load();
    }
}

