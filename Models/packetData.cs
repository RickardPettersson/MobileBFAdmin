using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BFAdmin.Models
{
    public class packetData
    {
        public Boolean isFromServer { get; set; }
        public Boolean isResponse { get; set; }
        public uint sequence { get; set; }
        public List<string> Words { get; set; }
    }
}
