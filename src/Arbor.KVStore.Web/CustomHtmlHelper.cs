using System;
using JetBrains.Annotations;

namespace Arbor.KVStore.Web
{
    public static class CustomHtmlHelper
    {
        public static string HtmlInput([NotNull] this IStoredValue storedValue)
        {
            if (!storedValue.IsValid)
            {
                throw new ArgumentException($"Stored value {storedValue} is not valid");
            }

            if (storedValue.Key.EndsWith("enabled", StringComparison.OrdinalIgnoreCase))
            {
                return CheckBox(storedValue);
            }

            if (storedValue.Key.EndsWith("password", StringComparison.OrdinalIgnoreCase))
            {
                return Password(storedValue);
            }

            if (storedValue.Key.EndsWith("token", StringComparison.OrdinalIgnoreCase))
            {
                return Password(storedValue);
            }

            if (storedValue.Key.EndsWith("license", StringComparison.OrdinalIgnoreCase))
            {
                return Password(storedValue);
            }

            if (storedValue.Key.EndsWith("email", StringComparison.OrdinalIgnoreCase))
            {
                return Email(storedValue);
            }

            return TextBox(storedValue);
        }

        private static string TextBox(IStoredValue storedValue)
        {
            return LabelFor(storedValue) + " " +
                   $"<span class=\"input-field\"><input {GetId(storedValue)} type=\"text\" value=\"{storedValue.Value}\" name=\"value\" /></span>";
        }

        private static string GetId(IStoredValue storedValue)
        {
            return $"id=\"{GetIdValue(storedValue)}\"";
        }

        private static string GetIdValue(IStoredValue storedValue)
        {
            return "id_" + storedValue.Key.Replace(":", "_").Replace(" ", "_").Replace(".", "_");
        }

        private static string Password(IStoredValue storedValue)
        {
            return LabelFor(storedValue) + " " +
                   $"<span class=\"input-field\" title=\"{storedValue.Value}\"><input {GetId(storedValue)} type=\"password\" value=\"{storedValue.Value}\" name=\"value\" /></span>";
        }
        private static string Email(IStoredValue storedValue)
        {
            return LabelFor(storedValue) + " " +
                   $"<span class=\"input-field\"><input {GetId(storedValue)} type=\"email\" value=\"{storedValue.Value}\" name=\"value\" /></span>";
        }

        private static string LabelFor(IStoredValue storedValue)
        {
            return $"<label for=\"{GetIdValue(storedValue)}\">{storedValue.Key}</label>";
        }

        private static string CheckBox(IStoredValue storedValue)
        {
            const string inputTextValue = "true";

            string checkedText = "";
            if (bool.TryParse(storedValue.Value, out bool inputValue) && inputValue)
            {
                checkedText = "checked=\"checked\"";
            }

            return LabelFor(storedValue) + " " +
                   $"<span class=\"input-field\"><input {GetId(storedValue)} type=\"checkbox\" value=\"{inputTextValue}\" name=\"value\" {checkedText} /></span>";
        }
    }
}