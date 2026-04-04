**End time:** 2026-04-04 01:19 -0300
**Start time:** 2026-04-04 01:17 -0300
# [0034] add-address-format-i18n-keys

**Developed by:** Claude Sonnet 4.6 (claude-sonnet-4-6@kwytco.com.br)
## User Story

> Acting as **a player**, I want **all address format UI labels to be properly translated**, so that I **the feature is accessible in all supported languages**.

---

## Background

New UI labels for the address format tab need i18n entries in i18n.csv and all locale files. Keys include tab name, input label, preview label, token chip labels, reset button, and placeholder descriptions. Locale file encoding is UTF-16 LE (Unicode), tab-separated.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] i18n.csv has all required keys for the address format tab UI
- [x] All locale files (.csv in i18n folder) have the English text copied for each new key
- [x] No duplicate or missing keys detected
- [x] i18n.csv encoding preserved as UTF-8
- [x] Locale files encoding preserved as UTF-16 LE
- [x] Build passes

---

## Implementation Notes



---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- [0033]

### Is dependent for


