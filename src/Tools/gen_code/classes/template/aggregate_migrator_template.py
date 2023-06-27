from classes.template.template import Template


class AggregateMigratorTemplate(Template):

	src_path = None

	def __init__(self, templates_path, src_path):
		self.src_path = src_path
		super().__init__(templates_path)

	def render(self, definition):
		filename = f"{definition.name}Migrator.cs"
		path = f"{self.src_path}/Infrastructure/Ports/Adapters/Repository/Migrators/{filename}"
		code = super().render(
			"aggregate_migrator", 
			f"aggregate_migrator.cs", 
			{
				'aggregate_name': f"{definition.name}",
				'obj_var_name': self._first_char_lower(definition.name),
			},
			filename,
			path)
		return code

	def _first_char_lower(self, string):
		return string[0].lower() + string[1:]
