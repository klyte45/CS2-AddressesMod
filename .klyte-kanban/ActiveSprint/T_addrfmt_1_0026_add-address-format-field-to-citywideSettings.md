**End time:** 2026-04-04 00:56 -0300
**Start time:** 2026-04-04 00:54 -0300
# [0026] add-address-format-field-to-citywideSettings

**Developed by:** Claude Sonnet 4.6 (claude-sonnet-4-6@kwytco.com.br)
## User Story

> Acting as **a developer**, I want **AdrCitywideSettings to have an AddressFormatPattern string field**, so that I **custom address format can be persisted per city**.

---

## Background

AdrCitywideSettings is an ISerializable class. Adding a field requires incrementing CURRENT_VERSION. Check the current version before editing — CRITICAL: any field addition/removal/type-change MUST bump CURRENT_VERSION to avoid savegame corruption.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] CURRENT_VERSION bumped by 1
- [x] New private nullable string field addressFormatPattern added
- [x] Public getter/setter AddressFormatPattern exposed
- [x] Serialize() writes the new field (empty string for null)
- [x] Deserialize() reads the field only when version >= new version
- [x] Old savegames (previous version) deserialize cleanly with null default
- [x] Build passes

---

## Implementation Notes



---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Forgetting to bump CURRENT_VERSION | Low (documented in memory) | Always check CURRENT_VERSION before adding any field to ISerializable classes |

---

## Related Tasks

### Depends on



### Is dependent for


