**Start time:** 2026-09-15 00:43 -0300
# [0096] TT-05-Road-Grouping-remove-PSI-kind-reject-pops

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/25-road-grouping-tooltips.md TT-05; BelzontAdr/Tools/AdrRoadGroupingTool.cs NotifyNetKindReject

## User Story

> Acting as **a Cities Skylines II player**, I want **no PSI pops for in-tool Road Grouping kind rejects**, so that I **so routine soft-fails stay at the cursor without notification spam**.

---

## Background

Locked product decision: once Error tooltips work, remove in-tool NotificationSystem.Pop for kind rejects. Keep LogUtils. Spec: 25-road-grouping-tooltips.md feedback channels.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] NotifyNetKindReject no longer calls NotificationSystem.Pop for in-tool kind rejects
- [x] LogUtils diagnostic line remains
- [x] Ctrl+toggle and drag soft-fails still surface via TT-02 Error tooltips

---

## Implementation Notes

1. Edit AdrRoadGroupingTool.NotifyNetKindReject
2. Do not remove overlay red wash
3. Coordinate playtest with TT-06
4. NotifyNetKindReject no longer calls NotificationSystem.Pop; LogUtils remains

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- [0093] TT-02-Road-Grouping-Error-StringTooltip-system

### Is dependent for


