﻿using System.Threading;
using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Ports.Email
{
	public interface IEmailPort
	{
		Task SendAsync(string fromEmail, string fromName, string toEmail, string toName, string subject, string message, bool isHtml, CancellationToken ct);
		bool HasSent(string toEmail);
		bool HasSent(string toEmail, string? msgContains);
		void Empty(CancellationToken ct);
		Task EmptyAsync(CancellationToken ct);
		void SetEnabled(bool enabled);
	}
}