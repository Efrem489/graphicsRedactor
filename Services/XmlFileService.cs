using System.IO;
using System.Xml.Serialization;


namespace VectorEditor.Services
{
    /// <summary>
    /// Реализация сервиса для сохранения и загрузки данных векторного редактора в формате XML.
    /// </summary>
    public class XmlFileService : IFileService
    {
        /// <summary>
        /// Сохраняет коллекцию данных ломаных линий в XML файл.
        /// </summary>
        /// <param name="data">Коллекция данных ломаных линий для сохранения.</param>
        /// <param name="filePath">Путь к файлу для сохранения.</param>
        /// <exception cref="System.IO.IOException">Возникает при ошибках ввода-вывода.</exception>
        /// <exception cref="System.InvalidOperationException">Возникает при ошибках сериализации.</exception>
        public void Save(IEnumerable<BrokenLineData> data, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(List<BrokenLineData>));
                serializer.Serialize(stream, data);
            }
        }

        /// <summary>
        /// Загружает коллекцию данных ломаных линий из XML файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу для загрузки.</param>
        /// <returns>Коллекция данных ломаных линий, загруженных из файла.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Возникает, если файл не найден.</exception>
        /// <exception cref="System.IO.IOException">Возникает при ошибках ввода-вывода.</exception>
        /// <exception cref="System.InvalidOperationException">Возникает при ошибках десериализации.</exception>
        public List<BrokenLineData> Load(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(List<BrokenLineData>));
                var result = serializer.Deserialize(stream) as List<BrokenLineData>;
                return result ?? throw new System.InvalidOperationException("Не удалось десериализовать файл. Файл может быть поврежден или иметь неверный формат.");
            }
        }
    }
}

