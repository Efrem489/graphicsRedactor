
namespace VectorEditor.Services
{
    /// <summary>
    /// Определяет интерфейс для операций сохранения и загрузки данных векторного редактора.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Сохраняет коллекцию данных ломаных линий в XML файл.
        /// </summary>
        /// <param name="data">Коллекция данных ломаных линий для сохранения.</param>
        /// <param name="filePath">Путь к файлу для сохранения.</param>
        /// <exception cref="System.IO.IOException">Возникает при ошибках ввода-вывода.</exception>
        /// <exception cref="System.InvalidOperationException">Возникает при ошибках сериализации.</exception>
        void Save(IEnumerable<BrokenLineData> data, string filePath);

        /// <summary>
        /// Загружает коллекцию данных ломаных линий из XML файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу для загрузки.</param>
        /// <returns>Коллекция данных ломаных линий, загруженных из файла.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Возникает, если файл не найден.</exception>
        /// <exception cref="System.IO.IOException">Возникает при ошибках ввода-вывода.</exception>
        /// <exception cref="System.InvalidOperationException">Возникает при ошибках десериализации.</exception>
        List<BrokenLineData> Load(string filePath);
    }
}

