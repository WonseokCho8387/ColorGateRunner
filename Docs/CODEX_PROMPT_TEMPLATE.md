# Color Gate Runner — Compact Codex Iteration Prompt

Use this template after replacing the bracketed task-specific sections.
Do not paste test counts, artifact hashes, common Git commands, or the full
regression suite into normal prompts.

```text
Color Gate Runner의 [ITERATION NAME]을 진행한다.

프로젝트:
D:/AIPrototype/ColorGateRunner

AGENTS.md의 Standard Iteration Protocol을 따른다.
CURRENT_STATUS.md의 Authoritative Baseline을 사용한다.
TEST_PLAN.md의 Standard Regression Suite를 적용한다.
관련 설계 문서의 현재 계약을 유지한다.

## Goal
- [사용자가 체감해야 하는 결과]
- [구조적으로 달성할 결과]

## Required decisions
- [기존 문서에 없는 이번 작업만의 구체적인 동작]
- [경계값, 우선순위, 화면 이동, 저장 정책 등]

## Scope
- [구현 대상 1]
- [구현 대상 2]
- [구현 대상 3]

## Excluded
- [혼동하기 쉬운 인접 기능]
- [이번에 하지 않을 마이그레이션/SDK/리디자인]

## Phase A focus
- [코드에서 먼저 확인할 불확실성]
- [소유권/시간/저장/Scene 경계]
- [최소 구현안과 예상 파일 범위]

Phase A에서 Baseline, Current Architecture, Proposed Design, Expected Files,
Risks, Non-Goals, Feature Tests를 보고하고 중단한다.
승인 전에는 코드, Scene, Asset, ProjectSettings, 문서를 수정하지 않는다.

승인 후 Phase B에서 구현하고 공통 회귀 검증을 완료한 뒤 문서를
갱신한다.

커밋 메시지:
[COMMIT MESSAGE]

Push하지 않는다.
```
