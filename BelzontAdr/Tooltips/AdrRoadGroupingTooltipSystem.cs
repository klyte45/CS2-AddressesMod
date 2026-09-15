using Game.Tools;
using Game.UI.Localization;
using Game.UI.Tooltip;
using Unity.Entities;
using UnityEngine.Scripting;

namespace BelzontAdr
{
    /// <summary>
    /// Mouse Error tooltips for Road Grouping soft-fails (kind reject / unreachable drag).
    /// Reads reject state from <see cref="AdrRoadGroupingTool"/> (TT-01).
    /// </summary>
    public partial class AdrRoadGroupingTooltipSystem : TooltipSystemBase
    {
        private const string KindMismatchKey = "K45::ADR.vuio[roadGrouping.kindMismatch]";
        private const string KindUnclassifiedKey = "K45::ADR.vuio[roadGrouping.kindUnclassified]";
        private const string DragUnreachableKey = "K45::ADR.vuio[roadGrouping.reject.dragUnreachable]";

        private ToolSystem m_ToolSystem;
        private AdrRoadGroupingTool m_RoadGroupingTool;
        private StringTooltip m_ErrorTooltip;

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

            if (!TryResolveError(out string localeId))
            {
                return;
            }

            m_ErrorTooltip.value = LocalizedString.Id(localeId);
            AddMouseTooltip(m_ErrorTooltip);
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
