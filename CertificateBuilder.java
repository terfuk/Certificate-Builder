// CertificateBuilder.java
import java.io.*;
import java.nio.file.*;
import java.time.*;
import java.util.*;
import com.google.gson.*;

class Template {
    String id;
    String name;
    String description;
    String content;
    String created_at;

    Template() {}
    Template(String name, String description, String content) {
        this.id = UUID.randomUUID().toString().substring(0,8);
        this.name = name;
        this.description = description;
        this.content = content != null && !content.isEmpty() ? content : "This certifies that {{name}} has completed {{course}} on {{date}}.";
        this.created_at = Instant.now().toString();
    }
}

public class CertificateBuilder {
    private List<Template> templates = new ArrayList<>();
    private final String dataFile = "templates.json";
    private final Gson gson = new GsonBuilder().setPrettyPrinting().create();

    public CertificateBuilder() { load(); }

    private void load() {
        try {
            Path path = Paths.get(dataFile);
            if (Files.exists(path)) {
                String json = new String(Files.readAllBytes(path));
                Template[] arr = gson.fromJson(json, Template[].class);
                templates = Arrays.asList(arr);
            }
        } catch (Exception e) {}
    }

    private void save() {
        try {
            Files.write(Paths.get(dataFile), gson.toJson(templates).getBytes());
        } catch (Exception e) {}
    }

    private Template getTemplate(String name) {
        for (Template t : templates) {
            if (t.name.equals(name)) return t;
        }
        return null;
    }

    public void create(String name, String description) {
        if (getTemplate(name) != null) {
            System.out.printf("❌ Template '%s' already exists.%n", name);
            return;
        }
        Template t = new Template(name, description, null);
        templates.add(t);
        save();
        System.out.printf("✅ Template created: %s (ID: %s)%n", t.name, t.id);
    }

    public void list() {
        if (templates.isEmpty()) {
            System.out.println("No templates.");
            return;
        }
        System.out.println("\n📋 Templates:");
        for (int i = 0; i < templates.size(); i++) {
            Template t = templates.get(i);
            String desc = t.description != null && !t.description.isEmpty() ? t.description : "No description";
            System.out.printf("%d. %s (%s)%n", i+1, t.name, desc);
        }
    }

    public void show(String name) {
        Template t = getTemplate(name);
        if (t == null) {
            System.out.printf("❌ Template '%s' not found.%n", name);
            return;
        }
        System.out.printf("\n📄 Template: %s%n", t.name);
        System.out.printf("Description: %s%n", t.description != null ? t.description : "None");
        System.out.println("Content:");
        System.out.println(t.content);
    }

    public void edit(String name, String content) {
        Template t = getTemplate(name);
        if (t == null) {
            System.out.printf("❌ Template '%s' not found.%n", name);
            return;
        }
        t.content = content;
        save();
        System.out.printf("✅ Template '%s' updated.%n", name);
    }

    public void generate(String templateName, String recipient, String date, String course, String output) {
        Template t = getTemplate(templateName);
        if (t == null) {
            System.out.printf("❌ Template '%s' not found.%n", templateName);
            return;
        }
        String dateStr = date != null ? date : LocalDate.now().toString();
        String courseStr = course != null ? course : "the course";
        String content = t.content.replace("{{name}}", recipient)
                                  .replace("{{date}}", dateStr)
                                  .replace("{{course}}", courseStr);

        if (output != null) {
            try {
                Files.write(Paths.get(output), content.getBytes());
                System.out.printf("✅ Certificate generated for %s -> %s%n", recipient, output);
            } catch (IOException e) {
                System.out.println("Error writing file: " + e.getMessage());
            }
        } else {
            System.out.printf("\n📜 Certificate for %s:\n\n%s%n", recipient, content);
        }
    }

    public static void main(String[] args) throws Exception {
        if (args.length < 1) {
            System.out.println("Usage: CertificateBuilder <command> [options]");
            return;
        }
        CertificateBuilder app = new CertificateBuilder();
        String cmd = args[0];
        Map<String, String> params = new HashMap<>();
        for (int i=1; i<args.length; i++) {
            if (args[i].startsWith("--") && i+1 < args.length) {
                params.put(args[i].substring(2), args[++i]);
            }
        }
        switch (cmd) {
            case "template":
                if (args.length < 2) { System.out.println("template: create, list, show, edit"); return; }
                String sub = args[1];
                switch (sub) {
                    case "create":
                        if (args.length < 3) { System.out.println("template create <name> [--desc DESCRIPTION]"); return; }
                        String name = args[2];
                        String desc = params.getOrDefault("desc", "");
                        app.create(name, desc);
                        break;
                    case "list":
                        app.list();
                        break;
                    case "show":
                        if (args.length < 3) { System.out.println("template show <name>"); return; }
                        app.show(args[2]);
                        break;
                    case "edit":
                        if (args.length < 4) { System.out.println("template edit <name> --content CONTENT"); return; }
                        String editName = args[2];
                        String content = params.get("content");
                        if (content == null) { System.out.println("--content is required"); return; }
                        app.edit(editName, content);
                        break;
                    default:
                        System.out.println("Unknown template subcommand");
                }
                break;
            case "certificate":
                if (args.length < 3) { System.out.println("certificate generate <template> <recipient> [--date DATE] [--course COURSE] [--output FILE]"); return; }
                String sub2 = args[1];
                if (!sub2.equals("generate")) { System.out.println("Unknown certificate subcommand"); return; }
                String tmpl = args[2];
                String recipient = args[3];
                String date = params.get("date");
                String course = params.get("course");
                String output = params.get("output");
                app.generate(tmpl, recipient, date, course, output);
                break;
            default:
                System.out.println("Unknown command.");
        }
    }
}
