from classes.template.template import Template


class EntityIdTemplate(Template):

	src_path = None

	def __init__(self, templates_path, src_path):
		self.src_path = src_path
		super().__init__(templates_path)

	def render(self, definition):
		filename = f"{definition.name}.cs"
		path = f"{self.src_path}/Domain/Model/{filename}"
		if definition.entity_is_aggregate:
			path = f"{self.src_path}/Domain/Model/{definition.entity_name}/{filename}"

		namespace = "namespace Domain.Model"
		if definition.entity_is_aggregate:
			namespace = f"namespace Domain.Model.{definition.entity_name}"

		code = super().render(
			"entity_id", 
			f"entity_id.cs", 
			{
				'namespace': namespace,
				'class_name': f"{definition.name}",
				'obj_var_name': f"{self._first_char_lower(definition.name)}"
			},
			filename,
			path)
		return code

	def _first_char_lower(self, string):
		return string[0].lower() + string[1:]
