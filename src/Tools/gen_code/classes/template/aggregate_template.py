from classes.template.template import Template


class AggregateTemplate(Template):

	src_path = None

	def __init__(self, templates_path, src_path):
		self.src_path = src_path
		super().__init__(templates_path)

	def render(self, definition):
		filename = f"{definition.name}.cs"
		path = f"{self.src_path}/Domain/Model/{definition.name}/{filename}"
		code = super().render(
			"aggregate", 
			f"aggregate.cs", 
			{
				'class_name': f"{definition.name}",
				'obj_var_name': self._first_char_lower(definition.name),
				'property_expressions': self._property_expressions(definition),
				'property_method_args': self._property_method_args(definition),
				'property_init_args': self._property_init_args(definition)
			},
			filename,
			path)
		return code

	def _property_expressions(self, definition):
		prop_expressions = []
		for prop in definition.properties:
			if prop.name != f"{definition.name}Id":
				expr = f"public {prop.type_} {prop.name} {{ get; set; }}"
				prop_expressions.append(expr)
		return "\n        ".join(prop_expressions)

	def _property_method_args(self, definition):
		method_args = []
		for prop in definition.properties:
			arg = f"{prop.type_} {self._first_char_lower(prop.name)}"
			method_args.append(arg)
		method_args.append("ActionId actionId")
		return ",\n            ".join(method_args)

	def _property_init_args(self, definition):
		init_args = ["DomainModelVersion = ContextDomainModelVersion.Latest()"]
		for prop in definition.properties:
			arg = f"{prop.name} = {self._first_char_lower(prop.name)}"
			init_args.append(arg)
		return ",\n                    ".join(init_args)

	def _first_char_lower(self, string):
		return string[0].lower() + string[1:]
