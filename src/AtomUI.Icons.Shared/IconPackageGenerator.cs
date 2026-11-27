namespace AtomUI.Icons;

/// <summary>
/// Base class of the generators that turn a folder of SVG files into the source files of an icon package.
/// A derived class supplies the scan of the folder, the icon kind enum and the source written for each icon.
/// </summary>
public abstract class AbstractIconPackageGenerator
{
    /// <summary>
    /// Name of the icon package. It is the prefix of the generated type names, such as the enum
    /// <c>{PackageName}IconKind</c>.
    /// </summary>
    public string PackageName { get; protected set; } = string.Empty;
    /// <summary>
    /// Namespace that the generated source files are declared in.
    /// </summary>
    public string PackageNamespace { get; protected set; } = string.Empty;
    
    /// <summary>
    /// Directory name under <see cref="TargetPath" /> that holds the generated icon class files when the
    /// constructor is given no directory name.
    /// </summary>
    public const string DefaultSavedDirectory = "GeneratedIcons";
    /// <summary>
    /// Directory that is scanned for the SVG source files.
    /// </summary>
    protected string SourcePath;
    /// <summary>
    /// Directory of the icon package project. It receives the files that are written once per package, such
    /// as the provider factory and the catalog, and it contains <see cref="GeneratedIconsPath" />.
    /// </summary>
    protected string TargetPath;
    /// <summary>
    /// Directory that receives the generated icon class files.
    /// <see cref="PrepareEnvironment" /> deletes and recreates it on every generation.
    /// </summary>
    protected string GeneratedIconsPath;
    /// <summary>
    /// The icons found by <see cref="ScanIconFiles" />, in the order the scanner yields them.
    /// The order fixes the values of the icon kind enum and the positions in the generated catalog.
    /// </summary>
    protected List<IconFileInfo> IconFiles;
    /// <summary>
    /// Parser for the SVG source of the icons. A derived class uses it for every icon of a run.
    /// </summary>
    protected readonly SvgParser SvgParser;

    /// <summary>
    /// Creates a generator that scans <paramref name="sourcePath" /> and writes into <paramref name="targetPath" />.
    /// </summary>
    /// <param name="sourcePath">
    /// Directory that is scanned for SVG source files.
    /// </param>
    /// <param name="targetPath">
    /// Directory of the icon package project that receives the generated files.
    /// </param>
    /// <param name="generatedIconsDir">
    /// Name of the directory under <paramref name="targetPath" /> that holds the generated icon class files,
    /// or <see langword="null" /> to use <see cref="DefaultSavedDirectory" />.
    /// </param>
    public AbstractIconPackageGenerator(string sourcePath, string targetPath, string? generatedIconsDir = null)
    {
        SourcePath         = sourcePath;
        TargetPath         = targetPath;
        GeneratedIconsPath = Path.Combine(TargetPath, generatedIconsDir ?? DefaultSavedDirectory);
        IconFiles          = new List<IconFileInfo>();
        SvgParser          = new SvgParser();
    }

    /// <summary>
    /// Checks that the source and target directories exist and deletes and recreates
    /// <see cref="GeneratedIconsPath" />, which discards everything in it.
    /// </summary>
    /// <exception cref="FileNotFoundException">
    /// <see cref="SourcePath" /> or <see cref="TargetPath" /> does not exist.
    /// </exception>
    /// <remarks>
    /// <see cref="GenerateAsync(CancellationToken)" /> calls it before any file is generated. An override
    /// that relies on these checks or on the empty directory must call the base implementation.
    /// </remarks>
    protected virtual void PrepareEnvironment()
    {
        if (!Directory.Exists(SourcePath))
        {
            throw new FileNotFoundException($"Icons file path {SourcePath} does not exist.");
        }

        if (!Directory.Exists(TargetPath))
        {
            throw new FileNotFoundException($"Icons file path {TargetPath} does not exist.");
        }

        if (Directory.Exists(GeneratedIconsPath))
        {
            Directory.Delete(GeneratedIconsPath, true);
        }
        Directory.CreateDirectory(GeneratedIconsPath);
    }
    
    /// <summary>
    /// Adds the icons that <see cref="ScanIconFilesRecursively(string)" /> finds under
    /// <see cref="SourcePath" /> to <see cref="IconFiles" />.
    /// </summary>
    /// <remarks>
    /// <see cref="GenerateAsync(CancellationToken)" /> calls it once, after <see cref="PrepareEnvironment" />.
    /// </remarks>
    protected void ScanIconFiles()
    {
        Console.WriteLine($"Beginning ScanIconFiles: {SourcePath}");
        IconFiles.AddRange(ScanIconFilesRecursively(SourcePath));
    }

    /// <summary>
    /// When overridden, enumerates the icons that are found under a directory.
    /// </summary>
    /// <param name="filePath">
    /// Directory to scan. In spite of its name, the argument is a directory.
    /// </param>
    /// <remarks>
    /// <see cref="ScanIconFiles" /> calls it once with <see cref="SourcePath" />. The implementation in
    /// <see cref="DefaultIconPackageGenerator" /> also descends into the subdirectories.
    /// </remarks>
    protected abstract IEnumerable<IconFileInfo> ScanIconFilesRecursively(string filePath);

