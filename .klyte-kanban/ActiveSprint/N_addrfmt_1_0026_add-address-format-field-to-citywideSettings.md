# [0026] add-address-format-field-to-citywideSettings

**Developed by:** 

## User Story

> Acting as **a developer**, I want **AdrCitywideSettings to have an AddressFormatPattern string field**, so that I **custom address format can be persisted per city**.

---

## Background

AdrCitywideSettings is an ISerializable class. Adding a field requires incrementing CURRENT_VERSION. Check the current version before editing — CRITICAL: any field addition/removal/type-change MUST bump CURRENT_VERSION to avoid savegame corruption.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] CURRENT_VERSION bumped by 1
- [ ] New private nullable string field addressFormatPattern added
- [ ] Public getter/setter AddressFormatPattern exposed
- [ ] Serialize() writes the new field (empty string for null)
- [ ] Deserialize() reads the field only when version >= new version
- [ ] Old savegames (previous version) deserialize cleanly with null default
- [ ] Build passes

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


