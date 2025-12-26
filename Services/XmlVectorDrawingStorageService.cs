using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;
using VectorEditor;

namespace VectorEditor.Services
{
    /// <summary>
    /// Реализация сервиса для сохранения и загрузки данных векторного редактора в формате XML с использованием диалоговых окон.
    /// </summary>
    public class XmlVectorDrawingStorageService : IVectorDrawingStorageService
    {
        /// <summary>
        /// Сохраняет коллекцию данных ломаных линий, открывая диалог выбора файла для сохранения.
        /// </summary>
        /// <param name="data">Коллекция данных ломаных линий для сохранения.</param>
        /// <returns>true, если сохранение выполнено успешно; иначе false.</returns>
        public bool Save(IEnumerable<BrokenLineData> data)
        {
            var saveDialog = new SaveFileDialog { Filter = "XML Files (*.xml)|*.xml" };
            if (saveDialog.ShowDialog() != true)
                return false;

            try
            {
                using (var stream = new FileStream(saveDialog.FileName, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(List<BrokenLineData>));
                    serializer.Serialize(stream, data);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Загружает коллекцию данных ломаных линий, открывая диалог выбора файла для загрузки.
        /// </summary>
        /// <returns>Коллекция данных ломаных линий, загруженных из файла, или null, если операция была отменена.</returns>
        public List<BrokenLineData>? Load()
        {
            var openDialog = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml" };
            if (openDialog.ShowDialog() != true)
                return null;

            try
            {
                using (var stream = new FileStream(openDialog.FileName, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(List<BrokenLineData>));
                    var result = serializer.Deserialize(stream) as List<BrokenLineData>;
                    return result ?? throw new System.InvalidOperationException("Не удалось десериализовать файл. Файл может быть поврежден или иметь неверный формат.");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}

