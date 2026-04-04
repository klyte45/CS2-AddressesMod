**Start time:** 2026-04-03 22:33 -0300
# [0018] fix-road-qualifier-missing-translations

**Developed by:** Claude Sonnet 4.6 (claude-sonnet-4-6@kwytco.com.br)
## User Story

> Acting as **a city builder**, I want **all road qualifier rule fields to have proper translated labels**, so that I **I can understand each field's purpose without guessing from the key names**.

---

## Background

Task 0009 added new fields to AdrRoadPrefixRule (MinCarLanes, MaxCarLanes, MinWidthM, MaxWidthM) and conditions (AnyElevated). The i18n.csv is missing keys for 4 of those fields, and 'requireAnyElevatedState' used in code does not match 'requireAnyElevated' in i18n.csv.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] roadPrefixSettings.requireAnyElevatedState key exists in i18n.csv (fixing the mismatch with code)
- [ ] roadPrefixSettings.minimumCarLanes, maximumCarLanes, minimumWidthM, maximumWidthM added to i18n.csv with EN and pt-BR text
- [ ] All 11 locale CSV files updated with translated values for the new/fixed keys
- [ ] No missing translation keys in RoadPrefixCmp.tsx when loaded in-game

---

## Implementation Notes

1. In i18n.csv line ~289 rename 'requireAnyElevated' to 'requireAnyElevatedState' to match the code key
2. Add 4 new rows: minimumCarLanes='Required minimum car lanes (0 = any)', maximumCarLanes='Required maximum car lanes (0 = any)', minimumWidthM='Required minimum road width (m, 0 = any)', maximumWidthM='Required maximum road width (m, 0 = any)'
3. Update all 11 locale CSV files with machine-translated equivalents using PowerShell Write-Locale pattern
4. Do NOT change any code — only i18n data files

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


