using System.IO;
using System.IO.Compression;
using System.Reflection;
using Newtonsoft.Json;
using ShipmentPdfReader;
using ShipmentPdfReader.Models;

namespace ShipmentPdfReader
{
    public class ConfigurationManager
    {
        private static ConfigurationManager _instance;
        public static ConfigurationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfigurationManager();
                }
                return _instance;
            }
        }
        public List<ColorInfo> AcceptableColors
        {
            get; private set;
        }
        public List<SizeInfo> AcceptableSizes
        {
            get; private set;
        }
        public List<SpecialSkuCodeInfo> SpecialSkuCodes
        {
            get; private set;
        }

        public string SourceDirectoryPath
        {
            get => Preferences.Get("SourceDirectoryPath", string.Empty);
            set => Preferences.Set("SourceDirectoryPath", value);
        }

        public string DestinationDirectoryPath
        {
            get => Preferences.Get("OutputDirectoryPath", string.Empty);
            set => Preferences.Set("OutputDirectoryPath", value);
        }

        public string SelectedFilePath
        {
            get => Preferences.Get("SelectedFilePath", string.Empty);
            set => Preferences.Set("SelectedFilePath", value);
        }

        public ConfigurationManager()
        {
            InitializeConfigurations();
        }
        private void InitializeConfigurations()
        {
            EnsureLocalFileExists("acceptableColors.json");
            EnsureLocalFileExists("acceptableSizes.json");
            EnsureLocalFileExists("specialSKUCodes.json");

            LoadConfigurations();
        }

        private void EnsureLocalFileExists(string filename)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);

            if (!File.Exists(filePath))
            {
                var initialData = LoadEmbeddedJson<object>(filename); // Adjust the type as needed
                SaveJsonToFile(initialData, filename);
            }
        }

        public void LoadConfigurations()
        {
            AcceptableColors = LoadJsonFromFile<ColorInfo>("acceptableColors.json");
            AcceptableSizes = LoadJsonFromFile<SizeInfo>("acceptableSizes.json");
            SpecialSkuCodes = LoadJsonFromFile<SpecialSkuCodeInfo>("specialSKUCodes.json");
        }

        private List<T> LoadJsonFromFile<T>(string filename)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);

                if (File.Exists(filePath))
                {
                    string jsonContent = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<List<T>>(jsonContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {filename} from file: {ex.Message}");
            }

            return new List<T>();
        }

        public void SaveJsonToFile<T>(List<T> data, string filename)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
            string jsonContent = JsonConvert.SerializeObject(data);
            File.WriteAllText(filePath, jsonContent);
        }
        private static List<T> LoadEmbeddedJson<T>(string filename)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"{assembly.GetName().Name}.Acceptables.{filename}";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    Console.WriteLine($"Resource not found: {resourceName}");
                    return new List<T>();
                }

                using var reader = new StreamReader(stream);
                var jsonContent = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<List<T>>(jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {filename}: {ex.Message}");
                return new List<T>();
            }
        }

        public void BatchExportConfigurations(string path)
        {
            try
            {
                var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDirectory);

                SaveJsonToFile(AcceptableColors, Path.Combine(tempDirectory, "acceptableColors.json"));
                SaveJsonToFile(AcceptableSizes, Path.Combine(tempDirectory, "acceptableSizes.json"));
                SaveJsonToFile(SpecialSkuCodes, Path.Combine(tempDirectory, "specialSKUCodes.json"));

                var zipFileName = "ConfigurationsExport.zip";
                var zipFilePath = Path.Combine(path, zipFileName); // Use the provided path

                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
                ZipFile.CreateFromDirectory(tempDirectory, zipFilePath);

                Directory.Delete(tempDirectory, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during batch export of configurations: {ex.Message}");
            }
        }

        public void ImportConfiguration(string importPath, ConfigurationType configType)
        {
            try
            {
                var jsonContent = File.ReadAllText(importPath);

                switch (configType)
                {
                    case ConfigurationType.Color:
                        this.AcceptableColors = JsonConvert.DeserializeObject<List<ColorInfo>>(jsonContent);
                        SaveJsonToFile(this.AcceptableColors, "acceptableColors.json");
                        break;

                    case ConfigurationType.Size:
                        this.AcceptableSizes = JsonConvert.DeserializeObject<List<SizeInfo>>(jsonContent);
                        SaveJsonToFile(this.AcceptableSizes, "acceptableSizes.json");
                        break;

                    case ConfigurationType.Sku:
                        this.SpecialSkuCodes = JsonConvert.DeserializeObject<List<SpecialSkuCodeInfo>>(jsonContent);
                        SaveJsonToFile(this.SpecialSkuCodes, "specialSKUCodes.json");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing configuration: {ex.Message}");
            }
        }

    }

}
