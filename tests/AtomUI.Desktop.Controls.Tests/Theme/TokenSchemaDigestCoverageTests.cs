using System.Reflection;
using AtomUI.Theme.Schema;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

/// <summary>The Ant Design value packages record the token schema digest of the control packages they derive (spec 3.3).</summary>
public class TokenSchemaDigestCoverageTests
{
    [Theory]
    [InlineData(typeof(AtomUI.Desktop.Controls.Themes.AntDesign.AntDesignDesktopControlsExtensions), typeof(Button))]
    [InlineData(typeof(AtomUI.Desktop.Controls.Themes.AntDesign.AntDesignColorPickerExtensions), typeof(AtomUI.Desktop.Controls.ColorPicker))]
    [InlineData(typeof(AtomUI.Desktop.Controls.Themes.AntDesign.AntDesignDataGridExtensions), typeof(AtomUI.Desktop.Controls.DataGrid))]
    [InlineData(typeof(AtomUI.Controls.Themes.AntDesign.AntDesignCommonControlsExtensions), typeof(AtomUI.Controls.IconToken))]
    public void Value_Package_Records_The_Loaded_Control_Package_Digest(Type valuePackageType, Type controlType)
    {
        var controlPackage = controlType.Assembly;
        var digest = controlPackage.GetCustomAttribute<TokenSchemaDigestAttribute>().ShouldNotBeNull().Digest;
        digest.Length.ShouldBe(64);

        var covers = valuePackageType.Assembly.GetCustomAttributes<CoversTokenSchemaAttribute>().ShouldHaveSingleItem();
        covers.AssemblyName.ShouldBe(controlPackage.GetName().Name);
        covers.Digest.ShouldBe(digest);
    }
}
