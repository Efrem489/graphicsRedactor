using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;
using VectorEditor;

namespace VectorEditor.Services
{
    /// <summary>
    /// Реализация сервиса для сохранения и загрузки данных векторного редактора в формате XML.
    /// </summary>
    public class XmlVectorDrawingStorageService : IVectorDrawingStorageService
    {
        public bool Save(IEnumerable<IFigureData> data)
        {
            var saveDialog = new SaveFileDialog { Filter = "XML Files (*.xml)|*.xml" };
            if (saveDialog.ShowDialog() != true)
                return false;

            try
            {
                using (var stream = new FileStream(saveDialog.FileName, FileMode.Create))
                {
                    // Преобразуем в список конкретного типа для сериализации
                    var brokenLineDataList = new List<BrokenLineData>();
                    foreach (var item in data)
                    {
                        if (item is BrokenLineData lineData)
                        {
                            brokenLineDataList.Add(lineData);
                        }
                    }

                    var serializer = new XmlSerializer(typeof(List<BrokenLineData>));
                    serializer.Serialize(stream, brokenLineDataList);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения файла: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public List<IFigureData>? Load()
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

                    if (result == null)
                        throw new System.InvalidOperationException("Не удалось десериализовать файл.");

                    // Преобразуем в список интерфейса
                    return new List<IFigureData>(result);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки файла: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}

