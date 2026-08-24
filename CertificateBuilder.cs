// CertificateBuilder.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

class Template
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("content")] public string Content { get; set; }
    [JsonPropertyName("created_at")] public string CreatedAt { get; set; }

    public Template() { }
    public Template(string name, string description, string content)
    {
        Id = Guid.NewGuid().ToString().Substring(0,8);
        Name = name;
        Description = description;
        Content = string.IsNullOrEmpty(content) ? "This certifies that {{name}} has completed {{course}} on {{date}}." : content;
        CreatedAt = DateTime.Now.ToString("o");
    }
}

class App
{
    private List<Template> templates = new List<Template>();
    private readonly string dataFile = "templates.json";
    private readonly JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

    public App() => Load();

    private void Load()
    {
        if (!File.Exists(dataFile)) return;
        string json = File.ReadAllText(dataFile);
        templates = JsonSerializer.Deserialize<List<Template>>(json) ?? new List<Template>();
    }

    private void Save()
    {
        string json = JsonSerializer.Serialize(templates, options);
        File.WriteAllText(dataFile, json);
    }

    private Template GetTemplate(string name) => templates.FirstOrDefault(t => t.Name == name);

    public void Create(string name, string description)
    {
        if (GetTemplate(name) != null)
        {
            Console.WriteLine($"❌ Template '{name}' already exists.");
            return;
        }
        var t = new Template(name, description, null);
        templates.Add(t);
        Save();
        Console.WriteLine($"✅ Template created: {t.Name} (ID: {t.Id})");
    }

    public void List()
    {
        if (!templates.Any())
        {
            Console.WriteLine("No templates.");
            return;
        }
        Console.WriteLine("\n📋 Templates:");
        for (int i = 0; i < templates.Count; i++)
        {
            var t = templates[i];
            string desc = string.IsNullOrEmpty(t.Description) ? "No description" : t.Description;
            Console.WriteLine($"{i+1}. {t.Name} ({desc})");
        }
    }

    public void Show(string name)
    {
        var t = GetTemplate(name);
        if (t == null)
        {
            Console.WriteLine($"❌ Template '{name}' not found.");
            return;
        }
        Console.WriteLine($"\n📄 Template: {t.Name}");
        Console.WriteLine($"Description: {string.IsNullOrEmpty(t.Description) ? "None" : t.Description}");
        Console.WriteLine("Content:");
        Console.WriteLine(t.Content);
    }

    public void Edit(string name, string content)
    {
        var t = GetTemplate(name);
        if (t == null)
        {
            Console.WriteLine($"❌ Template '{name}' not found.");
            return;
        }
        t.Content = content;
        Save();
        Console.WriteLine($"✅ Template '{name}' updated.");
    }

    public void Generate(string templateName, string recipient, string date, string course, string output)
    {
        var t = GetTemplate(templateName);
        if (t == null)
        {
            Console.WriteLine($"❌ Template '{templateName}' not found.");
            return;
        }
        string dateStr = string.IsNullOrEmpty(date) ? DateTime.Now.ToString("yyyy-MM-dd") : date;
        string courseStr = string.IsNullOrEmpty(course) ? "the course" : course;
        string content = t.Content.Replace("{{name}}", recipient)
                                  .Replace("{{date}}", dateStr)
                                  .Replace("{{course}}", courseStr);

        if (!string.IsNullOrEmpty(output))
        {
            File.WriteAllText(output, content);
            Console.WriteLine($"✅ Certificate generated for {recipient} -> {output}");
        }
        else
        {
            Console.WriteLine($"\n📜 Certificate for {recipient}:\n");
            Console.WriteLine(content);
        }
    }

    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: CertificateBuilder <command> [options]");
            return;
        }
        var app = new App();
        var parsed = ParseArgs(args);
        string cmd = args[0];
        switch (cmd)
        {
            case "template":
                if (args.Length < 2) { Console.WriteLine("template: create, list, show, edit"); return; }
                string sub = args[1];
                switch (sub)
                {
                    case "create":
                        if (args.Length < 3) { Console.WriteLine("template create <name> [--desc DESCRIPTION]"); return; }
                        string name = args[2];
                        string desc = parsed.GetValueOrDefault("desc", "");
                        app.Create(name, desc);
                        break;
                    case "list":
                        app.List();
                        break;
                    case "show":
                        if (args.Length < 3) { Console.WriteLine("template show <name>"); return; }
                        app.Show(args[2]);
                        break;
                    case "edit":
                        if (args.Length < 4) { Console.WriteLine("template edit <name> --content CONTENT"); return; }
                        string editName = args[2];
                        string content = parsed.GetValueOrDefault("content");
                        if (string.IsNullOrEmpty(content)) { Console.WriteLine("--content is required"); return; }
                        app.Edit(editName, content);
                        break;
                    default:
                        Console.WriteLine("Unknown template subcommand");
                        break;
                }
                break;
            case "certificate":
                if (args.Length < 3) { Console.WriteLine("certificate generate <template> <recipient> [--date DATE] [--course COURSE] [--output FILE]"); return; }
                string sub2 = args[1];
                if (sub2 != "generate") { Console.WriteLine("Unknown certificate subcommand"); return; }
                string tmpl = args[2];
                string recipient = args[3];
                string date = parsed.GetValueOrDefault("date");
                string course = parsed.GetValueOrDefault("course");
                string output = parsed.GetValueOrDefault("output");
                app.Generate(tmpl, recipient, date, course, output);
                break;
            default:
                Console.WriteLine("Unknown command.");
                break;
        }
    }

    static Dictionary<string, string> ParseArgs(string[] args)
    {
        var dict = new Dictionary<string, string>();
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i].StartsWith("--") && i + 1 < args.Length)
                dict[args[i].Substring(2)] = args[++i];
        }
        return dict;
    }
}
