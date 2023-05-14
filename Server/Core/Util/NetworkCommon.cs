using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Define
    {
        public const int USED = 1;
        public const int EMPTY = 0;

        public const int BUFFER_SIZE = 4092;
        public const int PACKETHEADER_SIZE = sizeof(ushort) + sizeof(ushort);
    }
}
