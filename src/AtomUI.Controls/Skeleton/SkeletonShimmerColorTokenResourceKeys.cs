namespace AtomUI.Controls;

/// <summary>
/// The resource keys of the two Tokens that give the shimmer of a skeleton its start and end color;
/// <c>ShimmerColorTokenResourceKeys</c> of the skeleton Tokens in AtomUI.Desktop.Controls exposes them.
/// </summary>
/// <remarks>
/// <c>From</c> and <c>To</c> are Token resource keys, not colors.
/// </remarks>
internal sealed record SkeletonShimmerColorTokenResourceKeys(object From, object To);
