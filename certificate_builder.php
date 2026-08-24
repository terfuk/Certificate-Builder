# certificate_builder.php
#!/usr/bin/env php
<?php

define('DATA_FILE', 'templates.json');

class Template {
    public $id;
    public $name;
    public $description;
    public $content;
    public $created_at;

    function __construct($name, $description = '', $content = '') {
        $this->id = substr(bin2hex(random_bytes(4)), 0, 8);
        $this->name = $name;
        $this->description = $description;
        $this->content = $content ?: 'This certifies that {{name}} has completed {{course}} on {{date}}.';
        $this->created_at = date('c');
    }

    function toArray() {
        return [
            'id' => $this->id,
            'name' => $this->name,
            'description' => $this->description,
            'content' => $this->content,
            'created_at' => $this->created_at
        ];
    }

    static function fromArray($data) {
        $t = new self($data['name'], $data['description'], $data['content']);
        $t->id = $data['id'];
        $t->created_at = $data['created_at'];
        return $t;
    }
}

class App {
    private $templates = [];

    function __construct() {
        $this->load();
    }

    function load() {
        if (file_exists(DATA_FILE)) {
            $data = json_decode(file_get_contents(DATA_FILE), true);
            $this->templates = array_map(function($d) { return Template::fromArray($d); }, $data);
        }
    }

    function save() {
        $data = array_map(function($t) { return $t->toArray(); }, $this->templates);
        file_put_contents(DATA_FILE, json_encode($data, JSON_PRETTY_PRINT));
    }

    function getTemplate($name) {
        foreach ($this->templates as $t) {
            if ($t->name == $name) return $t;
        }
        return null;
    }

    function create($name, $description = '') {
        if ($this->getTemplate($name)) {
            echo "❌ Template '$name' already exists.\n";
            return;
        }
        $t = new Template($name, $description);
        $this->templates[] = $t;
        $this->save();
        echo "✅ Template created: {$t->name} (ID: {$t->id})\n";
    }

    function list() {
        if (empty($this->templates)) {
            echo "No templates.\n";
            return;
        }
        echo "\n📋 Templates:\n";
        foreach ($this->templates as $i => $t) {
            $desc = $t->description ?: 'No description';
            echo ($i+1) . ". {$t->name} ($desc)\n";
        }
    }

    function show($name) {
        $t = $this->getTemplate($name);
        if (!$t) {
            echo "❌ Template '$name' not found.\n";
            return;
        }
        echo "\n📄 Template: {$t->name}\n";
        echo "Description: " . ($t->description ?: 'None') . "\n";
        echo "Content:\n{$t->content}\n";
    }

    function edit($name, $content) {
        $t = $this->getTemplate($name);
        if (!$t) {
            echo "❌ Template '$name' not found.\n";
            return;
        }
        $t->content = $content;
        $this->save();
        echo "✅ Template '$name' updated.\n";
    }

    function generate($templateName, $recipient, $date = null, $course = null, $output = null) {
        $t = $this->getTemplate($templateName);
        if (!$t) {
            echo "❌ Template '$templateName' not found.\n";
            return;
        }
        $dateStr = $date ?: date('Y-m-d');
        $courseStr = $course ?: 'the course';
        $content = str_replace('{{name}}', $recipient, $t->content);
        $content = str_replace('{{date}}', $dateStr, $content);
        $content = str_replace('{{course}}', $courseStr, $content);

        if ($output) {
            file_put_contents($output, $content);
            echo "✅ Certificate generated for $recipient -> $output\n";
        } else {
            echo "\n📜 Certificate for $recipient:\n\n$content\n";
        }
    }
}

if ($argc < 2) {
    die("Usage: php certificate_builder.php <command> [options]\n");
}
$app = new App();
$cmd = $argv[1];

switch ($cmd) {
    case 'template':
        if ($argc < 3) die("template: create, list, show, edit\n");
        $sub = $argv[2];
        switch ($sub) {
            case 'create':
                if ($argc < 4) die("template create <name> [--desc DESCRIPTION]\n");
                $name = $argv[3];
                $desc = '';
                for ($i=4; $i<$argc; $i++) {
                    if ($argv[$i] == '--desc' && isset($argv[$i+1])) { $desc = $argv[++$i]; }
                }
                $app->create($name, $desc);
                break;

            case 'list':
                $app->list();
                break;

            case 'show':
                if ($argc < 4) die("template show <name>\n");
                $app->show($argv[3]);
                break;

            case 'edit':
                if ($argc < 5) die("template edit <name> --content CONTENT\n");
                $name = $argv[3];
                $content = null;
                for ($i=4; $i<$argc; $i++) {
                    if ($argv[$i] == '--content' && isset($argv[$i+1])) { $content = $argv[++$i]; }
                }
                if ($content === null) die("--content is required\n");
                $app->edit($name, $content);
                break;

            default:
                echo "Unknown template subcommand\n";
        }
        break;

    case 'certificate':
        if ($argc < 4) die("certificate generate <template> <recipient> [--date DATE] [--course COURSE] [--output FILE]\n");
        $sub = $argv[2];
        if ($sub != 'generate') {
            echo "Unknown certificate subcommand\n";
            break;
        }
        $template = $argv[3];
        $recipient = $argv[4] ?? null;
        if (!$recipient) die("recipient required\n");
        $date = $course = $output = null;
        for ($i=5; $i<$argc; $i++) {
            if ($argv[$i] == '--date' && isset($argv[$i+1])) { $date = $argv[++$i]; }
            if ($argv[$i] == '--course' && isset($argv[$i+1])) { $course = $argv[++$i]; }
            if ($argv[$i] == '--output' && isset($argv[$i+1])) { $output = $argv[++$i]; }
        }
        $app->generate($template, $recipient, $date, $course, $output);
        break;

    default:
        echo "Unknown command.\n";
}
?>
