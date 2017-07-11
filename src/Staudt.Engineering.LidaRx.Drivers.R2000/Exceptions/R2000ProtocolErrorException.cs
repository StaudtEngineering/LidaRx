using System;
using Staudt.Engineering.LidaRx.Drivers.R2000.Serialization;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class R2000ProtocolErrorException : Exception
    {

        private IR2000ResponseWithError protocolFrame;

        public R2000ErrorCode ErrorCode => protocolFrame.ErrorCode;
        public string ErrorMessage => protocolFrame.ErrorText;

        internal R2000ProtocolErrorException(IR2000ResponseWithError frame, string message)
            : base(message)
        {
            this.protocolFrame = frame;
        }
    }
}
