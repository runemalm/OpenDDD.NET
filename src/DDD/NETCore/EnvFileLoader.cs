using System;
using System.Collections.Generic;
using System.IO;
using dotenv.net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DDD.NETCore
{
	public class EnvFileLoader
	{
		/*
         * EnvFileLoader is used to load configuration from env file.
         * 
         * If environment variable is set
         * and contains env file as json, it will be loaded.
         * 
         * If environment variable is set
         * and contains filename of env file as root, it will be loaded.
         */
		private string _defaultEnvFileVariableValue;
		private bool _overwriteExisting;
		private string _envVarName;
		private IConfiguration _config;

		public EnvFileLoader(
			string envVarName,
			IConfiguration config = null)
		{
			_envVarName = envVarName;
			_config = config;
		}

		public void Load(string defaultEnvFileVariableValue = "", bool overwriteExisting = true)
		{
			_defaultEnvFileVariableValue = defaultEnvFileVariableValue;
			_overwriteExisting = overwriteExisting;

			if (ShouldLoadEnvFile())
				LoadEnvFile();
		}

		private bool ShouldLoadEnvFile()
		{
			return EnvFileExists() || EnvJsonExists();
		}

		private bool EnvFileExists()
			=> File.Exists(GetEnvFilePath());

		private bool EnvJsonExists()
		{
			return EnvJsonExistsInVariable() || EnvJsonExistsInConfig();
		}

		private bool EnvJsonExistsInVariable()
		{
			return GetEnvJsonFromVariable() != null;
		}

		private bool EnvJsonExistsInConfig()
		{
			return GetEnvJsonFromConfig() != null;
		}

		private JObject GetEnvJsonFromVariable()
		{
			try
			{
				return JObject.Parse(GetEnvFileValue());
			}
			catch (JsonReaderException)
			{

			}
			return null;
		}

		private JObject GetEnvJsonFromConfig()
		{
			try
			{
				return JObject.Parse(GetConfigValue());
			}
			catch (JsonReaderException)
			{

			}
			return null;
		}

		private string GetEnvFileValue()
		{
			var value = Environment.GetEnvironmentVariable(_envVarName) ?? _defaultEnvFileVariableValue;
			return value ?? "";
		}

		private string GetConfigValue()
		{
			if (_config == null)
				return null;

			var value = _config.GetValue<string>(_envVarName);

			return value ?? "";
		}

		private string GetEnvFilePath()
		{
			var filename = GetEnvFileValue();
			var pathRoot = Path.GetPathRoot(Directory.GetCurrentDirectory());

			var dir = Directory.GetCurrentDirectory().ToString();
			var path = $"{dir}/{filename}";
			bool found = File.Exists(path);

			while (!found && dir != pathRoot)
			{
				dir = Directory.GetParent(dir).ToString();
				path = dir != "/" ? $"{dir}/{filename}" : $"/{filename}";
				found = File.Exists(path);
			}

			return path;
		}

		private void LoadEnvFile()
		{
			if (EnvJsonExists())
			{
				SetEnvVariables(GetEnvDict());
			}
			else if (EnvFileExists())
			{
				var opts = 
					DotEnv.Fluent()
						.WithExceptions()
						.WithEnvFiles(GetEnvFilePath())
						.WithTrimValues();

				if (_overwriteExisting)
					opts = opts.WithOverwriteExistingVars();
				else
					opts = opts.WithoutOverwriteExistingVars();
				
				opts.Load();
			}
		}

		private IDictionary<string, string> GetEnvDict()
		{
			var envDict = new Dictionary<string, string>();

			JObject envJson;

			if (EnvJsonExistsInVariable())
				envJson = GetEnvJsonFromVariable();
			else
				envJson = GetEnvJsonFromConfig();

			foreach (var var in envJson)
				envDict.Add(var.Key.ToString(), var.Value.ToString());

			return envDict;
		}

		private void SetEnvVariables(IDictionary<string, string> envDict)
		{
			foreach (var envVar in envDict)
				Environment.SetEnvironmentVariable(envVar.Key, envVar.Value);
		}
	}
}
