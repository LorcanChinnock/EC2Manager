using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace EC2Manager
{
    public class ControlPersistence<T> where T : class
    {
        private readonly string fileName = "ControlValues.json";

        public async Task Write(T objectToWrite, bool append = false)
        {
            using var stream = File.Open(GetLocalFilePath(), FileMode.OpenOrCreate);
            await JsonSerializer.SerializeAsync(stream, objectToWrite);
        }

        public async Task<T> Read()
        {
            using var stream = File.Open(GetLocalFilePath(), FileMode.OpenOrCreate);
            try
            {
                return await JsonSerializer.DeserializeAsync<T>(stream);
            }
            catch
            {
                return null;
            }
        }

        private string GetLocalFilePath()
        {
            var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(projectPath, fileName);
            return filePath;
        }
    }
}