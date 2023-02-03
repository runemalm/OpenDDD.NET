from classes.template.template import Template
from classes.utils import Utils


class EndpointTemplate(Template):

	src_path = None

	def __init__(self, templates_path, src_path):
		self.src_path = src_path
		super().__init__(templates_path)

	def render(self, definition):
		filename = f"HttpAdapter.cs"
		path = (
			f"{self.src_path}/Infrastructure/Ports/Adapters/Http/"
			f"{Utils.Utils.dot_version_to_vx(definition.version)}/"
			f"{filename}")
		code = super().render(
			"http_endpoint",
			"endpoint.cs",
			{
				'action_name': definition.name,
				'command_name': f"{definition.name}",
				'return_type': self._header_return_type(definition),
				'swagger_doc_defs_attribs': self._header_swagger_doc_defs(definition),
				'docs_section': definition.doc_section,
				'docs_desc': definition.doc_desc,
				'docs_returns': definition.doc_returns,
				'method': self._header_method(definition),
				'param_location': self._header_param_location(definition),
				'command_translator_var_name': self._command_translator_var_name(definition),
				'action_var_name': self._action_var_name(definition),
				'return_value_translator_var_name': self._return_value_translator_var_name(definition),
				'action_param_var_name': self._action_param_var_name(definition),
				'command_translator_param_var_name': self._command_translator_param_var_name(definition),
				'path': self._header_path(definition),
				'Vx': Utils.dot_version_to_Vx(definition.version)
			},
			filename,
			path)
		return code

	def _header_path(self, definition):
		return Utils.camel_to_snake_hyphen(definition.name)

	def _header_swagger_doc_defs(self, definition):
		attribs = []
		for attrib in definition.swagger_doc_defs:
			attribs.append(f"[{attrib}]")
		return "\n		".join(attribs)

	def _header_return_type(self, definition):
		if definition.returns == None:
			return "null"
		return f"typeof({definition.returns})"

	def _header_method(self, definition):
		if definition.method.lower() == "get":
			return "HttpGet"
		elif definition.method.lower() == "post":
			return "HttpPost"
		elif definition.method.lower() == "put":
			return "HttpPut"
		elif definition.method.lower() == "delete":
			return "HttpDelete"
		raise Exception(f"Unsupported method: '{definition.method}'.")

	def _header_param_location(self, definition):
		if definition.method.lower() == "post":
			return "FromBody"
		return "FromQuery"

	def _command_translator_var_name(self, definition):
		name = f"_{definition.name}CommandTranslator"
		name = name[0] + name[1].lower() + name[2:]
		return name

	def _action_var_name(self, definition):
		name = f"_{definition.name}Action"
		name = name[0] + name[1].lower() + name[2:]
		return name

	def _return_value_translator_var_name(self, definition):
		if definition.returns == None:
			return None
		returns = Utils.strip_api_version_suffix(definition.returns)
		name = f"_{returns}Translator"
		name = name[0] + name[1].lower() + name[2:]
		return name

	def _action_param_var_name(self, definition):
		return self._action_var_name(definition)[1:]

	def _command_translator_param_var_name(self, definition):
		return self._command_translator_var_name(definition)[1:]
