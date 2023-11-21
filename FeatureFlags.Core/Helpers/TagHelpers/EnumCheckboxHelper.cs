using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

namespace FeatureFlags.Core.Helpers.TagHelpers
{
    public static class EnumCheckboxHelper
    {
        public static IHtmlContent EnumCheckboxesFor<TModel, TEnum>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum>> expression,
            string? customCssClass = null,
            bool includeControlLabel = true)
            where TEnum : Enum
        {
            var enumType = typeof(TEnum);
            var values = Enum.GetValues(enumType);

            var stringBuilder = new StringBuilder();

            foreach (var value in values)
            {
                if ((int)value != 0)
                {
                    var valueString = value.ToString();
                    var fieldInfo = !string.IsNullOrEmpty(valueString)
                        ? enumType.GetField(valueString)
                        : null;

                    var displayAttribute = fieldInfo?
                        .GetCustomAttributes(typeof(DisplayAttribute), false)?
                        .OfType<DisplayAttribute>()?
                        .FirstOrDefault();

                    var displayName = displayAttribute?.Name ?? valueString;

                    stringBuilder.AppendLine($"<div class=\"form-group {customCssClass}\">");

                    if (includeControlLabel)
                    {
                        stringBuilder.AppendLine(
                            $@"<label class=""control-label"">{displayName}</label>");
                    }

                    stringBuilder.AppendLine(
                        $@"<div class=""form-check form-switch"">
                            <input class=""form-check-input"" type=""checkbox"" id=""{value}"" name=""{htmlHelper.NameFor(expression)}"" value=""{(int)value}"" />
                            <label class=""form-check-label"" for=""{value}"">{displayName}</label>
                        </div>");

                    stringBuilder.AppendLine("</div>");
                }
            }

            return new HtmlString(stringBuilder.ToString());
        }
    }
}
