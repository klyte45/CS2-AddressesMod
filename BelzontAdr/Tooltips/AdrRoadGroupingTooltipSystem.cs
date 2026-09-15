using Game.Input;
using Game.Tools;
using Game.UI.Localization;
using Game.UI.Tooltip;
using Unity.Entities;
using UnityEngine.Scripting;

namespace BelzontAdr
{
    /// <summary>
    /// Mouse Error tooltips + Toggle InputHint for Road Grouping.
    /// Apply/Cancel rows come from vanilla <see cref="InputHintsTooltipSystem"/> via Tool
    /// Mouse Apply / Mouse Cancel States (mutated display names in the tool).
    /// Toggle is ModData-only (not in baseToolActions) — hand-emit one row when its override is published.
    /// </summary>
    public partial class AdrRoadGroupingTooltipSystem : TooltipSystemBase
    {
        private const string KindMismatchKey = "K45::ADR.vuio[roadGrouping.kindMismatch]";
        private const string KindUnclassifiedKey = "K45::ADR.vuio[roadGrouping.kindUnclassified]";
        private const string DragUnreachableKey = "K45::ADR.vuio[roadGrouping.reject.dragUnreachable]";

        private ToolSystem m_ToolSystem;
        private AdrRoadGroupingTool m_RoadGroupingTool;
        private StringTooltip m_ErrorTooltip;
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

            TryAddToggleHint();
        }

        /// <summary>
        /// Mirror <see cref="InputHintsTooltipSystem"/> gate: only emit when
        /// <see cref="ProxyAction.displayOverride"/> is our published override (never fall back to action.title).
        /// </summary>
        private void TryAddToggleHint()
        {
            ProxyAction action = m_RoadGroupingTool.HintToggleAction;
            DisplayNameOverride hint = m_RoadGroupingTool.HintToggleOverride;
            if (action == null || hint == null || !hint.active || hint.priority == -1)
            {
                return;
            }

            if (action.displayOverride != hint || string.IsNullOrEmpty(hint.displayName))
            {
                return;
            }

            InputManager.DeviceType device = InputManager.instance.activeControlScheme.ToDeviceType();
            InputManager.DeviceType mask = action.displayMask & device;
            if (mask == InputManager.DeviceType.None)
            {
                return;
            }

            if (m_ToggleHintTooltip == null || m_ToggleHintTooltip.m_Action != action)
            {
                m_ToggleHintTooltip = new InputHintTooltip(action);
            }

            m_ToggleHintTooltip.Refresh(mask);
            AddMouseTooltip(m_ToggleHintTooltip);
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
