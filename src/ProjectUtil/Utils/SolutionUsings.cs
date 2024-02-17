public class SolutionUsings : IRootRunner
{
    public void Run(string root)
    {
        var di = new DirectoryInfo(root);
        var csFiles = di.GetFiles("*.cs", SearchOption.AllDirectories);
        var uniqueNamespaces = new HashSet<string>();
        int current = 0;
        foreach (var file in csFiles)
        {
            Console.WriteLine($"[{DateTime.Now}] ({++current}/{csFiles.Length}) Processing {file.FullName}");
            var normalizedFilePath = file.FullName.Replace('\\', '/');
            if (file.Name.EndsWith(".g.cs") || normalizedFilePath.Contains("/bin/") || normalizedFilePath.Contains("/obj/"))
            {
                continue;
            }
            ExtractUniqueNamespaces(file, uniqueNamespaces);
        }

        if (uniqueNamespaces.Any())
        {

            WriteImplicitUsingsFile(di, uniqueNamespaces);
        }
    }

    private void WriteImplicitUsingsFile(DirectoryInfo dir, HashSet<string> uniqueNamespaces)
    {
        var implicitUsingsFilePath = Path.Combine(dir.FullName, "RepoUsings.cs");
        Console.WriteLine($"[{DateTime.Now}] Creating {implicitUsingsFilePath}");
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

    private void ExtractUniqueNamespaces(FileInfo file, HashSet<string> uniqueNamespaces)
    {
        var lines = File.ReadAllLines(file.FullName);

        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("using ") && !line.Contains("=") && line.EndsWith(";"))
            {
                var ns = line.Substring(6, line.Length - 7).Trim();
                uniqueNamespaces.Add(ns);
            }
        }



    }
}

