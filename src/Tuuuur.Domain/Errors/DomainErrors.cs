namespace Tuuuur.Domain.Errors
{
    public static class DomainErrors
    {
        private const string DOMAIN_PREFIX = "Tuuuur.";

        public const string UnknowError = DOMAIN_PREFIX + "unknown-error";

        public static class Data
        {
            private const string DATA_PREFIX = DOMAIN_PREFIX + "data.";

            public const string NotFound = DATA_PREFIX + "notfound";

            public const string AlreadyExist = DATA_PREFIX + "alreadyexist";

            public const string IsNull = DATA_PREFIX + "isnull";
        }
    }

    internal static class GeneralErrors
    {
        public const string Empty = "empty";
        public const string Duplicated = "duplicated";

        public static class InvalidFormat
        {
            public const string InvalidEmail = "invalid-email";
            public const string InvalidLength = "invalid-length";
            public const string InvalidUppercase = "invalid-uppercase";
            public const string InvalidLowercase = "invalid-lowercase";
            public const string InvalidNumber = "invalid-number";
        }
    }
}