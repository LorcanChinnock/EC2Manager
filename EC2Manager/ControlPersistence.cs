using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace EC2Manager
{
    public class ControlPersistence<T> where T : class
    {
        private readonly string _filePath;

        public ControlPersistence()
        {
            _filePath = GetLocalFilePath();
        }

        public async Task Write(T objectToWrite, bool append = false)
        {
            using Stream stream = File.Open(_filePath, FileMode.OpenOrCreate);
            await JsonSerializer.SerializeAsync(stream, objectToWrite);
        }

        public async Task<T> Read()
        {
            using Stream stream = File.Open(_filePath, FileMode.OpenOrCreate);
            try
            {
                return await JsonSerializer.DeserializeAsync<T>(stream);
            }
            catch
            {
                return null;
            }
        }

        private static string GetLocalFilePath()
        {
            var fileName = "ControlValues.json";
            string projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(projectPath, fileName);
        }
    }
}