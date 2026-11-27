using System.Runtime.CompilerServices;
using Avalonia.LogicalTree;

namespace AtomUI.Controls;

/// <summary>
/// Extracted from Avalonia 12 internal API for radio button group management.
/// What <see cref="RadioButtonGroupManager" /> reads from and writes to a radio button of a group, so that only one
/// button of the group stays checked.
/// </summary>
internal interface IRadioButton : ILogical
{
    string? GroupName { get; }
    ItemToggleType ToggleType { get; }
    bool IsChecked { get; set; }
}

/// <summary>
/// Extracted from Avalonia 12 internal API for radio button group management.
/// Ensures mutual exclusivity of the radio buttons of a group. Each visual root has its own manager, created by
/// <see cref="GetOrCreateForRoot" />, so a group name is scoped to one root; a <see langword="null" /> root shares
/// one default manager.
/// </summary>
internal class RadioButtonGroupManager
{
    private static readonly RadioButtonGroupManager s_default = new();
    private static readonly ConditionalWeakTable<object, RadioButtonGroupManager> s_registeredVisualRoots = new();

    private readonly Dictionary<string, List<WeakReference<IRadioButton>>> _registeredGroups = new();
    private bool _ignoreCheckedChanges;

    public static RadioButtonGroupManager GetOrCreateForRoot(object? root)
    {
        if (root == null)
        {
            return s_default;
        }
        return s_registeredVisualRoots.GetValue(root, key => new RadioButtonGroupManager());
    }

    public void Add(IRadioButton radioButton)
    {
        var groupName = radioButton.GroupName;
        if (groupName is not null && radioButton.ToggleType == ItemToggleType.Radio)
        {
            if (!_registeredGroups.TryGetValue(groupName, out var group))
            {
                group = new List<WeakReference<IRadioButton>>(1);
                _registeredGroups.Add(groupName, group);
            }

            group.Add(new WeakReference<IRadioButton>(radioButton));
        }
    }

    public void Remove(IRadioButton radioButton, string? oldGroupName)
    {
        if (!string.IsNullOrEmpty(oldGroupName) && _registeredGroups.TryGetValue(oldGroupName, out var group))
        {
            int i = 0;
            while (i < group.Count)
            {
                if (!group[i].TryGetTarget(out var button) || button == radioButton)
                {
                    group.RemoveAt(i);
                    continue;
                }

                i++;
            }

            if (group.Count == 0)
            {
                _registeredGroups.Remove(oldGroupName);
            }
        }
    }

    public void OnCheckedChanged(IRadioButton radioButton)
    {
        if (_ignoreCheckedChanges || radioButton.ToggleType != ItemToggleType.Radio)
        {
            return;
        }

        _ignoreCheckedChanges = true;
        try
        {
            var groupName = radioButton.GroupName;
            if (!string.IsNullOrEmpty(groupName))
            {
                if (_registeredGroups.TryGetValue(groupName, out var group))
                {
                    var i = 0;
                    while (i < group.Count)
                    {
                        if (!group[i].TryGetTarget(out var current))
                        {
                            group.RemoveAt(i);
                            continue;
                        }

                        if (current != radioButton && current.IsChecked)
                        {
                            current.IsChecked = false;
                        }
                        i++;
                    }

                    if (group.Count == 0)
                    {
                        _registeredGroups.Remove(groupName);
                    }

                    var parent = radioButton.LogicalParent as IRadioButton;
                    while (parent is not null && parent.GroupName == groupName)
                    {
                        parent.IsChecked = true;
                        parent = parent.LogicalParent as IRadioButton;
                    }
                }
            }
            else
            {
                if (radioButton.LogicalParent is { } parent)
                {
                    foreach (var sibling in parent.LogicalChildren)
                    {
                        if (sibling != radioButton
                            && sibling is IRadioButton { ToggleType: ItemToggleType.Radio } button
                            && string.IsNullOrEmpty(button.GroupName)
                            && button.IsChecked)
                        {
                            button.IsChecked = false;
                        }
                    }
                }
            }
        }
        finally
        {
            _ignoreCheckedChanges = false;
        }
    }
}
