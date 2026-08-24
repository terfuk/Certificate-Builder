// certificate_builder.js
#!/usr/bin/env node
const fs = require('fs');
const { program } = require('commander');
const { v4: uuidv4 } = require('uuid');

const DATA_FILE = 'templates.json';

class Template {
    constructor(name, description = '', content = '') {
        this.id = uuidv4().slice(0,8);
        this.name = name;
        this.description = description;
        this.content = content || 'This certifies that {{name}} has completed {{course}} on {{date}}.';
        this.created_at = new Date().toISOString();
    }
}

class App {
    constructor() {
        this.templates = [];
        this.load();
    }

    load() {
        if (fs.existsSync(DATA_FILE)) {
            try {
                this.templates = JSON.parse(fs.readFileSync(DATA_FILE));
            } catch (e) {}
        }
    }

    save() {
        fs.writeFileSync(DATA_FILE, JSON.stringify(this.templates, null, 2));
    }

    getTemplate(name) {
        return this.templates.find(t => t.name === name);
    }

    create(name, description) {
        if (this.getTemplate(name)) {
            console.log(`❌ Template '${name}' already exists.`);
            return;
        }
        const t = new Template(name, description);
        this.templates.push(t);
        this.save();
        console.log(`✅ Template created: ${t.name} (ID: ${t.id})`);
    }

    list() {
        if (!this.templates.length) {
            console.log('No templates.');
            return;
        }
        console.log('\n📋 Templates:');
        this.templates.forEach((t, i) => {
            const desc = t.description || 'No description';
            console.log(`${i+1}. ${t.name} (${desc})`);
        });
    }

    show(name) {
        const t = this.getTemplate(name);
        if (!t) {
            console.log(`❌ Template '${name}' not found.`);
            return;
        }
        console.log(`\n📄 Template: ${t.name}`);
        console.log(`Description: ${t.description || 'None'}`);
        console.log('Content:');
        console.log(t.content);
    }

    edit(name, content) {
        const t = this.getTemplate(name);
        if (!t) {
            console.log(`❌ Template '${name}' not found.`);
            return;
        }
        t.content = content;
        this.save();
        console.log(`✅ Template '${name}' updated.`);
    }

    generate(templateName, recipient, date, course, output) {
        const t = this.getTemplate(templateName);
        if (!t) {
            console.log(`❌ Template '${templateName}' not found.`);
            return;
        }
        const dateStr = date || new Date().toISOString().slice(0,10);
        const courseStr = course || 'the course';
        let content = t.content;
        content = content.replace(/\{\{name\}\}/g, recipient);
        content = content.replace(/\{\{date\}\}/g, dateStr);
        content = content.replace(/\{\{course\}\}/g, courseStr);

        if (output) {
            fs.writeFileSync(output, content);
            console.log(`✅ Certificate generated for ${recipient} -> ${output}`);
        } else {
            console.log(`\n📜 Certificate for ${recipient}:\n`);
            console.log(content);
        }
    }
}

program
    .command('template create <name>')
    .option('--desc <description>', 'Description')
    .action((name, options) => {
        const app = new App();
        app.create(name, options.desc || '');
    });

program
    .command('template list')
    .action(() => {
        const app = new App();
        app.list();
    });

program
    .command('template show <name>')
    .action((name) => {
        const app = new App();
        app.show(name);
    });

program
    .command('template edit <name>')
    .option('--content <content>', 'New template content')
    .action((name, options) => {
        if (!options.content) {
            console.error('--content is required');
            return;
        }
        const app = new App();
        app.edit(name, options.content);
    });

program
    .command('certificate generate <template> <recipient>')
    .option('--date <date>', 'YYYY-MM-DD')
    .option('--course <course>', 'Course name')
    .option('--output <file>', 'Output file')
    .action((template, recipient, options) => {
        const app = new App();
        app.generate(template, recipient, options.date, options.course, options.output);
    });

program.parse(process.argv);
