from classes.template.template import Template


class ActionTemplate(Template):

	src_path = None

	def __init__(self, templates_path, src_path):
		self.src_path = src_path
		super().__init__(templates_path)

	def render(self, definition):
		filename = f"{definition.name}Action.cs"
		path = f"{self.src_path}/Application/Actions/{filename}"
		code = super().render(
			"action", 
			f"action.cs", 
			{
				'class_name': f"{definition.name}Action",
				'command_name': f"{definition.name}Command",
				'return_class_name': f"{definition.returns}"
			},
			filename,
			path)
		return code
