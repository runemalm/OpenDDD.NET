import re


class Utils(object):
	
	@classmethod
	def camel_to_snake_hyphen(cls, string):
		string = re.sub('(.)([A-Z][a-z]+)', r'\1-\2', string)
		string = re.sub('([a-z0-9])([A-Z])', r'\1-\2', string)
		return string.lower() 

	@classmethod
	def dot_version_to_api_version(cls, string):
		return f"v{string.replace('.', '_')}"

	@classmethod
	def strip_api_version_suffix(cls, string):
		return re.sub('([A-Z][a-z]+)(_v[0-9]_[0-9]_[0-9])', r'\1', string)

	@classmethod
	def is_wildcard_http_version_string(cls, string):
		pattern = re.compile(r'([0-9])\.([0-9])\.X')
		return pattern.match(string)
