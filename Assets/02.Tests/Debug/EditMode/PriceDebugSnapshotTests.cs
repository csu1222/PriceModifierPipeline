using NUnit.Framework;
using PriceModifierPipeline.Common;
using PriceModifierPipeline.Debugging;

namespace PriceModifierPipeline.Tests
{
    public class PriceDebugSnapshotTests
    {
        [TestCase(true, true, 234, 234, PriceConsistencyState.Pass)]
        [TestCase(true, true, 234, 235, PriceConsistencyState.Fail)]
        [TestCase(false, true, 234, 234, PriceConsistencyState.NotEvaluated)]
        [TestCase(true, false, 234, 234, PriceConsistencyState.NotEvaluated)]
        [TestCase(false, false, 234, 234, PriceConsistencyState.NotEvaluated)]
        public void ConsistencyRequiresBothResults(bool hasPreview, bool hasCommit,
            int preview, int commit, PriceConsistencyState expected)
        {
            var snapshot = new PriceDebugSnapshot("Test", new SellPriceInput(100, TradeItemType.Normal,
                SeasonPriceState.Favored, DistancePriceState.Long, TradeEventState.Lucky),
                1.2m, 1.3m, 1.5m, true, true, true, preview, commit, hasPreview, hasCommit, "Test");
            Assert.That(snapshot.ConsistencyState, Is.EqualTo(expected));
        }

        [TestCase(false, false, true)]
        [TestCase(true, true, false)]
        public void SnapshotPreservesSuppliedFlagsWithoutApplyingItemPolicy(bool season, bool distance, bool tradeEvent)
        {
            var snapshot = new PriceDebugSnapshot("Test", new SellPriceInput(100, TradeItemType.LocalSpecialty,
                SeasonPriceState.Favored, DistancePriceState.Long, TradeEventState.Lucky),
                1.2m, 1.3m, 1.5m, season, distance, tradeEvent, 150, 150, true, true, "Fixture");
            Assert.That(snapshot.SeasonApplied, Is.EqualTo(season));
            Assert.That(snapshot.DistanceApplied, Is.EqualTo(distance));
            Assert.That(snapshot.EventApplied, Is.EqualTo(tradeEvent));
            Assert.That(snapshot.SeasonMultiplier, Is.EqualTo(1.2m));
            Assert.That(snapshot.DistanceMultiplier, Is.EqualTo(1.3m));
            Assert.That(snapshot.EventMultiplier, Is.EqualTo(1.5m));
        }
    }
}
