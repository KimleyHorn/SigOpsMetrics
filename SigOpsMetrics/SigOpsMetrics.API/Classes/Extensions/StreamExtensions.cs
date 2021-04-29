using System.Data;
using System.IO;
using System.Threading.Tasks;

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

        public static MemoryStream ConvertToCSV(Task<DataTable> data)
        {
            MemoryStream headerMs = new MemoryStream();
            StreamWriter header = new StreamWriter(headerMs);

            foreach (DataColumn column in data.Result.Columns)
            {
                header.Write(column.ColumnName + ",");
            }
            header.WriteLine("");

            foreach (DataRow row in data.Result.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    header.Write(item + ",");
                }
                header.WriteLine("");
            }
            header.Flush();
            headerMs.Position = 0;
            return headerMs;
        }
    }
}
