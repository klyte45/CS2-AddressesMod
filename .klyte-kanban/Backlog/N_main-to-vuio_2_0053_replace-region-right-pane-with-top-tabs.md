# [0053] replace-region-right-pane-with-top-tabs

**Developed by:** 

## User Story

> Acting as **a developer**, I want **the region right-side pane to use top-based tabs instead of the inner left-side list**, so that I **the region editors can keep their current behavior while using the full width of the panel side**.

---

## Background

Sprint 6 add-on. Keep the map on the left and change only the right pane navigation chrome from left-list tabs to top tabs.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Region right pane uses top-based tabs instead of left-side inner tabs
- [ ] Highway and city inner content behavior remains unchanged
- [ ] Selected tab content uses full width of the right pane
- [ ] Map interactions and overlay resets still work when switching tabs
- [ ] Frontend build passes

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


