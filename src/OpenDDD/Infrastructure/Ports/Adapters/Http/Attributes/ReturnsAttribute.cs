using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OpenDDD.Infrastructure.Ports.Adapters.Http.Common
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReturnsAttribute : ProducesResponseTypeAttribute
    {
        public Type ObjectType;

        public ReturnsAttribute(Type objectType) : base(objectType, StatusCodes.Status200OK)
        {
            ObjectType = objectType;
        }
    }
}
