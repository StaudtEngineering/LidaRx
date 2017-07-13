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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Connectors
{
    /// <summary>
    /// Helper class for single subscriptions
    /// </summary>
    class Unsubscriber<T> : IDisposable
    {
        private List<IObserver<T>> _observers;
        private IObserver<T> _observer;

        public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }


    static class BinaryHelpers
    {
        public static T BytesToStruct<T>(this byte[] bytes, int startOffset) where T : struct
        {
            var structSize = Marshal.SizeOf<T>();
            var pointer = IntPtr.Zero;

            try
            {
                pointer = Marshal.AllocHGlobal(structSize);
                Marshal.Copy(bytes, startOffset, pointer, structSize);
                return Marshal.PtrToStructure<T>(pointer);
            }
            finally
            {
                if (pointer != IntPtr.Zero)
                    Marshal.FreeHGlobal(pointer);
            }
        }

    }

}
