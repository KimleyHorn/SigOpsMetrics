using System.Data;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Extensions
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

        public static MemoryStream ConvertToCSV(DataTable data)
        {
            MemoryStream headerMs = new MemoryStream();
            StreamWriter header = new StreamWriter(headerMs);

            foreach (DataColumn column in data.Columns)
            {
                header.Write(column.ColumnName + ",");
            }
            header.WriteLine("");

            foreach (DataRow row in data.Rows)
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

        public static MemoryStream ConvertToCSV(List<BaseEventLogModel> data)
        {
            MemoryStream headerMs = new MemoryStream();
            StreamWriter header = new StreamWriter(headerMs);

            header.Write("Label" + "," + "Average" + "," + "Delta" + "," + "ZoneGroup");          
            header.WriteLine("");

            foreach (BaseEventLogModel row in data)
            {
                header.Write(row.SignalID + "," + row.EventCode + "," + row.EventParam + "," + row.Timestamp);
                header.WriteLine("");
            }
            header.Flush();
            headerMs.Position = 0;
            return headerMs;
        }
    }
}
