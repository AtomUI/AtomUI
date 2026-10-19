using AtomUI.Controls;
using AtomUI.Controls.Commons;
using Shouldly;
using Xunit;
using AtomUITag = AtomUI.Desktop.Controls.Tag;

namespace AtomUI.Desktop.Controls.Tests.Tag;

public class TagContractTests
{

    [Fact]
    public void Tag_Defaults_To_Filled()
    {
        var tag = new AtomUITag();

        tag.Variant.ShouldBe(TagVariant.Filled);
    }
}
