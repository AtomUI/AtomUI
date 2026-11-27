using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Base class for control themes that assemble their animations, styles and control template in code through
/// the hooks that <see cref="Build" /> calls.
/// </summary>
public abstract class BaseControlTheme : ControlTheme
{
    public BaseControlTheme()
    {
    }

    /// <summary>
    /// Creates a theme that targets the controls of type <paramref name="targetType" />.
    /// </summary>
    /// <param name="targetType">
    /// The type of the controls the theme applies to, and the key that <see cref="ThemeResourceKey" /> returns.
    /// </param>
    public BaseControlTheme(Type targetType) : base(targetType)
    {
    }

    /// <summary>
    /// Assembles the theme by calling <see cref="NotifyPreBuild" />, <see cref="BuildThemeAnimations" />,
    /// <see cref="BuildStyles" />, <see cref="BuildTemplateStyle" /> and <see cref="NotifyBuildCompleted" />
    /// in that order.
    /// </summary>
    /// <remarks>
    /// <see cref="BuildInstanceStyles" /> is not part of the sequence. Each call adds the template setter again,
    /// so build a theme instance only once.
    /// </remarks>
    public void Build()
    {
        NotifyPreBuild();
        BuildThemeAnimations();
        BuildStyles();
        BuildTemplateStyle();
        NotifyBuildCompleted();
    }

    /// <summary>
    /// Adds the template returned by <see cref="BuildControlTemplate" /> as the setter of the control template
    /// property. Called by <see cref="Build" /> after <see cref="BuildStyles" />. Call the base member from an
    /// override to keep this setter; nothing is added when the template is <see langword="null" />.
    /// </summary>
    protected virtual void BuildTemplateStyle()
    {
        var template = BuildControlTemplate();
        if (template is not null)
        {
            Add(new Setter(TemplatedControl.TemplateProperty, template));
        }
    }

    /// <summary>
    /// Returns the key that identifies the theme as a resource. The base implementation returns
    /// <c>TargetType</c>, so a theme that has to be found under another key overrides this method.
    /// </summary>
    /// <returns>
    /// The resource key of the theme.
    /// </returns>
    public virtual object ThemeResourceKey()
    {
        Debug.Assert(TargetType is not null);
        return TargetType;
    }

    /// <summary>
    /// Returns the control template of the theme, or <see langword="null" /> when the theme has none. Called by
    /// <see cref="BuildTemplateStyle" />; the base implementation returns <see langword="null" />.
    /// </summary>
    /// <returns>
    /// The control template, or <see langword="null" /> for no template.
    /// </returns>
    protected virtual IControlTemplate? BuildControlTemplate()
    {
        return default;
    }

    /// <summary>
    /// Override to add the animations of the theme. Called by <see cref="Build" /> before
    /// <see cref="BuildStyles" />; the base implementation adds nothing.
    /// </summary>
    protected virtual void BuildThemeAnimations()
    {
    }

    /// <summary>
    /// Override to add the setters and styles of the theme. Called by <see cref="Build" /> after
    /// <see cref="BuildThemeAnimations" /> and before <see cref="BuildTemplateStyle" />; the base implementation
    /// adds nothing.
    /// </summary>
    protected virtual void BuildStyles()
    {
    }

    /// <summary>
    /// Override to add to <paramref name="control" /> the styles that depend on that control instance. It is not
    /// called by <see cref="Build" />; the base implementation adds nothing.
    /// </summary>
    /// <param name="control">
    /// The control instance that receives the styles.
    /// </param>
    protected virtual void BuildInstanceStyles(Control control)
    {
    }

    /// <summary>
    /// Called by <see cref="Build" /> before the build hooks run. The base implementation does nothing.
    /// </summary>
    protected virtual void NotifyPreBuild()
    {
    }

    /// <summary>
    /// Called by <see cref="Build" /> after the build hooks ran. The base implementation does nothing.
    /// </summary>
    protected virtual void NotifyBuildCompleted()
    {
    }
    
