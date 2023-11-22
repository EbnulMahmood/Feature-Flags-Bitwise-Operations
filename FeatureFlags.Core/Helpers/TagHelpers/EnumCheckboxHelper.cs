using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using System.Text;

namespace FeatureFlags.Core.Helpers.TagHelpers
{
    public static class EnumCheckboxHelper
    {
        public static IHtmlContent EnumCheckboxesFor<TModel, TEnum>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum>> expression,
            TEnum modelValue,
            string? customCssClass = null,
            bool includeControlLabel = true)
            where TEnum : Enum
        {
            var enumType = typeof(TEnum);
            var values = Enum.GetValues(enumType);

            var stringBuilder = new StringBuilder();

            foreach (var value in values)
            {
                int flagValue = (int)value;
                if (flagValue != 0)
                {
                    var displayName = Enum.GetName(enumType, value);

                    stringBuilder.AppendLine($"<div class=\"form-group {customCssClass}\">");

                    if (includeControlLabel)
                    {
                        stringBuilder.AppendLine(
                            $@"<label class=""control-label"">{displayName}</label>");
                    }

                    var isChecked = modelValue.HasFlag((TEnum)value) ? "checked" : "";

                    stringBuilder.AppendLine(
                        $@"<div class=""form-check form-switch"">
                    <input {isChecked} class=""form-check-input"" type=""checkbox"" id=""{flagValue}"" name=""{htmlHelper.NameFor(expression)}"" value=""{flagValue}"" />
                    <label class=""form-check-label"" for=""{flagValue}"">{displayName}</label>
                </div>");

                    stringBuilder.AppendLine("</div>");
                }
            }

            return new HtmlString(stringBuilder.ToString());
        }
    }
}
