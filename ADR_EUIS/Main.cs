#if !RELEASE
//#define LOCAL
#endif
using BelzontAdr;
using K45EUIS_Ext;
using System;

namespace ADR_EUIS
{
    public class ADR_EUIS_Main : IEUISAppRegister
    {
        public string ModAppIdentifier => "main";

        public string DisplayName => "Addresses Mod for CS2";

#if LOCAL

        public string UrlJs => "http://localhost:8715/k45-adr-main.js";
        public string UrlCss => "http://localhost:8715/k45-adr-main.css";
#else
        public string UrlJs => $"coui://{AddressesCs2Mod.Instance.CouiHost}/UI/k45-adr-main.js";
        public string UrlCss => $"coui://{AddressesCs2Mod.Instance.CouiHost}/UI/k45-adr-main.css";
#endif
        public string UrlIcon => $"coui://{AddressesCs2Mod.Instance.CouiHost}/UI/images/ADR.svg";

        public string ModderIdentifier => "k45";
        public string ModAcronym => "adr";
    }
    public class ADR_EUIS : IEUISModRegister
    {
        public string ModderIdentifier => "k45";
        public string ModAcronym => "adr";
        public Action<Action<string, object[]>> OnGetEventEmitter => (eventCaller) => AddressesCs2Mod.Instance.SetupCaller(eventCaller);
        public Action<Action<string, Delegate>> OnGetEventsBinder => (eventCaller) => AddressesCs2Mod.Instance.SetupEventBinder(eventCaller);
        public Action<Action<string, Delegate>> OnGetCallsBinder => (eventCaller) => AddressesCs2Mod.Instance.SetupCallBinder(eventCaller);
    }
}
