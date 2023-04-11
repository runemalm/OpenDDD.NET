from classes.template.template import Template
from classes.utils import Utils


class HttpAdapterCommandTemplate(Template):

	src_path = None

	def __init__(self, templates_path, src_path):
		self.src_path = src_path
		super().__init__(templates_path)

	def render(self, definition):
		vx = Utils.dot_version_to_vx(definition.version)
		Vx = Utils.dot_version_to_Vx(definition.version)
		filename = f"{definition.name}Command{Vx}.cs"
		path = f"{self.src_path}/Infrastructure/Ports/Adapters/Http/{vx}/Model/Commands/{filename}"
		code = super().render(
			"http_command", 
			f"http_adapter_command.cs", 
			{
				'class_name': f"{definition.name}Command{Vx}",
				'vx': vx,
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
