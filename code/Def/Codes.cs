using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Services.Drv.TLM3.Def
{
    class Codes
    {
        public const byte CODE_READ_ENERGY_CURR =   0x01;
        public const byte CODE_READ_POWER_CURR =    0x02;
        public const byte CODE_READ_U_CURR =        0x03;
        public const byte CODE_READ_I_CURR =        0x04;
        public const byte CODE_READ_POWER_COEFF =   0x05;
        public const byte CODE_READ_FREQ_CURR =     0x06;
        public const byte CODE_READ_TYPE_DEVICE =   0x07;
        public const byte CODE_READ_SERIAL_NUMBER = 0x08;
        public const byte CODE_READ_DATE_CREATE =   0x09;
        public const byte CODE_READ_SOFT_VERSION =  0x0A;
        public const byte CODE_READ_NETWORK_ADDR =  0x0B;
        public const byte CODE_READ_CONNECT_CONFIG = 0x0C;
        public const byte CODE_READ_IMPULS_CONST =  0x0D;
        public const byte CODE_READ_DATETIME =      0x0E;
        public const byte CODE_READ_ENERGY_3MIN =   0x0F;
        public const byte CODE_READ_ENERGY_30MIN =  0x10;
        public const byte CODE_READ_ENERGY_DAY =    0x11;
        public const byte CODE_READ_ENERGY_MONTH =  0x12;
        public const byte CODE_READ_ENERGY_YEAR =   0x13;
        public const byte CODE_READ_EVENT_ARCH =    0x14;

        public const byte CODE_WRITE_DATETIME =     0x72;

    }
}
