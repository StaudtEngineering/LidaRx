using RJCP.IO.Ports;

namespace Staudt.Engineering.LidaRx.Drivers.Sweep.Protocol
{
    interface ISweepCommand
    {
        char[] Command { get; }
        int ExpectedAnswerLength { get; }
        void ProcessResponse(char[] response);
    }
}
