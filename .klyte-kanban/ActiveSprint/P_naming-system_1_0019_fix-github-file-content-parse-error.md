**Start time:** 2026-04-03 22:29 -0300
# [0019] fix-github-file-content-parse-error

**Developed by:** Claude Sonnet 4.6 (claude-sonnet-4-6@kwytco.com.br)
## User Story

> Acting as **a city builder**, I want **GitHub name files to be imported correctly with each line as a separate entry**, so that I **I can use community name files from GitHub without each character becoming a separate entry**.

---

## Background

The parseGitHubFile function in CityNamesetLibraryCmp.tsx has a bug: .map(x => x.split(';')).map(x => x[1] ??= x[0]) returns string[] (each element is x[1] or x[0] string value, not the array). This causes: 1) each character treated as an entry (x[0] is char, not whole field), 2) TypeError on empty lines because x[1] is undefined. Also an error '[UI] [ERROR] Unhandled rejection: TypeError: Cannot read properties of undefined (reading replace)' occurs when clicking the Copy to City button.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] parseGitHubFile correctly splits each line into [main, alternative] string pairs
- [ ] Empty lines at end of file are filtered and do not cause errors
- [ ] Clicking 'Copy to City' on a GitHub file opens the import preview with correct entries (one per line)
- [ ] No TypeError exceptions in the UI console when loading GitHub files
- [ ] If the root cause cannot be fully confirmed without in-game testing, a debug log printing the raw downloaded content must be added (only active when using a debug build configuration)

---

## Implementation Notes

1. Fix parseGitHubFile: change the chain to const rawLines = fileContents.split('\n').filter(x => x.trim() !== ''); then const lines = rawLines.map(x => { const parts = x.split(';'); return [parts[0], parts[1] ?? parts[0]]; });
2. Then Values: lines.map(x => x[0].replace('{0}', '').trim()), ValuesAlternative: lines.map(x => x[1].replace('{0}', '').trim())
3. Also add debug logging: if process.env.NODE_ENV === 'development' then console.log('[ADR][parseGitHubFile] raw content:', fileContents)

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


