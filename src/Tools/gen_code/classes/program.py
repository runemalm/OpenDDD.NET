from classes.argument_parser import ArgumentParser
from classes.definition.definition_parser import DefinitionParser
from classes.generator import Generator
from classes.writer import Writer
from os.path import exists


class Program:
	arguments = None
	generator = None
	definitions = None
	writer = None

	def __init__(self):
		pass

	def run(self, raw_args):

		self.arguments = ArgumentParser().parse(raw_args)
		self.definitions = DefinitionParser().parse(self.arguments.def_path)
		self.generator = Generator(self.arguments.templates_path, self.arguments.src_path)
		self.writer = Writer()

		code = []
		
		if self.arguments.operation == "list":
			self._list()
		elif self.arguments.operation == "gen":
			if self.arguments.what == "all":
				self._gen_all()
			elif self.arguments.what == "action":
				name = self.arguments.name
				code.append(self._gen_action(name))
			elif self.arguments.what == "aggregate":
				name = self.arguments.name
				code.append(self._gen_aggregate(name))
			elif self.arguments.what == "command":
				name = self.arguments.name
				code.append(self._gen_command(name))
			elif self.arguments.what == "entity_id":
				name = self.arguments.name
				code.append(self._gen_entity_id(name))
			elif self.arguments.what == "http_command_translator":
				name = self.arguments.name
				version = self.arguments.version
				code.append(self._gen_http_command_translator(name, version))
			elif self.arguments.what == "http_command":
				name = self.arguments.name
				version = self.arguments.version
				code.append(self._gen_http_command(name, version))
			elif self.arguments.what == "http_endpoint":
				name = self.arguments.name
				code.append(self._gen_http_endpoint(name))
			else:
				raise Exception(f"Don't know how to generate a '{self.arguments.what}'.")

		for c in code:
			if c.what in ["action", "aggregate", "command", "entity_id", "http_command_translator", "http_command"]:
				if not exists(c.path) or input(f"Overwrite '{c.path}'? (y/n)") in ["y", "yes"]:
					self.writer.write(c)
			elif c.what in ["http_endpoint"]:
				self.writer.output(c)
			else:
				raise Exception(f"Don't know how to generate a '{c.what}'.")

	def _list(self):
		raise NotImplementedError()

	def _gen_all(self):
		raise NotImplementedError()

	def _gen_action(self, name):
		definition = self.definitions.get("action", name)
		if not definition:
			raise Exception(f"No action definition found in yaml file called '{name}'.")
		code = self.generator.gen_action(definition)
		return code

	def _gen_aggregate(self, name):
		definition = self.definitions.get("aggregate", name)
		if not definition:
			raise Exception(f"No aggregate definition found in yaml file called '{name}'.")
		code = self.generator.gen_aggregate(definition)
		return code

	def _gen_command(self, name):
		definition = self.definitions.get("command", name)
		if not definition:
			raise Exception(f"No command definition found in yaml file called '{name}'.")
		code = self.generator.gen_command(definition)
		return code

	def _gen_entity_id(self, name):
		definition = self.definitions.get("entity_id", name)
		if not definition:
			raise Exception(f"No entity ID definition found in yaml file called '{name}'.")
		code = self.generator.gen_entity_id(definition)
		return code

	def _gen_http_command_translator(self, name, version):
		command_definition = self.definitions.get("command", name)
		if not command_definition:
			raise Exception(f"No command definition found in yaml file called '{name}'.")

		command_translator_definition = self.definitions.get("http_command_translator", name, version)
		if not command_translator_definition:
			raise Exception(f"No command translator definition found in yaml file called '{name}' with version '{version}'.")
		
		code = self.generator.gen_http_command_translator(command_definition, command_translator_definition)
		
		return code

	def _gen_http_command(self, name, version):
		definition = self.definitions.get("http_command", name, version)
		if not definition:
			raise Exception(f"No http adapter command definition found in yaml file called '{name}' with version '{version}'.")
		code = self.generator.gen_http_command(definition)
		return code

	def _gen_http_endpoint(self, name):
		definition = self.definitions.get("http_endpoint", name)
		if not definition:
			raise Exception(f"No endpoint definition found in yaml file called '{name}'.")
		code = self.generator.gen_http_endpoint(definition)
		return code
