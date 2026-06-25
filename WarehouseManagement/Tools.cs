using WarehouseManagement.EFCore;

namespace WarehouseManagement
{
    static public class Tools
    {
        static public int Totallize(this List<WarehouseRecord> warehouse)
        {
            return warehouse.Where(u => u.RecordStateId != 3 && u.RecordTypeId == 2).Sum(u => u.ItemCount) 
                - warehouse.Where(u => u.RecordStateId != 3 && u.RecordTypeId == 1).Sum(u => u.ItemCount);
        }
    }
}
