using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AtomUI.Controls.Data;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.DataMemberAccessors;

public class DataGridDataMemberAccessorsTests
{
    [Fact]
    public void GeneratedAccessorsExposeDataGridMetadata()
    {
        DataMemberAccessorRegistry.TryGet(typeof(MetadataRow), out var descriptor).ShouldBeTrue();
        var descriptorMetadata = descriptor.ShouldBeAssignableTo<IDataMemberAccessorDescriptorMetadataProvider>();
        descriptorMetadata.IsDataTypeReadOnly.ShouldBeTrue();

        var visibleAccessor = descriptor.Accessors.Single(accessor => accessor.Path == nameof(MetadataRow.VisibleScore));
        var visibleMetadata = visibleAccessor.ShouldBeAssignableTo<IDataMemberAccessorMetadataProvider>().Metadata;
        visibleAccessor.ValueType.ShouldBe(typeof(int));
        visibleAccessor.CanWrite.ShouldBeTrue();
        visibleMetadata.DisplayName.ShouldBe("Visible score");
        visibleMetadata.AutoGenerateField.ShouldBe(true);
        visibleMetadata.Order.ShouldBe(2);
        visibleMetadata.IsReadOnly.ShouldBeFalse();
        visibleMetadata.IsEditable.ShouldBeTrue();

        var hiddenMetadata = descriptor.Accessors.Single(accessor => accessor.Path == nameof(MetadataRow.HiddenScore))
                                       .ShouldBeAssignableTo<IDataMemberAccessorMetadataProvider>()
                                       .Metadata;
        hiddenMetadata.AutoGenerateField.ShouldBe(false);

        var readOnlyMetadata = descriptor.Accessors.Single(accessor => accessor.Path == nameof(MetadataRow.ReadOnlyScore))
                                         .ShouldBeAssignableTo<IDataMemberAccessorMetadataProvider>()
                                         .Metadata;
        readOnlyMetadata.IsReadOnly.ShouldBeTrue();

        var nonEditableMetadata = descriptor.Accessors.Single(accessor => accessor.Path == nameof(MetadataRow.NonEditableScore))
                                            .ShouldBeAssignableTo<IDataMemberAccessorMetadataProvider>()
                                            .Metadata;
        nonEditableMetadata.IsEditable.ShouldBeFalse();
    }

    [Fact]
    public void LocalSourceSchemaIsAvailableForAnEmptyTypedSource()
    {
        var field = new DataGridFieldId("visible-score");
        var descriptor = DataGridLocalSourceDescriptor.For<MetadataRow>(
                static row => DataGridRowKey.FromInt64(row.VisibleScore))
            .Field(field, static row => row.VisibleScore);
        using var source = DataGridLocalSource.Create(Array.Empty<MetadataRow>(), descriptor);

        source.Schema.ItemType.ShouldBe(typeof(MetadataRow));
        source.Schema.Fields.Length.ShouldBe(1);
        source.Schema.Fields[0].Id.ShouldBe(field);
        source.Schema.Fields[0].ValueType.ShouldBe(typeof(int));
        var displayAccessor = source.Schema.Fields[0].DisplayAccessor.ShouldNotBeNull();
        displayAccessor.ItemType.ShouldBe(typeof(MetadataRow));
        displayAccessor.ValueType.ShouldBe(typeof(int));
        displayAccessor.PropertyChangedName.ShouldBeNull();
        displayAccessor.GetValue(new MetadataRow { VisibleScore = 42 }).ShouldBe(42);
    }

    [Fact]
    public void FieldIdIsNotUsedAsAPropertyChangedName()
    {
        var field = new DataGridFieldId("protocol-visible-score");
        var descriptor = DataGridLocalSourceDescriptor.For<MetadataRow>(
                static row => DataGridRowKey.FromInt64(row.VisibleScore))
            .Field(
                field,
                static row => row.VisibleScore,
                propertyChangedName: nameof(MetadataRow.VisibleScore));
        using var source = DataGridLocalSource.Create(Array.Empty<MetadataRow>(), descriptor);

        var displayAccessor = source.Schema.Fields[0].DisplayAccessor.ShouldNotBeNull();
        displayAccessor.PropertyChangedName.ShouldBe(nameof(MetadataRow.VisibleScore));
        displayAccessor.PropertyChangedName.ShouldNotBe(field.Value);
    }

    [GenerateDataMemberAccessors]
    [ReadOnly(true)]
    public partial class MetadataRow
    {
        [Display(ShortName = "Visible score", AutoGenerateField = true, Order = 2)]
        public int VisibleScore { get; set; }

        [Display(AutoGenerateField = false)]
        public int HiddenScore { get; set; }

        [ReadOnly(true)]
        public int ReadOnlyScore { get; set; }

        [Editable(false)]
        public int NonEditableScore { get; set; }
    }
}
