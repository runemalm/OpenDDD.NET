from classes.template.template import Template
from classes.utils import Utils


class HttpAdapterCommandTemplate(Template):

	src_path = None

	def __init__(self, templates_path, src_path):
		self.src_path = src_path
		super().__init__(templates_path)

	def render(self, definition):
		api_version = Utils.dot_version_to_api_version(definition.version)
		filename = f"{definition.name}Command_{api_version}.cs"
		path = f"{self.src_path}/Infrastructure/Ports/Adapters/Http/{api_version}/Model/Commands/{filename}"
		code = super().render(
			"http_command", 
			f"http_adapter_command.cs", 
			{
				'class_name': f"{definition.name}Command_{api_version}",
				'api_version': api_version,
				'param_expressions': self._param_expressions(definition)
			},
			filename,
			path)
		return code

	def _param_expressions(self, definition):
		param_expressions = []
		for param in definition.params:
			expr = f"public {param.type_} {param.name} {{ get; set; }}"
			param_expressions.append(expr)
		return "\n        ".join(param_expressions)
