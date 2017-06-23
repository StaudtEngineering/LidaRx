using System;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx
{
    public class LidaRxStateException : Exception
    {
        public LidaRxStateException(string message) : base(message)
        {
        }
    }
}
