import argparse
import sys
from classes.arguments import Arguments


class ArgumentParser:
  
  def __init__(self):
    pass

  def parse(self, raw_args):

    # Parse
    parser = argparse.ArgumentParser(description='Generate code from domain.yml.')

    subparsers = parser.add_subparsers(help='commands', dest='command')

    l_parser = subparsers.add_parser("list")
    g_parser = subparsers.add_parser("gen")

    gen_subparsers = g_parser.add_subparsers(help='what to generate', dest='what')
    
    gen_all_parser = gen_subparsers.add_parser("all")
    gen_action_parser = gen_subparsers.add_parser("action")
    gen_aggregate_parser = gen_subparsers.add_parser("aggregate")
    gen_aggregate_migrator_parser = gen_subparsers.add_parser("aggregate_migrator")
    gen_command_parser = gen_subparsers.add_parser("command")
    gen_entity_id_parser = gen_subparsers.add_parser("entity_id")
    gen_http_bb_translator_parser = gen_subparsers.add_parser("http_bb_translator")
    gen_http_command_translator_parser = gen_subparsers.add_parser("http_command_translator")
    gen_http_command_parser = gen_subparsers.add_parser("http_command")
    gen_http_endpoint_parser = gen_subparsers.add_parser("http_endpoint")

    gen_action_parser.add_argument("name", nargs=1, help='the name of the action')
    gen_aggregate_parser.add_argument("name", nargs=1, help='the name of the aggregate')
    gen_aggregate_migrator_parser.add_argument("name", nargs=1, help='the name of the aggregate')
    gen_command_parser.add_argument("name", nargs=1, help='the name of the command')
    gen_entity_id_parser.add_argument("name", nargs=1, help='the name of the entity ID')
    gen_http_bb_translator_parser.add_argument("name", nargs=1, help='the name of the building block')
    gen_http_command_translator_parser.add_argument("name", nargs=1, help='the name of the command')
    gen_http_command_translator_parser.add_argument("version", nargs=1, help='the api version')
    gen_http_command_parser.add_argument("name", nargs=1, help='the name of the command')
    gen_http_command_parser.add_argument("version", nargs=1, help='the api version')
    gen_http_endpoint_parser.add_argument("name", nargs=1, help='the name of the endpoint')

    args = parser.parse_args()

    # Construct
    arguments = Arguments()
    arguments.def_path = "/tools/gen_code/domain.yml"
    arguments.src_path = "/context/src"
    arguments.templates_path = "/tools/gen_code/templates"
    arguments.operation = args.command
    arguments.what = args.what if 'what' in args else None
    arguments.name = args.name[0] if 'name' in args else None
    arguments.version = args.version[0] if 'version' in args else None

    return arguments
