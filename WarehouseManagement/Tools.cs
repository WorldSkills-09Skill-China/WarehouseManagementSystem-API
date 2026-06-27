using ClosedXML.Excel;
using WarehouseManagement.EFCore;
using WarehouseManagement.ItemsClass;

namespace WarehouseManagement
{
    static public class Tools
    {
        static public int Totallize(this List<WarehouseRecord> warehouse)
        {
            return warehouse.Where(u => u.RecordStateId != 3 && u.RecordTypeId == 2).Sum(u => u.ItemCount)
                - warehouse.Where(u => u.RecordStateId != 3 && u.RecordTypeId == 1).Sum(u => u.ItemCount);
        }

        static public void PrintALabel(this List<FixedData> fixedDatas, string path)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Sheet1");
            ws.Cell(1, 1).Value = "";
            ws.Cell(1, 2).Value = "编码";
            ws.Cell(1, 3).Value = "物品名";
            ws.Cell(1, 4).Value = "批次";
            ws.Cell(1, 5).Value = "类型";
            ws.Cell(1, 6).Value = "创建时间";

            for (int i = 0; i < fixedDatas.Count; i++)
            {
                ws.Cell(i + 2, 1).Value = "软件应用开发";
                ws.Cell(i + 2, 2).Value = fixedDatas[i].Code;
                ws.Cell(i + 2, 3).Value = fixedDatas[i].ItemName;
                ws.Cell(i + 2, 4).Value = fixedDatas[i].Batch;
                ws.Cell(i + 2, 5).Value = fixedDatas[i].TypeName;
                ws.Cell(i + 2, 6).Value = DateTime.Now.ToString("yyyy-MM-dd");
            }
            ws.Columns().AdjustToContents();
            var filepath = Path.Combine(path, "src", "标签.xlsx");
            wb.SaveAs(filepath);
        }
    }
}
