using Belzont.Interfaces;
using Colossal;
using System;
using System.Linq;
using Unity.Entities;

namespace BelzontAdr
{
    public class AdrMainController : SystemBase, IBelzontBindable
    {
        private Action<string, object[]> m_eventCaller;
        public void SetupCallBinder(Action<string, Delegate> eventCaller)
        {
            eventCaller("main.listSimpleNames", () => AdrNameFilesManager.Instance.SimpleNamesDict.Values.ToArray());
            eventCaller("main.reloadSimpleNames", () =>
            {
                AdrNameFilesManager.Instance.ReloadNameFiles();
                return AdrNameFilesManager.Instance.SimpleNamesDict.Values.ToArray();
            });
            eventCaller("main.goToSimpleNamesFolder", () => { RemoteProcess.OpenFolder(AdrNameFilesManager.SimpleNameFolder); });
        }

        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            m_eventCaller = eventCaller;
        }

        public void SetupEventBinder(Action<string, Delegate> eventCaller)
        {
        }

        protected override void OnUpdate()
        {
        }
    }
}
