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

        private HttpClient commandClient;

        public R2000Scanner(IPAddress address, R2000ConnectionType connectionType)
        {
            // retrieve basic info
            commandClient = new HttpClient();
            commandClient.BaseAddress = new Uri($"http://{address.ToString()}/cmd/");
        }

        public override void Connect()
        {
            var r = commandClient.GetAsAsync<Protocolnformation>("get_protocol_info").Result;


            var basicInfoParameters = typeof(BasicSensorInformation).GetR2000ParametersList();
            var bsi = commandClient.GetAsAsync<BasicSensorInformation>($"get_parameter?list={String.Join(";", basicInfoParameters)}").Result;


            var sensorCapParameter = typeof(SensorCapabilitiesInformation).GetR2000ParametersList();
            var scap = commandClient.GetAsAsync<SensorCapabilitiesInformation>($"get_parameter?list={String.Join(";", sensorCapParameter)}").Result;

            var ethernetConfParams = typeof(EthernetConfigurationInformation).GetR2000ParametersList();
            var ethConf = commandClient.GetAsAsync<EthernetConfigurationInformation>($"get_parameter?list={String.Join(";", ethernetConfParams)}").Result;

            var measureConfParam = typeof(MeasuringConfigurationInformation).GetR2000ParametersList();
            var mesConf = commandClient.GetAsAsync<MeasuringConfigurationInformation>($"get_parameter?list={String.Join(";", measureConfParam)}").Result;

        }

        public override Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public override void StartScan()
        {
            throw new NotImplementedException();
        }

        public override Task StartScanAsync()
        {
            throw new NotImplementedException();
        }

        public override void StopScan()
        {
            throw new NotImplementedException();
        }

        public override Task StopScanAsync()
        {
            throw new NotImplementedException();
        }
    }
}
