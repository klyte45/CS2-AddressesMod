# [0020] revert-direct-to-city-github-import

**Developed by:** 

## User Story

> Acting as **a city builder**, I want **GitHub imports to always go through the preview/confirmation screen before being saved**, so that I **I can review and rename the file before it is added to my save game**.

---

## Background

Task 0013 added an 'Add to city' button that bypassed the import preview. The original behavior diagnosis was wrong: clicking 'Copy to City' already goes directly to the save game (via the import preview). The direct-to-city button must be removed and the GitHub screen must show only the single import button that opens the preview screen.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] addDirectToCityFromGitHub function is removed from CityNamesetLibraryCmp.tsx
- [ ] GitHub import screen shows only one action button per file that goes to goToImportDetailsGitHub
- [ ] cityNamesetsLibrary.addDirectToCity key removed from i18n.csv and all 11 locale files
- [ ] No broken references to the removed function or translation key remain

---

## Implementation Notes

1. Remove addDirectToCityFromGitHub function entirely from CityNamesetLibraryCmp.tsx
2. Change GitHub screen actionButtons to: (p) => <button className='positiveBtn' onClick={() => goToImportDetailsGitHub(p)}>{translate('cityNamesetsLibrary.copyToCity')}</button>
3. Delete the cityNamesetsLibrary.addDirectToCity row from i18n.csv
4. Delete the addDirectToCity row from all 11 locale CSV files using PowerShell

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


