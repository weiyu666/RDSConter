using System.Collections.Generic;
using System.Text;

namespace RegisterDiscoveryService
{
    public class Text
    {
        public static void PadRight(ref string str, int len)
        {
            var l = len - str.Length;
            var pad = "";
            for (var i = 0; i < l; i++) pad += " ";
            str += pad;
        }

        public static void PadLeft(ref string str, int len)
        {
            var l = len - str.Length;
            var pad = "";
            for (var i = 0; i < l; i++) pad += " ";
            str = pad + str;
        }

    }
}