    /// <summary>
    /// Binds the styled property <paramref name="property" /> on <paramref name="target" /> to the property
    /// <paramref name="parentProperty" /> of the target's templated parent.
    /// </summary>
    /// <typeparam name="T">
    /// The value type of the bound property.
    /// </typeparam>
    /// <param name="target">
    /// The element in the control template that receives the binding.
    /// </param>
    /// <param name="property">
    /// The property of the target that receives the templated parent's value.
    /// </param>
    /// <param name="parentProperty">
    /// The property that is read on the templated parent.
    /// </param>
    /// <param name="mode">
    /// The mode of the template binding.
    /// </param>
    /// <param name="converter">
    /// Converts the parent value before it reaches the target; none when omitted.
    /// </param>
    /// <returns>
    /// A disposable that removes the binding when it is disposed.
    /// </returns>
    protected static IDisposable CreateTemplateParentBinding<T>(
        AvaloniaObject target,
        StyledProperty<T> property,
        AvaloniaProperty parentProperty,
        BindingMode mode = BindingMode.Default,
        IValueConverter? converter = null)
    {
        return target.Bind(property, new TemplateBinding(parentProperty)
        {
            Mode      = mode,
            Converter = converter
        });
    }

    /// <summary>
    /// Binds the direct property <paramref name="property" /> on <paramref name="target" /> to the property
    /// <paramref name="parentProperty" /> of the target's templated parent.
    /// </summary>
    /// <typeparam name="T">
    /// The value type of the bound property.
    /// </typeparam>
    /// <param name="target">
    /// The element in the control template that receives the binding.
    /// </param>
    /// <param name="property">
    /// The direct property of the target that receives the templated parent's value.
    /// </param>
    /// <param name="parentProperty">
    /// The property that is read on the templated parent.
    /// </param>
    /// <param name="mode">
    /// The mode of the template binding.
    /// </param>
    /// <param name="converter">
    /// Converts the parent value before it reaches the target; none when omitted.
    /// </param>
    /// <returns>
    /// A disposable that removes the binding when it is disposed.
    /// </returns>
    protected static IDisposable CreateTemplateParentBinding<T>(
        AvaloniaObject target,
        DirectPropertyBase<T> property,
        AvaloniaProperty parentProperty,
        BindingMode mode = BindingMode.Default,
        IValueConverter? converter = null)
    {
        return target.Bind(property, new TemplateBinding(parentProperty)
        {
            Mode      = mode,
            Converter = converter
        });
    }

    /// <summary>
    /// Binds the property <paramref name="property" /> on <paramref name="target" /> to the property
    /// <paramref name="parentProperty" /> of the target's templated parent.
    /// </summary>
    /// <param name="target">
    /// The element in the control template that receives the binding.
    /// </param>
    /// <param name="property">
    /// The property of the target that receives the templated parent's value.
    /// </param>
    /// <param name="parentProperty">
    /// The property that is read on the templated parent.
    /// </param>
    /// <param name="mode">
    /// The mode of the template binding.
    /// </param>
    /// <param name="converter">
    /// Converts the parent value before it reaches the target; none when omitted.
    /// </param>
    /// <returns>
    /// A disposable that removes the binding when it is disposed.
    /// </returns>
    protected static IDisposable CreateTemplateParentBinding(
        AvaloniaObject target,
        AvaloniaProperty property,
        AvaloniaProperty parentProperty,
        BindingMode mode = BindingMode.Default,
        IValueConverter? converter = null)
    {
        return target.Bind(property, new TemplateBinding(parentProperty)
        {
            Mode      = mode,
            Converter = converter
        });
    }

