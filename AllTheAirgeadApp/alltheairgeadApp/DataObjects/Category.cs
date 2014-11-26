using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alltheairgeadApp.DataObjects
{
    /// <summary>
    /// Category class as returned from the mobile service.
    /// </summary>
    class Category
    {
        // Category name
        public string id { get; set; }

        // Default priority of the category
        public Byte DefaultPriority { get; set; }
    }
}
