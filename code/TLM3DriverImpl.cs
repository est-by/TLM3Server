using Sys.Configuration;
using Sys.DataBus.Common;
using Sys.Services.Components;
using Sys.Services.Drv.TLM3.Beauty;
using Sys.Services.Drv.TLM3.Culture;
using Sys.Services.Drv.TLM3.Def;
using Sys.Services.Drv.TLM3.Transport;
using Sys.Types.Components;
using Sys.Types.Components.DataDriverClient;
using Sys.Types.NodeEditor;
using Sys.Types.Om;
using System;

namespace Sys.Services.Drv.TLM3
{
    public class TLM3Driver : Sys.Types.Components.DriverElectroClient
    {
        /// <summary>Идентификатор типа реализации компонента</summary>
        public const string TypeGuidImpl = "est.by:Bus.TLM3DrvClientImpl";

        #region (FineTune)
        static FineTune FineTune;
        static TLM3Driver()
        {
            FineTune = FineTune.TryLoad("tlm3electro");
        }
        public Int32 ReadTimeOutRequestMSec()
        {
            return FineTune.ReadValue<int>("TimeOutRequestMSec", this, (v) => Int32.Parse(v), TLM3Request.TimeOutRequestMSecDeafult);
        }
        #endregion

        #region (DesignChange)
        internal static void DesignChange(Sys.Types.Om.INodeEditorContext context, object state)
        {
            if ((context.Action != NodeEditorAction.AfterAdd) && (context.Action != NodeEditorAction.AfterEdit)) return;

            if (context.Action == NodeEditorAction.AfterEdit)
                context.DeleteChildDynNodes(context.NodeId, DriverElectroClient.BN.ElectroChannel, true);

            context.AddDynNode(context.NodeId, new Node
            {
                BrowseName = DriverElectroClient.BN.ElectroChannel,
                LinkType = LinkType.Hard,
                DisplayName = context.NodeDisplayName,
                IdComponentType = Sys.Types.Components.ElectroChannel.TypeGuidImpl,
            }, null);
        }
        #endregion

        #region (IsReadIm)
        public override ModeDataDrv IsReadIm(SynchRequestDataDrv request)
        {
            ModeDataDrv result = ModeDataDrv.Manual;
            var ss = request.GetSharedSetting<TLM3SharedSetting>(() => new TLM3SharedSetting());
            if ((ss.EnblIm) && !ss.Im.IsEmpty()) result |= ModeDataDrv.Auto;
            return result;
        }
        #endregion
        #region (IsSynch)
        public override ModeDataDrv IsSynch(SynchRequestDataDrv request)
        {
            return ModeDataDrv.All;
        }
        #endregion
        #region (IsWriteIm)
        public override ModeDataDrv IsWriteIm(SynchRequestDataDrv request)
        {
            return ModeDataDrv.None;
        }
        #endregion
        #region (WriteIm)
        public override SynchResponse WriteIm(SynchRequestDataDrv request, WriteImParamsDataDrv param)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region (ReadIm)
        public override SynchResponse ReadIm(SynchRequestDataDrv request, ReadImParamsDataDrv param)
        {
            var ss = request.GetSharedSetting<TLM3SharedSetting>(() => new TLM3SharedSetting());
            var cs = request.GetContentSetting<TLM3ContentSetting>(() => new TLM3ContentSetting());
            var drvSetting = request.GetDriverSetting(() => new DriverSetting());
            TLM3Request tlm = new TLM3Request(this, drvSetting, ReadTimeOutRequestMSec());

            ImNextPoint nextPoint = new ImNextPoint(TimeZoneMap.Local, 0, imNextItem: new ImNextItem("im", ss.Im.ToSch()));
            using (var session = new PhysicalSessionIm<TLM3SynchState, ImNextPoint>(this, request, nextPoint))
            {
                session.Open();
                if (!session.LaunchPoint(nextPoint.GetItem("im"))) return session;
                if (session.BeginOperation())
                {
                    InstantaneousValues iv;
                    OperationResult resData = OperationResult.Good;
                    if (DataBusSetting.StubData) iv = new InstantaneousValues(true);
                    else
                    {
                        resData = tlm.TryReadInstantaneousValues(this.Channel, (byte)cs.Address, out iv);
                        if (resData.IsGood)
                        {
                            ElectroChannel electroChannel = this.ElectroChannel.ByIndex<ElectroChannel>(0);
                            iv.WriteTags(this.Storage, electroChannel, resData.Quality, DateTimeUtc.Now);
                        }
                    }
                    session.EndOperation(resData);
                    //Log.Trace.Write(1, (l) => l.Info(SR.READ_IM, resData.ToString()));
                }
                return session;
            }
        }
        #endregion

