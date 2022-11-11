from classes.template.action_template import ActionTemplate
from classes.template.aggregate_migrator_template import AggregateMigratorTemplate
from classes.template.aggregate_template import AggregateTemplate
from classes.template.command_template import CommandTemplate
from classes.template.command_translator_template import CommandTranslatorTemplate
from classes.template.entity_id_template import EntityIdTemplate
from classes.template.http_bb_translator_template import HttpBbTranslatorTemplate
from classes.template.http_adapter_command_template import HttpAdapterCommandTemplate
from classes.template.endpoint_template import EndpointTemplate


class Generator:

  templates_path = None
  src_path = None
  
  def __init__(self, templates_path, src_path):
    self.templates_path = templates_path
    self.src_path = src_path

  def gen_action(self, definition):
    template = ActionTemplate(self.templates_path, self.src_path)
    code = template.render(definition)
    return code

  def gen_aggregate(self, definition):
    template = AggregateTemplate(self.templates_path, self.src_path)
    code = template.render(definition)
    return code

  def gen_aggregate_migrator(self, definition):
    template = AggregateMigratorTemplate(self.templates_path, self.src_path)
    code = template.render(definition)
    return code

  def gen_command(self, definition):
    template = CommandTemplate(self.templates_path, self.src_path)
    code = template.render(definition)
    return code

  def gen_entity_id(self, definition):
    template = EntityIdTemplate(self.templates_path, self.src_path)
    code = template.render(definition)
    return code

  def gen_http_bb_translator(self, definition):
    template = HttpBbTranslatorTemplate(self.templates_path, self.src_path)
    code = template.render(definition)
    return code

  def gen_http_command(self, definition):
    template = HttpAdapterCommandTemplate(self.templates_path, self.src_path)
    code = template.render(definition)
    return code

  def gen_http_command_translator(self, command_definition, translator_definition):
    template = CommandTranslatorTemplate(self.templates_path, self.src_path)
    code = template.render(command_definition, translator_definition)
    return code

  def gen_http_endpoint(self, definition):
    template = EndpointTemplate(self.templates_path, self.src_path)
    code = template.render(definition)
    return code
