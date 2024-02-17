using CommandLine;
using ProjectUtil;
internal class Program
{
    private static string? _root;

    static string root => _root ?? throw new Exception("Root directory has not been set");
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<CommandArguments>(args)
            .WithParsed(options => Run(options, args))
            .WithNotParsed(errors =>
            {
                Console.WriteLine("Invalid arguments Error(s):");
                errors.ToList().ForEach(e => Console.WriteLine(e.ToString()));
            });
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: ProjectUtile <command>");
            return;
        }



    }

    private static void Run(CommandArguments options, string[] args)
    {
        switch (options.Command)
        {
            case "license":
                Parser.Default.ParseArguments<LicenseArguments>(args)

               .WithParsed(opts => Run<LicenseUtil, string>(opts, opts.LicensePath))
                .WithNotParsed(errors =>
                {
                    Console.WriteLine("Invalid arguments Error(s):");
                    errors.ToList().ForEach(e => Console.WriteLine(e.ToString()));
                });
                break;
            case "clean":
                Parser.Default.ParseArguments<CleanArguments>(args)

                .WithParsed(opts => Run<CleanUtil>(opts))
                 .WithNotParsed(errors =>
                 {
                     Console.WriteLine("Invalid arguments Error(s):");
                     errors.ToList().ForEach(e => Console.WriteLine(e.ToString()));
                 });
                break;
            case "repousings":
                Parser.Default.ParseArguments<RepoUsingArguments>(args)
                .WithParsed(opts => Run<SolutionUsings>(opts))
                 .WithNotParsed(errors =>
                 {
                     Console.WriteLine("Invalid arguments Error(s):");
                     errors.ToList().ForEach(e => Console.WriteLine(e.ToString()));
                 });
                break;
            case "implicitusings":
                Parser.Default.ParseArguments<ImplicitUsingsArguments>(args)
                    .WithParsed(opts => Run<ImplicitUsingsUtil>(opts))
                     .WithNotParsed(errors =>
                      {
                          Console.WriteLine("Invalid arguments Error(s):");
                          errors.ToList().ForEach(e => Console.WriteLine(e.ToString()));
                      });
                break;
            case "rename":
                Parser.Default.ParseArguments<RenameArguments>(args)
                    .WithParsed(opts => Run(opts))
                     .WithNotParsed(errors =>
                     {
                         Console.WriteLine("Invalid arguments Error(s):");
                         errors.ToList().ForEach(e => Console.WriteLine(e.ToString()));
                     });
                break;
            default:
                Console.WriteLine($"Unknown command: {options.Command}");
                break;
        }
    }
    private static void Run<T>(CleanArguments opts)
    where T : IRootRunner, new()
    {
        var instance = new T();
        instance.Run(opts.Root);
    }
    private static void Run<T>(IRootProvider opts)
        where T : IRootRunner, new()
    {
        var instance = new T();
        instance.Run(opts.Root);
    }

    private static void Run<T, TArg>(IRootProvider opts, TArg arg0)
      where T : IRootRunner<TArg>, new()
    {
        var instance = new T();
        instance.Run(opts.Root, arg0);
    }

    private static void Run(RenameArguments options)
    {
        if (!Directory.Exists(options.Root))
        {
            Console.WriteLine($"Directory not found: {options.Root}");
            return;
        }
        _root = options.Root;

        RenameUtil.ApplyMoniker(options.Root, options.OldMoniker, options.NewMoniker);
    }
}
public class RenameUtil
{

    static string _root = null!;
    internal static void ApplyMoniker(string root, string oldMoniker, string newMoniker)
    {
        _root = root;
        var di = new DirectoryInfo(root);
        ApplyMoniker(di, oldMoniker, newMoniker);
    }

    private static void ApplyMoniker(DirectoryInfo di, string oldMoniker, string newMoniker)
    {
        if (ExcludeDirectory(di)) return;
        var files = di.GetFiles().ToList();
        foreach (var fi in files)
        {
            ApplyMonikerInFileContents(fi, oldMoniker, newMoniker);
            ApplyMonikerToFileName(fi, oldMoniker, newMoniker);
        }
        var directories = di.GetDirectories().ToList();
        foreach (var child in directories)
        {
            ApplyMoniker(child, oldMoniker, newMoniker);
            ApplyMonikerToDirectoryName(child, oldMoniker, newMoniker);
        }
    }

    private static void ApplyMonikerToDirectoryName(DirectoryInfo di, string oldMoniker, string newMoniker)
    {
        if (di.Parent == null) return;
        if (di.Name.Contains(oldMoniker, StringComparison.InvariantCultureIgnoreCase))
        {
            Exec(() =>
            {
                var targetName = di.Name.Replace(oldMoniker, newMoniker, StringComparison.InvariantCultureIgnoreCase);
                var targetPath = Path.Combine(di.Parent.FullName, targetName);
                Console.WriteLine($"Moving Directory:");
                Console.WriteLine($"-> {di.FullName}");
                Console.WriteLine($"-> {targetPath}");
                di.MoveTo(targetPath);
            });
        }
    }

