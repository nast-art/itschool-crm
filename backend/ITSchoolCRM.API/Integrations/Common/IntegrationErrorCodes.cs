using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.Integrations.Common
{
    public static class IntegrationErrorCodes
    {
        public const string LmsUnavailable = "INTEGRATION_LMS_UNAVAILABLE";

        public const string LmsBadResponse = "INTEGRATION_LMS_BAD_RESPONSE";

        public const string WebsiteUnavailable = "INTEGRATION_WEBSITE_UNAVAILABLE";

        public const string WebsiteBadResponse = "INTEGRATION_WEBSITE_BAD_RESPONSE";

        public const string ConfigurationInvalid = "INTEGRATION_CONFIGURATION_INVALID";
    }
}