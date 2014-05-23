using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbx_lib
{
    static class ML
    {
        public static void Assert(bool value)
        {
            if (value)
                return;

            throw new Exception("Assert failed");
        }

        public static bool Validate(bool value, string desc)
        {
            if (!value)
            {
                Logging.Validation(desc);
                return false;
            }
            return true;
        }
    }
}
