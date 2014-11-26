using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alltheairgeadApp.DataObjects
{
    /// <summary>
    /// Data class for an expense. Includes diplay and constructor for transforming more common datatypes
    /// </summary>
    public class Expense
    {
        /// <summary>
        /// Constructor to buid expense data
        /// </summary>
        /// <param name="Category"></param>
        /// <param name="Price"></param>
        /// <param name="Date"></param>
        /// <param name="Time"></param>
        /// <param name="Priority"></param>
        /// <param name="MoreInfo"></param>
        public Expense(String Category, Decimal Price, DateTimeOffset Date, DateTime Time, Byte? Priority, String MoreInfo)
        {
            this.Category = Category;
            this.Price = Price;
            this.Date = Date.Date;
            this.Time = Time;
            this.Priority = Priority;
            this.MoreInfo = String.IsNullOrWhiteSpace(MoreInfo) ? "" : MoreInfo;
        }

        /// <summary>
        /// Returns a string object of the data object for display
        /// </summary>
        /// <returns></returns>
        public string Display()
        {
            // Transformer for porriority level to user readable
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
