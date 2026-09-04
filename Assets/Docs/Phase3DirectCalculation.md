# Controller Direct Calculation

## Architecture

Option 1 is a functional baseline. DirectPreviewController and DirectCommitController independently select and apply price policies. They share only Phase 1 input/enums/constants and the rounding mode; neither calls the other or a shared policy helper. Controllers are plain C# objects owned by DirectPriceRuntime, not extra scene components.

DirectPriceRuntime owns current input, separate preview/commit results and availability, latest result, and LastAction. Commands mutate this state. Source maps it to the unchanged Phase 2 snapshot. The panel refreshes after command execution. No polling is added.

Input defaults: Base 100 / Normal item / Normal season / Short / None. Input changes clear both results and latest applied state. Reset restores defaults and clears results. Commit works before Preview and never reads Preview to calculate its price.

## Policy Locations

Paths below are relative to Assets/01.Scripts/DirectCalculation. Both controller files have the same locations:

- DirectPreviewController.cs:18 and DirectCommitController.cs:18: Normal-only Season/Distance block; LocalSpecialty bypasses both.
- Each controller :21: Season switch; Favored :23, Unfavored :26; Normal retains multiplier 1.
- Each controller :32: Long distance condition.
- Each controller :37: Lucky event condition, outside the item exception block.
- Each controller :40: decimal final rounding once, AwayFromZero, checked int conversion.

## Preview Path

Panel Preview button -> IPriceArchitectureDebugCommand.Preview -> DirectPriceDebugCommand.Preview -> DirectPriceRuntime.Preview -> DirectPreviewController.CalculatePreview(Input) -> DirectPriceResult -> stored PreviewResult/LatestResult -> CaptureSnapshot -> panel Refresh.

## Commit Path

Panel Commit button -> IPriceArchitectureDebugCommand.Commit -> DirectPriceDebugCommand.Commit -> DirectPriceRuntime.Commit -> DirectCommitController.CalculateCommit(Input) -> DirectPriceResult -> stored CommitResult/LatestResult -> CaptureSnapshot -> panel Refresh.

## Duplication Evidence

Four semantic decision groups repeat in the two controllers: item applicability, season selection, distance selection, event selection. Both independently multiply Base -> Season -> Distance -> Event, then round. The identical calculations are intentional baseline duplication, not a candidate for cleanup in this phase. Expected prices are explicitly asserted by tests, not just equality between two potentially wrong implementations.

Applied flags mean the modifier stage executed, including neutral multiplier 1. Normal items therefore show all stages ON after calculation. LocalSpecialty shows Season OFF, Distance OFF, Event ON. Excluded stages carry an effective multiplier of 1.

The latest successful result supplies the single panel applied display. Preview and Commit retain separate complete results. All 24 context tests compare their full results, including flags and multipliers. The unchanged snapshot computes consistency from prices only; it does not expose a separate applied-state warning. Before calculation, bool-only panel contracts display OFF, results are unavailable and consistency is N/A; LastAction explains that no calculation exists. This avoids moving policy into Source or changing the common panel.

## LocalSpecialty Evidence

One item guard per controller excludes both Season and Distance; Event remains outside the guard. Base 100 / Favored / Long / Lucky produces 234 for Normal and 150 for LocalSpecialty in both independent paths.

## Test Result

Executed 2026-09-04 using Unity 6000.3.10f1 in the working project after the user's Editor was closed.

- Phase 1: 20/20 PASS.
- Phase 2: 8/8 PASS.
- Phase 3: 31/31 PASS.
- Total: 59 passed, 0 failed, 0 skipped.
- Phase 3: 24 exhaustive context cases; 3 final-rounding/zero cases; 1 midpoint case; 1 overflow case; 1 runtime command/reset/cycle test; 1 scene PlayMode integration test.
- Scene integration checks initial N/A, independent calculations, intermediate consistency, 234 and 150 examples, applied labels, reset, text preferred-height bounds and re-enable subscription behavior.
- Unity compiled new assemblies successfully; no C# compilation errors.
- Local ignored evidence: Logs/Phase3-build.log, Logs/Phase3-tests.log, Logs/Phase3-tests.xml.

## Runtime Result

Automated Scene PlayMode validation PASS. Separately, Windows computer-use pointer input verified the actual GameView on 2026-09-04:

- 1280x720 (GameView scale 0.52x): initial Direct Calculation / default input / unavailable results / N/A; Preview 100 then Commit 100/PASS; each input button updates the input and clears results; Normal Favored/Long/Lucky 234/234 PASS; LocalSpecialty 150/150 PASS with Season OFF / Distance OFF / Event ON x1.5.
- 1920x1080 Full HD (GameView scale 0.35x): LocalSpecialty 150/150 PASS remains visible, no overlapping or clipped panel text/buttons; physical Reset restores Base 100 / Normal / Normal / Short / None and N/A.
- All seven buttons received actual pointer clicks. Intermediate per-modifier Preview/Commit combinations were additionally covered by the automated Scene test.
- Both resolutions were inspected as scaled Editor GameViews, not standalone player builds or 1:1 monitor pixel captures.
- Play Mode was stopped and the validation Editor closed without saving scene edits. No runtime exceptions or C# errors were found in Logs/Phase3-editor.log; licensing/package diagnostics are not claimed absent.

## Baseline Metrics

- Policy Class Count: 2 (Preview and Commit controllers).
- Policy Location Count: 8 (four decision statements per controller: one item if, one season switch, one distance if, one event if).
- Duplicate Policy Count: 4 semantic policy groups, each present twice (8 occurrences total, 4 additional copies beyond one implementation).
- LocalSpecialty Branch Count: 2 (one Normal guard per controller with an implicit excluded path).

Counting convention: a switch counts as one decision location, not one per case label. Neutral multiplier initialization, multiplication, rounding, display logic and input-toggle commands are not selection decision locations. This convention must also be used for the later Pipeline comparison.

## Assets and Scope

Assets/Scenes/01_DirectCalculation.unity is created through DirectPriceSceneBuilder using Unity APIs. The scene contains a runtime with Source/Command, a camera, Canvas/CanvasScaler, an instance of the existing panel prefab with Source/Command overrides, and Input System EventSystem. Builder refuses overwrite and restores the previously active scene. Unity generates all metadata.

No existing script, Phase 1 contract, Phase 2 panel, prefab, package or project setting is intentionally changed. No Pipeline, shared resolver, Weather, architecture dropdown, performance instrumentation or change-impact experiment is implemented. No staging, commit or push is performed.

Existing Unity package duplicate-assembly/native-extension log messages persist; packages are not changed. The prior Temp validation copy disappeared on Editor shutdown, so successful generation and tests ran in the working project. Failed startup attempts did not constitute validation.

Unity automatically upgraded Assets/Settings/Mobile_RPAsset.asset and generated ProjectSettings/SceneTemplateSettings.json during validation. Both incidental changes were removed after Editor shutdown; final tracked-file diff is empty.
