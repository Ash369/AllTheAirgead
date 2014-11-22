using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alltheairgeadApp.DataObjects
{
    public class Expense
    {
        public Expense(String Category, Decimal Price, DateTimeOffset Date, DateTime Time, Byte? Priority, String MoreInfo)
        {
            this.Category = Category;
            this.Price = Price;
            this.Date = Date.Date;
            this.Time = Time;
            this.Priority = Priority;
            this.MoreInfo = String.IsNullOrWhiteSpace(MoreInfo) ? "" : MoreInfo;
        }

        public string Display()
        {
            IList<string> PriorityLevels = new ReadOnlyCollection<string>
                (new List<string> { "High", "Medium", "Low" });

            return
            (
                "Category: " + Category + Environment.NewLine +
                "Price: " + Price.ToString() + Environment.NewLine +
                "Date and time: " + new DateTime((Date + Time.TimeOfDay).Ticks).ToString() + Environment.NewLine +
                "Priority: " + PriorityLevels[(int)Priority-1] + Environment.NewLine +
                "More Info: " + MoreInfo
            );
        }

        public int Id;
        public String Category;
        public Decimal Price;
        public DateTime Date;
        public DateTime Time;
        public Byte? Priority;
        public String MoreInfo;
    }
}
