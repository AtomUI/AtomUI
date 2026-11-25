using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.ControlBaseline;

/// <summary>
/// Lists the style frames applied to every element of one control instance: how many there are, how many are active,
/// and which selectors produce them. Every applied frame costs a style instance and activators whether or not it is
/// active, so this shows where a control's styling cost comes from.
/// </summary>
internal static class FrameReport
{
    public static int Run(IReadOnlyList<Type> types, Func<Type, Control> create, Avalonia.Size measureSize)
    {
        foreach (var type in types)
        {
            var control = create(type);
            var window  = new Window { Width = measureSize.Width, Height = 600, Content = control };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var bySelector = new Dictionary<string, (int Frames, int Active, int Values)>(StringComparer.Ordinal);
            var byElement  = new Dictionary<string, (int Elements, int Frames, int Active, int Values)>(StringComparer.Ordinal);
            var totalFrames = 0;
            var totalActive = 0;
            foreach (var visual in control.GetSelfAndVisualDescendants().OfType<StyledElement>())
            {
                var diagnostic = visual.GetValueStoreDiagnostic();
                var element    = visual.GetType().Name + (string.IsNullOrEmpty(visual.Name) ? "" : "#" + visual.Name);
                var e          = byElement.GetValueOrDefault(element);
                e.Elements++;
                foreach (var frame in diagnostic.AppliedFrames)
                {
                    if (frame.Type is not (IValueFrameDiagnostic.FrameType.Style or IValueFrameDiagnostic.FrameType.Theme))
                    {
                        continue;
                    }

                    var values   = frame.Values.Count();
                    var selector = frame.Source is Style style ? style.Selector?.ToString() ?? "(no selector)" : frame.Source?.ToString() ?? "?";
                    var s        = bySelector.GetValueOrDefault(selector);
                    bySelector[selector] = (s.Frames + 1, s.Active + (frame.IsActive ? 1 : 0), s.Values + values);
                    e.Frames++;
                    e.Values += values;
                    if (frame.IsActive)
                    {
                        e.Active++;
                        totalActive++;
                    }

                    totalFrames++;
                }

                byElement[element] = e;
            }

            Console.WriteLine($"== {type.Name}: style frames {totalFrames} (active {totalActive}) on {byElement.Values.Sum(x => x.Elements)} elements");
            Console.WriteLine("-- by element");
            foreach (var (name, e) in byElement.OrderByDescending(x => x.Value.Frames).Take(25))
            {
                Console.WriteLine($"{e.Frames,5} frames {e.Active,4} active {e.Values,5} values  x{e.Elements,-3} {name}");
            }

            Console.WriteLine("-- by selector");
            foreach (var (selector, s) in bySelector.OrderByDescending(x => x.Value.Frames).Take(60))
            {
                Console.WriteLine($"{s.Frames,5} frames {s.Active,4} active {s.Values,5} values  {selector}");
            }

            window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        return 0;
    }
}