    /// <summary>
    /// When overridden, writes the enum that gives each icon of the package a value.
    /// </summary>
    /// <remarks>
    /// <see cref="GenerateAsync(CancellationToken)" /> awaits it after the icons were scanned and before
    /// <see cref="GenerateIconPackageClassesAsync" /> runs, so <see cref="IconFiles" /> is complete.
    /// </remarks>
    protected abstract Task GenerateIconPackageKindAsync();

    /// <summary>
    /// Writes one source file for each entry of <see cref="IconFiles" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For each icon, the method opens the path from
    /// <see cref="GenerateIconClassFileName(IconFileInfo)" /> in create mode, which overwrites an
    /// existing file, and passes the stream to
    /// <see cref="GenerateIconPackageClass(IconFileInfo, Stream)" />. The stream is disposed when that
    /// method returns.
    /// </para>
    /// <para>
    /// <see cref="GenerateAsync(CancellationToken)" /> awaits it after
    /// <see cref="GenerateIconPackageKindAsync" />.
    /// </para>
    /// </remarks>
    protected virtual async Task GenerateIconPackageClassesAsync()
    {
        foreach (var iconFileInfo in IconFiles)
        {
            await using var stream = new FileStream(GenerateIconClassFileName(iconFileInfo), FileMode.Create, FileAccess.Write);
            await GenerateIconPackageClass(iconFileInfo, stream);
        }
    }

    /// <summary>
    /// Builds the path of the source file that is generated for one icon.
    /// </summary>
    /// <param name="iconFileInfo">
    /// The icon whose source file path is built.
    /// </param>
    /// <returns>
    /// <see cref="GeneratedIconsPath" /> combined with the icon name, the theme and the extension
    /// <c>.g.cs</c>, for example <c>AccountBookFilled.g.cs</c>.
    /// </returns>
    protected virtual string GenerateIconClassFileName(IconFileInfo iconFileInfo)
    {
        return Path.Combine(GeneratedIconsPath, $"{iconFileInfo.Name}{iconFileInfo.ThemeType}.g.cs");
    }
    
    /// <summary>
    /// When overridden, writes the source code of the class of one icon.
    /// </summary>
    /// <param name="iconFileInfo">
    /// The icon whose class is written.
    /// </param>
    /// <param name="output">
    /// Stream of the new file. The caller owns the stream and disposes it.
    /// </param>
    /// <remarks>
    /// <see cref="GenerateIconPackageClassesAsync" /> calls it once for each entry of
    /// <see cref="IconFiles" />, with a stream that is open for the path from
    /// <see cref="GenerateIconClassFileName(IconFileInfo)" />.
    /// </remarks>
    protected abstract Task GenerateIconPackageClass(IconFileInfo iconFileInfo, Stream output);

    /// <summary>
    /// When overridden, writes the factory part of the icon provider.
    /// </summary>
    /// <remarks>
    /// The base implementation writes nothing, so an override need not call it.
    /// <see cref="GenerateAsync(CancellationToken)" /> awaits it after
    /// <see cref="GenerateIconPackageClassesAsync" />.
    /// </remarks>
    protected virtual Task GenerateIconProviderFactoryAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// When overridden, writes the catalog that lists the icons of the package.
    /// </summary>
    /// <remarks>
    /// The base implementation writes nothing, so an override need not call it.
    /// <see cref="GenerateAsync(CancellationToken)" /> awaits it last,
    /// after <see cref="GenerateIconProviderFactoryAsync" />.
    /// </remarks>
    protected virtual Task GenerateIconCatalogAsync()
    {
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Generates the icon package: it prepares the directories, scans the SVG files and then runs the
    /// generation steps one after another.
    /// </summary>
    /// <param name="cancellationToken">
    /// Checked after each generation step. Cancelling stops the run before the next step and faults the
    /// task with <see cref="OperationCanceledException" />.
    /// </param>
    /// <remarks>
    /// <para>
    /// The steps run in this order: <see cref="GenerateIconPackageKindAsync" />,
    /// <see cref="GenerateIconPackageClassesAsync" />, <see cref="GenerateIconProviderFactoryAsync" /> and
    /// <see cref="GenerateIconCatalogAsync" />. They run only when at least one icon was found; otherwise
    /// nothing is written, but <see cref="GeneratedIconsPath" /> has already been recreated.
    /// </para>
    /// <para>
    /// The task completes when all steps are finished. A canceled or failed run is not rolled back: the
    /// files of the finished steps stay, so the package is complete only after a run that finished.
    /// </para>
    /// </remarks>
    public async Task GenerateAsync(CancellationToken cancellationToken = default)
    {
        PrepareEnvironment();
        ScanIconFiles();
        if (IconFiles.Count > 0)
        {
            Console.WriteLine($"Found {IconFiles.Count} icon files.");
            await GenerateIconPackageKindAsync();
            cancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine("Generate Icon PackageKind enum successfully.");
            await GenerateIconPackageClassesAsync();
            cancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine("Generate Icon classes successfully.");
            await GenerateIconProviderFactoryAsync();
            cancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine("Generate IconProvider factory successfully.");
            await GenerateIconCatalogAsync();
            cancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine("Generate Icon catalog successfully.");
        }
    }
}
