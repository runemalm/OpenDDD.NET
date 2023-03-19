﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application.Settings;
using DDD.Infrastructure.Ports.Email;
using DDD.Logging;

namespace DDD.Infrastructure.Ports.Adapters.Email
{
	public abstract class EmailAdapter : IEmailPort
	{
		protected readonly ISettings _settings;
		protected readonly ILogger _logger;
		private List<IEmail> _sent = new List<IEmail>();

		public EmailAdapter(ISettings settings, ILogger logger)
		{
			_settings = settings;
			_logger = logger;
		}
		
		public abstract Task SendAsync(
			string fromEmail, string fromName, string toEmail, string toName, string subject,
			string message, bool isHtml, CancellationToken ct);

		public Task<bool> HasSent(string toEmail)
			=> HasSent(toEmail, null);

		public Task<bool> HasSent(string toEmail, string? msgContains)
			=> Task.FromResult(_sent.Any(s => s.ToEmail == toEmail && (msgContains == null || s.Message.Contains(msgContains))));

		public Task EmptyAsync(CancellationToken ct)
		{
			_sent = new List<IEmail>();
			return Task.CompletedTask;
		}

		protected Task AddToLog(IEmail email)
		{
			_sent.Add(email);
			return Task.CompletedTask;
		}
	}
}