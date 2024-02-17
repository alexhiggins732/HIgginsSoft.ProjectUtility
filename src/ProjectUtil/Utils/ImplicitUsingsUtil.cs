public class ImplicitUsingsUtil : IRootRunner
{
    public void Run(string root)
    {
        Console.WriteLine($"Processing source files in {root}");
        var projectDirectories = GetProjectDirectories(root);
        int current = 0;

        foreach (var dir in projectDirectories)
        {
            Console.Title = $"[{DateTime.Now}] ({++current}/{projectDirectories.Count}) Processing {dir.FullName}";
            ProcessSourceFilesInDirectory(dir);
        }

        var licenseUtil = new LicenseUtil();
        if (!string.IsNullOrEmpty(Constants.DefaultLicenseHeaerPath))
        {
            Console.WriteLine($"Applying license header from {Constants.DefaultLicenseHeaerPath}");
            licenseUtil.Run(root, Constants.DefaultLicenseHeaerPath);
        }
        //licenseUtil.Run(root, Constants.DefaultLicenseHeaerPath);
    }

    private List<DirectoryInfo> GetProjectDirectories(string root)
    {
        var rootDir = new DirectoryInfo(root);
        var result = rootDir.GetDirectories("*", SearchOption.AllDirectories)
                      .Where(d => d.GetFiles("*.csproj").Length > 0)
                      .ToList();
        if (rootDir.GetFiles("*.csproj").Length > 0)
        {
            result.Add(rootDir);
        }

        return result;
    }

    private void ProcessSourceFilesInDirectory(DirectoryInfo dir)
    {
        var csFiles = dir.GetFiles("*.cs", SearchOption.AllDirectories);
        var uniqueNamespaces = new HashSet<string>();
        int current = 0;
        foreach (var file in csFiles)
        {

            var normalizedFilePath = file.FullName.Replace('\\', '/');
            if (file.Name.EndsWith(".g.cs") || normalizedFilePath.Contains("/bin/") || normalizedFilePath.Contains("/obj/"))
            {
                continue;
            }
            Console.WriteLine($"[{DateTime.Now}] ({++current}/{csFiles.Length}) Processing {file.FullName}");
            ExtractUniqueNamespaces(file, uniqueNamespaces);
        }

        ExtractGlobalUsingsNamespaces(dir, uniqueNamespaces);

        if (uniqueNamespaces.Any())
        {
            Console.WriteLine($"[{DateTime.Now}] Creating {dir}\\GlobalUsings.g.cs");
            WriteImplicitUsingsFile(dir, uniqueNamespaces);
        }
    }

    private void ExtractUniqueNamespaces(FileInfo file, HashSet<string> uniqueNamespaces)
    {
        var lines = File.ReadAllLines(file.FullName);
        using (var sw = new StreamWriter(file.FullName, false))
        {
            bool wroteLine = false;
            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("using ") && !line.Contains("=") && line.EndsWith(";"))
                {
                    var ns = line.Substring(6, line.Length - 7).Trim();
                    uniqueNamespaces.Add(ns);
                }
                else
                {
                    if(!string.IsNullOrEmpty(line) || wroteLine)
                    {
                        wroteLine = true;
                        sw.WriteLine(line);
                    }
                    
                }
            }
        }

    }

    private void ExtractGlobalUsingsNamespaces(DirectoryInfo projectDirectory, HashSet<string> uniqueNamespaces)
    {
        var implicitUsingsFilePath = Path.Combine(projectDirectory.FullName, "GlobalUsings.cs");
        if (!File.Exists(implicitUsingsFilePath)) return;
        var lines = File.ReadAllLines(implicitUsingsFilePath);
        var existingNamespaces = lines.Where(x => x.StartsWith("global using")).Select(l => l.Substring(13, l.Length - 14).Trim()).ToList();
        foreach (var ns in existingNamespaces)
        {
            uniqueNamespaces.Add(ns);
        }
    }

    private void WriteImplicitUsingsFile(DirectoryInfo projectDirectory, HashSet<string> uniqueNamespaces)
    {
        var implicitUsingsFilePath = Path.Combine(projectDirectory.FullName, "GlobalUsings.cs");
        if (File.Exists(implicitUsingsFilePath))
        {
            var lines = File.ReadAllLines(implicitUsingsFilePath);
            var len = "global using ".Length;
            var existingNamespaces = lines.Where(x => x.StartsWith("global using")).Select(l => l.Substring(13, l.Length - 14).Trim()).ToList();
            foreach (var ns in existingNamespaces)
            {
                uniqueNamespaces.Add(ns);
            }
        }
        var usingsContent = string.Join(Environment.NewLine, uniqueNamespaces.Select(ns => $"global using {ns};").OrderBy(x => x));

        File.WriteAllText(implicitUsingsFilePath, usingsContent);
    }
}

