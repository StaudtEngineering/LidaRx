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

using Staudt.Engineering.LidaRx.Drivers.R2000.Helpers;
using Staudt.Engineering.LidaRx.Drivers.R2000.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Staudt.Engineering.LidaRx.Drivers.R2000
{
    public class R2000Scanner : LidarScannerBase
    {
        bool connected = false;
        public override bool Connected => this.connected;

        bool isScanning = false;
        public override bool IsScanning => this.isScanning;

        /// <summary>
        /// Note: use single HttpClient as it perfectly handles concurrent access
        /// </summary>
        private HttpClient commandClient;

        private BasicSensorInformation SensorInformation;
        private SensorCapabilitiesInformation SensorCapabilities;
        private EthernetConfigurationInformation EthernetConfiguration;
        private MeasuringConfigurationInformation MeasurementConfiguration;


        public R2000Scanner(IPAddress address, R2000ConnectionType connectionType)
        {
            // retrieve basic info
            commandClient = new HttpClient();
            commandClient.BaseAddress = new Uri($"http://{address.ToString()}/cmd/");
        }

        public override void Connect()
        {
            var protocolInfo = commandClient.GetAsAsync<ProtocolInformation>("get_protocol_info").Result;
            var protocolVersion = protocolInfo.GetProtocolVersion();

            this.SensorInformation = FetchConfigObject<BasicSensorInformation>(protocolVersion).Result;
            this.SensorCapabilities = FetchConfigObject<SensorCapabilitiesInformation>(protocolVersion).Result;
            this.EthernetConfiguration = FetchConfigObject<EthernetConfigurationInformation>(protocolVersion).Result;
            this.MeasurementConfiguration = FetchConfigObject<MeasuringConfigurationInformation>(protocolVersion).Result;
        }

        public override async Task ConnectAsync()
        {
            var protocolInfo = await commandClient.GetAsAsync<ProtocolInformation>("get_protocol_info");
            var protocolVersion = protocolInfo.GetProtocolVersion();

            this.SensorInformation = await FetchConfigObject<BasicSensorInformation>(protocolVersion);
            this.SensorCapabilities = await FetchConfigObject<SensorCapabilitiesInformation>(protocolVersion);
            this.EthernetConfiguration = await FetchConfigObject<EthernetConfigurationInformation>(protocolVersion);
            this.MeasurementConfiguration = await FetchConfigObject<MeasuringConfigurationInformation>(protocolVersion);
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override async Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public override void StartScan()
        {
            throw new NotImplementedException();
        }

        public override async Task StartScanAsync()
        {
            throw new NotImplementedException();
        }

        public override void StopScan()
        {
            throw new NotImplementedException();
        }

        public override async Task StopScanAsync()
        {
            throw new NotImplementedException();
        }

        #region Helpers

        /// <summary>
        /// Fetch the configuration object from the R2000 (automatically generates the 
        /// parameters list etc from attributes)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="protocolVersion"></param>
        /// <returns></returns>
        private async Task<T> FetchConfigObject<T>(R2000ProtocolVersion protocolVersion)
        {
            var parameters = typeof(T).GetR2000ParametersList(protocolVersion);
            return await commandClient.GetAsAsync<T>($"get_parameter?list={String.Join(";", parameters)}");
        }

        #endregion
    }
}
