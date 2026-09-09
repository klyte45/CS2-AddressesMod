using Game.Input;
using Game.UI;
using Unity.Entities;

namespace BelzontAdr
{
    public partial class AdrRoadGroupingUISystem : UISystemBase
    {
        private AdrRoadGroupingTool m_Tool;
        private ProxyAction m_EnableToolAction;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Tool = World.GetOrCreateSystemManaged<AdrRoadGroupingTool>();
            m_EnableToolAction = AdrModData.Instance.GetAction(AdrModData.kActionEnableRoadGrouping);
        }

        protected override void OnStartRunning()
        {
            m_EnableToolAction.shouldBeEnabled = true;
        }

        protected override void OnStopRunning()
        {
            m_EnableToolAction.shouldBeEnabled = false;
        }

        protected override void OnUpdate()
        {
            if (!m_Tool.IsSelected && m_EnableToolAction.WasPerformedThisFrame())
            {
                m_Tool.Select();
            }
        }
    }
}
