using PriceModifierPipeline.Common;

namespace PriceModifierPipeline.Comparison
{
    public readonly struct PriceArchitectureComparisonResult
    {
        public SellPriceInput Input { get; }
        public int ExpectedPrice { get; }
        public int DirectPreviewPrice { get; }
        public int DirectCommitPrice { get; }
        public int PipelinePreviewPrice { get; }
        public int PipelineCommitPrice { get; }
        public bool IsDirectConsistent => DirectPreviewPrice == DirectCommitPrice;
        public bool IsPipelineConsistent => PipelinePreviewPrice == PipelineCommitPrice;
        public bool IsCrossOptionEquivalent => IsDirectConsistent && IsPipelineConsistent && DirectPreviewPrice == PipelinePreviewPrice;
        public bool AllEquivalent => IsCrossOptionEquivalent && DirectPreviewPrice == ExpectedPrice;

        public PriceArchitectureComparisonResult(SellPriceInput input, int expectedPrice,
            int directPreview, int directCommit, int pipelinePreview, int pipelineCommit)
        {
            Input = input;
            ExpectedPrice = expectedPrice;
            DirectPreviewPrice = directPreview;
            DirectCommitPrice = directCommit;
            PipelinePreviewPrice = pipelinePreview;
            PipelineCommitPrice = pipelineCommit;
        }
    }
}
