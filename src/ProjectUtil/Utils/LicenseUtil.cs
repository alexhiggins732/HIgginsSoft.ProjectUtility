public class LicenseUtil : IRootRunner<string>
{

    public void Run(string root, string licenseHeaderPath)
    {
        var licenseHeader = new FileInfo(licenseHeaderPath);
        if (!licenseHeader.Exists)
        {
            throw new ArgumentException($"License header file {licenseHeader.FullName} does not exist");
        }
        var licenseText = File.ReadAllText(licenseHeader.FullName).Trim();
        var di = new DirectoryInfo(root);
        var result = di.GetFiles("*.cs", SearchOption.AllDirectories)
                       .ToList();

        var current = 0;
        foreach (var item in result)
        {
            var normalizedFilePath = item.FullName.Replace('\\', '/');
            if (normalizedFilePath.Contains("/bin/") || normalizedFilePath.Contains("/obj/"))
            {
                continue;
            }
            Console.WriteLine($"[{DateTime.Now}] ({++current}/{result.Count}) Processing {item.FullName}");
            ApplyLicense(item, licenseText);
        }
    }

    private void ApplyLicense(FileInfo item, string licenseText)
    {
        var lines = File.ReadAllLines(item.FullName);
        //File.WriteAllLines(item.FullName, lines);
        using (var sw = new StreamWriter(item.FullName, false))
        {
            if (!lines[0].Contains("/*"))
            {
                sw.WriteLine(licenseText);
                sw.WriteLine();
                foreach (var line in lines)
                {
                    sw.WriteLine(line);
                }
            }
            else
            {
                var i = 0;
                sw.WriteLine($"{licenseText}\r\n");
                if (lines.Length == 0) return;
                while (!lines[++i].Contains("*/"))
                {
                    if (lines.Length == i) return;
                }
                while (string.IsNullOrEmpty(lines[++i]))
                {
                    if (lines.Length == i) return;
                }
                for (var j = i; j < lines.Length; j++)
                {
                    sw.WriteLine(lines[j]);
                }
            }
        }
    }


}