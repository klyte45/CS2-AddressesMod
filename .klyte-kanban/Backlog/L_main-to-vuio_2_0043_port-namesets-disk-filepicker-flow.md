# [0043] port-namesets-disk-filepicker-flow

**Developed by:** 

## Reference

RefsLibrary/2026082521_main-to-vuio-migration/04-widget-conversion-map.md — FilePicker

## User Story

> Acting as **a developer**, I want **disk nameset selection migrated to FilePickerDialog**, so that I **disk nameset library pickers use the same vanilla dialog as other VUIO mods**.

---

## Background

Sprint 3 (Namesets). Disk library browse/import via FilePickerDialog only.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] NamesetLibrarySelectorCmp uses FilePickerDialog + disk generateDataProvider
- [ ] No custom EUIS disk folder browser UI
- [ ] ADR confirm flow after pick preserved if needed
- [ ] Disk library import/export works from VUIO panel

---

## Implementation Notes

1. Port NamesetLibrarySelectorCmp
2. Replace EUIS browse with FilePickerDialog
3. Keep adr-commons NamesetService calls

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


