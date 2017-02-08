using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChromeAutoUpdate
{
    class node_key : IComparable<node_key>
    {

        public int last_time { get; private set; }

        public node_key(int last_time)
        {
            this.last_time = last_time;
        }
        public int CompareTo(node_key other)
        {
            return -last_time.CompareTo(other.last_time);
        }
    }
}
