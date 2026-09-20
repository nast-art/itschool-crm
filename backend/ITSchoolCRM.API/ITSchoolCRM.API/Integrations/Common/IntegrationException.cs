using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.Integrations.Common
{
  public class IntegrationException : Exception
    {
        public IntegrationException(
            string errorCode,
            string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public IntegrationException(
            string errorCode,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; }
    }
}