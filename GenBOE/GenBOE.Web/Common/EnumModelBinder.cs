using System;
using System.Linq;
using System.Web.Mvc;

namespace GenBOE.Web.Common
{
    ///http://stackoverflow.com/questions/5017803/mvc-binding-issue-from-json-to-enum-customexception-from-int-to-enum
    /// <summary>
    /// Generic Custom Model Binder used to properly interpret int representation of enum types from JSON deserialization, including default values
    /// </summary>
    /// <typeparam name="T">The enum type to apply this Custom Model Binder to</typeparam>
    public class EnumBinder<T> : IModelBinder
    {
        private T DefaultValue { get; set; }

        public EnumBinder(T defaultValue)
        {
            DefaultValue = defaultValue;
        }

        #region IModelBinder Members
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            return bindingContext.ValueProvider.GetValue(bindingContext.ModelName) == null ? DefaultValue : GetEnumValue(DefaultValue, bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue);
        }
        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        #pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type
        public static T GetEnumValue<T>(T defaultValue, string value)
        #pragma warning restore CS0693 // Type parameter has the same name as the type parameter from outer type
        {
            T enumType = defaultValue;

            // check to see if enum is int (valid) or string value of an enum value (valid)
            int x;
            if (!String.IsNullOrEmpty(value) && (int.TryParse(value, out x) || (Contains(typeof(T), value))))
            {
                enumType = (T)Enum.Parse(typeof(T), value, true);
            }

            return enumType;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static bool Contains(Type enumType, string value)
        {
            return Enum.GetNames(enumType).Contains(value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
