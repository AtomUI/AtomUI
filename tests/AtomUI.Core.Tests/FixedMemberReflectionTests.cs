using AtomUI.Reflection;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests;

public class FixedMemberReflectionTests
{
    [Fact]
    public void Validators_Return_Resolved_Members()
    {
        var method = typeof(string).GetMethod(nameof(string.ToString), Type.EmptyTypes);
        var field = typeof(string).GetField(nameof(string.Empty));
        var property = typeof(string).GetProperty(nameof(string.Length));
        var eventInfo = typeof(AppDomain).GetEvent(nameof(AppDomain.AssemblyLoad));

        FixedMemberReflection.RequireMethod(method, typeof(string), nameof(string.ToString)).ShouldBeSameAs(method);
        FixedMemberReflection.RequireField(field, typeof(string), nameof(string.Empty)).ShouldBeSameAs(field);
        FixedMemberReflection.RequireProperty(property, typeof(string), nameof(string.Length)).ShouldBeSameAs(property);
        FixedMemberReflection.RequireEvent(eventInfo, typeof(AppDomain), nameof(AppDomain.AssemblyLoad)).ShouldBeSameAs(eventInfo);
    }

    [Fact]
    public void Validators_Throw_The_Member_Specific_Exception_With_Context()
    {
        Should.Throw<MissingMethodException>(() =>
                FixedMemberReflection.RequireMethod(null, typeof(string), "MissingMethod"))
            .Message.ShouldContain("System.String.MissingMethod");
        Should.Throw<MissingFieldException>(() =>
                FixedMemberReflection.RequireField(null, typeof(string), "MissingField"))
            .Message.ShouldContain("System.String.MissingField");
        Should.Throw<MissingMemberException>(() =>
                FixedMemberReflection.RequireProperty(null, typeof(string), "MissingProperty"))
            .Message.ShouldContain("System.String.MissingProperty");
        Should.Throw<MissingMemberException>(() =>
                FixedMemberReflection.RequireEvent(null, typeof(string), "MissingEvent"))
            .Message.ShouldContain("System.String.MissingEvent");
    }
}
