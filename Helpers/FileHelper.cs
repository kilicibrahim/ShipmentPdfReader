
namespace ShipmentPdfReader.Helpers
{
    public static class FileHelper
    {
        /// <summary>
        /// Generates a unique filename by appending an incremental number to the base filename
        /// if the original filename already exists in the specified directory.
        /// </summary>
        /// <param name="baseFilename">The base name of the file, without any numeric suffix.</param>
        /// <param name="directoryPath">The directory path where the file will be saved.</param>
        /// <returns>A unique filename within the specified directory.</returns>
        public static string GenerateUniqueFilename(string directory, string baseName, string extension)
        {
            int counter = 1;
            string fullPath;

            do
            {
                string fileName = $"{baseName}_{counter}{extension}";
                fullPath = Path.Combine(directory, fileName);
                counter++;
            } while (File.Exists(fullPath));

            return fullPath;
        }
    }
}
