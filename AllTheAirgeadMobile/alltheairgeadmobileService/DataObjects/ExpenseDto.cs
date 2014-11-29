using System;
using Microsoft.WindowsAzure.Mobile.Service;

namespace alltheairgeadmobileService.DataObjects
{
    /// <summary>
    /// Expense data object used for communicating with client. Uses Microsoft Entuty Data Framework
    /// </summary>
    public class ExpenseDto : EntityData
    {
        public int UserId { get; set; }

        public DateTime Date { get; set; }

        public string Category { get; set; }

        public decimal Price { get; set; }

        public string MoreInfo { get; set; }

        public DateTime? Time { get; set; }

        public Byte? Priority { get; set; }
    }
}