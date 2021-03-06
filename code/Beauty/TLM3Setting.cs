//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Пространство имен Sys.Services.Drv
namespace Sys.Services.Drv {
    using System;
    
    
    /// <summary/>
    public partial class TLM3SharedSetting : Sys.Encodeable.IEncodeableObject {
        
        /// <summary>
        /// Поле для свойства Включить корректировку времени
        /// </summary>
        private bool _enbltimecorr = true;
        
        /// <summary>
        /// Поле для свойства Включить опрос событий
        /// </summary>
        private bool _enblevents = false;
        
        /// <summary>
        /// Поле для свойства Включить опрос мгновенных значений
        /// </summary>
        private bool _enblim = false;
        
        /// <summary>
        /// Поле для свойства Включить опрос часовых
        /// </summary>
        private bool _enblhr = false;
        
        /// <summary>
        /// Поле для свойства Включить опрос суточных
        /// </summary>
        private bool _enblday = false;
        
        /// <summary>
        /// Поле для свойства Включить опрос месячных
        /// </summary>
        private bool _enblmon = false;
        
        /// <summary>
        /// Поле для свойства Рассписание синхронизации архивных данных
        /// </summary>
        private Sys.Types.Components.ScheduleDbWrap _arch;
        
        /// <summary>
        /// Поле для свойства Рассписание чтения мгновенных значений
        /// </summary>
        private Sys.Types.Components.ScheduleDbWrap _im;
        
        /// <summary>
        /// Интернал конструктор
        /// </summary>
        internal TLM3SharedSetting(bool enblTimeCorr, bool enblEvents, bool enblIm, bool enblHr, bool enblDay, bool enblMon, Sys.Types.Components.ScheduleDbWrap arch, Sys.Types.Components.ScheduleDbWrap im) {
            this.EnblTimeCorr = enblTimeCorr;
            this.EnblEvents = enblEvents;
            this.EnblIm = enblIm;
            this.EnblHr = enblHr;
            this.EnblDay = enblDay;
            this.EnblMon = enblMon;
            this.Arch = arch;
            this.Im = im;
        }
        
        /// <summary>
        /// Публичный конструктор
        /// </summary>
        public TLM3SharedSetting() {
        }
        
        /// <summary>
        /// Свойство Включить корректировку времени
        /// </summary>
        public virtual bool EnblTimeCorr {
            get {
                return _enbltimecorr;
            }
            set {
                this._enbltimecorr = value;
            }
        }
        
        /// <summary>
        /// Свойство Включить опрос событий
        /// </summary>
        public virtual bool EnblEvents {
            get {
                return _enblevents;
            }
            set {
                this._enblevents = value;
            }
        }
        
        /// <summary>
        /// Свойство Включить опрос мгновенных значений
        /// </summary>
        public virtual bool EnblIm {
            get {
                return _enblim;
            }
            set {
                this._enblim = value;
            }
        }
        
        /// <summary>
        /// Свойство Включить опрос часовых
        /// </summary>
        public virtual bool EnblHr {
            get {
                return _enblhr;
            }
            set {
                this._enblhr = value;
            }
        }
        
        /// <summary>
        /// Свойство Включить опрос суточных
        /// </summary>
        public virtual bool EnblDay {
            get {
                return _enblday;
            }
            set {
                this._enblday = value;
            }
        }
        
        /// <summary>
        /// Свойство Включить опрос месячных
        /// </summary>
        public virtual bool EnblMon {
            get {
                return _enblmon;
            }
            set {
                this._enblmon = value;
            }
        }
        
        /// <summary>
        /// Свойство Рассписание синхронизации архивных данных
        /// </summary>
        public virtual Sys.Types.Components.ScheduleDbWrap Arch {
            get {
                return _arch;
            }
            set {
                this._arch = value;
            }
        }
        
        /// <summary>
        /// Свойство Рассписание чтения мгновенных значений
        /// </summary>
        public virtual Sys.Types.Components.ScheduleDbWrap Im {
            get {
                return _im;
            }
            set {
                this._im = value;
            }
        }
        
        #region Encode
        /// <summary>
        /// Метод сериализации
        /// </summary>
        public virtual void Encode(Sys.Encodeable.IEncoderObject value) {
            value.PushNamespace("Sys.Services.Drv");
            value.WriteBoolean("EnblTimeCorr", this.EnblTimeCorr, true);
            value.WriteBoolean("EnblEvents", this.EnblEvents, false);
            value.WriteBoolean("EnblIm", this.EnblIm, false);
            value.WriteBoolean("EnblHr", this.EnblHr, false);
            value.WriteBoolean("EnblDay", this.EnblDay, false);
            value.WriteBoolean("EnblMon", this.EnblMon, false);
            value.WriteEncodeable<Sys.Types.Components.ScheduleDbWrap>("Arch", this.Arch);
            value.WriteEncodeable<Sys.Types.Components.ScheduleDbWrap>("Im", this.Im);
            value.PopNamespace();
        }
        #endregion
        
