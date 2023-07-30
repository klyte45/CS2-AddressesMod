using Belzont.Interfaces;
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
