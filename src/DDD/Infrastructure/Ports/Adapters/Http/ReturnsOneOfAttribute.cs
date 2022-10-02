using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DDD.Infrastructure.Ports.Adapters.Http
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReturnsOneOfAttribute : ProducesResponseTypeAttribute
    {
        public Type[] ObjectTypes;

        public ReturnsOneOfAttribute(Type[] objectTypes) : base(objectTypes[0], StatusCodes.Status200OK)
        {
            ObjectTypes = objectTypes;
        }
    }
}
