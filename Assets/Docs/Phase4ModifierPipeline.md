# Modifier Pipeline

## Architecture

SellPriceInput -> SellPriceCalculationContext -> SellPriceModifierResolver.Resolve -> ResolvedSellPriceModifiers -> PriceCalculator.Calculate -> PipelinePriceResult.

PipelinePriceRuntime owns one PipelinePriceService, which owns a resolver and calculator as plain C# objects. Only Runtime, DebugSource and DebugCommand are scene components. Production Pipeline references Common and Debug.Contracts only; Direct is referenced only by the test assembly.

## Context Responsibility

SellPriceCalculationContext is immutable, Unity-independent data copied from the validated SellPriceInput. It deliberately contains exactly the same five fields. Its only additional meaning is an explicit Pipeline request contract; it currently provides no richer transformation or independent business behavior. This small duplication is an architectural comparison cost, not a requirement for extra fields or validation policies.

## Resolver Responsibility

SellPriceModifierResolver is the only Pipeline class selecting domain price policies. The Normal guard includes Season and Distance; LocalSpecialty bypasses both. Event selection is outside this guard. Applied flags describe stage execution, including neutral x1 stages. Excluded stages carry neutral multipliers. Resolver never computes a price.

## Calculator Responsibility

PriceCalculator accepts an int base price and resolved data, applies enabled Season -> Distance -> Event stages using decimal, and rounds once with SellPriceRules.RoundingMode (AwayFromZero). Checked int conversion throws OverflowException. Applied-flag checks execute resolved instructions; they do not choose policies from domain enums. Calculator has no item, season, distance or event domain input.

## Preview Path

Panel Preview -> IPriceArchitectureDebugCommand.Preview -> PipelinePriceDebugCommand.Preview -> PipelinePriceRuntime.Preview -> PipelinePriceService.Calculate(Input) -> Context -> Resolver -> Calculator -> PipelinePriceResult -> PreviewResult/LatestResult -> DebugSource.CaptureSnapshot -> unchanged panel refresh.

## Commit Path

Panel Commit -> IPriceArchitectureDebugCommand.Commit -> PipelinePriceDebugCommand.Commit -> PipelinePriceRuntime.Commit -> the same PipelinePriceService.Calculate(Input) -> Context -> Resolver -> Calculator -> PipelinePriceResult -> CommitResult/LatestResult -> snapshot -> panel refresh.

Commit does not read PreviewResult. Both calls recalculate independently. Input commands invalidate both results and latest applied state; reset restores Base 100 / Normal item / Normal season / Short / None and unavailable prices / NotEvaluated consistency. Before calculation, the unchanged bool-only panel displays modifier OFF and unavailable result dashes, with consistency N/A.

## Policy Evidence

Counting convention matches Phase 3: one switch is one location; neutral initializers, arithmetic, resolved-flag execution, display and input-toggle commands are excluded.

- Policy Class Count: 1, SellPriceModifierResolver.
- Policy Location Count: 4, SellPriceModifierResolver.cs lines 14 (item guard), 17 (season switch), 26 (distance), 29 (event).
- Duplicate Policy Count: 0 repeated semantic policy groups inside Pipeline.
- LocalSpecialty Branch Count: 1, the implicit excluded path of the Normal guard at line 14.
- Shared Calculation Path: yes; Runtime Preview and Commit both invoke the same service, resolver and calculator.

### Direct vs Pipeline

| Metric | Phase 3 Direct | Phase 4 Pipeline |
| --- | ---: | ---: |
| Policy classes | 2 | 1 |
| Policy locations | 8 | 4 |
| Duplicated semantic policy groups | 4 | 0 |
| LocalSpecialty exception branches | 2 | 1 |
| Shared Preview/Commit calculation | No | Yes |

Direct counts are rechecked in the frozen controller sources using the same decision-statement convention. Cross-option duplication is intentional and excluded from the per-option duplication measure.

## Functional Equivalence

The new test assembly independently enumerates all 24 contexts with explicit expected values and compares Direct Preview == Direct Commit == Pipeline Preview == Pipeline Commit. Pipeline Runtime executes Commit before Preview for every context, proving Commit has no dependency on an existing Preview. Multipliers and applied flags are checked in addition to final prices.

Base price 100; columns below list Short/None, Short/Lucky, Long/None, Long/Lucky:

| Item | Season | Short/None | Short/Lucky | Long/None | Long/Lucky |
| --- | --- | ---: | ---: | ---: | ---: |
| Normal | Normal | 100 | 150 | 130 | 195 |
| Normal | Favored | 120 | 180 | 156 | 234 |
| Normal | Unfavored | 80 | 120 | 104 | 156 |
| LocalSpecialty | Normal | 100 | 150 | 100 | 150 |
| LocalSpecialty | Favored | 100 | 150 | 100 | 150 |
| LocalSpecialty | Unfavored | 100 | 150 | 100 | 150 |

## Test Result

