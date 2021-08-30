using Sys.Async;
using Sys.DataBus.Common;
using Sys.Services.Components;
using Sys.Services.Drv.TLM3.Beauty;
using Sys.Services.Drv.TLM3.Culture;
using Sys.Services.Drv.TLM3.Def;
using Sys.Services.Drv.TLM3.Utils;
using Sys.StorageModel;
using Sys.Types.Components;
using Sys.Types.HistoryWriter;
using Sys.Types.Om;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Services.Drv.TLM3.Transport
{
    /// <summary>Представляет реализацию всех запросов к устройству</summary>

    public class TLM3Request
    {
        public const int TimeOutRequestMSecDeafult = 500;
        public const int WaitAnswerMSecDeafult = 10000;
        public const int CountRepeatDeafult = 1;

        /// <summary>Глубина хранения получасов</summary>
        public const int Depth30MinDefault = 14 * 48;
        /// <summary>Глубина хранения суточных</summary>
        public const int DepthDay = 32;
        /// <summary>Глубина месяцев</summary>
        public const int DepthMonth = 13;
        /// <summary>Глубина лет</summary>
        public const int DepthYear = 7;
        /// <summary>Глубина событий количество</summary>
        public const int DepthEvents = 32;


        private int timeOutRequestMSec;
        public int WaitAnswerMSec;
        public int nRepeatGlobal = CountRepeatDeafult;
        private ICancelSync cancel;

        public TLM3Request(ICancelSync cancel, DriverSetting drvSetting, int timeOutRequestMSec)
        {
            this.WaitAnswerMSec = drvSetting.WaitTimeout;
            this.nRepeatGlobal = drvSetting.RepeatCount;
            this.timeOutRequestMSec = timeOutRequestMSec;
            this.cancel = cancel;
        }

        public OperationData<bool> TryWriteDateTime(IIODriverClient channel, byte address, DeviceCompOn diff)
        {
            OperationResult result = OperationResult.Bad;
            var opData = new OperationData<bool>(false, result);
            DateTimeZone dateTime = diff.GetServerToDeviceTime(TimeZoneMap.Local);
            DateTimeZone datel = DateTimeLocal.Now;

            byte[] send = CreateRequest(address, Codes.CODE_WRITE_DATETIME, (ulong)Converter.DateLocalToUnix(DateTimeLocal.Now));

            result = WaitRequest(channel);
            if (!result.IsGood) return opData;

            result = WriteReadCheck(channel, nRepeatGlobal, send, out byte[] answer);
            //if (answer.Length > 0) { }
            return new OperationData<bool>(true, result);
        }

        public OperationData<ReadDateTime> TryReadDateTime(IIODriverClient channel, byte address)
        {
            OperationResult result = OperationResult.Bad;
            DateTimeUtc responseDate = DateTimeUtc.MinValue;
            int timeTwoSidePathMsec = 0;
            var opData = new OperationData<ReadDateTime>(new ReadDateTime(false, responseDate, timeTwoSidePathMsec), result);

            byte[] send = CreateRequest(address, Codes.CODE_READ_DATETIME, 0);

            result = WaitRequest(channel);
            if (!result.IsGood) return opData;

            var span = SpanSnapshot.Now;
            result = WriteReadCheck(channel, nRepeatGlobal, send, out byte[] answer);
            timeTwoSidePathMsec = (int)span.DiffNowMsec();
            if (!result.IsGood) return OperationData<ReadDateTime>.Bad(result);

            try 
            {
                responseDate = Converter.UnixToDateLocal(BitConverter.ToInt32(answer, 0));
            }
            catch (Exception e)
            {
                result = OperationResult.From(e);
            }
            return new OperationData<ReadDateTime>(new ReadDateTime(false, responseDate, timeTwoSidePathMsec), result);
        }

        public OperationData<PhysicalInfo> TryReadPhysicalInfo(IIODriverClient channel, byte address)
        {
            OperationResult result = OperationResult.Bad;
            var pInfo = new PhysicalInfo();
            int sn = 0;
            var opData = new OperationData<PhysicalInfo>(pInfo, result);
            if (DataBusSetting.StubData) return opData;

            result = TryReadSerialNumber(channel, address, out sn);
            if (!result.IsGood) return OperationData<PhysicalInfo>.Bad(result);
            pInfo.SerialNumber = sn.ToString();

            string sv = String.Empty;
            result = TryReadVersion(channel, address, out sv);
            if (!result.IsGood) return OperationData<PhysicalInfo>.Bad(result);
            pInfo.SoftVersion = sv;

            return new OperationData<PhysicalInfo>(pInfo, result);
        }

        public OperationResult TryReadType(IIODriverClient channel, byte address, out String type)
        {
            OperationResult result = OperationResult.Bad;
            type = String.Empty;
            byte[] send = CreateRequest(address, Codes.CODE_READ_TYPE_DEVICE, 0);

            result = WaitRequest(channel);
            if (!result.IsGood) return result;

            result = WriteReadCheck(channel, nRepeatGlobal, send, out byte[] answer);
            type = System.Text.Encoding.Default.GetString(answer);
            return result;
        }

        public OperationResult TryReadSerialNumber(IIODriverClient channel, byte address, out int number)
        {
            OperationResult result = OperationResult.Bad;
            number = 0;
            byte[] send = CreateRequest(address, Codes.CODE_READ_SERIAL_NUMBER, 0);

            result = WaitRequest(channel);
            if (!result.IsGood) return result;

            result = WriteReadCheck(channel, nRepeatGlobal, send, out byte[] answer);
            number = BitConverter.ToInt32(answer, 0);
            return result;
        }

        public OperationResult TryReadVersion(IIODriverClient channel, byte address, out String version)
        {
            OperationResult result = OperationResult.Bad;
            version = String.Empty;
            byte[] send = CreateRequest(address, Codes.CODE_READ_SOFT_VERSION, 0);

            result = WaitRequest(channel);
            if (!result.IsGood) return result;

            result = WriteReadCheck(channel, nRepeatGlobal, send, out byte[] dataByte);
            version = $"{dataByte[0]}.{dataByte[1]}.{dataByte[2]}.{dataByte[3]}";
            return result;
        }

        public OperationResult TryReadDateIssue(IIODriverClient channel, byte address, out DateTime date)
        {
            OperationResult result = OperationResult.Bad;
            date = DateTime.MinValue;
            byte[] send = CreateRequest(address, Codes.CODE_READ_DATE_CREATE, 0);

            result = WaitRequest(channel);
            if (!result.IsGood) return result;

            result = WriteReadCheck(channel, nRepeatGlobal, send, out byte[] dataByte);
            date = new DateTime(dataByte[3] + 2000, dataByte[2], dataByte[1]);
            return result;
        }

        public OperationResult TryReadNetNumber(IIODriverClient channel, byte address, out int number)
        {
            OperationResult result = OperationResult.Bad;
            number = 0;
            byte[] send = CreateRequest(address, Codes.CODE_READ_NETWORK_ADDR, 0);

            result = WaitRequest(channel);
            if (!result.IsGood) return result;

            result = WriteReadCheck(channel, nRepeatGlobal, send, out byte[] answer);
            number = answer[0];
            return result;
        }

        public OperationResult TryReadInstantaneousValues(IIODriverClient channel, byte address, out InstantaneousValues values)
        {
            OperationResult result = OperationResult.Bad;
            values = null;
            var send = CreateRequest(address, Codes.CODE_READ_POWER_CURR, 0);
            result = WriteReadCheck(channel, nRepeatGlobal, send, out byte[] answerP);
            if (!result.IsGood) return result;
            double[] arrP = new double[6];
            for (int i = 0; i < 6; i++) arrP[i] = BitConverter.ToInt32(answerP, i * 4) * 0.001;

            send = CreateRequest(address, Codes.CODE_READ_I_CURR, 0);
            result = WriteReadCheck(channel, nRepeatGlobal, send, out byte[] answerI);
            if (!result.IsGood) return result;
            double[] arrI = new double[3];
            for (int i = 0; i < 3; i++) arrI[i] = BitConverter.ToInt32(answerI, i * 4) * 0.001;

            send = CreateRequest(address, Codes.CODE_READ_U_CURR, 0);
            result = WriteReadCheck(channel, nRepeatGlobal, send, out byte[] answerU);
            if (!result.IsGood) return result;
            double[] arrU = new double[3];
            for (int i = 0; i < 3; i++) arrU[i] = BitConverter.ToInt32(answerU, i * 4) * 0.1;

            send = CreateRequest(address, Codes.CODE_READ_POWER_COEFF, 0);
            result = WriteReadCheck(channel, nRepeatGlobal, send, out byte[] answerFi);
            if (!result.IsGood) return result;
            double[] arrFi = new double[3];
            for (int i = 0; i < 3; i++) arrFi[i] = BitConverter.ToInt32(answerFi, i * 4) * 0.01;

            send = CreateRequest(address, Codes.CODE_READ_FREQ_CURR, 0);
            result = WriteReadCheck(channel, nRepeatGlobal, send, out byte[] answerF);
            if (!result.IsGood) return result;
            double[] arrF = new double[3];
            for (int i = 0; i < 3; i++) arrF[i] = BitConverter.ToInt32(answerF, i * 4) * 0.01;

            try
            {
                values = new InstantaneousValues
                {
                    InsActivePower = new InstantaneousActivePower
                    {
                        InsPowerPhase = new Phase
                        {
                            Phase_A = arrP[0],
                            Phase_B = arrP[2],
                            Phase_C = arrP[4]
                        }
                    },
                    InsReactivePower = new InstantaneousReactivePower
                    {
                        InsPowerPhase = new Phase
                        {
                            Phase_A = arrP[1],
                            Phase_B = arrP[3],
                            Phase_C = arrP[5]
                        }
                    },
                    Voltage = new Phase
                    {
                        Phase_A = arrU[0],
                        Phase_B = arrU[1],
                        Phase_C = arrU[2]
                    },
                    Amperage = new Phase
                    {
                        Phase_A = arrI[0],
                        Phase_B = arrI[1],
                        Phase_C = arrI[2]
                    },
                    PowerFactor = new Phase
                    {
                        Phase_A = arrFi[0],
                        Phase_B = arrFi[1],
                        Phase_C = arrFi[2]
                    },
                    Frequency = arrF[0]
                };
                result = OperationResult.Good;
            }
            catch (Exception e)
            {
                result = OperationResult.From(e);
            }

            return result;
        }

        internal void TryReadMin3Slices(QueryInfo info, DateTimeZone fromTime)
        {
            //TODO
        }

        internal void TryReadHalfHourSlices(QueryInfo info, DateTimeZone fromTime)
        {
            TagDef aplus = info.ElectroChannel.Energy.Aplus.Min30;
            TagDef aminus = info.ElectroChannel.Energy.Aminus.Min30;
            TagDef rplus = info.ElectroChannel.Energy.Rplus.Min30;
            TagDef rminus = info.ElectroChannel.Energy.Rminus.Min30;

            var holes = info.GetHoles(TypeInc.Min30, fromTime, Depth30MinDefault, info.RequestParam.DeepSync, new[] { aplus });
            foreach (DateTimeZone item in holes)
            {
                int read = 0;
                OperationResult oper = OperationResult.Bad;
                if (info.Session.BeginOperation())
                {
                    if (DataBusSetting.StubData) //STUBDATA->
                    {
                        read = 500;
                        oper = new OperationResult(Quality.Good);
                    }
                    else
                    {
                        int datEndUnix = Converter.DateLocalToUnix(item);
                        int datStartUnix = datEndUnix - 60 * 30;

                        byte[] data = new byte[8];
                        BitConverter.GetBytes(datStartUnix).CopyTo(data, 0);
                        BitConverter.GetBytes(datEndUnix).CopyTo(data, 4);

                        ulong ul = BitConverter.ToUInt64(data, 0);
                        var send = CreateRequest((byte)info.Cs.Address, Codes.CODE_READ_ENERGY_30MIN, ul);
                        oper = ReadArchiveData(info.DataBus, nRepeatGlobal, send, out Dictionary<DateTimeZone, int> arch);

                        if (!oper.IsGood) info.DataBus.DiscardInBuffer();
                        else if (arch.Count < 2)
                        {
                            oper = new OperationResult(Quality.Bad, SR.Result_Empty);
                        } 
                        else
                        {
                            try
                            {
                                //arch.TryGetValue()
                                int endPeriodData = arch[item];
                                int startPeriodData = arch[item.AddMinutes(-30)];
                                read = (endPeriodData - startPeriodData);
                            } catch (KeyNotFoundException)
                            {
                                oper = new OperationResult(Quality.Bad, SR.Result_Empty);
                            }
                        }
                    }

                    info.Session.EndOperation(oper);
                }
                else break;

                info.Storage.WriteTagsValue(
                  HW(oper.Quality, read * 0.001, item, aplus),
                  HW(Quality.Bad, 0, item, aminus),
                  HW(Quality.Bad, 0, item, rplus),
                  HW(Quality.Bad, 0, item, rminus));

                if (info.Log.Trace.IsOn(2)) { info.Log.Trace.Info(2, "A+ energy tag value {0} was written on time {1}, qual: {2}, mess: {3}", read * 0.001, item, oper.Quality, oper.ErrorMsg); }

            }
        }

        internal void TryReadDaySlices(QueryInfo info, DateTimeZone fromTime)
        {
            TagDef aplusC =  info.ElectroChannel.Counter.Aplus.Day1;
            TagDef aminusC = info.ElectroChannel.Counter.Aminus.Day1;
            TagDef rplusC = info.ElectroChannel.Counter.Rplus.Day1;
            TagDef rminusC = info.ElectroChannel.Counter.Rminus.Day1;

            TagDef aplusE = info.ElectroChannel.Energy.Aplus.Day1;
            TagDef aminusE = info.ElectroChannel.Energy.Aminus.Day1;
            TagDef rplusE = info.ElectroChannel.Energy.Rplus.Day1;
            TagDef rminusE = info.ElectroChannel.Energy.Rminus.Day1;

            Dictionary<DateTimeZone, int> arch = null;

            var holes = info.GetHoles(TypeInc.Day, fromTime, DepthDay, info.RequestParam.DeepSync, new[] { aplusC });
            foreach (DateTimeZone item in holes)
            {
                int readC = 0, readE = 0;
                OperationResult oper = OperationResult.Good;
                if (info.Session.BeginOperation())
                {
                    if (DataBusSetting.StubData) //STUBDATA->
                    {
                        readE = 500;
                        readC = 12500;
                        oper = new OperationResult(Quality.Good);
                    }
                    else
                    {
                        if (arch == null)
                        {
                            int endUnix = Converter.DateLocalToUnix(holes.First<DateTimeZone>());
                            int startUnix = Converter.DateLocalToUnix(holes.Last<DateTimeZone>().AddDays(-1));
                            byte[] dataUnix = new byte[8];
                            BitConverter.GetBytes(startUnix).CopyTo(dataUnix, 0);
                            BitConverter.GetBytes(endUnix).CopyTo(dataUnix, 4);

                            var send = CreateRequest((byte)info.Cs.Address, Codes.CODE_READ_ENERGY_DAY, BitConverter.ToUInt64(dataUnix, 0));
                            oper = ReadArchiveData(info.DataBus, nRepeatGlobal, send, out arch);
                        }
                        //oper = WriteReadCheck(info.DataBus, nRepeatGlobal, send, out byte[] answer);

                        if (!oper.IsGood) info.DataBus.DiscardInBuffer();

                        bool res = arch.TryGetValue(item, out readC);
                        if (res)
                        {
                            oper = new OperationResult(Quality.Good);
                            int readNext = 0;
                            bool res2 = arch.TryGetValue(item.AddDays(-1), out readNext);
                            if (res2)
                            {
                                readE = readC - readNext;
                            } else
                            {
                                oper.Quality = Quality.SubNormal;
                            }
                        }
                        else
                        {
                            oper = new OperationResult(Quality.Bad, SR.Result_Empty);
                        }

                    }

                    info.Session.EndOperation(oper);
                }
                else break;

                info.Storage.WriteTagsValue(
                  HW(oper.Quality.Equals(Quality.SubNormal) ? Quality.Good : oper.Quality, readC * 0.001, item, aplusC),
                  HW(Quality.Bad, 0, item, aminusC),
                  HW(Quality.Bad, 0, item, rplusC),
                  HW(Quality.Bad, 0, item, rminusC));

                if (info.Log.Trace.IsOn(2)) { info.Log.Trace.Info(2, "A+ counter tag value {0} was written on time {1}, qual: {2}, mess: {3}", readC * 0.001, item, oper.Quality, oper.ErrorMsg); }

                info.Storage.WriteTagsValue(
                  HW(oper.Quality.Equals(Quality.SubNormal) ? Quality.Bad : oper.Quality, readE * 0.001, item, aplusE),
                  HW(Quality.Bad, 0, item, aminusE),
                  HW(Quality.Bad, 0, item, rplusE),
                  HW(Quality.Bad, 0, item, rminusE));

                if (info.Log.Trace.IsOn(2)) { info.Log.Trace.Info(2, "A+ energy tag value {0} was written on time {1}, qual: {2}, mess: {3}", readE * 0.001, item, oper.Quality, oper.ErrorMsg); }

            }

        
        }

        internal void TryReadMonthSlices(QueryInfo info, DateTimeZone fromTime)
        {
            TagDef aplusC = info.ElectroChannel.Counter.Aplus.Month1;
            TagDef aminusC = info.ElectroChannel.Counter.Aminus.Month1;
            TagDef rplusC = info.ElectroChannel.Counter.Rplus.Month1;
            TagDef rminusC = info.ElectroChannel.Counter.Rminus.Month1;

            TagDef aplusE = info.ElectroChannel.Energy.Aplus.Month1;
            TagDef aminusE = info.ElectroChannel.Energy.Aminus.Month1;
            TagDef rplusE = info.ElectroChannel.Energy.Rplus.Month1;
            TagDef rminusE = info.ElectroChannel.Energy.Rminus.Month1;
            Dictionary<DateTimeZone, int> arch = null;

            var holes = info.GetHoles(TypeInc.Month, fromTime, DepthMonth, info.RequestParam.DeepSync, new[] { aplusE });
            foreach (DateTimeZone item in holes)
            {
                int readC = 0, readE = 0;
                OperationResult oper = OperationResult.Good;
                if (info.Session.BeginOperation())
                {
                    if (DataBusSetting.StubData) //STUBDATA->
                    {
                        readE = 500;
                        readC = 12500;
                        oper = new OperationResult(Quality.Good);
                    }
                    else
                    {
                        if (arch == null)
                        {
                            int endUnix = Converter.DateLocalToUnix(holes.First<DateTimeZone>());
                            int startUnix = Converter.DateLocalToUnix(holes.Last<DateTimeZone>());
                            byte[] dataUnix = new byte[8];
                            BitConverter.GetBytes(startUnix).CopyTo(dataUnix, 0);
                            BitConverter.GetBytes(endUnix).CopyTo(dataUnix, 4);

                            var send = CreateRequest((byte)info.Cs.Address, Codes.CODE_READ_ENERGY_MONTH, BitConverter.ToUInt64(dataUnix, 0));
                            oper = ReadArchiveData(info.DataBus, nRepeatGlobal, send, out arch);
                        }

                        if (!oper.IsGood) info.DataBus.DiscardInBuffer();
                        else
                        {
                            bool res = arch.TryGetValue(item, out readC);
                            if (res)
                            {
                                int readNext = 0;
                                bool res2 = arch.TryGetValue(item.AddMonths(-1), out readNext);
                                if (res2)
                                {
                                    readE = readC - readNext;
                                }
                                else
                                {
                                    oper.Quality = Quality.SubNormal;
                                }
                            }
                            else
                            {
                                oper = new OperationResult(Quality.Bad, SR.Result_Empty);
                            }
                        }

                    }

                    info.Session.EndOperation(oper);
                }
                else break;

                info.Storage.WriteTagsValue(
                  HW(oper.Quality.Equals(Quality.SubNormal) ? Quality.Good : oper.Quality, readC * 0.001, item, aplusC),
                  HW(Quality.Bad, 0, item, aminusC),
                  HW(Quality.Bad, 0, item, rplusC),
                  HW(Quality.Bad, 0, item, rminusC));

                if (info.Log.Trace.IsOn(2)) { info.Log.Trace.Info(2, "A+ counter tag value {0} was written on time {1}, qual: {2}, mess: {3}", readC * 0.001, item, oper.Quality, oper.ErrorMsg); }

                info.Storage.WriteTagsValue(
                  HW(oper.Quality.Equals(Quality.SubNormal) ? Quality.Bad : oper.Quality, readE * 0.001, item, aplusE),
                  HW(Quality.Bad, 0, item, aminusE),
                  HW(Quality.Bad, 0, item, rplusE),
                  HW(Quality.Bad, 0, item, rminusE));

                if (info.Log.Trace.IsOn(2)) { info.Log.Trace.Info(2, "A+ energy tag value {0} was written on time {1}, qual: {2}, mess: {3}", readE * 0.001, item, oper.Quality, oper.ErrorMsg); }
            }
        }

        internal void TryReadYearSlices(QueryInfo info, DateTimeZone fromTime)
        {
            TagDef aplusE = info.ElectroChannel.Energy.Aplus.Year1;
            TagDef aminusE = info.ElectroChannel.Energy.Aminus.Year1;
            TagDef rplusE = info.ElectroChannel.Energy.Rplus.Year1;
            TagDef rminusE = info.ElectroChannel.Energy.Rminus.Year1;

            Dictionary<DateTimeZone, int> arch = null;

            var holes = info.GetHoles(TypeInc.Year, fromTime, DepthYear, info.RequestParam.DeepSync, new[] { aplusE });
            foreach (DateTimeZone item in holes)
            {
                int read = 0;
                OperationResult oper = OperationResult.Good;
                if (info.Session.BeginOperation())
                {
                    if (DataBusSetting.StubData) //STUBDATA->
                    {
                        read = 500;
                        oper = new OperationResult(Quality.Good);
                    }
                    else
                    {
                        if (arch == null)
                        {
                            var send = CreateRequest((byte)info.Cs.Address, Codes.CODE_READ_ENERGY_YEAR, 0);
                            oper = ReadArchiveData(info.DataBus, nRepeatGlobal, send, out arch);
                        }

                        if (!oper.IsGood) info.DataBus.DiscardInBuffer();
                        else
                        {
                            bool res = arch.TryGetValue(item, out read);
                            if (res)
                            {
                                int readNext = 0;
                                bool res2 = arch.TryGetValue(item.AddMonths(-1), out readNext);
                                if (res2)
                                {
                                    read -= readNext;
                                }
                            }
                            else
                            {
                                oper = new OperationResult(Quality.Bad, SR.Result_Empty);
                            }
                        }

                    }

                    info.Session.EndOperation(oper);
                }
                else break;

                info.Storage.WriteTagsValue(
                  HW(oper.Quality, read * 0.001, item, aplusE),
                  HW(Quality.Bad, 0, item, aminusE),
                  HW(Quality.Bad, 0, item, rplusE),
                  HW(Quality.Bad, 0, item, rminusE));

                if (info.Log.Trace.IsOn(2)) { info.Log.Trace.Info(2, "A+ energy tag value {0} was written on time {1}", read * 0.001, item); }
            }
        }

        public OperationResult WaitRequest(IIODriverClient channel)
        {
            return channel.TryWaitRequest(timeOutRequestMSec, cancel);
        }

        private HistoryData HW(Quality quality, double @value, DateTimeUtc timeWrite, TagDef tagDef)
        {
            return new HistoryData(
              tagDef,
              new TagData(quality, DateTimeUtc.Now, value),
              timeWrite,
              HistoryWriterKind.InsertUpdate);
        }

        private HistoryData HW(Quality quality, DataDriverEvent @value, DateTimeUtc timeWrite, TagDef tagDef)
        {
            return new HistoryData(
              tagDef,
              new TagData(quality, DateTimeUtc.Now, TagValue.FromObject(DataType.Structured, value)),
              timeWrite,
              HistoryWriterKind.Insert);
        }

        //multivalue - для событий, чтобы хранить на одном ключе несколько интов
        private OperationResult ReadArchiveData(IIODriverClient channel, int nRepet, byte[] sendBuf, out Dictionary<DateTimeZone, int> arch, bool multivalue = false)
        {
            Dictionary<DateTimeZone, int> outArch = null;
            OperationResult result = DriverTransport.TryRepeater(nRepet, WaitAnswerMSec, true, channel, cancel, TimeRange.None, () =>
            {
                var res = ReadArchiveDataPrivate(channel, sendBuf, out outArch, multivalue);
                return new OperationData<bool>(true, res);
            });
            arch = outArch;
            return result;
        }

        //multivalue - для событий, чтобы хранить на одном ключе несколько интов
        private OperationResult ReadArchiveDataPrivate(IIODriverClient channel, byte[] sendBuf, out Dictionary<DateTimeZone, int> arch, bool multivalue = false)
        {
            arch = new Dictionary<DateTimeZone, int>();
            OperationResult result = WaitRequest(channel);
            if (!result.IsGood) return result;

            try
            {
                channel.DiscardInBuffer();
                channel.DiscardOutBuffer();
                channel.Write(sendBuf, 0, sendBuf.Length);

                List<byte> read = new List<byte>();

                byte[] head = new byte[3];
                byte[] dataByte = new byte[248];

                do
                {
                    Array.Clear(head, 0, head.Length);
                    Array.Clear(dataByte, 0, dataByte.Length);
                    channel.Read(head, 0, 3);
                    byte len = head[2];
                    if (len != 0)
                    {
                        channel.Read(dataByte, 0, len);
                    }
                    byte crcRead = (byte)channel.ReadByte();

                    /*channel.DiscardInBuffer();
                    channel.DiscardOutBuffer();*/

                    byte[] crcByte = new byte[len + 3];
                    Array.Copy(head, crcByte, 3);
                    Array.Copy(dataByte, 0, crcByte, 3, len);
                    byte crc = CRC.crc8tab(crcByte, len + 3);
                    if (crc != crcRead)
                    {
                        return new OperationResult(Quality.BadCRC, SR.CRC);
                    }

                    for (int i = 0; i < len; i += 8)
                    {
                        int dateTimeUnix = BitConverter.ToInt32(dataByte, i);
                        DateTimeZone dateTimeZ = Converter.UnixToDateLocal(dateTimeUnix);
                        int value = BitConverter.ToInt32(dataByte, i + 4);
                        if (multivalue)
                        {
                            int archValue = 0;
                            bool getValue = arch.TryGetValue(dateTimeZ, out archValue);
                            if (getValue)
                            {
                                value = archValue * 100 + value;
                                arch.Remove(dateTimeZ);
                            }
                        }
                        arch.Add(dateTimeZ, value);
                    }

                } while (head[1] > 0);
            }
            catch (Exception e)
            {
                result.Quality = e.ToQuality();
                result.ErrorMsg = e.GetFullMessage();
            }
            return result;
        }

        //Записать и считать пакет с повтором 
        private OperationResult WriteReadCheck(IIODriverClient channel, int nRepet, byte[] sendBuf, out byte[] readBuf)
        {
            byte[] outBuf = null;
            OperationResult result = DriverTransport.TryRepeater(nRepet, WaitAnswerMSec, true, channel, cancel, TimeRange.None, () =>
            {
                var res = WriteReadCheckPrivate(channel, sendBuf, out outBuf);
                return new OperationData<bool>(true, res);
            });
            readBuf = outBuf;
            return result;
        }

        /// <summary>Записать и считать пакет</summary>
        /// <param name="channel"></param>
        /// <param name="sendBuf"></param>
        /// <param name="readBuf"></param>
        /// <returns></returns>
        private OperationResult WriteReadCheckPrivate(IIODriverClient channel, byte[] sendBuf, out byte[] readPacket)
        {
            readPacket = null;
            OperationResult result = WaitRequest(channel);
            if (!result.IsGood) return result;

            try
            {
                channel.DiscardInBuffer();
                channel.DiscardOutBuffer();
                channel.Write(sendBuf, 0, sendBuf.Length);

                List<byte> read = new List<byte>();

                byte[] head = new byte[3];
                channel.Read(head, 0, 3);
                byte len = head[2];
                byte[] dataByte = new byte[len];
                if (len != 0)
                {
                    channel.Read(dataByte, 0, len);
                }
                byte crcRead = (byte)channel.ReadByte();

                channel.DiscardInBuffer();
                channel.DiscardOutBuffer();

                byte[] crcByte = new byte[len + 3];
                Array.Copy(head, crcByte, 3);
                Array.Copy(dataByte, 0, crcByte, 3, len);
                byte crc = CRC.crc8tab(crcByte, len + 3);
                if (crc != crcRead)
                {
                    return new OperationResult(Quality.BadCRC, SR.CRC);
                }


                readPacket = dataByte;
            }
            catch (Exception e)
            {
                result.Quality = e.ToQuality();
                result.ErrorMsg = e.GetFullMessage();
            }
            finally
            {
                //sw.Restart();
            }
            return result;
        }

        /// <summary>Создать запрос на счетчик </summary>
        /// <param name="addresssDevice">Адрес устройства</param>
        /// <param name="codeCommand">Код команды</param>
        /// <param name="data">Массив параметров для комманды как ulong</param>
        private byte[] CreateRequest(byte addresssDevice, byte codeCommand, UInt64 data)
        {
            var buff = new List<byte>();
            buff.Add(addresssDevice);
            buff.Add(codeCommand);
            byte[] head = { addresssDevice, codeCommand };
            byte[] dataByte = BitConverter.GetBytes(data);
            for (byte i = 0; i < dataByte.Length; i++)
            {
                buff.Add(dataByte[i]);
            }

            byte[] crcByte = new byte[dataByte.Length + 2];
            Array.Copy(head, crcByte, 2);
            Array.Copy(dataByte, 0, crcByte, 2, dataByte.Length);
            byte crc = CRC.crc8tab(crcByte, dataByte.Length + 2);
            buff.Add(crc);
            return buff.ToArray();
        }

        internal void ReadEvents(QueryInfo info)
        {
            if (DataBusSetting.StubData) return;

            var lastVal = info.Storage.ReadLastValue(info.Events);

            DateTimeZone from = DateTimeZone.UtcNow.AddYears(-5);
            if ((lastVal != null) && (lastVal[0].Error == DataError.Ok))
                from = lastVal[0].Value.Data.Time.ToDateTimeZone(info.NextPoint.Zone);

            var listEvent = new List<HistoryEvent>();
            OperationResult res;
            //Значит записываем сообщения
            bool writeListEvent = true;

            info.Log.Trace.Write(2, (l) => l.Info("Read events from {0}", from.ToString()));

            //Byte depth = 1;
            if (info.Session.BeginOperation())
            {
                //bool _break = false;
                List<Event> evList;
                res = info.Request.TryGetAnyEvents(
                    info.DataBus,
                    (byte)info.Cs.Address,
                    out evList);

                if ((!res.IsGood) || (evList.Count == 0))
                {
                    info.Log.Trace.Warn(1, "Error parse event. ");
                    info.Session.EndOperation(res);
                    return;
                }

                foreach (Event ev in evList)
                {
                    if (from < ev.DateEvent)
                    {
                        info.Log.Trace.Info(2, "Read event from [{0}]. {1}", ev.DateEvent, ev.EventSource);
                        listEvent.Add(new HistoryEvent(info.Events, ev.TLMEvent, ev.DateEvent, Quality.Good));
                    }
                }
                //Заканчиваем по коду 5, если по этому времени мы уже считали, достигли макс глубины
                //Думаю надо изменить from >= ev.DataEvent т.к. похоже есть какойто баг возможно в приборе
                /*if ((res.IsGood) && (from >= ev.DateEvent))
                {
                    if (listEvent.Count != 0) writeListEvent = true;
                    //_break = true;
                    res = OperationResult.Good;
                }
                info.Session.EndOperation(res);

                //if (!_break) 
                info.Log.Trace.Info(2, "Read event from [{0}]. {1}", depth, ((res.IsGood) && (ev != null)) ? ev.ToString() : "");

                //if ((_break) || (!res.IsGood)) break;
                listEvent.Add(new HistoryEvent(info.Events, ev.TLMEvent, ev.DateEvent, Quality.Good));
                if ((depth == DepthEvents) && (listEvent.Count != 0)) writeListEvent = true;*/
                info.Session.EndOperation(res);
            }
            if (writeListEvent)
            {
                info.Storage.WriteEvents(listEvent.ToArray());
            }
        }

        public OperationResult TryGetAnyEvents(IIODriverClient channel, byte address, out List<Event> response)
        {
            response = new List<Event>();
            OperationResult result = OperationResult.Bad;

            var send = CreateRequest(address, Codes.CODE_READ_EVENT_ARCH, 0);
            Dictionary<DateTimeZone, int> archEvts;
            result = ReadArchiveData(channel, nRepeatGlobal, send, out archEvts, true);
            try
            {
                if (result.IsGood && archEvts.Count > 0)
                {
                    var keys = archEvts.Keys;
                    foreach (DateTimeZone dtz in keys)
                    {
                        int evtValue;
                        archEvts.TryGetValue(dtz, out evtValue);
                        while (evtValue > 99)
                        {
                            response.Add(new Event(dtz, evtValue % 100));
                            evtValue /= 100;
                        }
                        response.Add(new Event(dtz, evtValue));
                    }

                }
            }
            catch (Exception e)
            {
                result = OperationResult.From(e);
            }
            return result;
        }

    }
}
