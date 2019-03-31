using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wx.Qunkong360.Wpf.Events;
using Wx.Qunkong360.Wpf.Implementation;
using Xzy.EmbeddedApp.Utils;

namespace Wx.Qunkong360.Wpf.Utils
{
    public class DeviceConnectionManager
    {
        public string GetDeviceNameByMobileIndex(int index)
        {
            string possibleDeviceName1 = $"127.0.0.1:{5555 + index * 2}";
            string possibleDeviceName2 = $"emulator-{5554 + index * 2}";

            string deviceName = string.Empty;

            if (Devices.Any(device => device == possibleDeviceName1))
            {
                deviceName = possibleDeviceName1;
            }

            if (Devices.Any(device => device == possibleDeviceName2))
            {
                deviceName = possibleDeviceName2;
            }

            return deviceName;
        }

        public static readonly DeviceConnectionManager Instance = new DeviceConnectionManager();

        //public static readonly object TimerSyncObj = new object();

        private Timer _checkCountTimer = null;

        private Timer _checkBlockTimer = null;

        //private Timer _checkBlockTimer2 = null;

        private int _connectionFailedTimes = 0;

        public string[] Devices
        {
            get; set;
        }

        private List<string> _oldDevices = new List<string>();
        private List<string> _newDevices = new List<string>();

        private string[] _rawDevices;

        //private string _killServerResult = null;
        //private string _startServerResult = null;


        private DeviceConnectionManager()
        {

        }

        public void LaunchAndroidTestOnTheBackground(int dueTime)
        {
            if (_checkCountTimer == null)
            {
                _checkCountTimer = new Timer(CheckDeviceConnection, null, Timeout.Infinite, Timeout.Infinite);
            }

            if (_checkBlockTimer == null)
            {
                _checkBlockTimer = new Timer(CheckBlock, null, Timeout.Infinite, Timeout.Infinite);
            }

            //if (_checkBlockTimer2 == null)
            //{
            //    _checkBlockTimer2 = new Timer(CheckBlock2, null, Timeout.Infinite, Timeout.Infinite);
            //}

            Reset();

            _checkBlockTimer.Change(dueTime, Timeout.Infinite);
        }

        //private void CheckBlock2(object state)
        //{
        //    if (_killServerResult == null)
        //    {
        //        Console.WriteLine("_killServerResult is null");
        //        _checkBlockTimer2.Change(4000, Timeout.Infinite);
        //        Task.Run(() =>
        //        {
        //            _killServerResult = ProcessUtils.KillAdbServer();
        //        });
        //    }
        //    else
        //    {
        //        if (_startServerResult == null)
        //        {
        //            Console.WriteLine("_startServerResult is null");
        //            _checkBlockTimer2.Change(4000, Timeout.Infinite);
        //            Task.Run(() =>
        //            {
        //                _startServerResult = ProcessUtils.StartAdbServer();
        //            });
        //        }
        //        else
        //        {
        //            LaunchAndroidTestOnTheBackground(1000);
        //        }
        //    }
        //}

        //public void ReconnectDevices()
        //{
        //    Devices = new string[0];

        //    if (VmManager.Instance.RunningGroupIndex == -1)
        //    {
        //        MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo));

        //        return;
        //    }

        //    lock (TimerSyncObj)
        //    {
        //        _checkCountTimer.Change(Timeout.Infinite, Timeout.Infinite);
        //    }

        //    EventAggregatorManager.Instance.EventAggregator.GetEvent<VmClosedEvent>().Publish();

        //    _checkBlockTimer2.Change(1000, Timeout.Infinite);
        //}

        private void CheckBlock(object state)
        {
            if (_rawDevices == null)
            {
                Console.WriteLine("TRY getting attached devices");
                _checkBlockTimer.Change(6000, Timeout.Infinite);
                Task.Run(() =>
                {
                    _rawDevices = ProcessUtils.GetAttachedDevices();
                });
            }
            else
            {
                Console.WriteLine($"TRY getting attached devices SUCCESSFULLY, count={_rawDevices?.Length}");
                //lock (TimerSyncObj)
                //{
                    _checkCountTimer.Change(1000, Timeout.Infinite);
                //}
            }
        }

        private async void CheckDeviceConnection(object state)
        {
            Console.WriteLine("START getting attached devices");

            Devices = ProcessUtils.GetAttachedDevices();

            Console.WriteLine($"START getting attached devices SUCCESSFULLY, count={Devices?.Length}");

            _newDevices.Clear();

            foreach (var device in Devices)
            {
                if (!_oldDevices.Contains(device))
                {
                    _newDevices.Add(device);
                    _oldDevices.Add(device);
                }
            }

            if (VmManager.Instance.GetRunningCount() != Devices.Length)
            {
                _connectionFailedTimes++;

                if (VmManager.Instance.RunningGroupIndex != -1)
                {
                    if (_connectionFailedTimes > 30)
                    {
                        //ReconnectDevices();
                    }
                    else
                    {
                        //lock (TimerSyncObj)
                        //{
                            _checkCountTimer.Change(5000, Timeout.Infinite);
                        //}
                    }
                }
            }

            if (_newDevices.Count > 0)
            {
                // new device(s) attached

                EventAggregatorManager.Instance.EventAggregator.GetEvent<NewDeviceAttachedEvent>().Publish();

                List<Task> tasks = new List<Task>();

                foreach (var device in _newDevices)
                {
                    Console.WriteLine($"{device} ATTACHED");
                    tasks.Add(LaunchAndroidCommanderAsync(device));
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task LaunchAndroidCommanderAsync(string device)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"LAUNCHING {device}");
                ProcessUtils.LaunchAndroidCommander(device);
                Console.WriteLine($"LAUNCHED {device}");
            });
        }

        public void Reset()
        {
            _connectionFailedTimes = 0;

            Devices = new string[0];
            _oldDevices.Clear();
            _newDevices.Clear();
            _rawDevices = null;
        }
    }
}
