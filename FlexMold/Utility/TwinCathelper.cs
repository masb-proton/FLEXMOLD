﻿using FlexMold.MVVM.Model;
using FlexMold.MVVM.ViewModel;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;
using static FlexMold.Utility.TwinCathelper;

namespace FlexMold.Utility
{
    public partial class TwinCathelper : ITwinCathelper
    {
        private bool machineEmergencyDisconnected = false;
        public void SetMachineStatusTimer()
        {
            ValidStateMachines = new ESlaveStateMachine[DeviceCount];
            ValidElinkStates = new ESlaveLink[DeviceCount];
            defaultStateMachines = new ESlaveStateMachine[DeviceCount];
            defaultElinkStates = new ESlaveLink[DeviceCount];

            Array.Fill(ValidStateMachines, ESlaveStateMachine.EC_DEVICE_STATE_OP);
            Array.Fill(defaultStateMachines, ESlaveStateMachine.UnAvailable);
            Array.Fill(ValidElinkStates, ESlaveLink.EC_LINK_STATE_OK);
            Array.Fill(defaultElinkStates, ESlaveLink.UnAvailable);

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(Timer_Elapsed);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
            dispatcherTimer.Start();
        }
        private void Timer_Elapsed(object sender, EventArgs e)
        {
            GetSlaveMachineState();
        }
        private void GetSlaveMachineState()
        {
            try
            {
                _log.Log(LogLevel.Trace, $"Writing {TCSymbols.ECAbExecute} : 1");

                var pp = MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbExecute}").FirstOrDefault().TryWriteValue(1, 200);

                ECAbError = (bool)MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbError}").FirstOrDefault().ReadAnyValue(typeof(bool));
                _log.Log(LogLevel.Trace, $"Readed {TCSymbols.ECAbError} : {JsonSerializer.Serialize(ECAbError)}");

                if (!ECAbError)
                {
                    var deviceandlinkstatusbytes = (byte[])MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAdevStates}").FirstOrDefault().ReadValue();
                    _log.Log(LogLevel.Trace, $"Readed {TCSymbols.ECAdevStates} : {JsonSerializer.Serialize(deviceandlinkstatusbytes)}");

                    var oddIndexesValues = deviceandlinkstatusbytes.Where((value, index) => index % 2 != 0);
                    Slave_Link_Status = oddIndexesValues.Select(b => (ESlaveLink)Enum.ToObject(typeof(ESlaveLink), b)).ToArray();
                    var evenIndexesValues = deviceandlinkstatusbytes.Where((value, index) => index % 2 == 0);
                    Slave_StateMachine_Status = evenIndexesValues.Select(b => (ESlaveStateMachine)Enum.ToObject(typeof(ESlaveStateMachine), b)).ToArray();

