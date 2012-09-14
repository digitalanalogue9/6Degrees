using System.Collections.Generic;

namespace SixDegrees.Models
{

    public class ItemList
    {
        private IList<Item> Items { get; set; }

        public ItemList()
        {
            Items = new List<Item>();
        }
    }


    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Classification { get; set; }
        public IList<string> Relations { get; set; }

        public Item()
        {
            Relations = new List<string>();
        }
    }
}