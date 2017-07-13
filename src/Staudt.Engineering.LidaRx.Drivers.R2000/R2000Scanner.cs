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
using Staudt.Engineering.LidaRx.Drivers.R2000.Connectors;
using Staudt.Engineering.LidaRx.Drivers.R2000.Exceptions;
using Staudt.Engineering.LidaRx.Drivers.R2000.Helpers;
using Staudt.Engineering.LidaRx.Drivers.R2000.Serialization;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
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
        /// On Connect() we start this thread to periodically check for the device's current status
        /// </summary>
        readonly int fetchStatusInterval;
        Thread fetchStatusThread;
        CancellationTokenSource fetchStatusCts;

        /// <summary>
        /// Note: use single HttpClient as it perfectly handles concurrent access
        /// </summary>
        private HttpClient commandClient;
        private IR2000Connector dataStreamConnector;


        private R2000ProtocolVersion instanceProtocolVersion;
        private BasicSensorInformation SensorInformation;
        private SensorCapabilitiesInformation SensorCapabilities;
        private EthernetConfigurationInformation EthernetConfiguration;
        private MeasuringConfigurationInformation MeasurementConfiguration;

        /// <summary>
        /// Create a new R2000Scanner object given the scanner's IP address and 
        /// the connection type used to stream LIDAR data
        /// </summary>
        /// <param name="address">IP address of the scanner</param>
        /// <param name="connectionType">TCP or UDP for data streaming</param>
        /// <param name="fetchStatusInterval">Interval to fetch the scanner status. Set to 0 to disable</param>
        public R2000Scanner(IPAddress address, R2000ConnectionType connectionType, int fetchStatusInterval = 10000)
        {
            this.fetchStatusInterval = fetchStatusInterval;

            // retrieve basic info
            commandClient = new HttpClient();
            commandClient.BaseAddress = new Uri($"http://{address.ToString()}/cmd/");

            if(connectionType == R2000ConnectionType.TCPConnection)
            {
                this.dataStreamConnector = new TCPConnector(commandClient, address, true, 10000);
            }
            else if(connectionType == R2000ConnectionType.UDPConnection)
            {
                throw new NotImplementedException();
            }

            var latestNativeScanCounter = 0;

            // publish the points
            this.dataStreamConnector.Subscribe<ScanFramePoint>(pt =>
            {
                // we don't use the native R2000 scan counter at it wraps around quite quickly
                // thus, we store the "latestNativeCounter" and check for increment or wraparound
                if (pt.ScanCounter > latestNativeScanCounter || pt.ScanCounter < latestNativeScanCounter)
                {
                    latestNativeScanCounter = pt.ScanCounter;
                    base.ScanCounter++;
                }

                // don't publish invalid packets
                if (!pt.Valid)
                    return;

                var carthCoordinate = base.TransfromScannerToSystemCoordinates(pt.Angle, pt.Distance);

                var point = new LidarPoint(
                    carthCoordinate, 
                    pt.Angle, 
                    pt.Distance,
                    (byte)(pt.Amplitude / 256), 
                    base.ScanCounter,
                    this);

                PublishLidarEvent(point);
            });

            // redirect the error/status events
            this.dataStreamConnector.Subscribe<LidarStatusEvent>(ev => this.PublishLidarEvent(ev));
        }

        public override void Connect()
        {
            ConnectAsync().Wait();
        }

        public override async Task ConnectAsync()
        {
            // don't do this again if we're already connected :O
            if (Connected)
                return;

            var protocolInfo = await commandClient.GetAsAsync<ProtocolInformation>("get_protocol_info");
            this.instanceProtocolVersion = protocolInfo.GetProtocolVersion();

            this.SensorInformation = await FetchConfigObject<BasicSensorInformation>();
            this.SensorCapabilities = await FetchConfigObject<SensorCapabilitiesInformation>();
            this.EthernetConfiguration = await FetchConfigObject<EthernetConfigurationInformation>();
            this.MeasurementConfiguration = await FetchConfigObject<MeasuringConfigurationInformation>();

            // start the periodical fetching if required
            if (fetchStatusInterval > 0)
            {
                this.fetchStatusCts = new CancellationTokenSource();
                this.fetchStatusThread = new Thread(ThreadFetchStatusPeriodically);
                this.fetchStatusThread.Start();
            }

            // Note: well... yes this is somewhat stupid, but hey(!) we managed to talk with
            // the R2000, so everything's fine and dandy
            this.connected = true;
        }

        public override void Disconnect()
        {
            fetchStatusCts.Cancel();

            if (IsScanning)
                StopScan();
        }

        public override async Task DisconnectAsync()
        {
            fetchStatusCts.Cancel();

            if (IsScanning)
                await StopScanAsync();
        }

        public override void StartScan()
        {
            this.dataStreamConnector.StartAsync().Wait();
            isScanning = true;
        }

        public override async Task StartScanAsync()
        {
            await this.dataStreamConnector.StartAsync();
            isScanning = true;
        }

        public override void StopScan()
        {
            if (IsScanning)
                this.dataStreamConnector.StopAsync().Wait();

            isScanning = false;
        }

        public override async Task StopScanAsync()
        {
            if (IsScanning)
                await this.dataStreamConnector.StopAsync();

            isScanning = false;
        }

        #region Fetch status stuff
        /// <summary>
        /// Note: This is running in a thread
        /// </summary>
        private async void ThreadFetchStatusPeriodically()
        {
            while(true)
            {
                if (fetchStatusCts.IsCancellationRequested)
                    break;

                var status = await FetchScannerStatusAsync();
                this.MeasurementConfiguration.CurrentScanFrequency = status.CurrentScanFrequency;
                PublishLidarEvent(status);

                await Task.Delay(fetchStatusInterval);
            }

            // not connected any more!
            this.connected = false;
        }

        /// <summary>
        /// Retrive the scanner status
        /// </summary>
        /// <returns></returns>
        public async Task<R2000Status> FetchScannerStatusAsync()
        {
            if (!Connected)
                throw new LidaRxStateException("This instance is not yet connected to the R2000 scanner.");

            // fetch and publish the status
            var status = await FetchConfigObject<R2000Status>();

            if (status.ErrorCode == R2000ErrorCode.Success)
            {
                return status;
            }
            else
                throw new R2000ProtocolErrorException(null, "Retrieving R2000 status failed");            
        }

        /// <summary>
        /// Retrive the scanner status
        /// </summary>
        /// <returns></returns>
        public R2000Status FetchScannerStatus()
        {
            return FetchScannerStatusAsync().Result;
        }

        #endregion

        #region Configuration API

        /// <summary>
        /// Set the scan frequency
        /// </summary>
        /// <param name="frequencyHz">Refer  to the vendor's manual for the acceptable range</param>
        public void SetScanFrequency(double frequencyHz)
        {
            SetScanFrequencyAsync(frequencyHz).Wait();
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

        /// <summary>
        /// Set the sampling rate
        /// </summary>
        /// <param name="targetSamplingRate">Target rate or AutoMaximum</param>
        public void SetSamplingRate(R2000SamplingRate targetSamplingRate)
        {
            SetSamplingRateAsync(targetSamplingRate).Wait();
        }

        /// <summary>
        /// Set the sampling rate
        /// </summary>
        /// <param name="targetSamplingRate">Target rate or AutoMaximum</param>
        public async Task SetSamplingRateAsync(R2000SamplingRate targetSamplingRate)
        {
            if (!Connected)
                throw new LidaRxStateException("This instance is not yet connected to the R2000 scanner.");

            // on recent devices we can check the configurable sample rate range!
            if (this.instanceProtocolVersion >= R2000ProtocolVersion.v101 
                && targetSamplingRate != R2000SamplingRate.AutomaticMaximum)
            {
                var targetAsInt = (uint)targetSamplingRate;

                if (targetAsInt < SensorCapabilities.SamplingRateMin || targetAsInt > SensorCapabilities.SamplingRateMax)
                    throw new ArgumentOutOfRangeException(
                        "targetSamplingRate",
                        $"Acceptable range is [{SensorCapabilities.SamplingRateMin}, {SensorCapabilities.SamplingRateMax}]");
            }

            uint targetSamplesPerScan = 0;

            // select the max sr automatically if the user asks for it ;)
            if (targetSamplingRate == R2000SamplingRate.AutomaticMaximum)
            {
                var currentDeviceFamily = this.SensorInformation.DeviceFamilly;
                var currentScanFrequency = this.MeasurementConfiguration.ScanFrequency;

                targetSamplesPerScan = SamplingRateSetting.Table
                    .Where(x => x.DeviceFamily == currentDeviceFamily)
                    .Where(x => x.MaximumScanFrequency <= currentScanFrequency)
                    .Select(x => x.SamplesPerScan)
                    .Max();
            }
            // if the user specified a sample rate, then check if it's in range
            // given the current scan frequency configuration
            else
            {
                var currentDeviceFamily = this.SensorInformation.DeviceFamilly;
                var currentScanFrequency = this.MeasurementConfiguration.ScanFrequency;

                targetSamplesPerScan = SamplingRateSetting.Table
                    .Where(x => x.DeviceFamily == currentDeviceFamily)
                    .Where(x => x.MaximumScanFrequency >= currentScanFrequency)
                    .Where(x => x.MaximumSampleRate == targetSamplingRate)
                    .Select(x => x.SamplesPerScan)
                    .Max();
            }

            if (targetSamplesPerScan == 0)
                throw new ArgumentException("targetSamplingRate", 
                    "There's no valid configuration for your device and the chosen target sample rate. " 
                    + "Please check for valid configurations in the R2000 manual");

            // write the config to the R2000
            await SetConfigParameter<MeasuringConfigurationInformation, uint>(x => x.SamplesPerScan, targetSamplesPerScan);

            // reflect the value in the local config
            this.MeasurementConfiguration.SamplesPerScan = targetSamplesPerScan;
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Publish a ILidarEvent
        /// </summary>
        /// <param name="ev"></param>
        void PublishLidarEvent(ILidarEvent ev)
        {
            foreach (var observer in base.observers)
            {
                observer.OnNext(ev);
            }
        }

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
            var jsonValue = JsonConvert.SerializeObject(value);
            var paramEncoded = System.Net.WebUtility.UrlEncode(jsonValue);
            var request = $"set_parameter?{jsonAttribute.PropertyName}={paramEncoded}";

            var result = await commandClient.GetAsAsync<SetParameterResult>(request);

            if (result.ErrorCode != R2000ErrorCode.Success)
            {
                if (result is IR2000ResponseWithError)
                    throw new R2000ProtocolErrorException((IR2000ResponseWithError)result, $"Could not set parameter { name } to value '{paramEncoded}'");
                else
                    throw new Exception($"Could not set parameter { name } to value '{paramEncoded}' because {result.ErrorText}");

            }
        }

        #endregion
    }
}
