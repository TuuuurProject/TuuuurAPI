using System.Diagnostics.CodeAnalysis;

namespace Tuuuur.Domain.Errors;

[ExcludeFromCodeCoverage]
public static class DomainErrors
{
    private const string DomainPrefix = "tuuuur.";

    public const string UnknowError = DomainPrefix + "unknown-error";

    public static class Data
    {
        private const string DataPrefix = DomainPrefix + "data.";

        public const string NotFound = DataPrefix + "notfound";

        public const string AlreadyExist = DataPrefix + "alreadyexist";

        public const string IsNull = DataPrefix + "isnull";
    }

    public static class Pagination
    {
        private const string PaginationPrefix = DomainPrefix + "pagination.";

        public const string Invalid = PaginationPrefix + GeneralErrors.Invalid;
    }

    public static class Authentication
    {
        private const string AuthentPrefix = DomainPrefix + "authentication.";

        public const string Missmatch = AuthentPrefix + "missmatch";
        public const string Invalid = AuthentPrefix + GeneralErrors.Invalid;
        
        public static class Google
        {
            private const string GooglePrefix = AuthentPrefix + "google.";
            public const string InvalidGoogle = GooglePrefix + GeneralErrors.Invalid;
        }

        public static class NickName
        {
            private const string NicknamePrefix = AuthentPrefix + "nickname.";
            public const string InvalidNickName = NicknamePrefix + GeneralErrors.Invalid;
        }

        public static class Login
        {
            private const string LoginPrefix = AuthentPrefix + "login.";

            public const string Empty = LoginPrefix + GeneralErrors.Empty;
            public const string InvalidEmail = LoginPrefix + GeneralErrors.InvalidFormat.InvalidEmail;
            public const string Duplicated = LoginPrefix + GeneralErrors.Duplicated;
            public const string NotExist = LoginPrefix + GeneralErrors.NotExist;
        }

        public static class Password
        {
            private const string PasswordPrefix = AuthentPrefix + "password.";

            public const string Empty = PasswordPrefix + GeneralErrors.Empty;
            public const string InvalidLength = PasswordPrefix + GeneralErrors.InvalidFormat.InvalidLength;
            public const string InvalidUppercase = PasswordPrefix + GeneralErrors.InvalidFormat.InvalidUppercase;
            public const string InvalidLowercase = PasswordPrefix + GeneralErrors.InvalidFormat.InvalidLowercase;
            public const string InvalidNumber = PasswordPrefix + GeneralErrors.InvalidFormat.InvalidNumber;
        }

        public static class Code
        {
            private const string CodePrefix = AuthentPrefix + "code.";
            
            public const string Empty = CodePrefix + GeneralErrors.Empty;
            public const string InvalidLength = CodePrefix + GeneralErrors.InvalidFormat.InvalidLength;
            public const string TooMuchDemand = CodePrefix + "too-much-demand";
        }

        public static class RefreshToken
        {
            private const string RefreshTokenPrefix = AuthentPrefix + "refresh-token.";
            public const string Invalid = RefreshTokenPrefix + GeneralErrors.Invalid;
        }
    }

    public static class Party
    {
        private const string PartyPrefix = DomainPrefix + "party.";
        public const string InvalidSettings = "invalid-settings";
        public const string InProgress = "in-progress";
        
        public static class Id
        {
            private const string IdPrefix = DomainPrefix + "id.";
            public const string Empty = IdPrefix + GeneralErrors.Empty;
            public const string Invalid = IdPrefix + GeneralErrors.Invalid;
        }
        public static class Answer
        {
            private const string AnswerPrefix = PartyPrefix + "anwser.";
            
            public const string Empty = AnswerPrefix + GeneralErrors.Empty;
        }

        public static class Code
        {
            private const string CodePrefix = DomainPrefix + "code.";
            public const string Empty = CodePrefix + GeneralErrors.Empty;
            public const string Invalid = CodePrefix + GeneralErrors.Invalid;
        }

        public static class NbQuestions
        {
            private const string NbQuestionsPrefix = DomainPrefix + "nbquestions.";
            public const string Invalid = NbQuestionsPrefix + GeneralErrors.Invalid;
        }
    }

    public static class Difficulty
    {
        private const string PartyDifficultyPrefix = DomainPrefix + "partydifficulty.";
        public const string Empty = PartyDifficultyPrefix + GeneralErrors.Empty;
        public const string Invalid = PartyDifficultyPrefix + GeneralErrors.Invalid;
    }

    public static class Theme
    {
        private const string ThemePrefix = DomainPrefix + "theme.";
        public const string Empty = ThemePrefix + GeneralErrors.Empty;
        public const string Invalid = ThemePrefix + GeneralErrors.Invalid;
    }

    public static class User
    {
        private const string UserPrefix = DomainPrefix + "user.";

        public static class Avatar
        {
            private const string AvatarPrefix = UserPrefix + "avatar.";
            public const string Empty = AvatarPrefix + GeneralErrors.Empty;
            public const string InvalidFormat = AvatarPrefix + GeneralErrors.Invalid;
            
        }
        
        public static class Nickname
        {
            private const string NicknamePrefix = UserPrefix + "nickname.";
            public const string Empty = NicknamePrefix + GeneralErrors.Empty;
        }
    }
}

internal static class GeneralErrors
{
    public const string Empty = "empty";
    public const string Duplicated = "duplicated";
    public const string NotExist = "not-exist";
    public const string Invalid = "invalid";

    public static class InvalidFormat
    {
        public const string InvalidEmail = "invalid-email";
        public const string InvalidLength = "invalid-length";
        public const string InvalidUppercase = "invalid-uppercase";
        public const string InvalidLowercase = "invalid-lowercase";
        public const string InvalidNumber = "invalid-number";
    }
}
