// certificate_builder.go
package main

import (
	"encoding/json"
	"flag"
	"fmt"
	"os"
	"strings"
	"time"
	"github.com/google/uuid"
)

type Template struct {
	ID          string `json:"id"`
	Name        string `json:"name"`
	Description string `json:"description"`
	Content     string `json:"content"`
	CreatedAt   string `json:"created_at"`
}

func NewTemplate(name, desc, content string) Template {
	if content == "" {
		content = "This certifies that {{name}} has completed {{course}} on {{date}}."
	}
	return Template{
		ID:          uuid.New().String()[:8],
		Name:        name,
		Description: desc,
		Content:     content,
		CreatedAt:   time.Now().Format(time.RFC3339),
	}
}

type App struct {
	Templates []Template `json:"templates"`
}

var dataFile = "templates.json"

func (a *App) load() {
	data, err := os.ReadFile(dataFile)
	if err != nil {
		return
	}
	json.Unmarshal(data, a)
}

func (a *App) save() {
	data, _ := json.MarshalIndent(a, "", "  ")
	os.WriteFile(dataFile, data, 0644)
}

func (a *App) getTemplate(name string) *Template {
	for i := range a.Templates {
		if a.Templates[i].Name == name {
			return &a.Templates[i]
		}
	}
	return nil
}

func (a *App) create(name, desc string) {
	if a.getTemplate(name) != nil {
		fmt.Printf("❌ Template '%s' already exists.\n", name)
		return
	}
	t := NewTemplate(name, desc, "")
	a.Templates = append(a.Templates, t)
	a.save()
	fmt.Printf("✅ Template created: %s (ID: %s)\n", t.Name, t.ID)
}

func (a *App) list() {
	if len(a.Templates) == 0 {
		fmt.Println("No templates.")
		return
	}
	fmt.Println("\n📋 Templates:")
	for i, t := range a.Templates {
		desc := t.Description
		if desc == "" {
			desc = "No description"
		}
		fmt.Printf("%d. %s (%s)\n", i+1, t.Name, desc)
	}
}

func (a *App) show(name string) {
	t := a.getTemplate(name)
	if t == nil {
		fmt.Printf("❌ Template '%s' not found.\n", name)
		return
	}
	fmt.Printf("\n📄 Template: %s\n", t.Name)
	fmt.Printf("Description: %s\n", t.Description)
	fmt.Println("Content:")
	fmt.Println(t.Content)
}

func (a *App) edit(name, content string) {
	t := a.getTemplate(name)
	if t == nil {
		fmt.Printf("❌ Template '%s' not found.\n", name)
		return
	}
	t.Content = content
	a.save()
	fmt.Printf("✅ Template '%s' updated.\n", name)
}

func (a *App) generate(templateName, recipient, date, course, output string) {
	t := a.getTemplate(templateName)
	if t == nil {
		fmt.Printf("❌ Template '%s' not found.\n", templateName)
		return
	}
	if date == "" {
		date = time.Now().Format("2006-01-02")
	}
	if course == "" {
		course = "the course"
	}
	content := t.Content
	content = strings.ReplaceAll(content, "{{name}}", recipient)
	content = strings.ReplaceAll(content, "{{date}}", date)
	content = strings.ReplaceAll(content, "{{course}}", course)

	if output != "" {
		err := os.WriteFile(output, []byte(content), 0644)
		if err != nil {
			fmt.Printf("Error writing file: %v\n", err)
			return
		}
		fmt.Printf("✅ Certificate generated for %s -> %s\n", recipient, output)
	} else {
		fmt.Printf("\n📜 Certificate for %s:\n\n%s\n", recipient, content)
	}
}

func main() {
	if len(os.Args) < 2 {
		fmt.Println("Usage: certificate_builder <command> [options]")
		return
	}
	app := &App{}
	app.load()
	cmd := os.Args[1]

	switch cmd {
	case "template":
		if len(os.Args) < 3 {
			fmt.Println("template: create, list, show, edit")
			return
		}
		sub := os.Args[2]
		switch sub {
		case "create":
			createCmd := flag.NewFlagSet("create", flag.ExitOnError)
			name := createCmd.String("name", "", "")
			desc := createCmd.String("desc", "", "")
			createCmd.Parse(os.Args[3:])
			if *name == "" && len(createCmd.Args()) > 0 {
				*name = createCmd.Args()[0]
			}
			if *name == "" {
				fmt.Println("create requires a name")
				return
			}
			app.create(*name, *desc)

		case "list":
			app.list()

		case "show":
			if len(os.Args) < 4 {
				fmt.Println("show <name>")
				return
			}
			app.show(os.Args[3])

		case "edit":
			editCmd := flag.NewFlagSet("edit", flag.ExitOnError)
			name := editCmd.String("name", "", "")
			content := editCmd.String("content", "", "")
			editCmd.Parse(os.Args[3:])
			if *name == "" && len(editCmd.Args()) > 0 {
				*name = editCmd.Args()[0]
			}
			if *name == "" || *content == "" {
				fmt.Println("edit requires name and --content")
				return
			}
			app.edit(*name, *content)

		default:
			fmt.Println("Unknown template subcommand")
		}

	case "certificate":
		if len(os.Args) < 4 {
			fmt.Println("certificate generate <template> <recipient> [--date DATE] [--course COURSE] [--output FILE]")
			return
		}
		sub := os.Args[2]
		if sub != "generate" {
			fmt.Println("Unknown certificate subcommand")
			return
		}
		genCmd := flag.NewFlagSet("generate", flag.ExitOnError)
		tmpl := genCmd.String("template", "", "")
		recipient := genCmd.String("recipient", "", "")
		date := genCmd.String("date", "", "")
		course := genCmd.String("course", "", "")
		output := genCmd.String("output", "", "")
		genCmd.Parse(os.Args[3:])
		if *tmpl == "" && len(genCmd.Args()) > 0 {
			*tmpl = genCmd.Args()[0]
		}
		if *recipient == "" && len(genCmd.Args()) > 1 {
			*recipient = genCmd.Args()[1]
		}
		if *tmpl == "" || *recipient == "" {
			fmt.Println("generate requires template and recipient")
			return
		}
		app.generate(*tmpl, *recipient, *date, *course, *output)

	default:
		fmt.Println("Unknown command.")
	}
}
