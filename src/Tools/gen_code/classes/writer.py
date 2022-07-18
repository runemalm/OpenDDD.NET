import os


class Writer:

  def __init__(self):
    pass

  def write(self, code):
    if (code.what == "action"):
      self._write_action(code)
    elif (code.what == "aggregate"):
      self._write_aggregate(code)
    elif (code.what == "command"):
      self._write_command(code)
    elif (code.what == "entity_id"):
      self._write_entity_id(code)
    elif (code.what == "http_command_translator"):
      self._write_http_command_translator(code)
    elif (code.what == "http_command"):
      self._write_http_command(code)
    else:
      raise Exception(f"Don't know how to write code of '{code.what}'.")

  def _write_action(self, code):
    self._write_file(code.path, code.code)

  def _write_aggregate(self, code):
    self._write_file(code.path, code.code)

  def _write_command(self, code):
    self._write_file(code.path, code.code)

  def _write_entity_id(self, code):
    self._write_file(code.path, code.code)

  def _write_http_command_translator(self, code):
    self._write_file(code.path, code.code)

  def _write_http_command(self, code):
    self._write_file(code.path, code.code)

  def _write_file(self, path, content):
    directory = os.path.dirname(path)

    if not os.path.exists(directory):
      # print("Creating dir:", directory)
      os.makedirs(directory)

    # print(content)

    with open(path, 'w') as out:
      out.write(content + "\n")

  def output(self, code):
    print("\n\n")
    print(code.code)
