using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFL
{
    public static class TriangulationUtils
    {
        private static readonly int[] _cw = { 2, 0, 1 };
        private static readonly int[] _ccw = { 1, 2, 0 };
        private static readonly int[] _mod3 = { 0, 1, 2, 0, 1 };

        public static int cw(int i) => _ccw[i];

        public static int ccw(int i) => _cw[i]; 

        public static int mod3(int i) => _mod3[i];
    };
}