    /// <summary>
    /// Binds the property <paramref name="property" /> on <paramref name="target" /> to the property
    /// <paramref name="parentProperty" /> of the target's templated parent. The <paramref name="priority" />
    /// argument is not applied.
    /// </summary>
    /// <param name="target">
    /// The element in the control template that receives the binding.
    /// </param>
    /// <param name="property">
    /// The property of the target that receives the templated parent's value.
    /// </param>
    /// <param name="parentProperty">
    /// The property that is read on the templated parent.
    /// </param>
    /// <param name="priority">
    /// Has no effect: the value is not passed on, and the binding gets the default priority.
    /// </param>
    /// <param name="mode">
    /// The mode of the template binding.
    /// </param>
    /// <param name="converter">
    /// Converts the parent value before it reaches the target; none when omitted.
    /// </param>
    /// <returns>
    /// A disposable that removes the binding when it is disposed.
    /// </returns>
    protected static IDisposable CreateTemplateParentBinding(AvaloniaObject target, AvaloniaProperty property,
                                                             AvaloniaProperty parentProperty,
                                                             BindingPriority priority,
                                                             BindingMode mode = BindingMode.Default,
                                                             IValueConverter? converter = null)
    {
        return target.Bind(property, new TemplateBinding(parentProperty)
        {
            Mode      = mode,
            Converter = converter
        });
    }
    
    /// <summary>
    /// Binds the styled property <paramref name="property" /> on <paramref name="target" /> to the property
    /// <paramref name="parentProperty" /> of the target's templated parent. The <paramref name="priority" />
    /// argument is not applied.
    /// </summary>
    /// <typeparam name="T">
    /// The value type of the bound property.
    /// </typeparam>
    /// <param name="target">
    /// The element in the control template that receives the binding.
    /// </param>
    /// <param name="property">
    /// The property of the target that receives the templated parent's value.
    /// </param>
    /// <param name="parentProperty">
    /// The property that is read on the templated parent.
    /// </param>
    /// <param name="priority">
    /// Has no effect: the value is not passed on, and the binding gets the default priority.
    /// </param>
    /// <param name="mode">
    /// The mode of the template binding.
    /// </param>
    /// <param name="converter">
    /// Converts the parent value before it reaches the target; none when omitted.
    /// </param>
    /// <returns>
    /// A disposable that removes the binding when it is disposed.
    /// </returns>
    protected static IDisposable CreateTemplateParentBinding<T>(AvaloniaObject target, StyledProperty<T> property,
                                                                AvaloniaProperty parentProperty,
                                                                BindingPriority priority,
                                                                BindingMode mode = BindingMode.Default,
                                                                IValueConverter? converter = null)
    {
        return target.Bind(property, new TemplateBinding(parentProperty)
        {
            Mode      = mode,
            Converter = converter
        });
    }
    
    /// <summary>
    /// Binds the direct property <paramref name="property" /> on <paramref name="target" /> to the property
    /// <paramref name="parentProperty" /> of the target's templated parent. The <paramref name="priority" />
    /// argument is not applied.
    /// </summary>
    /// <typeparam name="T">
    /// The value type of the bound property.
    /// </typeparam>
    /// <param name="target">
    /// The element in the control template that receives the binding.
    /// </param>
    /// <param name="property">
    /// The direct property of the target that receives the templated parent's value.
    /// </param>
    /// <param name="parentProperty">
    /// The property that is read on the templated parent.
    /// </param>
    /// <param name="priority">
    /// Has no effect: the value is not passed on, and the binding gets the default priority.
    /// </param>
    /// <param name="mode">
    /// The mode of the template binding.
    /// </param>
    /// <param name="converter">
    /// Converts the parent value before it reaches the target; none when omitted.
    /// </param>
    /// <returns>
    /// A disposable that removes the binding when it is disposed.
    /// </returns>
    protected static IDisposable CreateTemplateParentBinding<T>(AvaloniaObject target, DirectPropertyBase<T> property,
                                                                AvaloniaProperty parentProperty,
                                                                BindingPriority priority,
                                                                BindingMode mode = BindingMode.Default,
                                                                IValueConverter? converter = null)
    {
        return target.Bind(property, new TemplateBinding(parentProperty)
        {
            Mode      = mode,
            Converter = converter
        });
    }
}
