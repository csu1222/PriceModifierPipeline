# Phase 5 — Architecture Comparison / Pre-Weather Baseline

## Experiment Question

가격 정책을 각 사용처에서 직접 판단하는 방식과 공통 Modifier Pipeline으로 처리하는 방식은 기능적으로 동일한 결과를 유지하면서 코드 구조에 어떤 차이를 만드는가?

## Frozen Source Identity

- Baseline commit: `2bc99c1022d6c04ce88d586d6f08f3d82257e350`.
- Unity: 6000.3.10f1. Evidence date: 2026-09-04.
- Phase 1–4 source and serialized assets are frozen. Phase 5 adds an observation/debug/test layer only.
- Working tree was clean at start; the handoff's uncommitted-work assumption did not match the observed repository.
- Local execution evidence: `Logs/Phase5/baseline.json`, `build.log`, `tests.log`, `tests.xml`, and resolution PNGs. Logs are ignored, not part of the committed deliverable.

## Measurement Definitions (also binding for Phase 6)

Count each option independently. Include production classes that select price policies from domain input. Exclude comparison/debug/test/editor code, the Phase 1 reference evaluator, shared constant declarations, enum validation, UI commands, neutral initializers, arithmetic, rounding, and execution of already resolved flags.

1. **Policy Class Count:** classes directly deciding at least one Season, Distance, Event, or LocalSpecialty application policy. Extend this same category to Weather when Phase 6 adds that policy; do not change the counting unit.
2. **Policy Location Count:** independent decision statements selecting a policy. One switch is one location regardless of its case count. One if/conditional expression is one location; an else path is not counted again. Repeated decisions in Preview and Commit are separate locations.
3. **Duplicate Policy Count:** semantic policy kinds implemented repeatedly on different production paths within one option. Count kinds, not occurrences in excess of the first. Season, Distance, Event and LocalSpecialty repeated across two Direct controllers count as four. Intentional cross-option duplication is excluded.
4. **LocalSpecialty Branch Count:** decision locations separating LocalSpecialty from Normal. A positive `ItemType == Normal` guard counts because its excluded path implements the specialty exception. This is a subset of policy locations, not an additional count.

These definitions measure decision distribution and duplication, not total class count or architecture quality. Phase 6 must additionally report actual changed files/locations against this commit; no weather impact is inferred before that experiment.

## Baseline Metrics

| Metric | Direct | Pipeline |
| --- | ---: | ---: |
| Policy Class Count | 2 | 1 |
| Policy Location Count | 8 | 4 |
| Duplicate Policy Count | 4 | 0 |
| LocalSpecialty Branch Count | 2 | 1 |

Evidence, repository-relative paths under `Assets/01.Scripts`:

- `DirectCalculation/DirectPreviewController.cs`: item guard line 19, season switch line 22, distance condition line 34, event condition line 39.
- `DirectCalculation/DirectCommitController.cs`: item guard line 19, season switch line 22, distance condition line 34, event condition line 39.
- `ModifierPipeline/SellPriceModifierResolver.cs`: item guard line 14, season switch line 17, distance condition line 26, event condition line 29.
- Direct's two classes independently implement all four policy kinds. Pipeline resolves those four kinds in one class; calculator flag checks execute the resolved instructions without choosing domain policies.

## Functional Equivalence

Fixed matrix: BasePrice 100 × 2 ItemTypes × 3 Seasons × 2 Distances × 2 Events = 24 contexts.

The Phase 5 test uses `PipelinePriceTests.Contexts()` directly as its NUnit source. The existing 24 Cross-Option tests remain unchanged. New coverage checks the Comparison Validator, the frozen display lookup, and both real runtimes for each existing context. The display lookup contains the Phase 4 explicit expectations and is checked against that source; it is not a new rule evaluator. Non-100 base prices are rejected by this lookup. The validator itself accepts caller-supplied expectations and never calculates a reference price.

For each context: Expected == Direct Preview == Direct Commit == Pipeline Preview == Pipeline Commit. Applied states are also checked. Applied ON means the stage participates, including a neutral ×1 multiplier; it does not mean that the numerical price increased.

Expected rows, with columns Short/None, Short/Lucky, Long/None, Long/Lucky:

| Item | Season | Short/None | Short/Lucky | Long/None | Long/Lucky |
| --- | --- | ---: | ---: | ---: | ---: |
| Normal | Normal | 100 | 150 | 130 | 195 |
| Normal | Favored | 120 | 180 | 156 | 234 |
| Normal | Unfavored | 80 | 120 | 104 | 156 |
| LocalSpecialty | Normal | 100 | 150 | 100 | 150 |
| LocalSpecialty | Favored | 100 | 150 | 100 | 150 |
| LocalSpecialty | Unfavored | 100 | 150 | 100 | 150 |

## Calculation Path

Direct Preview → DirectPreviewController.CalculatePreview.

Direct Commit → DirectCommitController.CalculateCommit.

Pipeline Preview / Commit → PipelinePriceService.Calculate → SellPriceCalculationContext → SellPriceModifierResolver → PriceCalculator → PipelinePriceResult.

