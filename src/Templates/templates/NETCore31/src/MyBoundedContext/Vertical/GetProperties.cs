using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenDDD.Application;
using OpenDDD.Domain.Model.Event;
using OpenDDD.Infrastructure.Ports.Adapters.Http;
using OpenDDD.Infrastructure.Ports.Adapters.Http.Common;
using OpenDDD.Infrastructure.Ports.Adapters.Http.NET;
using OpenDDD.NET;
using OpenDDD.NET.Services.DatabaseConnection;
using OpenDDD.NET.Services.Outbox;
using MyBoundedContext.Domain.Model.Property;
using MyBoundedContext.Domain.Model.Site;
using MyBoundedContext.Infrastructure.Ports.Adapters.Http.Dto;
using MyBoundedContext.Tests;
using Xunit;

namespace MyBoundedContext.Vertical
{
	[Route("v1")]
	[Version("v1.0.0")]
	public class GetPropertiesController : NetHttpAdapter
	{
		private readonly IAction<GetProperties.Command, IEnumerable<Property>> _action;
		private readonly HttpTranslation _httpTranslation;
		
		public GetPropertiesController(GetProperties.Action action, HttpTranslation httpTranslation)
		{
			_action = action;
			_httpTranslation = httpTranslation;
		}

		/// <remarks>
		/// Get all properties.
		/// </remarks>
		/// <returns>
		/// Returns the properties.
		/// </returns>
		[Public]
		[Section("Properties")]
		[HttpGet("properties/get-properties")]
		[Returns(typeof(IEnumerable<HttpTranslation.PropertyV1>))]
		public async Task<IActionResult> GetPropertiesEndpoint([FromQuery] GetProperties.Request request, CancellationToken ct)
		{
			var actionId = ActionId.Create();
			var command = _httpTranslation.FromV1(request);
			var result = await _action.ExecuteTrxAsync(command, actionId, ct);
			return Ok(_httpTranslation.ToV1(result));
		}
	}

	public static class GetProperties
	{
		public class Request : RequestBase
		{
			
		}

		public class Command : CommandBase
		{
			
		}
		
		public class Action : Application.BaseAction<Command, IEnumerable<Property>>
		{
			private readonly IPropertyRepository _propertyRepository;

			public Action(
				IActionDatabaseConnection actionDatabaseConnection,
				IActionOutbox outbox,
				IPropertyRepository propertyRepository,
				IIdealistaPort idealistaAdapter,
				IThailandPropertyPort thailandPropertyAdapter, 
				IDateTimeProvider dateTimeProvider, 
				IDomainPublisher domainPublisher,
				ILogger<Action> logger) 
				: base(actionDatabaseConnection, outbox, idealistaAdapter, thailandPropertyAdapter, dateTimeProvider, domainPublisher, logger)
			{
				_propertyRepository = propertyRepository;
			}

			public override async Task<IEnumerable<Property>> ExecuteAsync(Command command, ActionId actionId, CancellationToken ct)
			{
				var properties = await _propertyRepository.GetAllAsync(actionId, ct);
				return properties;
			}
		}
		
		[Collection("Group A")]
	    public class GetPropertiesTests : BaseActionUnitTests
	    {
		    [Fact]
	        public async Task TestSuccess_Result()
	        {
	            // Arrange
	            await FixtureAddSiteAsync();
	            await FixtureAddPropertyAsync(siteId: Site.SiteId);

	            // Act
	            var command = new Command
	            {
		            
	            };

	            var actual = (await GetPropertiesAction.ExecuteAsync(command, ActionId, CancellationToken.None)).ToList();
	            var expected = Properties.ToList();
	            
	            // Assert
	            AssertEntity(expected, actual);
	        }
	        
	        [Fact]
	        public async Task TestSuccess_Http_Response_Ok()
	        {
		        // Arrange
		        await FixtureAddSiteAsync();
		        await FixtureAddPropertyAsync(siteId: Site.SiteId);
		        
		        // Act
		        var response = 
			        await GetAsync("/v1/properties/get-properties");
		        
		        // Assert
		        AssertSuccessResponse(response);
	        }
	    }
	}
}
