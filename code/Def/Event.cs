using Sys.Services.Drv.TLM3.Culture;
using Sys.Types.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Services.Drv.TLM3.Def
{

    /// <summary>Описания события</summary>
    public class Event
    {
        internal Event(Sys.DateTimeZone dateEvent, int code)
        {
            this.DateEvent = dateEvent;
            this.Code = code;
            this.TLMEvent = null;

            if (code == 30)
            {
                TLMEvent = new EvAdminDrv();
                EventSource = SR.Jrn_Change_Settings;
            }
            else if (code  == 31)
            {
                TLMEvent = new EvAdjustTime();
                EventSource = SR.Jrn_Change_Time;
            }
            else if (code > 19 && code < 24)
            {
                EventSource = SR.Jrn_Phase_Disappear;
                switch (code)
                {
                    case 20:
                        TLMEvent = new EvPhaseDrv(false);
                        break;
                    case 21:
                        TLMEvent = new EvPhaseDrv(false, true, true, false, true, true);
                        break;
                    case 22:
                        TLMEvent = new EvPhaseDrv(true, false, true, true, false, true);
                        break;
                    case 23:
                        TLMEvent = new EvPhaseDrv(true, true, false, true, true, false);
                        break;
                }
            }
            else if (code > 9 && code < 14)
            {
                EventSource = SR.Jrn_Phase_Appear;
                switch (code)
                {
                    case 10:
                        TLMEvent = new EvPhaseDrv(true);
                        break;
                    case 11:
                        TLMEvent = new EvPhaseDrv(true, false, false, true, false, false);
                        break;
                    case 12:
                        TLMEvent = new EvPhaseDrv(false, true, false, false, true, false);
                        break;
                    case 13:
                        TLMEvent = new EvPhaseDrv(false, false, true, false, false, true);
                        break;
                }
            }
            else
            {
                TLMEvent = new EvErrorDrv();
                EventSource = SR.Jrn_Unknown;
            }

        }

        public Event() { }

        /// <summary>Дата и время события</summary>
        public Sys.DateTimeZone DateEvent;

        /// <summary>Код события</summary>
        public int Code;

        /// <summary>Текстовое описания события</summary>
        public string EventSource;

        /// <summary>Событие в виде TagValue</summary>
        public DataDriverEvent TLMEvent;

        public override string ToString()
        {
            return String.Format("DateTime: {0}, Event: {1}", DateEvent, TLMEvent.ToString(System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName));
        }

        public string GetLogEvent(byte evt)
        {
            string log = string.Empty;
            switch (evt)
            {
                case 10:
                    log = "Появление по фазам А, B, С";
                    break;
                case 11:
                    log = "Появление по фазе А";
                    break;
                case 12:
                    log = "Появление по фазам B";
                    break;
                case 13:
                    log = "Появление по фазам C";
                    break;
                case 20:
                    log = "Пропание по фазам А, B, С";
                    break;
                case 21:
                    log = "Пропание по фазе А";
                    break;
                case 22:
                    log = "Пропание по фазам B";
                    break;
                case 23:
                    log = "Пропание по фазам C";
                    break;
                default:
                    log = "Нет описания события";
                    break;
                case 30:
                    log = "Изменение настроек";
                    break;
                case 31:
                    log = "Синхронизация времени";
                    break;
            }
            return log;
        }
    }

}
