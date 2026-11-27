using System.Collections;
using Avalonia.Collections;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;

namespace AtomUI.Controls;

/// <summary>
/// A XAML markup extension that provides the validator list for the
/// <see cref="AbstractFormItem.Validators" /> property of a form item. It is written as the content of the
/// property element, and its child elements are the validators.
/// </summary>
public class FormValidatorProvider : MarkupExtension, IList<IFormValidator>
{
    private readonly AvaloniaList<IFormValidator> _items = new()
    {
        ResetBehavior = ResetBehavior.Remove
    };

    /// <summary>
    /// The list that receives the validators declared as XAML content. It is the same instance that
    /// <see cref="ProvideValue(System.IServiceProvider)" /> returns.
    /// </summary>
    [Content]
    public IList Items => _items;

    /// <inheritdoc />
    public int Count => _items.Count;
    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public IFormValidator this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    /// <inheritdoc />
    public void Add(IFormValidator item)
    {
        _items.Add(item);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _items.Clear();
    }

    /// <inheritdoc />
    public bool Contains(IFormValidator item)
    {
        return _items.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(IFormValidator[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public IEnumerator<IFormValidator> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    /// <inheritdoc />
    public int IndexOf(IFormValidator item)
    {
        return _items.IndexOf(item);
    }

    /// <inheritdoc />
    public void Insert(int index, IFormValidator item)
    {
        _items.Insert(index, item);
    }

    /// <inheritdoc />
    public bool Remove(IFormValidator item)
    {
        return _items.Remove(item);
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        _items.RemoveAt(index);
    }

    /// <summary>
    /// Returns the list that holds the validators declared as content, so that the property the extension is
    /// assigned to receives them. Every call on the same provider returns the same list instance.
    /// </summary>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _items;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
