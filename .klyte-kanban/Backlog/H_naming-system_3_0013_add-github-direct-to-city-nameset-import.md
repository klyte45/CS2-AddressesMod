# [0013] add-github-direct-to-city-nameset-import

**Developed by:** 

## User Story

> Acting as **a player browsing the GitHub name file repository**, I want **to add a name file directly to my city without downloading it to the local library first**, so that I **reduce friction when trying a name file from GitHub for a specific city**.

---

## Background

The GitHub file browser (NamesetGitHubSelectorCmp.tsx, GitHubAddressesFilesSevice.tsx) currently only supports downloading files to the local library folder. Users then need a second step to assign them to the city. A direct-to-city path would streamline the workflow.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] The GitHub file browser shows an 'Add to city' action in addition to the existing 'Download to library' action
- [ ] Selecting 'Add to city' adds the name file directly to the city's nameset list via NamesetService.sendNamesetForCity
- [ ] The file is not saved to disk (it's ephemeral - only stored in the city save)
- [ ] The UI reflects the new city nameset immediately after the call

---

## Implementation Notes



---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


