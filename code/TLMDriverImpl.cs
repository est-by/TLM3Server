using Sys.Configuration;
using Sys.DataBus.Common;
using Sys.Services.Components;
using Sys.Services.Drv.TLM3.Culture;
using Sys.Types.Components;
using Sys.Types.Components.DataDriverClient;
using Sys.Types.NodeEditor;
using Sys.Types.Om;
using System;

namespace Sys.Services.Drv.TLM3
{
    public class TLMDriver : Sys.Types.Components.DriverElectroClient
    {
        /// <summary>Идентификатор типа реализации компонента</summary>
        public const string TypeGuidImpl = "est.by:Bus.TLM3DrvClientImpl";

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
            var ss = request.GetSharedSetting<TLMSharedSetting>(() => new TLMSharedSetting());
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
            var ss = request.GetSharedSetting<TLMSharedSetting>(() => new TLMSharedSetting());
            var cs = request.GetContentSetting<TLMContentSetting>(() => new TLMContentSetting());
            ImNextPoint nextPoint = new ImNextPoint(TimeZoneMap.Local, 0, imNextItem: new ImNextItem("im", ss.Im.ToSch()));
            using (var session = new PhysicalSessionIm<TLMSynchState, ImNextPoint>(this, request, nextPoint))
            {
                session.Open();
                if (!session.LaunchPoint(nextPoint.GetItem("im"))) return session;
                if (session.BeginOperation())
                {
                }
                return session;
            }
        }
        #endregion

        #region (Synch)
        public override SynchResponse Synch(SynchRequestDataDrv requestData, SynchParamsDataDrv requestParam)
        {
            return null;
        }
        #endregion
       
        #region (Test)
        public override TestResult Test(TestRequestDataDrv request)
        {
            TestResult result = new TestResult();

            return result;
        }
        #endregion
    }
}
