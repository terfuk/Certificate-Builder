🎓 Certificate Builder (Templates) — Multi‑Language Certificate Generator
8 languages, one powerful certificate designer – create templates with placeholders, generate certificates for recipients, and export to HTML or text – right from your terminal.

✨ Features
📋 Create templates – design certificate templates with placeholders like {{name}}, {{date}}, {{course}}

📝 Edit templates – update the content of any template

📋 List templates – view all available templates

📄 Generate certificates – fill in placeholders with recipient data

📁 Export to file – save certificates as HTML or plain text

💾 Persistent storage – all templates saved in templates.json

🧰 Supported Languages & Files
Language	File	Dependencies
Python	certificate_builder.py	none (stdlib)
Go	certificate_builder.go	none (stdlib)
JavaScript (Node)	certificate_builder.js	commander (optional)
Ruby	certificate_builder.rb	json, date
PHP	certificate_builder.php	none (extensions)
Java	CertificateBuilder.java	Java 8+
C#	CertificateBuilder.cs	.NET Core 3.1+
C++	certificate_builder.cpp	nlohmann/json
🚀 Quick Start
All implementations follow the same CLI pattern:

bash
# Create a template
<command> template create "Course Certificate" --desc "For course completion"

# Show a template
<command> template show "Course Certificate"

# List all templates
<command> template list

# Edit a template's content (use placeholders like {{name}}, {{date}}, {{course}})
<command> template edit "Course Certificate" --content "This certifies that {{name}} has completed {{course}} on {{date}}."

# Generate a certificate from a template
<command> certificate generate "Course Certificate" "John Doe" --date "2026-08-24" --course "Python Basics" --output john_cert.html

# Generate with default placeholders (if not provided, they are left as-is)
<command> certificate generate "Course Certificate" "Jane Doe" --output jane_cert.txt
Commands/Arguments:

template create <name> [--desc DESCRIPTION] – create template

template show <name> – display template content

template list – list all templates

template edit <name> --content TEXT – update template content

certificate generate <template> <recipient> [--date DATE] [--course COURSE] [--output FILE] – generate certificate

📸 Example Output
text
📋 Templates:
1. Course Certificate (For course completion)
2. Achievement Award (For special achievements)

📄 Template: Course Certificate
Description: For course completion
Content:
This certifies that {{name}} has completed {{course}} on {{date}}.

✅ Certificate generated for John Doe -> john_cert.html
📁 Repository Structure
text
.
├── README.md
├── python/
│   └── certificate_builder.py
├── go/
│   └── certificate_builder.go
├── javascript/
│   └── certificate_builder.js
├── ruby/
│   └── certificate_builder.rb
├── php/
│   └── certificate_builder.php
├── java/
│   └── CertificateBuilder.java
├── csharp/
│   └── CertificateBuilder.cs
└── cpp/
    └── certificate_builder.cpp
