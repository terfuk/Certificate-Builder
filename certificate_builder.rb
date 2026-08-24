# certificate_builder.rb
#!/usr/bin/env ruby
require 'json'
require 'securerandom'
require 'date'

DATA_FILE = 'templates.json'

class Template
  attr_accessor :id, :name, :description, :content, :created_at

  def initialize(name, description = '', content = '')
    @id = SecureRandom.hex(4)
    @name = name
    @description = description
    @content = content.empty? ? 'This certifies that {{name}} has completed {{course}} on {{date}}.' : content
    @created_at = Time.now.iso8601
  end

  def to_hash
    { id: @id, name: @name, description: @description, content: @content, created_at: @created_at }
  end

  def self.from_hash(h)
    t = new(h['name'], h['description'], h['content'])
    t.id = h['id']
    t.created_at = h['created_at']
    t
  end
end

class App
  attr_reader :templates

  def initialize
    @templates = []
    load
  end

  def load
    if File.exist?(DATA_FILE)
      data = JSON.parse(File.read(DATA_FILE))
      @templates = data.map { |h| Template.from_hash(h) }
    end
  end

  def save
    File.write(DATA_FILE, JSON.pretty_generate(@templates.map(&:to_hash)))
  end

  def get_template(name)
    @templates.find { |t| t.name == name }
  end

  def create(name, description = '')
    if get_template(name)
      puts "❌ Template '#{name}' already exists."
      return
    end
    t = Template.new(name, description)
    @templates << t
    save
    puts "✅ Template created: #{t.name} (ID: #{t.id})"
  end

  def list
    if @templates.empty?
      puts "No templates."
      return
    end
    puts "\n📋 Templates:"
    @templates.each_with_index do |t, i|
      desc = t.description.empty? ? 'No description' : t.description
      puts "#{i+1}. #{t.name} (#{desc})"
    end
  end

  def show(name)
    t = get_template(name)
    unless t
      puts "❌ Template '#{name}' not found."
      return
    end
    puts "\n📄 Template: #{t.name}"
    puts "Description: #{t.description.empty? ? 'None' : t.description}"
    puts "Content:"
    puts t.content
  end

  def edit(name, content)
    t = get_template(name)
    unless t
      puts "❌ Template '#{name}' not found."
      return
    end
    t.content = content
    save
    puts "✅ Template '#{name}' updated."
  end

  def generate(template_name, recipient, date = nil, course = nil, output = nil)
    t = get_template(template_name)
    unless t
      puts "❌ Template '#{template_name}' not found."
      return
    end
    date_str = date || Date.today.to_s
    course_str = course || 'the course'
    content = t.content.gsub('{{name}}', recipient)
                       .gsub('{{date}}', date_str)
                       .gsub('{{course}}', course_str)

    if output
      File.write(output, content)
      puts "✅ Certificate generated for #{recipient} -> #{output}"
    else
      puts "\n📜 Certificate for #{recipient}:\n"
      puts content
    end
  end
end

if ARGV.empty?
  puts "Usage: certificate_builder.rb <command> [options]"
  exit
end

app = App.new
cmd = ARGV.shift

case cmd
when 'template'
  sub = ARGV.shift
  case sub
  when 'create'
    name = ARGV.shift
    if name.nil?
      puts "template create <name> [--desc DESCRIPTION]"
      exit
    end
    desc = ''
    if ARGV.include?('--desc')
      idx = ARGV.index('--desc')
      desc = ARGV[idx+1] if idx
    end
    app.create(name, desc)

  when 'list'
    app.list

  when 'show'
    name = ARGV.shift
    if name.nil?
      puts "template show <name>"
      exit
    end
    app.show(name)

  when 'edit'
    if ARGV.size < 2
      puts "template edit <name> --content CONTENT"
      exit
    end
    name = ARGV.shift
    content = nil
    if ARGV.include?('--content')
      idx = ARGV.index('--content')
      content = ARGV[idx+1] if idx
    end
    if content.nil?
      puts "--content is required"
      exit
    end
    app.edit(name, content)

  else
    puts "Unknown template subcommand"
  end

when 'certificate'
  sub = ARGV.shift
  if sub != 'generate'
    puts "certificate generate <template> <recipient> [--date DATE] [--course COURSE] [--output FILE]"
    exit
  end
  template = ARGV.shift
  recipient = ARGV.shift
  if template.nil? || recipient.nil?
    puts "certificate generate requires template and recipient"
    exit
  end
  date = nil
  course = nil
  output = nil
  while ARGV.any?
    case ARGV[0]
    when '--date'
      ARGV.shift
      date = ARGV.shift
    when '--course'
      ARGV.shift
      course = ARGV.shift
    when '--output'
      ARGV.shift
      output = ARGV.shift
    else
      break
    end
  end
  app.generate(template, recipient, date, course, output)

else
  puts "Unknown command."
end
