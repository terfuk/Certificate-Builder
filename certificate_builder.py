# certificate_builder.py
import json
import os
import sys
import argparse
import uuid
from datetime import datetime

DATA_FILE = "templates.json"

class Template:
    def __init__(self, name, description="", content="", template_id=None):
        self.id = template_id or str(uuid.uuid4())[:8]
        self.name = name
        self.description = description
        self.content = content or "This certifies that {{name}} has completed {{course}} on {{date}}."
        self.created_at = datetime.now().isoformat()

    def to_dict(self):
        return {
            "id": self.id,
            "name": self.name,
            "description": self.description,
            "content": self.content,
            "created_at": self.created_at
        }

    @classmethod
    def from_dict(cls, data):
        t = cls(data["name"], data.get("description", ""), data.get("content", ""), data.get("id"))
        t.created_at = data.get("created_at", datetime.now().isoformat())
        return t

class CertificateBuilder:
    def __init__(self):
        self.templates = []
        self.load()

    def load(self):
        if os.path.exists(DATA_FILE):
            with open(DATA_FILE, "r") as f:
                data = json.load(f)
                self.templates = [Template.from_dict(t) for t in data]

    def save(self):
        with open(DATA_FILE, "w") as f:
            json.dump([t.to_dict() for t in self.templates], f, indent=2)

    def get_template(self, name):
        for t in self.templates:
            if t.name == name:
                return t
        return None

    def create(self, name, description=""):
        if self.get_template(name):
            print(f"❌ Template '{name}' already exists.")
            return
        t = Template(name, description)
        self.templates.append(t)
        self.save()
        print(f"✅ Template created: {t.name} (ID: {t.id})")

    def list(self):
        if not self.templates:
            print("No templates.")
            return
        print("\n📋 Templates:")
        for i, t in enumerate(self.templates, 1):
            print(f"{i}. {t.name} ({t.description or 'No description'})")

    def show(self, name):
        t = self.get_template(name)
        if not t:
            print(f"❌ Template '{name}' not found.")
            return
        print(f"\n📄 Template: {t.name}")
        print(f"Description: {t.description or 'None'}")
        print("Content:")
        print(t.content)

    def edit(self, name, content):
        t = self.get_template(name)
        if not t:
            print(f"❌ Template '{name}' not found.")
            return
        t.content = content
        self.save()
        print(f"✅ Template '{name}' updated.")

    def generate(self, template_name, recipient, date=None, course=None, output=None):
        t = self.get_template(template_name)
        if not t:
            print(f"❌ Template '{template_name}' not found.")
            return
        date_str = date or datetime.now().strftime("%Y-%m-%d")
        course_str = course or "the course"
        content = t.content
        content = content.replace("{{name}}", recipient)
        content = content.replace("{{date}}", date_str)
        content = content.replace("{{course}}", course_str)

        if output:
            with open(output, "w") as f:
                f.write(content)
            print(f"✅ Certificate generated for {recipient} -> {output}")
        else:
            print(f"\n📜 Certificate for {recipient}:\n")
            print(content)

def main():
    parser = argparse.ArgumentParser(description="Certificate Builder")
    subparsers = parser.add_subparsers(dest="cmd", required=True)

    template_parser = subparsers.add_parser("template")
    template_sub = template_parser.add_subparsers(dest="subcmd", required=True)

    create_parser = template_sub.add_parser("create")
    create_parser.add_argument("name")
    create_parser.add_argument("--desc", default="")

    template_sub.add_parser("list")

    show_parser = template_sub.add_parser("show")
    show_parser.add_argument("name")

    edit_parser = template_sub.add_parser("edit")
    edit_parser.add_argument("name")
    edit_parser.add_argument("--content", required=True)

    cert_parser = subparsers.add_parser("certificate")
    cert_sub = cert_parser.add_subparsers(dest="subcmd", required=True)

    gen_parser = cert_sub.add_parser("generate")
    gen_parser.add_argument("template")
    gen_parser.add_argument("recipient")
    gen_parser.add_argument("--date", help="YYYY-MM-DD")
    gen_parser.add_argument("--course")
    gen_parser.add_argument("--output")

    args = parser.parse_args()
    app = CertificateBuilder()

    if args.cmd == "template":
        if args.subcmd == "create":
            app.create(args.name, args.desc)
        elif args.subcmd == "list":
            app.list()
        elif args.subcmd == "show":
            app.show(args.name)
        elif args.subcmd == "edit":
            app.edit(args.name, args.content)
    elif args.cmd == "certificate":
        if args.subcmd == "generate":
            app.generate(args.template, args.recipient, args.date, args.course, args.output)

if __name__ == "__main__":
    main()
