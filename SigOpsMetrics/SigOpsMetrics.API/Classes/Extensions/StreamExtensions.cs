using System.IO;

namespace SigOpsMetrics.API.Classes.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream inStream)
        {
            if (inStream is MemoryStream stream)
                return stream.ToArray();

            using var memoryStream = new MemoryStream();
            inStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
