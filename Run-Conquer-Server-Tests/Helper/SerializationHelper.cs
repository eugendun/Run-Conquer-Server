using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace Run_Conquer_Server_Tests
{
    class SerializationHelper
    {
        public static string Serialize<T>(MediaTypeFormatter formatter, T value)
        {
            Stream stream = new MemoryStream();
            var content = new StreamContent(stream);
            formatter.WriteToStreamAsync(typeof(T), value, stream, content, null).Wait();
            stream.Position = 0;
            return content.ReadAsStringAsync().Result;
        }

        public static T Deserialize<T>(MediaTypeFormatter formatter, string str) where T : class
        {
            Stream stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return formatter.ReadFromStreamAsync(typeof(T), stream, null, null).Result as T;
        }
    }
}