        /// <summary>
        /// Метод создания TLM3SharedSetting_CreateSys_Types_Components_ScheduleDbWrap
        /// </summary>
        private Sys.Types.Components.ScheduleDbWrap TLM3SharedSetting_CreateSys_Types_Components_ScheduleDbWrap() {
            return new Sys.Types.Components.ScheduleDbWrap();
        }
        
        #region Decode
        /// <summary>
        /// Метод сериализации
        /// </summary>
        public virtual void Decode(Sys.Encodeable.IDecoderObject value) {
            value.PushNamespace("Sys.Services.Drv");
            this.EnblTimeCorr = value.ReadBoolean("EnblTimeCorr", true);
            this.EnblEvents = value.ReadBoolean("EnblEvents", false);
            this.EnblIm = value.ReadBoolean("EnblIm", false);
            this.EnblHr = value.ReadBoolean("EnblHr", false);
            this.EnblDay = value.ReadBoolean("EnblDay", false);
            this.EnblMon = value.ReadBoolean("EnblMon", false);
            this.Arch = value.ReadEncodeableClass<Sys.Types.Components.ScheduleDbWrap>("Arch", this.TLM3SharedSetting_CreateSys_Types_Components_ScheduleDbWrap);
            this.Im = value.ReadEncodeableClass<Sys.Types.Components.ScheduleDbWrap>("Im", this.TLM3SharedSetting_CreateSys_Types_Components_ScheduleDbWrap);
            value.PopNamespace();
        }
        #endregion
    }
    
    /// <summary/>
    public partial class TLM3ContentSetting : Sys.Encodeable.IEncodeableObject {
        
        /// <summary>
        /// Поле для свойства Адресс прибора в сети
        /// </summary>
        private int _address;
        
        /// <summary>
        /// Интернал конструктор
        /// </summary>
        internal TLM3ContentSetting(int address) {
            this.Address = address;
        }
        
        /// <summary>
        /// Публичный конструктор
        /// </summary>
        public TLM3ContentSetting() {
        }
        
        /// <summary>
        /// Свойство Адресс прибора в сети
        /// </summary>
        public virtual int Address {
            get {
                return _address;
            }
            set {
                this._address = value;
            }
        }
        
        #region Encode
        /// <summary>
        /// Метод сериализации
        /// </summary>
        public virtual void Encode(Sys.Encodeable.IEncoderObject value) {
            value.PushNamespace("Sys.Services.Drv");
            value.WriteInt32("Address", this.Address);
            value.PopNamespace();
        }
        #endregion
        
        #region Decode
        /// <summary>
        /// Метод сериализации
        /// </summary>
        public virtual void Decode(Sys.Encodeable.IDecoderObject value) {
            value.PushNamespace("Sys.Services.Drv");
            this.Address = value.ReadInt32("Address");
            value.PopNamespace();
        }
        #endregion
    }
    
    /// <summary/>
    internal partial class TLM3SynchState : Sys.Encodeable.IEncodeableObject {
        
        /// <summary>
        /// Поле для свойства 
        /// </summary>
        private string _serialnumber;
        
        /// <summary>
        /// Интернал конструктор
        /// </summary>
        internal TLM3SynchState(string serialNumber) {
            this.SerialNumber = serialNumber;
        }
        
        /// <summary>
        /// Публичный конструктор
        /// </summary>
        public TLM3SynchState() {
        }
        
        /// <summary>
        /// Свойство 
        /// </summary>
        public virtual string SerialNumber {
            get {
                return _serialnumber;
            }
            set {
                this._serialnumber = value;
            }
        }
        
        #region Encode
        /// <summary>
        /// Метод сериализации
        /// </summary>
        public virtual void Encode(Sys.Encodeable.IEncoderObject value) {
            value.PushNamespace("Sys.Services.Drv");
            value.WriteString("SerialNumber", this.SerialNumber);
            value.PopNamespace();
        }
        #endregion
        
        #region Decode
        /// <summary>
        /// Метод сериализации
        /// </summary>
        public virtual void Decode(Sys.Encodeable.IDecoderObject value) {
            value.PushNamespace("Sys.Services.Drv");
            this.SerialNumber = value.ReadString("SerialNumber");
            value.PopNamespace();
        }
        #endregion
    }
}
