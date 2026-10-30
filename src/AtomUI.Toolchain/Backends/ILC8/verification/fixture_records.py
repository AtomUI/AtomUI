"""Private mechanism fixture: real format-1 records and exact generated member templates."""
import hashlib
import json
import shutil

CORE = 'AtomUI.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
COMPONENTS = 'P1.Components, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
SECOND = 'P1.Second, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
VOID = 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
# The generic consumer remains GenericControl<Payload>. Payload is its legal,
# non-generic condition; its type reachability is supplied only by real generic IL.
FRAGMENTS = [
    ('a', 'A', [('a', 'A')]), ('b', 'B', [('b', 'B')]),
    ('direct', 'Direct', [('direct', 'DirectControl'), ('direct-alias', 'DirectAlias')]),
    ('generic', 'Generic', [('generic', 'Payload')]),
    ('root1', 'Root1', [('root1', 'Root1')]), ('root2', 'Root2', [('root2', 'Root2')]),
    ('static', 'Static', [('static', 'StaticOnly')]), ('style', 'Style', [('style', 'StyleOnly')]),
    ('throw', 'Throw', [('throw', 'ThrowControl')]), ('token', 'Token', [('token', 'TokenOnly')]),
    ('unused', 'Unused', [('unused', 'Unused')]), ('unused1', 'Unused1', [('unused1', 'Unused1')]),
    ('unused2', 'Unused2', [('unused2', 'Unused2')])]


def canonical(value):
    # Contract control characters use lowercase unicode escapes, including LF.
    return json.dumps(value, sort_keys=True, separators=(',', ':'), ensure_ascii=False).replace('\\n', '\\u000a')


def type_id(assembly, name):
    return dict(assembly=assembly, metadataName=name)


def qualified(identity):
    return identity['metadataName'] + ', ' + identity['assembly']


def generate_package(directory, assembly, owner, fragments):
    package = assembly.split(',')[0]
    group = type_id(assembly, 'P1.' + owner)
    group_id = qualified(group)
    builder = type_id(CORE, 'AtomUI.Registration.ControlPackageRegistrationBuilder')

    def method(name):
        return dict(declaringType=group, name=name, genericArity=0, callingConvention='static',
                    returnType=type_id(VOID, 'System.Void'), parameters=[builder])

    payload = dict(packageId=package, assembly=assembly, group=group, collect=method('Collect'),
                   collectFull=method('CollectFull'), collectSelected=method('CollectSelected'),
                   collectorTemplateId='atomui.collector.v1', requiredCapability='atomui.conditional.v1')
    records = [dict(format=1, kind='package', identity=group_id, payload=payload)]
    aliases = {}
    proxies = []
    thunks = []
    for order, (key, action, conditions) in enumerate(fragments):
        fragment_id = package + ':' + key
        identity = group_id + '\n' + fragment_id
        proxy = 'Proxy_' + key
        thunk = 'Register_' + key
        records.append(dict(format=1, kind='fragment', identity=identity, payload=dict(
            group=group_id, fragmentId=fragment_id, proxy=type_id(assembly, 'P1.' + proxy),
            registrationMethod=method(thunk), order=order)))
        for alias, trigger_name in conditions:
            trigger = type_id(COMPONENTS, 'P1.' + trigger_name)
            kind = 'token' if key == 'token' else 'style' if key == 'style' else 'control'
            condition_id = identity + '\n' + qualified(trigger) + '\n' + kind
            aliases[condition_id] = alias
            records.append(dict(format=1, kind='condition', identity=condition_id,
                                payload=dict(group=group_id, fragment=identity, trigger=trigger, kind=kind)))
        target = 'AdditionalFragments.Throw' if key == 'throw' else ('SecondPackage.Add' if assembly == SECOND else 'Fragments.' + action)
        proxies.append(f'''[Proxy_{key}]
public sealed class {proxy} : ControlRegistrationFragmentAttribute
{{
    public override string FragmentId => {json.dumps(fragment_id)};
    public override void Add(Builder builder) {{ {target}(builder); }}
}}''')
        thunks.append(f'[MethodImpl(MethodImplOptions.NoInlining)] internal static void {thunk}(Builder builder) {{ builder.AddFragment(new {proxy}()); }}')
    payload['recordsHash'] = hashlib.sha256(canonical(sorted(records, key=lambda r: (r['kind'], r['identity']))).encode()).hexdigest()
    attributes = '\n'.join('[assembly: ConditionalRegistrationRecord(1, ' + json.dumps(r['kind']) + ', ' +
                           json.dumps(r['identity']) + ', ' + json.dumps(canonical(r['payload'])) + ')]' for r in records)
    full = ' '.join('Register_' + key + '(builder);' for key, _, _ in fragments)
    second_action = '[MethodImpl(MethodImplOptions.NoInlining)] public static void Add(Builder builder) { builder.Add("second"); }' if assembly == SECOND else ''
    source = f'''using System.Runtime.CompilerServices;
using AtomUI.Registration;
using Builder = AtomUI.Registration.ControlPackageRegistrationBuilder;
[assembly: ControlPackageMarker("{package}", typeof(P1.{owner}), 1)]
{attributes}
namespace P1;
public static class {owner}
{{
    [MethodImpl(MethodImplOptions.NoInlining)] public static void Run(Builder builder) {{ Collect(builder); }}
    [MethodImpl(MethodImplOptions.NoInlining)] private static void Collect(Builder builder) {{ CollectFull(builder); }}
    private static void CollectFull(Builder builder) {{ {full} }}
    private static void CollectSelected(Builder builder) {{ throw new InvalidOperationException("AtomUI conditional registration was not materialized by a supported publish backend."); }}
    {chr(10).join(thunks)}
    {second_action}
}}
{chr(10).join(proxies)}
'''
    (directory / 'Registration.cs').write_text(source)
    return group_id, aliases


