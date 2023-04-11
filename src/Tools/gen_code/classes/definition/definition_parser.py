import yaml
from classes.definition.definitions import Definitions
from classes.definition.action_definition import ActionDefinition
from classes.definition.aggregate_definition import AggregateDefinition
from classes.definition.command_definition import CommandDefinition
from classes.definition.command_translator_definition import CommandTranslatorDefinition
from classes.definition.entity_id_definition import EntityIdDefinition
from classes.definition.http_adapter_command_definition import HttpAdapterCommandDefinition
from classes.definition.http_bb_translator_definition import HttpBbTranslatorDefinition
from classes.definition.endpoint_definition import EndpointDefinition
from classes.param import Param
from classes.property import Property
from classes.utils import Utils


class DefinitionParser:
  
  def __init__(self):
    pass

  def parse(self, path):
    definitions = Definitions()

    with open(path) as f:
      yml =  yaml.safe_load(f)
      
      for d in yml['Actions']:
        action = self._parse_action(d)
        definitions.add(action)
        
        cd = d['Command']
        definitions.add(self._parse_command(cd, action.name))

      for key, vd in yml['Adapters']['Http'].items():
        if Utils.is_http_version_string(key):
          version = key
          for d in vd['Endpoints']:
            endpoint = self._parse_http_endpoint(d, version)
            definitions.add(endpoint)

            cd = d['Command']
            definitions.add(self._parse_http_command(cd, endpoint.name, version))

        elif key == "BuildingBlockTranslators":
          for bt in vd:
            http_bb_translator = self._parse_http_bb_translator(bt)
            definitions.add(http_bb_translator)

        elif key == "CommandTranslators":
          for ct in vd:
            command_translator = self._parse_http_command_translator(ct)
            definitions.add(command_translator)

      for d in yml['Aggregates']:
        aggregate = self._parse_aggregate(d)
        definitions.add(aggregate)

        entity_id = self._parse_entity_id(f"{aggregate.name}Id", aggregate.name, True)
        definitions.add(entity_id)

    return definitions

  def _parse_action(self, d):
    definition = ActionDefinition()

    definition.name = d['Name']
    definition.returns = d['Returns']

    return definition

  def _parse_aggregate(self, d):
    definition = AggregateDefinition()

    definition.name = d['Name']
    properties = []

    for name, type_ in d['Properties'].items():
      prop = Property(name, type_)
      definition.properties.append(prop)

    return definition

  def _parse_command(self, d, action_name):
    definition = CommandDefinition()

    definition.name = f"{action_name}"
    params = []

    for name, type_ in d['Params'].items():
      param = Param(name, type_)
      definition.params.append(param)

    return definition

  def _parse_entity_id(self, entity_id_name, entity_name, entity_is_aggregate):
    definition = EntityIdDefinition()
    definition.name = entity_id_name
    definition.entity_name = entity_name
    definition.entity_is_aggregate = entity_is_aggregate
    return definition

  def _parse_http_bb_translator(self, d):
    definition = HttpBbTranslatorDefinition()

    definition.name = d['Name']

    return definition

  def _parse_http_command_translator(self, d):
    definition = CommandTranslatorDefinition()

    definition.name = d['Name']

    return definition

  def _parse_http_command(self, d, action_name, version):
    definition = HttpAdapterCommandDefinition()

    definition.name = f"{action_name}"
    definition.version = version
    params = []

    for name, type_ in d['Params'].items():
      param = Param(name, type_)
      definition.params.append(param)

    return definition

  def _parse_http_endpoint(self, d, version):
    definition = EndpointDefinition()

    definition.name = d['Name']
    definition.version = version
    definition.swagger_doc_defs = d['SwaggerDocDefs']
    definition.doc_section = d['DocSection']
    definition.doc_desc = d['DocDesc']
    definition.doc_returns = d['DocReturns']
    definition.method = d['Method']
    definition.returns = d['Returns']

    return definition
