using Sys.DataBus.Common;
using Sys.Diagnostics.Logger;
using Sys.Services.Components;
using Sys.Services.Drv.TLM3.Culture;
using Sys.Services.Drv.TLM3.Def;
using Sys.Services.Drv.TLM3.Transport;
using Sys.Types.Components;
using Sys.Types.Components.DataDriverClient;
using Sys.Types.Om;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Services.Drv.TLM3.Beauty
{
    internal class QueryInfo
    {
        public TagDef Events { get { return driver.Events; } }
        public StorageDataDriver Storage { get { return driver.Storage; } }
        public LogObject Log { get { return driver.Log; } }
        //public string DisplayName { get { return driver.DisplayName; } }
        //В чем брать информацию со счетчика
        public SynchRequestDataDrv RequestData { get; private set; }
        public SynchParamsDataDrv RequestParam { get; private set; }
        public string DeviceConfiguration { get; private set; }
        //public double DeviceKoef { get; private set; }
        public TLM3Request Request { get; private set; }
        public IIODriverClient DataBus { get; private set; }
        public TLM3SharedSetting Ss { get; private set; }
        public TLM3ContentSetting Cs { get; private set; }
        public AccountNextPoint NextPoint { get; private set; }
        public DriverSetting DriverSetting { get; private set; }
        public ElectroChannel ElectroChannel { get; private set; }
        public PhysicalSession<TLM3SynchState, AccountNextPoint> Session { get; private set; }
        /// <summary>
        /// текущее время в локале прибора
        /// </summary>
        public DateTimeZone NowTimeInZone { get { return NextPoint.Zone.Now; } }

        private TLM3Driver driver;

        public QueryInfo(TLM3Driver driver, SynchRequestDataDrv requestData, SynchParamsDataDrv requestParam, TLM3SharedSetting ss, TLM3ContentSetting cs, DriverSetting driverSetting)
        {
            RequestData = requestData;
            RequestParam = requestParam;
            this.driver = driver;
            Request = new TLM3Request(driver, driverSetting, driver.ReadTimeOutRequestMSec());
            DriverSetting = driverSetting;
            DataBus = driver.Channel;
            Ss = ss;
            Cs = cs;
            NextPoint = new AccountNextPoint(
              TimeZoneMap.Local,
              timeOffset: TimeOffset.Level_1,
              useMin3: true,
              useMin30: true,
              useDay1: true,
              useMonth1: true,
              useYear1: true,
              archSync: ss.Arch.ToSch());
            ElectroChannel = driver.ElectroChannel.ByBrowseName<ElectroChannel>(ElectroChannels.BN.ElectroChannel);
        }

        internal void SetDeviceConfiguration(string deviceConfiguration)
        {
            DeviceConfiguration = deviceConfiguration;
        }

        internal IEnumerable<DateTimeZone> GetHoles(TypeInc typeInc, DateTimeZone fromTime, int depth, DateTimeUtc deepSyncTime, TagDef[] tagsWatch)
        {
            Discrete disc;
            int discVal;
            GetDiscrette(typeInc, out disc, out discVal);

            DateTimeZone beginTime = RequestParam.GetFromSync(() => fromTime, true, disc, discVal);

            var holes = driver.Storage.ReadHoles(
              RequestParam.HolesMode,
              beginTime,
              depth - 1,
              deepSyncTime,
              tagsWatch);
            this.driver.Log.Trace.Info(2, SR.ReadHoles, fromTime, depth, deepSyncTime, beginTime, holes.Count());
            return holes;
        }

        internal void SetSession(PhysicalSession<TLM3SynchState, AccountNextPoint> session)
        {
            Session = session;
        }

        #region (GetDiscrette)
        private void GetDiscrette(TypeInc typeInc, out Discrete disc, out int discVal)
        {
            disc = Discrete.Min;
            discVal = 3;
            if (typeInc == TypeInc.Min30) discVal = 30;
            else if (typeInc == TypeInc.Day)
            {
                disc = Discrete.Day;
                discVal = 1;
            }
            else if (typeInc == TypeInc.Month)
            {
                disc = Discrete.Month;
                discVal = 1;
            }
            else if (typeInc == TypeInc.Year)
            {
                disc = Discrete.Year;
                discVal = 1;
            }
        }
        #endregion


    }
}
