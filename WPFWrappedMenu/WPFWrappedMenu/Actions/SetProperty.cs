// https://stackoverflow.com/questions/942548/setting-a-property-with-an-eventtrigger

using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Interactivity;

namespace WPFWrappedMenu.Mvvm.Actions
{
    public class SetProperty : TargetedTriggerAction<FrameworkElement>
    {
        #region Properties

        #region PropertyName

        /// <summary>
        /// Property that is being set by this setter.
        /// </summary>
        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(SetProperty),
            new PropertyMetadata(string.Empty));

        #endregion

        #region Value

        /// <summary>
        /// Property value that is being set by this setter.
        /// </summary>
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(SetProperty),
            new PropertyMetadata(null));

        #endregion

        #endregion

        #region Overrides

        protected override void Invoke(object parameter)
        {
            var target = TargetObject ?? AssociatedObject;

            var targetType = target.GetType();

            var property = targetType.GetProperty(PropertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (property == null)
                throw new ArgumentException(string.Format("Property not found: {0}", PropertyName));

            if (property.CanWrite == false)
                throw new ArgumentException(string.Format("Property is not settable: {0}", PropertyName));

            object convertedValue;

            if (Value == null)
                convertedValue = null;

            else
            {
                var valueType = Value.GetType();
                var propertyType = property.PropertyType;

                if (valueType == propertyType)
                    convertedValue = Value;

                else
                {
                    var propertyConverter = TypeDescriptor.GetConverter(propertyType);

                    if (propertyConverter.CanConvertFrom(valueType))
                        convertedValue = propertyConverter.ConvertFrom(Value);

                    else if (valueType.IsSubclassOf(propertyType))
                        convertedValue = Value;

                    else
                        throw new ArgumentException(string.Format("Cannot convert type '{0}' to '{1}'.", valueType, propertyType));
                }
            }

            property.SetValue(target, convertedValue);
        }

        #endregion
    }
}
