import re


class Utils(object):
	
	@classmethod
	def camel_to_snake_hyphen(cls, string):
		string = re.sub('(.)([A-Z][a-z]+)', r'\1-\2', string)
		string = re.sub('([a-z0-9])([A-Z])', r'\1-\2', string)
		return string.lower() 

	@classmethod
	def dot_version_to_api_version(cls, string):
		return f"V{Utils.dot_version_to_major_version(string)}"

	@classmethod
	def dot_version_to_major_version(cls, string):
		return int(string.split(".")[0])

	@classmethod
	def strip_api_version_suffix(cls, string):
		return re.sub('([A-Z][a-z]+)(_v[0-9]_[0-9]_[0-9])', r'\1', string)

	@classmethod
	def is_http_version_string(cls, string):
		pattern = re.compile(r'([0-9])\.([0-9])\.([0-9])')
		return pattern.match(string)
