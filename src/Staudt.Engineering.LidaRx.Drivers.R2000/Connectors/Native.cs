#region Copyright
//
// This file is part of Staudt Engineering's LidaRx library
//
// Copyright (C) 2017 Yannic Staudt / Staudt Engieering
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion

using System.Runtime.InteropServices;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Connectors
{
    /// <summary>
    /// One packet header
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ScanFrameHeader
    {
        public ushort Magic;
        public ushort PacketType;
        public uint PacketSize;
        public ushort HeaderSize;

        public ushort ScanNumber;
        public ushort PacketNumber;

        public ulong Ntp64TimestampRaw;
        public ulong Ntp64TimestampSync;

        public uint StatusFlags;
        public uint ScanFrequency;
        public ushort NumberOfPointsPerScan;
        public ushort NumberOfPointsThisPacket;
        public ushort FirstIndexInThisPacket;
        public int FirstAngleInThisPacket;
        public int AnglularIncrement;

        public uint IQInput;
        public uint IQOverload;
    }


    struct ScanFrame
    {
        public ScanFrameHeader Header;
        public ScanFramePoint[] Points;
    }

    struct ScanFramePoint
    {
        public uint Distance;
        public ushort Amplitude;
        public float Angle;
        public ushort ScanCounter;

        // see datasheet 3.4.6
        public bool Valid => Distance < 0xfffff;
    }

}