    private static void Exec(Action value)
    {
        try { value(); } catch (Exception ex) { LogError(ex.Message); }
    }

    private static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($" -> Error: {message}");
        Console.ResetColor();
    }

    private static void ApplyMonikerToFileName(FileInfo fi, string oldMoniker, string newMoniker)
    {
        if (fi.Name.Contains(oldMoniker, StringComparison.InvariantCultureIgnoreCase))
        {
            Exec(() =>
            {
                var targetName = fi.Name.Replace(oldMoniker, newMoniker, StringComparison.InvariantCultureIgnoreCase);
                var targetPath = Path.Combine(fi.Directory!.FullName, targetName);
                Console.WriteLine($"Moving File:");
                Console.WriteLine($"-> {fi.FullName}");
                Console.WriteLine($"-> {targetPath}");
                fi.MoveTo(targetPath);
            });
        }
    }

    private static void ApplyMonikerInFileContents(FileInfo fi, string oldMoniker, string newMoniker)
    {
        if (ExcludeFile(fi))
        {
            return;
        }

        var text = File.ReadAllText(fi.FullName);
        if (text.Contains(oldMoniker, StringComparison.InvariantCultureIgnoreCase))
        {
            text = text.Replace(oldMoniker, newMoniker, StringComparison.InvariantCultureIgnoreCase);
            Console.WriteLine($"Update File: {fi.FullName.Substring(_root.Length)}");

            Exec(() =>
            {
                File.WriteAllText(fi.FullName, text);
            });
        }
    }

    private static bool ExcludeDirectory(DirectoryInfo di)
    {
        var name = di.Name;
        if (name.StartsWith('.')) return true;

        switch (name)
        {
            case "bin":
                return true;
            case "obj":
                return true;
            case ".git":
                return true;
            case ".vs":
                return true;
            default:
                return false;
        }
    }

    private static bool ExcludeFile(FileInfo fi)
    {
        var ext = fi.Extension.ToLower().Trim('.');
        switch (ext)
        {
            case "dll":
                return true;
            default:
                return false;
        }
    }

}

public interface IRootProvider
{
    string Root { get; set; }
}

public interface IRootRunner
{
    void Run(string root);
}

public interface IRootRunner<TArg>
{
    void Run(string root, TArg arg0);
}
public class CommandArguments
{
    [Value(0, MetaName = "Command", Required = true, HelpText = "The command to execute.")]
    public string Command { get; set; } = null!;
}

public class LicenseArguments : IRootProvider
{
    [Value(0, MetaName = "Command", Required = true, HelpText = "The command to execute.")]
    public string Command { get; set; } = null!;

    [Value(1, MetaName = "Root", Required = true, HelpText = "The root directory to process.")]
    public string Root { get; set; } = null!;

    [Value(2, MetaName = "License Path", Required = false, Default="LicenseHeader.txt", HelpText = "The path to the file containing the license text.")]
    public string LicensePath { get; set; } = null!;
}

public class CleanArguments : IRootProvider
{
    [Value(0, MetaName = "Command", Required = true, HelpText = "The command to execute.")]
    public string Command { get; set; } = null!;

    [Value(1, MetaName = "Root", Required = true, HelpText = "The root directory to process.")]
    public string Root { get; set; } = null!;
}

public class ImplicitUsingsArguments : IRootProvider
{
    [Value(0, MetaName = "Command", Required = true, HelpText = "The command to execute.")]
    public string Command { get; set; } = null!;

    [Value(1, MetaName = "Root", Required = true, HelpText = "The root directory to process.")]
    public string Root { get; set; } = null!;
}

public class RepoUsingArguments : IRootProvider
{
    [Value(0, MetaName = "Command", Required = true, HelpText = "The command to execute.")]
    public string Command { get; set; } = null!;

    [Value(1, MetaName = "Root", Required = true, HelpText = "The root directory to process.")]
    public string Root { get; set; } = null!;
}


public class RenameArguments : CommandArguments
{
    [Value(1, MetaName = "Root", Required = true, HelpText = "The root directory to rename.")]
    public string Root { get; set; } = null!;
    [Value(2, MetaName = "OldMoniker", Required = true, HelpText = "The old moniker to rename.")]
    public string OldMoniker { get; set; } = null!;
    [Value(3, MetaName = "NewMoniker", Required = true, HelpText = "The new moniker to rename.")]
    public string NewMoniker { get; set; } = null!;
}
