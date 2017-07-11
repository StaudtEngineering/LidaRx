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


namespace Staudt.Engineering.LidaRx.Drivers.R2000.Serialization
{
    public enum R2000ErrorCode
    {
        /// <summary>
        /// Success
        /// </summary>
        Success = 0,

        /// <summary>
        /// Unknown argument %s
        /// </summary>
        UnknownArgument = 100,

        /// <summary>
        /// Unknown parameter %s
        /// </summary>
        UnknownParameter = 110,

        /// <summary>
        /// Invalid handle or no handle provided
        /// </summary>
        InvalidHandle = 120,

        /// <summary>
        /// Required argument missing
        /// </summary>
        ArgumentMissing = 130,

        /// <summary>
        /// Invalid value for parameter %s
        /// </summary>
        InvalidValue = 200,

        /// <summary>
        /// Value for parameter %s is out of range
        /// </summary>
        ValueOutOfRange = 210,

        /// <summary>
        /// Write access to read only parameter %s
        /// </summary>
        WriteToReadOnlyParameter = 220,

        /// <summary>
        /// Insufficient memory
        /// </summary>
        OutOfMemory = 230,

        /// <summary>
        /// Ressource still/already in use
        /// </summary>
        RessourceInUse = 240,

        /// <summary>
        /// Internal error while processing command %s
        /// </summary>
        InternalError = 333,

        /// <summary>
        /// Not initialized value
        /// </summary>
        Undefined = -1
    }
}
