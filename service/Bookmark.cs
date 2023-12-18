using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surfario
{
    public class Bookmark
    {
        public Bookmark(string name, string address)
        {
            Name = name;
            Address = address;
        }

        public string Name { get; set; }
        public string Address { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Bookmark bookmark &&
                   Address == bookmark.Address;
        }
    }
}
