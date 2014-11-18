using System;
using Microsoft.WindowsAzure.Mobile.Service;

namespace alltheairgeadmobileService.DataObjects
{
    public class ExpenseDto : EntityData
    {
        public int UserId { get; set; }

        public DateTime Date { get; set; }

        public string Category { get; set; }

        public decimal Price { get; set; }

        public string MoreInfo { get; set; }

        public DateTime? Time { get; set; }

        public short? Priority { get; set; }
    }
}