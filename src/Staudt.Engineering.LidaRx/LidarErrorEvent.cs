using System;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx
{
    public class LidarErrorEvent : ILidarEvent
    {
        public string Msg { get; private set; }

        public LidarErrorEvent(string msg)
        {
            this.Msg = msg;
        }

    }
}
