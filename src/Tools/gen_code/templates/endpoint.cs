
		private readonly {{action_name}}Action {{action_var_name}};
		private readonly {{action_name}}CommandTranslator {{command_translator_var_name}};

		---

		{{action_name}}Action {{action_param_var_name}},
		{{action_name}}CommandTranslator {{command_translator_param_var_name}},

		---

		{{action_var_name}} = {{action_param_var_name}};
		{{command_translator_var_name}} = {{command_translator_param_var_name}};

		---

		/// <remarks>
		/// {{docs_desc}}
		/// </remarks>
		/// <returns>
		/// {{docs_returns}}
		/// </returns>
		{{swagger_doc_defs_attribs}}
		[Section("{{docs_section}}")]
		[{{method}}("{{path}}")]
		[Returns({{return_type}})]
		public async Task<IActionResult> {{action_name}}(
			[{{param_location}}] {{command_name}}Command{{Vx}} command{{Vx}}, CancellationToken ct)
		{
			// var command = {{command_translator_var_name}}.From{{Vx}}(command{{Vx}});
			// {{ 'var xxx = ' if return_type else '' }}await {{action_var_name}}.ExecuteAsync(command, ct);
			{{ '// return Ok('+return_value_translator_var_name+'.To'+Vx+'(xxx));' if return_type != "null" else '// return Ok();' }}
		}