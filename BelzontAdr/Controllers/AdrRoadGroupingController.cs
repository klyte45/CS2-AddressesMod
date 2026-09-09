using Belzont.Interfaces;
using Belzont.Utils;
using System;
using Unity.Entities;

namespace BelzontAdr
{
    public partial class AdrRoadGroupingController : SystemBase, IBelzontBindable
    {
        private const string PREFIX = "roadGrouping.";

        private Action<string, object[]> m_eventCaller;
        private Action<string, Delegate> m_callBinder;
        private bool m_initialized;

        public MultiUIValueBinding<Entity> SelectedAggregation { get; private set; }
        public MultiUIValueBinding<bool> ShowNumberCards { get; private set; }

        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            m_eventCaller = eventCaller;
            if (m_callBinder != null) InitValueBindings();
        }

        public void SetupEventBinder(Action<string, Delegate> eventBinder)
        {
        }

        public void SetupCallBinder(Action<string, Delegate> callBinder)
        {
            m_callBinder = callBinder;
            callBinder($"{PREFIX}enableTool", OnEnableTool);
            callBinder($"{PREFIX}disableTool", OnDisableTool);
            if (m_eventCaller != null) InitValueBindings();
        }

        private void InitValueBindings()
        {
            if (m_initialized) return;
            m_initialized = true;
            SelectedAggregation = new(Entity.Null, $"{PREFIX}{nameof(SelectedAggregation)}", m_eventCaller, m_callBinder);
            ShowNumberCards = new(false, $"{PREFIX}{nameof(ShowNumberCards)}", m_eventCaller, m_callBinder);
        }

        private void OnEnableTool()
        {
            World.GetOrCreateSystemManaged<AdrRoadGroupingTool>().Select();
        }

        private void OnDisableTool()
        {
            World.GetOrCreateSystemManaged<AdrRoadGroupingTool>().RequestDisable();
        }

        public void ClearSelection()
        {
            if (SelectedAggregation != null)
            {
                SelectedAggregation.Value = Entity.Null;
            }
        }

        protected override void OnUpdate()
        {
        }
    }
}
