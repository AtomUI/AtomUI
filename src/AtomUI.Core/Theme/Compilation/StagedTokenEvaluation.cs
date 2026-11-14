using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

/// <summary>
/// One algorithm's part of an optional stage: its identity and the call that runs its stage on the token.
/// </summary>
internal readonly record struct StagedAlgorithmStep(ThemeAlgorithm Algorithm, Action<AbstractDesignToken> Evaluate);

/// <summary>
/// Runs an optional algorithm stage with the three-step flow of spec 4.3, so that algorithm writes take priority
/// over the Ant Design default calculation while still driving the defaults derived from them, and explicit theme
/// values keep the last word. Shared by the Alias stage and the control Own-token stage.
/// </summary>
internal static class StagedTokenEvaluation
{
    internal const int MaxRounds = 3;

    /// <summary>
    /// Runs the stage on <paramref name="token"/>.
    /// </summary>
    /// <remarks>
    /// Step 1 marks and writes the explicit values, runs the default calculation and writes the explicit values
    /// again; with no <paramref name="steps"/> nothing else runs and nothing is allocated here. Step 2 runs the
    /// steps with write recording on and checks every recorded name against the stage. When something was written,
    /// step 3 marks the written tokens explicit, writes the explicit values, reruns the default calculation, runs
    /// the steps again and writes the explicit values a last time. Step 3 repeats while the written tokens or their
    /// values differ from the previous run, at most <see cref="MaxRounds"/> times. The marks are reset on every exit.
    /// </remarks>
    /// <exception cref="ThemeAlgorithmContractException">
    /// An algorithm wrote a token outside the stage, did not stabilize, or threw.
    /// </exception>
    internal static void Run(
        AbstractDesignToken token,
        string stage,
        Action markAndApplyExplicit,
        Action runDefault,
        Action applyExplicit,
        Action resetMarks,
        IReadOnlyList<StagedAlgorithmStep> steps,
        Func<string, TokenDescriptor?> resolveToken,
        Func<TokenDescriptor, bool> isWritableInStage,
        Action<TokenDescriptor> markAlgorithmWritten)
    {
        try
        {
            markAndApplyExplicit();
            runDefault();
            applyExplicit();
            if (steps.Count == 0)
            {
                return;
            }

            var previous = RunSteps(token, stage, steps, resolveToken, isWritableInStage);
            if (previous.Count == 0)
            {
                return;
            }

            for (var round = 1;; round++)
            {
                foreach (var write in previous.Writes)
                {
                    markAlgorithmWritten(write.Descriptor);
                }

                // The steps may have overwritten explicit values; the defaults must derive from the explicit ones.
                applyExplicit();
                runDefault();
                var current = RunSteps(token, stage, steps, resolveToken, isWritableInStage);
                applyExplicit();
                if (current.FindDifference(previous) is not { } unstable)
                {
                    return;
                }

                if (round == MaxRounds)
                {
                    throw new ThemeAlgorithmContractException(
                        unstable.Writer,
                        unstable.Descriptor.Name,
                        $"Algorithm '{unstable.Writer}' did not stabilize after {MaxRounds} rounds; " +
                        $"unstable token '{unstable.Descriptor.Name}'.");
                }

                previous = current;
            }
        }
        finally
        {
            resetMarks();
        }
    }

    private static StageWrites RunSteps(
        AbstractDesignToken token,
        string stage,
        IReadOnlyList<StagedAlgorithmStep> steps,
        Func<string, TokenDescriptor?> resolveToken,
        Func<TokenDescriptor, bool> isWritableInStage)
    {
        var writes = new StageWrites();
        for (var index = 0; index < steps.Count; index++)
        {
            var step = steps[index];
            IReadOnlyCollection<string> names;
            token.BeginRecordingWrites();
            try
            {
                step.Evaluate(token);
            }
            catch (Exception exception)
            {
                throw new ThemeAlgorithmContractException(
                    step.Algorithm,
                    null,
                    $"Algorithm '{step.Algorithm}' failed in the {stage} stage.",
                    exception);
            }
            finally
            {
                // Always stop recording: a token left recording would make the next Begin throw and hide the error.
                names = token.EndRecordingWrites();
            }

            foreach (var name in names)
            {
                var descriptor = resolveToken(name);
                if (descriptor is null || !isWritableInStage(descriptor))
                {
                    throw new ThemeAlgorithmContractException(
                        step.Algorithm,
                        name,
                        $"Algorithm '{step.Algorithm}' wrote token '{name}' outside the {stage} stage.");
                }

                writes.Add(descriptor, step.Algorithm);
            }
        }

        writes.CaptureValues(token);
        return writes;
    }

    private readonly record struct StageWrite(TokenDescriptor Descriptor, ThemeAlgorithm Writer, object? Value);

    /// <summary>
    /// The tokens written by one run of all steps, with the last algorithm that wrote each and the value it holds
    /// after the run.
    /// </summary>
    private sealed class StageWrites
    {
        private readonly List<StageWrite> _writes = new();
        private readonly Dictionary<TokenDescriptor, int> _indexes = new(ReferenceEqualityComparer.Instance);

        internal int Count => _writes.Count;

        internal IReadOnlyList<StageWrite> Writes => _writes;

        internal void Add(TokenDescriptor descriptor, ThemeAlgorithm writer)
        {
            if (_indexes.TryGetValue(descriptor, out var index))
            {
                _writes[index] = _writes[index] with { Writer = writer };
                return;
            }

            _indexes.Add(descriptor, _writes.Count);
            _writes.Add(new StageWrite(descriptor, writer, null));
        }

        internal void CaptureValues(AbstractDesignToken token)
        {
            for (var index = 0; index < _writes.Count; index++)
            {
                var descriptor = _writes[index].Descriptor;
                _writes[index] = _writes[index] with
                {
                    Value = descriptor.CopyValue(descriptor.GetValue(token))
                };
            }
        }

        /// <summary>
        /// Returns the first write of this run whose token was not written by <paramref name="previous"/> or whose
        /// value differs from it, or a write of <paramref name="previous"/> this run no longer makes.
        /// </summary>
        internal StageWrite? FindDifference(StageWrites previous)
        {
            foreach (var write in _writes)
            {
                if (!previous._indexes.TryGetValue(write.Descriptor, out var index) ||
                    !ValueEquals(write.Descriptor, write.Value, previous._writes[index].Value))
                {
                    return write;
                }
            }

            foreach (var write in previous._writes)
            {
                if (!_indexes.ContainsKey(write.Descriptor))
                {
                    return write;
                }
            }

            return null;
        }

        private static bool ValueEquals(TokenDescriptor descriptor, object? left, object? right)
        {
            if (Equals(left, right))
            {
                return true;
            }

            // Reference-typed values without value equality compare by their canonical text.
            return left is not null && right is not null &&
                   string.Equals(descriptor.Format(left), descriptor.Format(right), StringComparison.Ordinal);
        }
    }
}
