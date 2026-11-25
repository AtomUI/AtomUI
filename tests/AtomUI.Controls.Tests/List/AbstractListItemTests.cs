using AtomUI.Controls.Commons;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.List;

public class AbstractListItemTests
{
    static AbstractListItemTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private sealed class TestListItem : AbstractListItem
    {
        public IEnumerable<ILogical> Logicals => ((ILogical)this).LogicalChildren;
    }

    private static bool Has(TestListItem item, string pseudoClass)
    {
        return item.Classes.Contains(pseudoClass);
    }

    [Fact]
    public void Defaults_are_null()
    {
        var item = new TestListItem();

        item.Prefix.ShouldBeNull();
        item.Title.ShouldBeNull();
        item.Description.ShouldBeNull();
        item.Extra.ShouldBeNull();
        Has(item, AbstractListItem.HasPrefixPseudoClass).ShouldBeFalse();
        Has(item, AbstractListItem.HasTitlePseudoClass).ShouldBeFalse();
        Has(item, AbstractListItem.HasDescriptionPseudoClass).ShouldBeFalse();
        Has(item, AbstractListItem.HasExtraPseudoClass).ShouldBeFalse();
        item.Logicals.ShouldBeEmpty();
    }

    [Fact]
    public void Pseudo_class_constants_have_expected_values()
    {
        AbstractListItem.HasPrefixPseudoClass.ShouldBe(":has-prefix");
        AbstractListItem.HasTitlePseudoClass.ShouldBe(":has-title");
        AbstractListItem.HasDescriptionPseudoClass.ShouldBe(":has-description");
        AbstractListItem.HasExtraPseudoClass.ShouldBe(":has-extra");
    }

    [Fact]
    public void Pseudo_classes_follow_values()
    {
        var item = new TestListItem();

        item.Prefix = "p";
        Has(item, AbstractListItem.HasPrefixPseudoClass).ShouldBeTrue();
        Has(item, AbstractListItem.HasTitlePseudoClass).ShouldBeFalse();
        Has(item, AbstractListItem.HasDescriptionPseudoClass).ShouldBeFalse();
        Has(item, AbstractListItem.HasExtraPseudoClass).ShouldBeFalse();

        item.Title = "t";
        item.Description = "d";
        item.Extra = "e";
        Has(item, AbstractListItem.HasPrefixPseudoClass).ShouldBeTrue();
        Has(item, AbstractListItem.HasTitlePseudoClass).ShouldBeTrue();
        Has(item, AbstractListItem.HasDescriptionPseudoClass).ShouldBeTrue();
        Has(item, AbstractListItem.HasExtraPseudoClass).ShouldBeTrue();

        item.Title = null;
        Has(item, AbstractListItem.HasPrefixPseudoClass).ShouldBeTrue();
        Has(item, AbstractListItem.HasTitlePseudoClass).ShouldBeFalse();
        Has(item, AbstractListItem.HasDescriptionPseudoClass).ShouldBeTrue();
        Has(item, AbstractListItem.HasExtraPseudoClass).ShouldBeTrue();

        item.Prefix = null;
        item.Extra = null;
        Has(item, AbstractListItem.HasPrefixPseudoClass).ShouldBeFalse();
        Has(item, AbstractListItem.HasDescriptionPseudoClass).ShouldBeTrue();
        Has(item, AbstractListItem.HasExtraPseudoClass).ShouldBeFalse();

        item.Description = null;
        Has(item, AbstractListItem.HasDescriptionPseudoClass).ShouldBeFalse();
    }

    [Fact]
    public void Control_values_become_logical_children()
    {
        var item = new TestListItem();
        var prefix = new Border();
        var title = new TextBlock();
        var description = new TextBlock();
        var extra = new Border();

        item.Prefix = prefix;
        item.Title = title;
        item.Description = description;
        item.Extra = extra;

        item.Logicals.ShouldBe(new ILogical[] { prefix, title, description, extra }, ignoreOrder: true);

        var replacement = new Border();
        item.Prefix = replacement;
        item.Logicals.ShouldNotContain(prefix);
        item.Logicals.ShouldContain(replacement);

        item.Title = "text";
        item.Logicals.ShouldNotContain(title);

        item.Description = null;
        item.Extra = null;
        item.Prefix = null;
        item.Logicals.ShouldBeEmpty();
    }

    [Fact]
    public void Plain_values_are_not_logical_children()
    {
        var item = new TestListItem { Prefix = "p", Title = 1, Description = "d", Extra = 2.5 };

        item.Logicals.ShouldBeEmpty();
    }
}
