from jinja2 import Environment, PackageLoader, select_autoescape, FileSystemLoader
from classes.code import Code


class Template:

	templates_path = None
	jinja_env = None

	def __init__(self, templates_path):
		self.templates_path = templates_path
		self.jinja_env = Environment(
		    loader=FileSystemLoader(self.templates_path),
		    autoescape=select_autoescape()
		)

	def render(self, what, tpl_filename, variables, filename, path):
		template = self.jinja_env.get_template(tpl_filename)

		code = Code(
			what,
			template.render(**variables),
			filename,
			path)

		return code
