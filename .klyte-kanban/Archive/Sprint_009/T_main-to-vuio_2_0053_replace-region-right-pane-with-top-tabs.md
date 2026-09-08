**End time:** 2026-09-08 20:57 -0300
# [0053] replace-region-right-pane-with-top-tabs

**Developed by:** Leandro Klyte <klyte45@kwyt.com.br>
## User Story

> Acting as **a developer**, I want **the region right-side pane to use top-based tabs instead of the inner left-side list**, so that I **the region editors can keep their current behavior while using the full width of the panel side**.

---

## Background

Sprint 6 add-on. Keep the map on the left and change only the right pane navigation chrome from left-list tabs to top tabs.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] Region right pane uses top-based tabs instead of left-side inner tabs
- [x] Highway and city inner content behavior remains unchanged
- [x] Selected tab content uses full width of the right pane
- [x] Map interactions and overlay resets still work when switching tabs
- [x] Frontend build passes

---

## Implementation Notes

1. Reuse VUIO TabBar and Tab pattern from AdrSettingsPanel
2. Use old main region editor horizontal-tab behavior as UX reference only
3. Do not change HighwayRegisterManagement or RegionCitiesManagement behavior beyond layout fallout

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