def prepare_fixture(root, templates):
    fixture = root / 'fixture'
    for name in ('core', 'components', 'second', 'app'):
        (fixture / name).mkdir(parents=True, exist_ok=True)
    core, components, second, app = (fixture / name for name in ('core', 'components', 'second', 'app'))
    project = (templates / 'Components.csproj.txt').read_text()
    (core / 'AtomUI.Core.csproj').write_text(project)
    shutil.copyfile(templates / 'Core.cs.txt', core / 'Core.cs')
    core_ref = '<Reference Include="AtomUI.Core"><HintPath>../core/bin/Release/net8.0/AtomUI.Core.dll</HintPath></Reference>'
    component_ref = '<Reference Include="P1.Components"><HintPath>../components/bin/Release/net8.0/P1.Components.dll</HintPath></Reference>'
    (components / 'P1.Components.csproj').write_text(project.replace('</Project>', '<ItemGroup>' + core_ref + '</ItemGroup></Project>'))
    (components / 'Components.cs').write_text((templates / 'Components.cs.txt').read_text().replace('Package.Collect(builder)', 'Package.Run(builder)'))
    (second / 'P1.Second.csproj').write_text(project.replace('</Project>', '<ItemGroup>' + core_ref + component_ref + '</ItemGroup></Project>'))
    primary_group, primary_aliases = generate_package(components, COMPONENTS, 'Package', FRAGMENTS)
    second_group, second_aliases = generate_package(second, SECOND, 'SecondPackage', [('direct', 'Direct', [('direct', 'DirectControl')])])
    (app / 'Program.cs').write_text((templates / 'Program.cs.txt').read_text().replace('Package.Collect(builder)', 'Package.Run(builder)'))
    app_project = (templates / 'App.csproj.txt').read_text().replace('<ItemGroup>', '<ItemGroup>' + core_ref + '<Reference Include="P1.Second"><HintPath>../second/bin/Release/net8.0/P1.Second.dll</HintPath></Reference>', 1)
    (app / 'P1.App.csproj').write_text(app_project)
    return fixture, primary_group, second_group, primary_aliases | second_aliases
