using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PanicCall
{
    enum ERROR : byte
    {
        OK = 0,
        DATA_ERROR = 41,
        HEADER_KEY_ERROR = 42,
        COMMAND_ERROR = 43,
        NOT_FOUND_MEMBER = 30,
        NOT_FOUND_FRIEND = 31,
        NOT_DEFINITION_ERROR = 99
    };

    enum SMARTPHONE_ERROR : byte { OK = 0, IP_ERROR = 1, SERVER_CONNTECTION_ERROR = 2, NOT_DEFINITION_ERROR = 3 };

    class EnumerationType
    {
    }
}
