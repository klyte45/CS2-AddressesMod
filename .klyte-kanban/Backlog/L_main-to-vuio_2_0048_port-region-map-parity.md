# [0048] port-region-map-parity

**Developed by:** 

## Reference

RefsLibrary/2026082521_main-to-vuio-migration/07-tab-by-tab-migration.md — M5 checklist

## User Story

> Acting as **a developer**, I want **full region map parity in the VUIO settings panel**, so that I **region map behavior matches EUIS main before we delete main**.

---

## Background

Sprint 6 (Region map). Full map parity — hard gate before S7 cutover.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] MapDiv + svgutils ported; map.scss and regionEditor.scss as CSS modules
- [ ] Pan/zoom works
- [ ] Double-click point selection feeds highway start ref / city tools
- [ ] Outside connection markers render
- [ ] Highway / rail / urban overlays work
- [ ] Neighborhood sector overlays (None/Land/Water/Air) work
- [ ] Reload + coordinate HUD work
- [ ] Selected highway path highlights
- [ ] No stubbed select-on-map left

---

## Implementation Notes

1. Port MapDiv.tsx and utility/svgutils.ts
2. Convert map.scss and regionEditor.scss to CSS modules
3. Test Gameface SVG/mask and pointer events

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