Executed 2026-09-04 using Unity 6000.3.10f1 in batch mode in this working project. Unity compilation completed with no C# compile errors.

- Phase 1: 20/20 PASS.
- Phase 2: 8/8 PASS.
- Phase 3: 31/31 PASS.
- Phase 4: 43/43 PASS (24 context equivalence cases, 6 service/runtime cases, 10 resolver/calculator cases, 3 Scene PlayMode tests).
- Total: 102/102 PASS; 0 failed, 0 skipped.
- Local ignored evidence: Logs/Phase4/build.log, tests.log, tests.xml and eight resolution PNGs.
- Existing package duplicate-assembly/native-extension diagnostics remain; no package configuration was changed.

Calculator tests use non-domain multipliers to distinguish execution of resolved flags, decimal precision and overflow-sensitive order; the final-rounding tests distinguish rounding at the end from intermediate rounding. Service tests include zero, midpoint and int overflow. Runtime tests cover invalidation, reset and input cycles. Scene tests execute the existing prefab button callbacks in PlayMode, including subscription re-enable behavior.

## Runtime Result

Scene: Assets/Scenes/02_ModifierPipeline.unity, generated with Unity Editor APIs by PipelinePriceSceneBuilder. Existing panel prefab is instantiated and only new scene instance Source/Command overrides are assigned. Builder refuses overwrite, uses Undo/dirty-state APIs and restores the prior active scene.

Scene PlayMode test PASS: initial Architecture/default input/N/A, Preview, Commit, per-input invalidation, Normal 234/234 PASS, LocalSpecialty 150/150 PASS with Season OFF / Distance OFF / Event ON, Reset and panel re-enable.

1280x720 and 1920x1080 automated render tests both PASS. Normal 1280x720 and LocalSpecialty 1920x1080 images were visually inspected: all fields and seven command buttons are visible without overlap or clipping. Resolution tests temporarily render the loaded scene Canvas through a camera to 1280x720 and 1920x1080 RenderTextures, checking actual Canvas pixelRect, label height and functional labels; initial, normal, specialty and reset images are saved under Logs/Phase4. Saved scene remains ScreenSpaceOverlay. These are automated camera-render checks, not physical pointer or standalone player validation.

## Trade-off

Nine production C# types replace two independent calculation controllers plus Runtime/Result/Source/Command. Explicit Context and Resolved types, one service and the separated calculation add navigation and model overhead. They centralize current policy edits and expose independently testable arithmetic without adding interfaces, a plugin system or a generic engine. Runtime and Debug adapter UX structure remains deliberately parallel with Direct. Default readonly structs have zero-initialized data; the service always constructs resolved data through the resolver before calculation. Failed calculation propagates an exception without publishing a new result; no rollback or thread-safety guarantees are introduced.

## Regression and Scope

All pre-existing Assets, ProjectSettings and Packages files were hash-compared against Logs/Phase4/baseline.json after Unity execution: no changed or missing files. Phase 1/2 and Direct remain byte-identical. Unity-generated ProjectSettings/SceneTemplateSettings.json was removed after Editor shutdown to restore the original configuration footprint. No Weather, Change Impact experiment, performance benchmark, package changes, rule changes, shared contract edits or Direct refactoring. Phase 5 can compare both options through the existing debug contracts and the equivalence test assembly; future behavior is not implemented here. No staging, commit or push.

## Files Added

- Assets/01.Scripts/ModifierPipeline/: SellPriceCalculationContext.cs, ResolvedSellPriceModifiers.cs, SellPriceModifierResolver.cs, PriceCalculator.cs, PipelinePriceResult.cs, PipelinePriceService.cs, PipelinePriceRuntime.cs, PipelinePriceDebugSource.cs, PipelinePriceDebugCommand.cs, PriceModifierPipeline.Pipeline.asmdef.
- Assets/01.Scripts/ModifierPipeline/Editor/: PipelinePriceSceneBuilder.cs, PriceModifierPipeline.Pipeline.Editor.asmdef.
- Assets/02.Tests/Pipeline/EditMode/: PipelinePolicyTests.cs, PipelinePriceTests.cs, PipelinePriceSceneTests.cs, PriceModifierPipeline.Pipeline.Tests.asmdef.
- Assets/Scenes/02_ModifierPipeline.unity.
- Assets/Docs/Phase4ModifierPipeline.md.
- Corresponding Unity-generated asset and folder metadata.

No pre-existing script was modified. Existing untracked Phase 3 work was preserved. Final tracked diff is empty; Phase 4 files remain untracked, as required. No staging/commit/push.

## Final Verdict

PASS for implementation, all 102 tests, 24-context equivalence and automated Scene/resolution rendering. Physical pointer input, original overlay GameView inspection at both resolutions, and standalone player execution were not performed; the render tests temporarily use ScreenSpaceCamera and do not constitute those checks. Phase 5 comparison validation can proceed with this stated runtime-validation boundary.
