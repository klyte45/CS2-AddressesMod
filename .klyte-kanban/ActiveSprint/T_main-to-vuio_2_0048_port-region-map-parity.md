**End time:** 2026-09-08 20:57 -0300
**Start time:** 2026-09-08 19:51 -0300
# [0048] port-region-map-parity

**Developed by:** Leandro Klyte <klyte45@kwyt.com.br>
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

- [x] MapDiv + svgutils ported; map.scss and regionEditor.scss as CSS modules
- [x] Pan/zoom works
- [x] Double-click point selection feeds highway start ref / city tools
- [x] Outside connection markers render
- [x] Highway / rail / urban overlays work
- [x] Neighborhood sector overlays (None/Land/Water/Air) work
- [x] Reload + coordinate HUD work
- [x] Selected highway path highlights
- [x] No stubbed select-on-map left

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

- 0047
### Is dependent for

---
