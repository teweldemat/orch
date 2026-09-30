namespace orch.core.command
{
    public static class PasswordPolicy
    {
        public const int MinimumLength = 8;
        public const int RequiredUniqueCharacters = 4;

        public const string RequirementMessage =
            "Password must be at least 8 characters, include an uppercase letter, a lowercase letter, and a digit, use at least 4 different characters, and contain no spaces.";

        public static void EnsureValid(string? password)
        {
            if (!IsValid(password))
                throw new ArgumentException(RequirementMessage);
        }

        public static bool IsValid(string? password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < MinimumLength)
                return false;

            var hasUpper = false;
            var hasLower = false;
            var hasDigit = false;
            var unique = new HashSet<char>();

            foreach (var ch in password)
            {
                if (char.IsWhiteSpace(ch))
                    return false;

                if (char.IsUpper(ch))
                    hasUpper = true;
                else if (char.IsLower(ch))
                    hasLower = true;
                else if (char.IsDigit(ch))
                    hasDigit = true;

                unique.Add(ch);
            }

            return hasUpper && hasLower && hasDigit && unique.Count >= RequiredUniqueCharacters;
        }
    }
}
