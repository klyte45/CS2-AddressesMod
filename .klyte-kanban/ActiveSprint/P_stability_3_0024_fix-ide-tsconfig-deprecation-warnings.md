**Start time:** 2026-04-03 23:14 -0300
# [0024] fix-ide-tsconfig-deprecation-warnings

**Developed by:** Claude Sonnet 4.6 (claude-sonnet-4-6@kwytco.com.br)
## User Story

> Acting as **a developer**, I want **no IDE TypeScript warnings in the frontend tsconfig files**, so that I **I can trust the IDE error panel and not be confused by false deprecation warnings**.

---

## Background

Two tsconfig.json files report TypeScript 6+ deprecation warnings about 'baseUrl' and 'moduleResolution=node10'. The project compiles correctly but the IDE shows these as errors. Fix: add ignoreDeprecations: '6.0' to each affected tsconfig. The main/tsconfig.json also needs rootDir set to ./src to fix the declarationDir layout warning.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] No TypeScript deprecation warnings appear in the IDE for _Frontends/UI/main/tsconfig.json
- [ ] No TypeScript deprecation warnings appear in the IDE for _Frontends/UI/_shared/adr-commons/tsconfig.json
- [ ] Project still compiles correctly via MSBuild after tsconfig changes
- [ ] rootDir is set to ./src in the main tsconfig to fix the declarationDir layout warning

---

## Implementation Notes

1. In _Frontends/UI/main/tsconfig.json: add 'ignoreDeprecations': '6.0' and 'rootDir': './src' inside compilerOptions
2. In _Frontends/UI/_shared/adr-commons/tsconfig.json: add 'ignoreDeprecations': '6.0' inside compilerOptions
3. Verify by running MSBuild on AddressesCS2.sln to confirm no new errors

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


