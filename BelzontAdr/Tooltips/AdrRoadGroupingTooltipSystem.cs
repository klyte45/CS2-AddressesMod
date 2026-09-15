using Game.Input;
using Game.Tools;
using Game.UI.Localization;
using Game.UI.Tooltip;
using Unity.Entities;
using UnityEngine.Scripting;

namespace BelzontAdr
{
    /// <summary>
    /// Mouse Error tooltips + InputHint rows for Road Grouping.
    /// Hint actions use ModData <see cref="ProxyAction"/>s with <see cref="DisplayNameOverride"/>
    /// because <c>ToolBaseSystem.toolActions</c> is <c>private protected</c> (not overridable from this assembly).
    /// </summary>
    public partial class AdrRoadGroupingTooltipSystem : TooltipSystemBase
    {
        private const string KindMismatchKey = "K45::ADR.vuio[roadGrouping.kindMismatch]";
        private const string KindUnclassifiedKey = "K45::ADR.vuio[roadGrouping.kindUnclassified]";
        private const string DragUnreachableKey = "K45::ADR.vuio[roadGrouping.reject.dragUnreachable]";

        private ToolSystem m_ToolSystem;
        private AdrRoadGroupingTool m_RoadGroupingTool;
        private StringTooltip m_ErrorTooltip;
        private InputHintTooltip m_ApplyHintTooltip;
        private InputHintTooltip m_CancelHintTooltip;
        private InputHintTooltip m_ToggleHintTooltip;

        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_RoadGroupingTool = World.GetOrCreateSystemManaged<AdrRoadGroupingTool>();
            m_ErrorTooltip = new StringTooltip
            {
                path = "k45AdrRoadGroupingError",
                color = TooltipColor.Error,
            };
        }

        [Preserve]
        protected override void OnUpdate()
        {
            if (m_ToolSystem.activeTool != m_RoadGroupingTool)
            {
                return;
            }

            bool showingError = TryResolveError(out string localeId);
            if (showingError)
            {
                m_ErrorTooltip.value = LocalizedString.Id(localeId);
                AddMouseTooltip(m_ErrorTooltip);
            }

            EnsureHintTooltips();
            InputManager.DeviceType device = InputManager.instance.activeControlScheme.ToDeviceType();
            TryAddHint(m_ApplyHintTooltip, m_RoadGroupingTool.HintApplyAction, device);
            TryAddHint(m_CancelHintTooltip, m_RoadGroupingTool.HintCancelAction, device);
            TryAddHint(m_ToggleHintTooltip, m_RoadGroupingTool.HintToggleAction, device);
        }

        private void EnsureHintTooltips()
        {
            ProxyAction apply = m_RoadGroupingTool.HintApplyAction;
            ProxyAction cancel = m_RoadGroupingTool.HintCancelAction;
            ProxyAction toggle = m_RoadGroupingTool.HintToggleAction;
            if (apply != null && (m_ApplyHintTooltip == null || m_ApplyHintTooltip.m_Action != apply))
            {
                m_ApplyHintTooltip = new InputHintTooltip(apply);
            }

            if (cancel != null && (m_CancelHintTooltip == null || m_CancelHintTooltip.m_Action != cancel))
            {
                m_CancelHintTooltip = new InputHintTooltip(cancel);
            }

            if (toggle != null && (m_ToggleHintTooltip == null || m_ToggleHintTooltip.m_Action != toggle))
            {
                m_ToggleHintTooltip = new InputHintTooltip(toggle);
            }
        }

        private void TryAddHint(InputHintTooltip tooltip, ProxyAction action, InputManager.DeviceType device)
        {
            if (tooltip == null || action == null)
            {
                return;
            }

            DisplayNameOverride display = action.displayOverride;
            if (display == null || !display.active || display.priority <= 0)
            {
                return;
            }

            InputManager.DeviceType mask = action.displayMask & device;
            if (mask == InputManager.DeviceType.None)
            {
                return;
            }

            tooltip.Refresh(mask);
            AddMouseTooltip(tooltip);
        }

        private bool TryResolveError(out string localeId)
        {
            localeId = null;

            if (m_RoadGroupingTool.IsEndpointDragging)
            {
                AdrNetKindJoinResult dragReject = m_RoadGroupingTool.CurrentDragRejectReason;
                if (dragReject != AdrNetKindJoinResult.Ok)
                {
                    localeId = LocaleIdForKindReject(dragReject);
                    return true;
                }

                if (m_RoadGroupingTool.IsDragDestinationUnreachable)
                {
                    localeId = DragUnreachableKey;
                    return true;
                }

                return false;
            }

            AdrNetKindJoinResult hoverReject = m_RoadGroupingTool.CurrentHoverJoinResult;
            if (hoverReject != AdrNetKindJoinResult.Ok)
            {
                localeId = LocaleIdForKindReject(hoverReject);
                return true;
            }

            return false;
        }

        private static string LocaleIdForKindReject(AdrNetKindJoinResult result) =>
            result == AdrNetKindJoinResult.Unclassified
                ? KindUnclassifiedKey
                : KindMismatchKey;
    }
}
