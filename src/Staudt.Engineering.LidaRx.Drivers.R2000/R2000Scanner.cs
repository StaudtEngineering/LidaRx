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

using Newtonsoft.Json;
using Staudt.Engineering.LidaRx.Drivers.R2000.Helpers;
using Staudt.Engineering.LidaRx.Drivers.R2000.Serialization;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
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

        private R2000ProtocolVersion instanceProtocolVersion;
        private BasicSensorInformation SensorInformation;
        private SensorCapabilitiesInformation SensorCapabilities;
        private EthernetConfigurationInformation EthernetConfiguration;
        private MeasuringConfigurationInformation MeasurementConfiguration;

        /// <summary>
        /// Create a new R2000Scanner object given the scanner's IP address and 
        /// the connection type used to stream LIDAR data
        /// </summary>
        /// <param name="address"></param>
        /// <param name="connectionType"></param>
        public R2000Scanner(IPAddress address, R2000ConnectionType connectionType)
        {
            // retrieve basic info
            commandClient = new HttpClient();
            commandClient.BaseAddress = new Uri($"http://{address.ToString()}/cmd/");
        }

        public override void Connect()
        {
            var protocolInfo = commandClient.GetAsAsync<ProtocolInformation>("get_protocol_info").Result;
            this.instanceProtocolVersion = protocolInfo.GetProtocolVersion();

            this.SensorInformation = FetchConfigObject<BasicSensorInformation>().Result;
            this.SensorCapabilities = FetchConfigObject<SensorCapabilitiesInformation>().Result;
            this.EthernetConfiguration = FetchConfigObject<EthernetConfigurationInformation>().Result;
            this.MeasurementConfiguration = FetchConfigObject<MeasuringConfigurationInformation>().Result;

            // Note: well... yes this is somewhat stupid, but hey(!) we managed to talk with
            // the R2000, so everything's fine and dandy
            this.connected = true;
        }

        public override async Task ConnectAsync()
        {
            var protocolInfo = await commandClient.GetAsAsync<ProtocolInformation>("get_protocol_info");
            this.instanceProtocolVersion = protocolInfo.GetProtocolVersion();

            this.SensorInformation = await FetchConfigObject<BasicSensorInformation>();
            this.SensorCapabilities = await FetchConfigObject<SensorCapabilitiesInformation>();
            this.EthernetConfiguration = await FetchConfigObject<EthernetConfigurationInformation>();
            this.MeasurementConfiguration = await FetchConfigObject<MeasuringConfigurationInformation>();

            // Note: well... yes this is somewhat stupid, but hey(!) we managed to talk with
            // the R2000, so everything's fine and dandy
            this.connected = true;
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

        #region Configuration API

        /// <summary>
        /// Set the scan frequency
        /// </summary>
        /// <param name="frequencyHz">Refer  to the vendor's manual for the acceptable range</param>
        public void SetScanFrequency(double frequencyHz)
        {
            if (!Connected)
                throw new LidaRxStateException("This instance is not yet connected to the R2000 scanner.");

            // on recent devices we can check the configurable frequency range!
            if (this.instanceProtocolVersion >= R2000ProtocolVersion.v101)
            {
                if (frequencyHz < SensorCapabilities.ScanFrequencyMin || frequencyHz > SensorCapabilities.ScanFrequencyMax)
                    throw new ArgumentOutOfRangeException(
                        "frequencyHz",
                        $"Acceptable range is [{SensorCapabilities.ScanFrequencyMin}, {SensorCapabilities.ScanFrequencyMax}]");
            }

            SetConfigParameter<MeasuringConfigurationInformation, double>(x => x.ScanFrequency, frequencyHz).Wait();

            // when it didn't fail...
            this.MeasurementConfiguration.ScanFrequency = frequencyHz;
        }

        /// <summary>
        /// Set the scan frequency
        /// </summary>
        /// <param name="frequencyHz">Refer  to the vendor's manual for the acceptable range</param>
        /// <returns></returns>
        public async Task SetScanFrequencyAsync(double frequencyHz)
        {
            if (!Connected)
                throw new LidaRxStateException("This instance is not yet connected to the R2000 scanner.");

            // on recent devices we can check the configurable frequency range!
            if (this.instanceProtocolVersion >= R2000ProtocolVersion.v101)
            {
                if (frequencyHz < SensorCapabilities.ScanFrequencyMin || frequencyHz > SensorCapabilities.ScanFrequencyMax)
                    throw new ArgumentOutOfRangeException(
                        "frequencyHz",
                        $"Acceptable range is [{SensorCapabilities.ScanFrequencyMin}, {SensorCapabilities.ScanFrequencyMax}]");
            }

            await SetConfigParameter<MeasuringConfigurationInformation, double>(x => x.ScanFrequency, frequencyHz);

            // when it didn't fail...
            this.MeasurementConfiguration.ScanFrequency = frequencyHz;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Fetch the configuration object from the R2000 (automatically generates the 
        /// parameters list etc from attributes)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="protocolVersion"></param>
        /// <returns></returns>
        private async Task<T> FetchConfigObject<T>()
        {
            var parameters = typeof(T).GetR2000ParametersList(this.instanceProtocolVersion);
            return await commandClient.GetAsAsync<T>($"get_parameter?list={String.Join(";", parameters)}");
        }

        /// <summary>
        /// Set a configuration parameter to a value
        /// </summary>
        /// <typeparam name="TObj"></typeparam>
        /// <typeparam name="TParam"></typeparam>
        /// <param name="selector">Select the property on one of the config objects</param>
        /// <param name="value">Target value</param>
        /// <returns></returns>
        private async Task SetConfigParameter<TObj, TParam>(Expression<Func<TObj,TParam>> selector, TParam value)
        {
            
            Type type = typeof(TObj);

            MemberExpression member = selector.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    selector.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    selector.ToString()));

            var name = propInfo.Name;
            var jsonAttribute = propInfo.GetCustomAttribute<JsonPropertyAttribute>();
            var r2000InfoAttr = propInfo.GetCustomAttribute<R2000ParameterInfoAttribute>();

            if (jsonAttribute == null || r2000InfoAttr == null)
                throw new ArgumentException("The chosen property is not correctly annotated (JsonPropertyAttribute or R2000ParameterInfoAttribute missing)");

            if (r2000InfoAttr.AccessType == R2000ParameterType.ReadOnlyStatic || r2000InfoAttr.AccessType == R2000ParameterType.ReadOnly)
                throw new ArgumentException("The chosen property is read only");

            {
                var minVersionOk = r2000InfoAttr.MinProtocolVersion == R2000ProtocolVersion.Any || r2000InfoAttr.MinProtocolVersion <= this.instanceProtocolVersion;
                var maxVersionOk = r2000InfoAttr.MaxProtocolVersion == R2000ProtocolVersion.Any || r2000InfoAttr.MaxProtocolVersion >= this.instanceProtocolVersion;

                if (!minVersionOk || !maxVersionOk)
                    throw new ArgumentException($"This parameter is not supported on this devices firmware version (min: {r2000InfoAttr.MinProtocolVersion} / max: {r2000InfoAttr.MaxProtocolVersion})");
            }

            // build the url
            var paramEncoded = System.Net.WebUtility.UrlEncode(value.ToString());
            var request = $"set_parameter?{jsonAttribute.PropertyName}={paramEncoded}";

            var result = await commandClient.GetAsAsync<SetParameterResult>(request);

            if (result.ErrorCode != R2000ErrorCode.Success)
                throw new Exception($"Could not set parameter {name} to value '{paramEncoded}' because '{result.ErrorText}'");
        }

        #endregion
    }
}
