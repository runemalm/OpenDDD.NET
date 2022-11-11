from classes.definition.action_definition import ActionDefinition
from classes.definition.aggregate_definition import AggregateDefinition
from classes.definition.command_definition import CommandDefinition
from classes.definition.command_translator_definition import CommandTranslatorDefinition
from classes.definition.entity_id_definition import EntityIdDefinition
from classes.definition.http_adapter_command_definition import HttpAdapterCommandDefinition
from classes.definition.http_bb_translator_definition import HttpBbTranslatorDefinition
from classes.definition.endpoint_definition import EndpointDefinition


class Definitions:

	definitions = []

	def __init__(self):
		pass

	def add(self, definition):
		self.definitions.append(definition)

	def get(self, what, name, version=None):
		if what not in ["action", "aggregate", "command", "entity_id", "http_bb_translator", "http_command_translator", "http_command", "http_endpoint"]:
			raise Exception(f"Don't know how to get a '{what}'.")

		for definition in self.definitions:
			if what == "action" and type(definition) == ActionDefinition:
				if definition.name == name:
					return definition
			elif what == "aggregate" and type(definition) == AggregateDefinition:
				if definition.name == name:
					return definition
			elif what == "command" and type(definition) == CommandDefinition:
				if definition.name == name:
					return definition
			elif what == "entity_id" and type(definition) == EntityIdDefinition:
				if definition.name == name:
					return definition
			elif what == "http_bb_translator" and type(definition) == HttpBbTranslatorDefinition:
				if definition.name == name:
					return definition
			elif what == "http_command_translator" and type(definition) == CommandTranslatorDefinition:
				if definition.name == name:
					return definition
			elif what == "http_command" and type(definition) == HttpAdapterCommandDefinition:
				if definition.name == name and definition.version == version:
					return definition
			elif what == "http_endpoint" and type(definition) == EndpointDefinition:
				if definition.name == name:
					return definition

		return None
  