using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace EC2Manager
{
    public class ControlPersistence<T> where T : class
    {
        private readonly string _filePath;

        public ControlPersistence()
        {
            _filePath = GetLocalFilePath();
        }

        public void Write(T objectToWrite, bool append = false)
        {
            using Stream stream = File.Open(_filePath, FileMode.OpenOrCreate);
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, objectToWrite);
            stream.Close();
        }

        public T Read()
        {
            using Stream stream = File.Open(_filePath, FileMode.OpenOrCreate);
            var binaryFormatter = new BinaryFormatter();
            try
            {
                var deserializedObject = (T)binaryFormatter.Deserialize(stream);
                return deserializedObject;
            }
            catch
            {
                return null;
            }
        }

        private static string GetLocalFilePath()
        {
            var fileName = "ControlValues.bin";
            string projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(projectPath, fileName);
        }
    }
}