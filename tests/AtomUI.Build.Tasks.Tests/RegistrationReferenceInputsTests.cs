using System.Reflection;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

public class RegistrationReferenceInputsTests
{
    [Fact]
    public void Same_complete_identity_reference_is_shadowed_but_actual_implementation_is_not()
    {
        var path = typeof(RegistrationReferenceInputsTests).Assembly.Location;
        var identity = AssemblyName.GetAssemblyName(path).FullName!;
        RegistrationReferenceInputs.Select([new TestTaskItem(path, ("Kind", "Reference"))], [identity]).ShouldBeEmpty();
        RegistrationReferenceInputs.Select([new TestTaskItem(path)], [identity]).ShouldBe(new[] { path });
    }

    [Theory]
    [InlineData("version")]
    [InlineData("culture")]
    [InlineData("key")]
    public void Same_simple_name_with_different_complete_identity_is_never_shadowed(string difference)
    {
        var path = typeof(RegistrationReferenceInputsTests).Assembly.Location;
        var different = AssemblyName.GetAssemblyName(path);
        switch (difference)
        {
            case "version": different.Version = new Version(99, 0, 0, 0); break;
            case "culture": different.CultureName = "fr"; break;
            case "key": different.SetPublicKeyToken([1, 2, 3, 4, 5, 6, 7, 8]); break;
        }
        RegistrationReferenceInputs.Select([new TestTaskItem(path, ("Kind", "Reference"))], [different.FullName]).ShouldBe(new[] { path });
    }
}
