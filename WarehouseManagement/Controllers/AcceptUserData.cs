using Microsoft.OpenApi.Writers;

namespace WarehouseManagement.Controllers
{
    public class AcceptUserData
    {
        public int RecordId { get; set; }
        public int UserId { get; set; }
    }
}