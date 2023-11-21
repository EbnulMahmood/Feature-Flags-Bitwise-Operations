using FeatureFlags.Core.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace FeatureFlags.Core.Helpers.TagHelpers
{
    [HtmlTargetElement("enum-checkboxes", Attributes = "for")]
    public class EnumCheckboxesTagHelper : TagHelper
    {
        public required string For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var enumType = typeof(UserFlags);
            var values = Enum.GetValues(enumType).Cast<UserFlags>().Where(e => e != UserFlags.None).ToList();

            var stringBuilder = new StringBuilder();

            foreach (var value in values)
            {
                stringBuilder.AppendLine(
                    $@"<div class=""form-group"">
                        <label class=""control-label"">{Enum.GetName(enumType, value)}</label>
                        <div class=""form-check form-switch"">
                            <input class=""form-check-input"" type=""checkbox"" id=""{value}"" name=""{For}"" value=""{(int)value}"" />
                            <label class=""form-check-label"" for=""{value}"">{Enum.GetName(enumType, value)}</label>
                        </div>
                    </div>");
            }

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetHtmlContent(stringBuilder.ToString());
        }
    }
}
