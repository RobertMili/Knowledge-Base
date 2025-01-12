using System.ComponentModel.DataAnnotations;

namespace StarPointApi.Shared
{
    internal class NotNullAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            return value != null;
        }
    }

    internal class StringNotNullOrEmptyOrWhiteSpace : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            return !string.IsNullOrWhiteSpace((string) value);
        }
    }

    internal class StringNotEmptyOrWhiteSpace : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            return value == null || !((string) value).IsEmptyOrWhiteSpace();
        }
    }
}