using System.Collections.Generic;
using Internal.IL;
using Internal.TypeSystem;

namespace ILCompiler;

// Use the pinned compiler's existing substitution, branch and EH processing.
// The getter identity is admitted only after TrimmedSwitchTemplate validates its IL.
internal sealed class RegistrationFeatureSwitchManager : FeatureSwitchManager
{
    private readonly MethodDesc _trimmedGetter;
    private static readonly BodySubstitution TrimmedValue = BodySubstitution.Create(1);

    public RegistrationFeatureSwitchManager(ILProvider inner, Logger logger,
        IReadOnlyDictionary<string, bool> featureSwitches, BodyAndFieldSubstitutions substitutions,
        MethodDesc trimmedGetter) : base(inner, logger, featureSwitches, substitutions)
    {
        _trimmedGetter = trimmedGetter;
    }

    protected override BodySubstitution GetSubstitution(MethodDesc method)
    {
        return method == _trimmedGetter ? TrimmedValue : base.GetSubstitution(method);
    }
}