        #region (Synch)
        public override SynchResponse Synch(SynchRequestDataDrv requestData, SynchParamsDataDrv requestParam)
        {
            Log.Trace.Write(1, (l) => l.Info("Start"));
            //requestParam.HolesMode = QueryHolesMode.WithoutGeneration;
            var info = new QueryInfo(
                this,
                requestData,
                requestParam,
                requestData.GetSharedSetting(() => new TLM3SharedSetting()),
                requestData.GetContentSetting(() => new TLM3ContentSetting()),
                requestData.GetDriverSetting(() => new DriverSetting())
            );
            TLM3Request tlm = new TLM3Request(this, info.DriverSetting, ReadTimeOutRequestMSec());
            var NextPoint = new AccountNextPoint(
              TimeZoneMap.Local,
              timeOffset: TimeOffset.Level_1,
              useMin3: false,
              useMin30: info.Ss.EnblHr,
              useDay1: info.Ss.EnblDay,
              useMonth1: info.Ss.EnblMonth,
              useYear1: false,
              archSync: info.Ss.Arch.ToSch());

            //Любая работа с устройством должна начинаться с открытия сессии
            using (var session = new PhysicalSession<TLM3SynchState, AccountNextPoint>(this, requestData, NextPoint))
            {
                info.SetSession(session);
                //чтение времени
                session.OnReadDateTime = () => { return tlm.TryReadDateTime(this.Channel, (byte)info.Cs.Address); };
                //чтение серийного номера и т.д.
                session.OnReadPhysicalInfo = () => { return tlm.TryReadPhysicalInfo(this.Channel, (byte)info.Cs.Address); };
                if (info.Ss.EnblTimeCorr)
                {
                    //записть времени TODO
                    session.OnWriteDateTime = (diff) => { return tlm.TryWriteDateTime(this.Channel, (byte)info.Cs.Address, diff); };
                }
                //session.Open();
                session.AutoOpen = true;

                var eDef = info.ElectroChannel.Energy;
                var eDefCounter = info.ElectroChannel.Counter;

                if (info.Ss.EnblEvents)
                {
                    tlm.ReadEvents(info);
                }
                if (session.LaunchPoint(NextPoint.Min30))
                {
                    Log.Trace.Info(1, SR.Read30Min);
                    tlm.TryReadHalfHourSlices(info, TimeStep.Minute_30.Round(NextPoint.Zone.Now));
                    //if ((!info.Ss.Enbl3min) &&  (info.Ss.EnblEvents)) IO.ReadAllEvents(info);
                }
                if (session.LaunchPoint(NextPoint.Day1))
                {
                    Log.Trace.Info(1, SR.ReadDay);
                    tlm.TryReadDaySlices(info, TimeStep.Day_1.Round(NextPoint.Zone.Now));
                }
                if (session.LaunchPoint(NextPoint.Month1))
                {
                    Log.Trace.Info(1, SR.ReadMonth);
                    tlm.TryReadMonthSlices(info, TimeStep.Month.Round(NextPoint.Zone.Now));
                    tlm.TryReadYearSlices(info, TimeStep.Year_1.Round(NextPoint.Zone.Now));
                }

                if (session.IsOpen) { Log.Trace.Write(1, (l) => l.Info("Next session from {0} min", Math.Round((session.Result.Next - DateTimeUtc.Now).TotalMinutes), 1)); }
                return session;
            }
        }
        #endregion

        #region (OneTest)
        internal bool OneTest(string nameTest, TestRequestDataDrv request, TestResult res, Func<MsgTest> func, bool error = false)
        {
            bool result = true;
            TestDriverError testDriverError = null;
            MsgTest readResult = null;
            try
            {
                readResult = func();
                if ((!readResult.OperationResult.IsGood) && (!error))
                {
                    testDriverError = new TestDriverError(false, "{0}, {1}", nameTest, readResult.OperationResult.ErrorMsg);
                    result = false;
                }
            }
            catch (Exception e)
            {
                testDriverError = new TestDriverError(false, "{0}, {1}", nameTest, e.GetFullMessageDisplay());
                result = false;
            }
            if (testDriverError != null) res.Add(testDriverError);

            if (Log.Trace.IsOn(1))
            {
                var mgg = String.Format("{0}: {1}", nameTest, (result) ? readResult.Message : testDriverError.Message);
                if ((readResult.OperationResult.IsGood) || (error)) Log.Trace.Info(1, mgg);
                else Log.Trace.Error(1, mgg);
            }
            return result;
        }
        #endregion

