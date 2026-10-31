// Polyfills for netstandard2.1 only. net8.0 already ships these
// compiler types; defining them unconditionally would clash.
// They exist so `init` accessors, `record struct`, and `required`
// members compile to the same IL for both targets, with no
// behaviour change on either target. These are compiler-only
// types: they are never referenced by game code directly.
#if NETSTANDARD2_1
namespace System.Runtime.CompilerServices
{
    internal sealed class IsExternalInit
    {
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        public string FeatureName { get; }
    }
}
#endif
