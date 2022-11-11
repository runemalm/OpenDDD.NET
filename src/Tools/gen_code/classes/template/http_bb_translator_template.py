from classes.template.template import Template
from classes.utils import Utils


class HttpBbTranslatorTemplate(Template):

	src_path = None

	def __init__(self, templates_path, src_path):
		self.src_path = src_path
		super().__init__(templates_path)

	def render(self, definition):
		filename = f"{definition.name}Translator.cs"
		path = f"{self.src_path}/Infrastructure/Ports/Adapters/Http/Common/Translation/{filename}"
		code = super().render(
			"http_bb_translator", 
			f"http_bb_translator.cs", 
			{
				'bb_name': f"{definition.name}",
				'obj_var_name': self._first_char_lower(definition.name)
			},
			filename,
			path)
		return code

	def _first_char_lower(self, string):
		return string[0].lower() + string[1:]
