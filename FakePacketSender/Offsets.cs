using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakePacketSender
{
    [Serializable]
    public class Offsets
    {
        public int Send2 { get; set; }
        public int VTable { get; set; }
    }
}
