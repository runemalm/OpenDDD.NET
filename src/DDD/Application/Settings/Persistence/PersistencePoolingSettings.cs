﻿using DDD.NETCore.Extensions;
using Microsoft.Extensions.Options;

namespace DDD.Application.Settings.Persistence
{
	public class PersistencePoolingSettings : IPersistencePoolingSettings
	{
		public bool Enabled { get; }
		public int MinSize { get; }
		public int MaxSize { get; }

		public PersistencePoolingSettings() { }

		public PersistencePoolingSettings(IOptions<Options> options)
		{
			Enabled = options.Value.PERSISTENCE_POOLING_ENABLED.IsTrue();
			MinSize = options.Value.CFG_PERSISTENCE_POOLING_MIN_SIZE.IntValue();
			MaxSize = options.Value.CFG_PERSISTENCE_POOLING_MAX_SIZE.IntValue();
		}
	}
}