        #region (Test)
        public override TestResult Test(TestRequestDataDrv request)
        {
            TestResult result = new TestResult();
            Log.Trace.Info(1, "Test request processing started");

            var drvSetting = request.GetDriverSetting(() => new DriverSetting());
            var cs = request.GetContentSetting<TLM3ContentSetting>(() => new TLM3ContentSetting());
            TLM3Request tlm = new TLM3Request(this, drvSetting, ReadTimeOutRequestMSec());

            if ((request.TestLevel == TestLevel.Ping) || (request.TestLevel == TestLevel.Search))
            {
                if (!DataBusSetting.StubData)
                {
                    var readRes = tlm.TryReadDateTime(this.Channel, (byte)cs.Address);
                    if (!readRes.IsGood) result.Add(new TestDriverError(false, "Error Connect. {0}", readRes.Result.ErrorMsg));
                }
            }
            else // (request.TestLevel == TestLevel.Full)
            {
                //Log.Trace.Info(1, "Test request processing datetime");
                DateTimeUtc devTimeUtc = DateTimeUtc.MinValue;
                if (!OneTest(SR.Test_DT, request, result, () =>
                {
                    var res = tlm.TryReadDateTime(this.Channel, (byte)cs.Address);
                    devTimeUtc = res.Value.DeviceTime;
                    return new MsgTest(res.Result, res.IsGood ? devTimeUtc.ToLocal().ToString() : string.Empty);
                })) return result;
                TimeSpan diffTime = DateTimeUtc.Now - devTimeUtc;

                int sn = 0;
                //Log.Trace.Info(1, "Test request processing serial");
                if (!OneTest(SR.Test_SN, request, result, () =>
                {
                    var res = tlm.TryReadSerialNumber(this.Channel, (byte)cs.Address, out sn);
                    return new MsgTest(res, sn.ToString());
                })) return result;

                string sv = string.Empty;
                //Log.Trace.Info(1, "Test request processing config");
                if (!OneTest(SR.Test_SV, request, result, () =>
                {
                    var res = tlm.TryReadVersion(this.Channel, (byte)cs.Address, out sv);
                    return new MsgTest(res, sv);
                })) return result;

                string type = string.Empty;
                //Log.Trace.Info(1, "Test request processing config");
                if (!OneTest(SR.Test_Type, request, result, () =>
                {
                    var res = tlm.TryReadType(this.Channel, (byte)cs.Address, out type);
                    return new MsgTest(res, type);
                })) return result;

                int netNumber = 0;
                //Log.Trace.Info(1, "Test request processing config");
                if (!OneTest(SR.Test_NetNum, request, result, () =>
                {
                    var res = tlm.TryReadNetNumber(this.Channel, (byte)cs.Address, out netNumber);
                    return new MsgTest(res, netNumber.ToString());
                })) return result;

                DateTime createDate = DateTime.MinValue;
                //Log.Trace.Info(1, "Test request processing config");
                if (!OneTest(SR.Test_DateIssue, request, result, () =>
                {
                    var res = tlm.TryReadDateIssue(this.Channel, (byte)cs.Address, out createDate);
                    return new MsgTest(res, res.IsGood ? createDate.ToShortDateString() : string.Empty);
                })) return result;

                /*if (!OneTest(SR.Test_Error_Req, request, result, () =>
                {
                  Energy eRead;
                  var res = emera.TryReadSlicesEnergy(this.Channel, DeviceCompOn.Default, SlicesQuery.GetEmulError(sr.Address), TypeInfo.Imp, out eRead);
                  return new MsgTest(res, res.IsGood ? "" : "Ok");
                }, true)) return result;*/

                //Log.Trace.Info(1, "Test request processing finished");
                result.Message = String.Format("Сер.Номер: {0}, Временной разрыв: {1} sec, Сетевой адресс: {2}, Тип: {3}, Версия ПО: {4}, Дата выпуска: {5}", sn, (int)diffTime.TotalSeconds, netNumber, type, sv, createDate.ToShortDateString());
            }
            return result;
        }
        #endregion
    }
}
