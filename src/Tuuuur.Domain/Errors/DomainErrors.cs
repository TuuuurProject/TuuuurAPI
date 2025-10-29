namespace Tuuuur.Domain.Errors
{
    public static class DomainErrors
    {
        private const string DOMAIN_PREFIX = "tuuuur.";

        public const string UnknowError = DOMAIN_PREFIX + "unknown-error";

        public static class Data
        {
            private const string DATA_PREFIX = DOMAIN_PREFIX + "data.";

            public const string NotFound = DATA_PREFIX + "notfound";

            public const string AlreadyExist = DATA_PREFIX + "alreadyexist";

            public const string IsNull = DATA_PREFIX + "isnull";
        }

        public static class Authentication
        {
            private const string AUTHENT_PREFIX = DOMAIN_PREFIX + "authentication.";

            public const string Missmatch = AUTHENT_PREFIX + "missmatch";
            public const string Invalid = AUTHENT_PREFIX + "invalid";

            public static class NickName
            {
                private const string NICKNAME_PREFIX = AUTHENT_PREFIX + "nickname.";
                public const string Invalid_NickName = NICKNAME_PREFIX + Invalid;
            }

            public static class Login
            {
                private const string LOGIN_PREFIX = AUTHENT_PREFIX + "login.";

                public const string Empty = LOGIN_PREFIX + GeneralErrors.Empty;
                public const string InvalidEmail = LOGIN_PREFIX + GeneralErrors.InvalidFormat.InvalidEmail;
                public const string Duplicated = LOGIN_PREFIX + GeneralErrors.Duplicated;
                public const string NotExist = LOGIN_PREFIX + GeneralErrors.NotExist;
            }

            public static class Password
            {
                private const string PASSWORD_PREFIX = AUTHENT_PREFIX + "password.";

                public const string Empty = PASSWORD_PREFIX + GeneralErrors.Empty;
                public const string InvalidLength = PASSWORD_PREFIX + GeneralErrors.InvalidFormat.InvalidLength;
                public const string InvalidUppercase = PASSWORD_PREFIX + GeneralErrors.InvalidFormat.InvalidUppercase;
                public const string InvalidLowercase = PASSWORD_PREFIX + GeneralErrors.InvalidFormat.InvalidLowercase;
                public const string InvalidNumber = PASSWORD_PREFIX + GeneralErrors.InvalidFormat.InvalidNumber;
            }

            public static class Code
            {
                private const string CODE_PREFIX = AUTHENT_PREFIX + "code.";
                
                public const string Empty = CODE_PREFIX + GeneralErrors.Empty;
                public const string InvalidLength = CODE_PREFIX + GeneralErrors.InvalidFormat.InvalidLength;
                public const string TooMuchDemand = CODE_PREFIX + "too-much-demand";
            }
        }

        public static class Party
        {
            private const string PARTY_PREFIX = DOMAIN_PREFIX + "party.";
            
            public static class Answer
            {
                private const string ANSWER_PREFIX = PARTY_PREFIX + "anwser.";
                
                public const string Empty = ANSWER_PREFIX + GeneralErrors.Empty;
            }
            
        }
    }

    internal static class GeneralErrors
    {
        public const string Empty = "empty";
        public const string Duplicated = "duplicated";
        public const string NotExist = "not-exist";

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