from classes.template.template import Template
from classes.utils import Utils


class CommandTranslatorTemplate(Template):

	src_path = None

	def __init__(self, templates_path, src_path):
		self.src_path = src_path
		super().__init__(templates_path)

	def render(self, command_definition, translator_definition):
		filename = f"{translator_definition.name}CommandTranslator.cs"
		api_version = Utils.dot_version_to_api_version(translator_definition.version)
		path = f"{self.src_path}/Infrastructure/Ports/Adapters/Http/{api_version}/Translation/Commands/{filename}"
		code = super().render(
			"http_command_translator", 
			f"command_translator.cs", 
			{
				'action_name': translator_definition.name,
				'param_expressions': self._param_expressions(command_definition, api_version),
				'api_version': api_version,
				'class_name': f"{translator_definition.name}CommandTranslator"
			},
			filename,
			path)
		return code

	def _param_expressions(self, command_definition, api_version):
		param_expressions = []
		for param in command_definition.params:
			expr = f"// {param.name} = command_{api_version}.{param.name}"
			param_expressions.append(expr)
		return ",\n                ".join(param_expressions)