                    if (Slave_Link_Status[0].ToString().Contains("EC_LINK_STATE_NOT_PRESENT"))
                    {
                        if (!machineEmergencyDisconnected)//if false
                        {
                            machineEmergencyDisconnected = true;
                            //FlexMold.MVVM.ViewModel.Project.ProjectViewModel.EventStopToggle = true;

                            //if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled)
                            //{
                            //    _log.Log(LogLevel.Info, "Starting timer for Stop/Resume!");

                            if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled)
                            {
                                FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = 'N';
                                FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.Start(); }
                            //}
                        }

                    }
                    //2024-02-12 14:25:11.9061|INFO|Machine Link Status|FlexMold.MVVM.ViewModel.Home.MachineDetailViewModel.DispLinkStatus|Box_16 --> Link State EC_LINK_STATE_OK
                    else if (Slave_Link_Status[0].ToString().Contains("EC_LINK_STATE_OK") || Slave_Link_Status[0].ToString().Contains("EC_DEVICE_STATE_INIT"))//&& machineEmergencyDisconnected
                    {
                        machineEmergencyDisconnected = false;
                        //FlexMold.MVVM.ViewModel.Project.ProjectViewModel.EventStopToggle = true;                        
                    }
                }
                else
                {
                    Slave_Link_Status = defaultElinkStates;
                    Slave_StateMachine_Status = defaultStateMachines;
                }
                _log.Log(LogLevel.Trace, $"Writing {TCSymbols.ECAbExecute} : 0");

                MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbExecute}").FirstOrDefault().WriteValue(0);
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.Message);


                //if (!machineEmergencyDisconnected)
                //{
                //    machineEmergencyDisconnected = true;
                //    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.Start();
                //}
            }
        }
        public ESlaveLink[] Slave_Link_Status
        {
            get => _Slave_Link_Status;
            set
            {
                _Slave_Link_Status = value;
                TSlave_Link_Status_Changed.Invoke(null, null);
            }
        }
        public ESlaveStateMachine[] Slave_StateMachine_Status
        {
            get => _Slave_StateMachine_Status;
            set
            {
                _Slave_StateMachine_Status = value;
                TSlave_StateMachine_Status_Changed.Invoke(null, null);
            }
        }
        public void SetDefaultMode(Logger log)
        {
            if (Motor_Diagnostic_Abort == false &&
                Slave_Link_Abort == false &&
                Slave_StateMachine_Abort == false)
            {
                for (int i = 0; i < DeviceCount; i++)
                    PLCMotors[i].mMode = (int)EMode.Displacement;
                WriteEthercatDeviceData(DeviceSymbol.mMode);
                log.Log(LogLevel.Info, $"Configured to Default Device Mode : {EMode.Displacement}");
                TNF.ShowSuccess($"Default Device Mode : {EMode.Displacement}");
            }
        }
        public void SetAbortMode(Logger log)
        {
            if (Motor_Diagnostic_Abort == true ||
                Slave_Link_Abort == true ||
                Slave_StateMachine_Abort == true)
            {
                //for (int i = 0; i < DeviceCount; i++)
                //    PLCMotors[i].mRPM = 0;
                //WriteEthercatDeviceData(DeviceSymbol.mRPM);
                
                for (int i = 0; i < DeviceCount; i++)
                    PLCMotors[i].mMode = (int)EMode.Abort;

                WriteEthercatDeviceData(DeviceSymbol.mMode);
                log.Log(LogLevel.Warn, $"Device Mode Changed to : {EMode.Abort} This is an Abort Command");
                //log.Log(LogLevel.Warn, $"RPM values Changed to : 0");
                TNF.ShowError($"Device Mode: {EMode.Abort}");
            }
        }
        public bool TwinCatConnect851()
        {
            _port = 851;
            _log.Log(LogLevel.Info, $"Connecting to Local Host, Port : {_port}");
            Session = new AdsSession(TwinCAT.Ads.AmsNetId.Local, _port);
            Client = (AdsConnection)Session.Connect();
            if (Client.IsConnected)
            {
                _log.Log(LogLevel.Info, $"Connected to : {Client.Address}, Port : {_port}");
                if (Client.IsActive)
                {
                    if (GetPLCVariables())
                    {
                        InitializeVarables();
                        return true;
                    }
                }
            }
            return false;
        }
        public void TwinCatDiconnectConnect851()
        {
            _log.Log(LogLevel.Info, $"DisConnecting to Local Host, Port : {_port}");
            if (Client!=null && Client.IsConnected)
                Client.Disconnect();
            if (Session != null &&  Session.IsConnected)
                Session.Disconnect();
            Client = null;
            Session = null;
        }
        public void InitializeNotifications()
        {
            Session.ConnectionStateChanged += Session_ConnectionStateChanged;
            ClientSystemService.ConnectionStateChanged += Connection_ConnectionStatusChanged;
            Client.RouterStateChanged += Connection_RouterStateChanged;
            Client.RegisterAdsStateChangedAsync(Connection_AdsStateChanged, CancellationToken.None);

            RegisterNotification($"MAIN.{TCSymbols.mPosition}");
            RegisterNotification($"MAIN.{TCSymbols.MotorDiagnostic}");
            RegisterNotification($"MAIN.{TCSymbols.DeviceStatus}");
            Client.AdsNotification += Client_AdsNotification;
        }
        private void Client_AdsNotification(object sender, AdsNotificationEventArgs e)
        {

            try
            {
                var _mysymbol = e.UserData as Symbol;
                if (_mysymbol == null)
                    return;

                TCSymbols aa;
                Enum.TryParse(_mysymbol.InstancePath.Replace("MAIN.", ""), out aa);
                switch (aa)
                {
                    case TCSymbols.mPosition:
                        var data = BitConverter.ToSingle(e.Data.ToArray());
                        _log.Log(LogLevel.Trace, $"Readed {TCSymbols.mPosition} : {JsonSerializer.Serialize(data)}");
                        for (int i = 0; i < PLCMotors.Length; i++)
                            PLCMotorsStatuses.mPostion[i] = BitConverter.ToSingle(e.Data.ToArray(), i * sizeof(float));
                        TMotor_PositionChanged?.Invoke(null, e);
                        break;
                    case TCSymbols.MotorDiagnostic:
                        _log.Log(LogLevel.Trace, $"Readed {TCSymbols.MotorDiagnostic} : {JsonSerializer.Serialize(e.Data.ToArray())}");

                        Buffer.BlockCopy(e.Data.ToArray(), 0, PLCMotorsStatuses.MotorDiagnistic, 0, e.Data.Length);
                        TMotor_MotorDiagnosticChanged?.Invoke(null, e);
                        try { _log.Log(LogLevel.Info, $"MotorDiagnistic Updated : {(EDeviceStatus)int.Parse(String.Join(",", PLCMotorsStatuses.MotorDiagnistic))}"); } catch { }
                        break;
                    case TCSymbols.DeviceStatus:
                        _log.Log(LogLevel.Trace, $"Readed {TCSymbols.DeviceStatus} : {JsonSerializer.Serialize(e.Data.ToArray())}");
                        Buffer.BlockCopy(e.Data.ToArray(), 0, PLCMotorsStatuses.DeviceStatus, 0, e.Data.Length);                        
                        TMotor_DeviceStatusChanged?.Invoke(null, e);
                        
                        try
                        {
                            if (((EDeviceStatus)int.Parse(String.Join(",", PLCMotorsStatuses.DeviceStatus))).ToString() == "CalibrationSquence02Success")
                            {
                                CalibrateSeq02_HomeUp();
                            }
                        }
                        catch { }
                        try { _log.Log(LogLevel.Info, $"DeviceStatus Updated : {(EDeviceStatus)int.Parse(String.Join(",", PLCMotorsStatuses.DeviceStatus))}"); } catch { }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.Message);
            }

            TMotor_PositionChanged?.Invoke(null, e);
        }

        void CalibrateSeq02_HomeUp()
        {
            try
            {
                Thread.Sleep(200);

                for (int i = 0; i < DeviceCount; i++)
                {
                    PLCMotors[i].mDisplacement = 0.200F;
                    PLCMotors[i].mDirection = false;
                    PLCMotors[i].mRampDelay = CSVHelper.MotorRampDelayValue;
                    PLCMotors[i].mTime = 6.3334F;
                    PLCMotors[i].mMode = (int)EMode.Project;

                    //log.Log(LogLevel.Info, "CalibrateHard-WriteCSVData loop1:" + i.ToString() + " : " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + _TwinCathelper.PLCMotors[i].mDirection.ToString()
                    //+ "  >> RampDlay: " + CSVHelper.MotorRampDelayValue.ToString()
                    //+ "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime.ToString()
                    //+ "  >> mMode: " + EMode.Project.ToString());
                }

                WriteEthercatDeviceData();
                _log.Log(LogLevel.Info, $"{DeviceCount} CalibrateHard value(s) has been written sucessfully");

                Thread.Sleep(200);
                uint zero = 0;
                for (uint i = 0; i < DeviceCount; i++)
                    WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
                _log.Log(LogLevel.Info, $"{DeviceCount} zero value(s) has been written sucessfully");
            }
            catch (Exception ee){ _log.Log(LogLevel.Info, "CalibrateSeq02_HomeUp>>>>"+ ee.Message); }
        }

        private void Session_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            MyConnectionStateOld = e.OldState;
            MyConnectionStateNew = e.NewState;
            MyConnectionStateChangedReason = e.Reason;
            _log.Log(LogLevel.Info, $"Session Connection Status : {MyConnectionStateNew}");
            TSession_ConnectionStateChanged?.Invoke(null, e);
        }
        private void Connection_ConnectionStatusChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            MyConnectionStateOld = e.OldState;
            MyConnectionStateNew = e.NewState;
            MyConnectionStateChangedReason = e.Reason;
            _log.Log(LogLevel.Info, $"Connection Status : {MyConnectionStateNew}");
            TConnection_ConnectionStatusChanged?.Invoke(null, e);
        }
        private void Connection_RouterStateChanged(object sender, AmsRouterNotificationEventArgs e)
        {
            MyAmsRouterState = e.State;
            _log.Log(LogLevel.Info, $"Router State : {MyAmsRouterState}");
            TConnection_RouterStateChanged?.Invoke(null, e);
        }
        private void Connection_AdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {
            MyAdsState = e.State.AdsState;
            switch (e.State.AdsState)
            {
                case AdsState.Run:
                    _log.Log(LogLevel.Info, $"AdsState : {MyAdsState}");
                    break;
                default:
                    _log.Log(LogLevel.Error, $"AdsState : {MyAdsState}");
                    break;
            }
            TConnection_AdsStateChanged?.Invoke(null, e);
        }

        public bool InitializeSlaves(EMachineType emt)
        {
            try
            {
                switch (emt)
                {
                    case EMachineType.Motor:
                        DeviceCount = (short)MySymbols.FirstOrDefault(a =>
                           a.InstancePath == TCVarName.DeviceCount).ReadAnyValue(typeof(short));
                        PLCMotors = new EthercatDevice[DeviceCount];
                        PLCMotorsStatuses = new DeviceStatuses((uint)DeviceCount);
                        SymbolEthercatDevice = MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.device}").FirstOrDefault();
                        structSize = (SymbolEthercatDevice.Size) / DeviceCount;
                        buffer = new byte[structSize * DeviceCount];
                        ReadAllEthercatDeviceData();
                        if (SessionSystemService == null)
                        {
                            SessionSystemService = new AdsSession(TwinCAT.Ads.AmsNetId.Local, (int)AmsPort.SystemService);
                            ClientSystemService = (AdsConnection)SessionSystemService.Connect();
                        }
                        return true;
                    case EMachineType.ScanDevices:
                        DeviceCount = (short)MySymbols.FirstOrDefault(a =>
                           a.InstancePath == TCVarName.DeviceCount).ReadAnyValue(typeof(short));
                        if (MySymbols.Any(a => a.InstancePath == $"MAIN.{TCSymbols.SlaveCount}"))
                        {
                            if (SessionSystemService == null)
                            {
                                SessionSystemService = new AdsSession(TwinCAT.Ads.AmsNetId.Local, (int)AmsPort.SystemService);
                                ClientSystemService = (AdsConnection)SessionSystemService.Connect();
                            }
                            return true;
                        }
                        return false;
                    default:
                        return false;
                }

            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.Message);
                return false;
            }
        }

        public int RegisterNotification(string _deviceStatusVariableName)
        {
            if (Client.State == ConnectionState.Connected)
            {
                var deviceStatusSymbol = MySymbols.Where(a => a.InstancePath == _deviceStatusVariableName).FirstOrDefault();
                return (int)Client.AddDeviceNotification(deviceStatusSymbol.IndexGroup,
                   deviceStatusSymbol.IndexOffset, (int)(deviceStatusSymbol.Size), NotificationSettings.Default, deviceStatusSymbol);
            }
            _log.Log(LogLevel.Error, $"TwinCAT Connection is {Client.State}");
            return -1;
        }

        private void ReadAllEthercatDeviceData()
        {
            Client.Read(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset, buffer.AsMemory());

            for (int i = 0; i < DeviceCount; i++)
            {
                var mtarray = new byte[structSize];
                Array.Copy(buffer, structSize * i, mtarray, 0, structSize);
                PLCMotors[i].Unpack(mtarray);
            }
            _log.Log(LogLevel.Trace, $"Readed SymbolEthercatDevice : {JsonSerializer.Serialize(PLCMotors)}");

        }

        private void InitializeVarables()
        {
            SAmsNetId = (string)MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_sAmsNetId}").FirstOrDefault().ReadAnyValue(typeof(string));
            _log.Log(LogLevel.Trace, $"Readed SAmsNetId : {JsonSerializer.Serialize(SAmsNetId)}");
            SAmsPort = (UInt16[])MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_sAmsPort}").FirstOrDefault().ReadAnyValue(typeof(UInt16[]));
            _log.Log(LogLevel.Trace, $"Readed SAmsPort : {JsonSerializer.Serialize(SAmsPort)}");
        }

        public void ReadEthercatDeviceData(uint _motorNumber)
        {
            var mtarray = new byte[structSize];
            Client.Read(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset + (uint)_motorNumber * (uint)structSize, mtarray.AsMemory());
            PLCMotors[_motorNumber].Unpack(mtarray);
            _log.Log(LogLevel.Trace, $"Readed SymbolEthercatDevice[{_motorNumber}] : {JsonSerializer.Serialize(PLCMotors[_motorNumber])}");
        }
        public void ReadEthercatDeviceData(DeviceSymbol deviceSymbol)
        {
            switch (deviceSymbol)
            {
                case DeviceSymbol.mDirection:
                    for (uint i = 0; i < DeviceCount; i++)
                        PLCMotors[i].mDirection = Convert.ToBoolean(ReadEthercatDeviceData(i, deviceSymbol, sizeof(float))[0]);
                    break;
                case DeviceSymbol.mRPM:
                    for (uint i = 0; i < DeviceCount; i++)
                        PLCMotors[i].mRPM = Convert.ToSingle(ReadEthercatDeviceData(i, deviceSymbol, sizeof(float)));
                    break;
                case DeviceSymbol.mDisplacement:
                    for (uint i = 0; i < DeviceCount; i++)
                        PLCMotors[i].mDisplacement = Convert.ToSingle(ReadEthercatDeviceData(i, deviceSymbol, sizeof(float)));
                    break;
                case DeviceSymbol.mTime:
                    for (uint i = 0; i < DeviceCount; i++)
                        PLCMotors[i].mTime = Convert.ToSingle(ReadEthercatDeviceData(i, deviceSymbol, sizeof(float)));
                    break;
                case DeviceSymbol.mMode:
                    for (uint i = 0; i < DeviceCount; i++)
                        PLCMotors[i].mMode = Convert.ToUInt32(ReadEthercatDeviceData(i, deviceSymbol, sizeof(float)));
                    break;
                case DeviceSymbol.mRampDelay:
                    for (uint i = 0; i < DeviceCount; i++)
                        PLCMotors[i].mRampDelay = Convert.ToSingle(ReadEthercatDeviceData(i, deviceSymbol, sizeof(float)));
                    break;
                default:
                    break;
            }
        }
        public byte[] ReadEthercatDeviceData(uint _motorNumber, DeviceSymbol deviceSymbol, int dataLength)
        {
            var aa = SymbolEthercatDevice
                      .SubSymbols[(int)_motorNumber]
                      .SubSymbols[(int)deviceSymbol].InstancePath;
            var mtarray = new byte[dataLength];
            try
            {
                var handle = Client.CreateVariableHandle(aa);
                Client.Read(handle, mtarray.AsMemory());
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, $"Motor[{_motorNumber}] Variable : {deviceSymbol} " + ex.Message);
            }
            _log.Log(LogLevel.Trace, $"Readed {deviceSymbol} : {JsonSerializer.Serialize(mtarray)}");
            return mtarray;
        }

        public void WriteEthercatDeviceData()
        {
            try
            {
                for (int i = 0; i < PLCMotors.Length; i++)
                {
                    PLCMotors[i].Pack().CopyTo(buffer, structSize * i);
                }
                _log.Log(LogLevel.Trace, $"Writing SymbolEthercatDevice : {JsonSerializer.Serialize(buffer)}");
                Client.Write(SymbolEthercatDevice.IndexGroup,
                    SymbolEthercatDevice.IndexOffset, buffer.AsMemory());
            }
            catch(Exception ee) { _log.Log(LogLevel.Trace, $"Writing SymbolEthercatDevice WriteEthercatDeviceData1111:"+ee.Message); }
        }

        public void WriteEthercatDeviceData(uint _motorNumber)
        {
            try
            {
                var mtarray = new byte[structSize];
                PLCMotors[_motorNumber].Pack().CopyTo(mtarray, 0);
                _log.Log(LogLevel.Trace, $"Writing SymbolEthercatDevice[{_motorNumber}] : {JsonSerializer.Serialize(mtarray)}");
                Client.Write(SymbolEthercatDevice.IndexGroup,
                    SymbolEthercatDevice.IndexOffset + (uint)_motorNumber * (uint)structSize, mtarray.AsMemory());
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, "WriteEthercatDeviceData>>>>>>22222" + ex.Message);
            }
        }
        public void WriteEthercatDeviceData(DeviceSymbol deviceSymbol)
        {
            try
            {
                switch (deviceSymbol)
                {
                    case DeviceSymbol.mDirection:
                        for (uint i = 0; i < DeviceCount; i++)
                            WriteEthercatDeviceData(i, deviceSymbol, new byte[] { PLCMotors[i].mDirection.CastToArray()[0] });
                        break;
                    case DeviceSymbol.mRPM:
                        for (uint i = 0; i < DeviceCount; i++)
                            WriteEthercatDeviceData(i, deviceSymbol, PLCMotors[i].mRPM.CastToArray());
                        break;
                    case DeviceSymbol.mDisplacement:
                        for (uint i = 0; i < DeviceCount; i++)
                            WriteEthercatDeviceData(i, deviceSymbol, PLCMotors[i].mDisplacement.CastToArray());
                        break;
                    case DeviceSymbol.mTime:
                        for (uint i = 0; i < DeviceCount; i++)
                            WriteEthercatDeviceData(i, deviceSymbol, PLCMotors[i].mTime.CastToArray());
                        break;
                    case DeviceSymbol.mMode:
                        for (uint i = 0; i < DeviceCount; i++)
                            WriteEthercatDeviceData(i, deviceSymbol, PLCMotors[i].mMode.CastToArray());
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, "WriteEthercatDeviceData>>>>>>3333" + ex.Message);
            }
        }

        public void WriteEthercatDeviceData(uint _motorNumber, DeviceSymbol deviceSymbol, byte[] data)
        {
            var aa = SymbolEthercatDevice
                      .SubSymbols[(int)_motorNumber]
                      .SubSymbols[(int)deviceSymbol].InstancePath;
            try
            {
                var mtarray = new byte[data.Length];
                Array.Copy(data, mtarray, mtarray.Length);
                var handle = Client.CreateVariableHandle(aa);
                _log.Log(LogLevel.Trace, $"Writing {deviceSymbol} : {JsonSerializer.Serialize(data)}");
                Client.Write(handle, mtarray.AsMemory());
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, $"WriteEthercatDeviceData>>>>>>>Motor[{_motorNumber}] Variable : {deviceSymbol} " + ex.Message);
            }
        }

        public void StartpPLC()
        {
            _log.Log(LogLevel.Info, $"Resuming PLC ....");
            Client.WriteControl(new StateInfo(AdsState.Run, ClientSystemService.ReadState().DeviceState));
        }

        public void StoppPLC()
        {
            ClientSystemService.WriteControl(new StateInfo(AdsState.Stop, ClientSystemService.ReadState().DeviceState));
        }

        public void ConfigPLC()
        {
            ClientSystemService.WriteControl(new StateInfo(AdsState.Config, ClientSystemService.ReadState().DeviceState));
        }

        public void ReConfigPLC()
        {
            ClientSystemService.WriteControl(new StateInfo(AdsState.Reconfig, ClientSystemService.ReadState().DeviceState));
        }

        public StateInfo GetTwincatState()
        {
            StateInfo info = new StateInfo();
            if (Client.IsConnected)
            {
                AdsErrorCode errorCode = Client.TryReadState(out info);
            }
            return info;
        }

        private bool GetPLCVariables()
        {
            _log.Log(LogLevel.Info, $"Fetching PLC Variables");
            MySymbols = new ObservableCollection<Symbol>();
            _dataTypes = Session.SymbolServer.DataTypes;
            var symbols = Session.SymbolServer.Symbols;
            if (symbols.Count != 0)
            {
                ISymbolLoader loader = SymbolLoaderFactory.Create(Client, SymbolLoaderSettings.Default);

                foreach (var item in from item in symbols
                                     where item.InstanceName == "MAIN"
                                     select item)
                {
                    foreach (var itemsymbols in item.SubSymbols)
                    {
                        MySymbols.Add((Symbol)loader.Symbols[itemsymbols.InstancePath]);
                        _log.Log(LogLevel.Info, $"Symbol : {itemsymbols.InstancePath}");
                    }
                }
                return true;
            }
            _log.Log(LogLevel.Error, "No Symbol Found...");
            return false;
        }
        public bool LoadSetting()
        {
            var log = LogManager.GetLogger("AppSettings");

            // Create an XmlSerializer instance for the Person class
            XmlSerializer serializer = new(typeof(AppSettings));
            try
            {
                // Read the complete contents of the text file
                string appSettings;
                if (!File.Exists(AppSettings.SettingRoot + "AppSettings.xml"))
                {
                    log.Log(LogLevel.Warn, "AppSettings.xml doesn't exist");
                    log.Log(LogLevel.Warn, "Loading Default AppSettings.xml");
                    if (File.Exists(AppSettings.SettingRoot + "AppSettings_Default.xml"))
                    {
                        log.Log(LogLevel.Warn, "Loading the Default AppSettings.xml");
                        // Create a StringWriter to hold the XML
                        StringWriter stringWriter = new();
                        // Serialize the object to XML
                        serializer.Serialize(stringWriter, new AppSettings());

                        // Write the content to the text file
                        if (!Directory.Exists(AppSettings.SettingRoot))
                            Directory.CreateDirectory(AppSettings.SettingRoot);
                        File.WriteAllText(AppSettings.SettingRoot + "AppSettings.xml", stringWriter.ToString());
                    }
                }
                appSettings = File.ReadAllText(AppSettings.SettingRoot + "AppSettings.xml");
                using StringReader reader = new(appSettings);
                // Deserialize the XML back into a Person object
                AppSettings = serializer.Deserialize(reader) as AppSettings;
                return true;
            }
            catch (Exception ex)
            {
                var tmsg = $"AppSettings_Default.xml : {ex.Message}";
                log.Log(LogLevel.Error, tmsg);
                MessageBox.Show(tmsg);
                return false;
            }
        }
        public bool WriteSetting()
        {
            var log = LogManager.GetLogger("AppSettings");

            // Create an XmlSerializer instance for the Person class
            XmlSerializer serializer = new(typeof(AppSettings));
            try
            {
                // Create a StringWriter to hold the XML
                StringWriter stringWriter = new();
                // Serialize the object to XML
                serializer.Serialize(stringWriter, AppSettings);

                // Write the content to the text file
                if (!Directory.Exists(AppSettings.SettingRoot))
                    Directory.CreateDirectory(AppSettings.SettingRoot);
                File.WriteAllText(AppSettings.SettingRoot + "AppSettings.xml", stringWriter.ToString());

                return true;
            }
            catch (Exception ex)
            {
                var tmsg = $"AppSettings.xml : {ex.Message}";
                log.Log(LogLevel.Error, tmsg);
                MessageBox.Show(tmsg);
                return false;
            }
        }
    }
}
