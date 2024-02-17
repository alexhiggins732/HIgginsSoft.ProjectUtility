public class CleanUtil : IRootRunner
{
    public void Run(string root)
    {
        var di = new DirectoryInfo(root);
        var result = di.GetDirectories("*", SearchOption.AllDirectories)
                       .Where(d => d.Name == "bin" || d.Name == "obj" || d.Name == "nuget"
                       || (d.Parent != null && d.Parent.Name == "lib"))
                       .ToList();

        foreach (var item in result)
        {
            Delete(item);
        }
    }

    private void Delete(DirectoryInfo di)
    {
        Console.WriteLine($"[{DateTime.Now}] Deleting {di.FullName}");
        foreach (var fi in di.GetFiles())
        {
            try
            {
                fi.Delete();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now}] Error deleting {fi.FullName}: {ex.Message}");
                Console.ResetColor();
            }

        }
        foreach (var child in di.GetDirectories())
        {
            Delete(child);
        }
        try
        {
            di.Delete(true);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now}] Error deleting {di.FullName}: {ex.Message}");
            Console.ResetColor();
        }

    }
}

