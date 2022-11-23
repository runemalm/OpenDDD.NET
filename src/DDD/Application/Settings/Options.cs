namespace DDD.Application.Settings
{
	public class Options
	{
		public string GENERAL_CONTEXT { get; set; }

		public string AUTH_ENABLED { get; set; }
		public string AUTH_JWT_TOKEN_PRIVATE_KEY { get; set; }
		public string AUTH_JWT_TOKEN_NAME { get; set; }
		public string AUTH_JWT_TOKEN_LOCATION { get; set; }
		public string AUTH_JWT_TOKEN_SCHEME { get; set; }
		public string EMAIL_ENABLED { get; set; }
		public string EMAIL_PROVIDER { get; set; }
		public string EMAIL_SMTP_HOST { get; set; }
		public string EMAIL_SMTP_PORT { get; set; }
		public string HTTP_URLS { get; set; }
		public string HTTP_CORS_ALLOWED_ORIGINS { get; set; }
		public string HTTP_DOCS_DESCRIPTION { get; set; }
		public string HTTP_DOCS_ENABLED { get; set; }
		public string HTTP_DOCS_DEFINITIONS { get; set; }
		public string HTTP_DOCS_AUTH_EXTRA_TOKENS { get; set; }
		public string HTTP_DOCS_HTTP_ENABLED { get; set; }
		public string HTTP_DOCS_HTTPS_ENABLED { get; set; }
		public string HTTP_DOCS_HOSTNAME { get; set; }
		public string HTTP_DOCS_TITLE { get; set; }

		public string MONITORING_PROVIDER { get; set; }

		public string PERSISTENCE_PROVIDER { get; set; }
		
		public string POSTGRES_CONN_STR { get; set; }

		public string PUBSUB_PROVIDER { get; set; }
		public string PUBSUB_CLIENT { get; set; }
		public string PUBSUB_INTERCHANGE_TOPIC { get; set; }
		public string PUBSUB_DOMAIN_TOPIC { get; set; }
		public string PUBSUB_MAX_DELIVERY_RETRIES { get; set; }
		public string PUBSUB_MAX_PUBLISHING_RETRIES { get; set; }
		public string PUBSUB_PUBLISHER_ENABLED { get; set; }

		public string RABBIT_HOST { get; set; }
		public string RABBIT_PORT { get; set; }
		public string RABBIT_USERNAME { get; set; }
		public string RABBIT_PASSWORD { get; set; }

		public string SERVICEBUS_CONN_STRING { get; set; }
		public string SERVICEBUS_SUB_NAME { get; set; }
	}
}
