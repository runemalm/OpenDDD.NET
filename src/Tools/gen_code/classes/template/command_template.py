from classes.template.template import Template


class CommandTemplate(Template):

	src_path = None

	def __init__(self, templates_path, src_path):
		self.src_path = src_path
		super().__init__(templates_path)

	def render(self, definition):
		filename = f"{definition.name}Command.cs"
		path = f"{self.src_path}/Application/Actions/Commands/{filename}"
		code = super().render(
			"command", 
			f"command.cs", 
			{
				'class_name': f"{definition.name}Command",
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