Only `PriceModifierPipeline.Comparison` references both options. No Direct → Pipeline or Pipeline → Direct production reference is added. No common debug panel or production script changes are required.

## Comparison Scene

`Assets/Scenes/03_ArchitectureComparison.unity` is created by `PriceArchitectureComparisonSceneBuilder` using Editor APIs, not YAML edits. It contains a camera, Canvas/Scaler, EventSystem with InputSystemUIInputModule, a comparison panel, and one owner with Comparison/Direct/Pipeline runtimes. Builder refuses to overwrite an existing scene and uses Undo and scene dirty/save APIs.

The panel displays shared input, each option's Preview/Commit, modifier applied states, consistency, input synchronization, cross-option equivalence and expected-price agreement. Persistent buttons invoke Next Season, Toggle Distance, Toggle Event, Toggle Item Type, Run Both and Reset. The panel formats runtime snapshots and forwards commands; it does not select price policies.

Each input command reaches both existing runtimes, which clear their own prior results. Uncalculated or desynchronized results display N/A. Run Both executes all four paths and does not reuse Preview for Commit. A desynchronized comparison is blocked; Reset restores the same initial input to both. Input equality is checked from current runtimes, not assumed from command delivery. The panel refreshes from current state so external input changes are also visible.

## Trade-off and Limits

Direct offers a short call path and fewer production types, but Preview and Commit repeat policy decisions. Pipeline introduces context/result/service/resolver/calculator navigation while centralizing policy selection and calculation. Neither is declared universally superior, and class count alone is not a quality metric.

The comparison layer is intentionally diagnostic, not a third production option. Its frozen lookup assumes the existing enum order and BasePrice 100; the reused matrix test detects drift. Price equivalence alone does not prove equivalence for future policies, arbitrary inputs or side effects. Existing regression tests retain zero/rounding/overflow coverage. No benchmark, generic rule engine, dynamic modifier registry or architecture refactoring is included.

## Pre-Weather Baseline

- Direct: Policy Class 2 / Policy Location 8 / Duplicate Policy 4 / LocalSpecialty Branch 2.
- Pipeline: Policy Class 1 / Policy Location 4 / Duplicate Policy 0 / LocalSpecialty Branch 1.
- Weather Modifier: **NOT IMPLEMENTED**.
- Phase 6 compares its changed source and measurements against the frozen commit and definitions above.

## Verification

- Unity batch EditMode suite (including EnterPlayMode Scene tests): **130/130 PASS**, zero failed or skipped. Final test execution has zero C# compile errors and no NullReferenceException.
- Existing regression: Phase 1 20/20, Phase 2 8/8, Phase 3 31/31, Phase 4 43/43; total **102/102 PASS**.
- Phase 5: **28/28 PASS** = 24 reused contexts + command/invalidation/reset/desynchronization test + failure-state distinction test + 2 Scene/resolution tests.
- Functional matrix: **24/24 PASS; mismatch 0**. All four paths match the frozen expectations.
- Normal/Favored/Long/Lucky: both Preview/Commit **234/234**, both consistency PASS, cross-option and expected agreement PASS.
- LocalSpecialty/Favored/Long/Lucky: both **150/150**, Season OFF / Distance OFF / Event ON, consistency and equivalence PASS.
- All six saved persistent button callbacks exercised; input synchronization, results invalidation, reset, external desynchronization detection and panel re-enable verified.
- **1280×720 and 1920×1080 automated rendering PASS**. Both resolutions capture initial/normal/specialty/reset states, test Canvas pixel size and text height, and check label bounds. Normal 1280×720 and specialty 1920×1080 PNGs were visually inspected; displayed text and all six buttons are visible without clipping or overlap.
- Resolution tests temporarily use ScreenSpaceCamera and RenderTextures. The saved scene uses ScreenSpaceOverlay. Physical pointer interaction, overlay GameView inspection and standalone player execution are **unverified**. These results must not be reported as manual testing.
- The first scene-generation run exposed a new panel Reset callback before serialized references were assigned; a null guard fixed that initialization issue. The final 130-test run passes with the corrected source. The initial build log is retained as history, not represented as an error-free run.
- Existing package duplicate-assembly/native-extension diagnostics remain; package settings were not changed.
- Source integrity: SHA-256 comparison of all **183 pre-existing files** under Assets, ProjectSettings and Packages found **0 changed or missing files** after testing. Existing production, common debug panel, scenes, prefabs and metadata remain byte-identical.

Final verdict: **CONDITIONAL PASS** against the handoff's complete checklist: implementation and automated verification pass; literal manual Scene interaction at both resolutions is still unverified. Phase 6's source/measurement baseline is ready, with that runtime validation boundary recorded.

## Files Added

- `Assets/01.Scripts/ArchitectureComparison/`: result, validator, frozen baseline lookup, runtime coordinator, panel, assembly definition; `Editor/` scene builder and editor assembly definition.
- `Assets/02.Tests/Comparison/EditMode/`: matrix/runtime/failure-state tests, scene/resolution tests and test assembly definition.
- `Assets/Scenes/03_ArchitectureComparison.unity`.
- This document and Unity-generated metadata for all new assets/folders.

No staging, commit or push is performed by Phase 5.
