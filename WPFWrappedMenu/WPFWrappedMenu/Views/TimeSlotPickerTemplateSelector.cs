using System.Windows;
using System.Windows.Controls;

namespace WPFWrappedMenu.Views
{
    /// <summary>
    /// TimeSlot ピッカーのテンプレート選択機構を提供します。
    /// </summary>
    public class TimeSlotPickerTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// 空白のテンプレートを取得または設定します。
        /// </summary>
        public DataTemplate FillerTemplate { get; set; }

        /// <summary>
        /// TimeSlot 選択ショートカットボタンのテンプレートを取得または設定します。
        /// </summary>
        public DataTemplate ShortcutButtonTemplate { get; set; }

        /// <inheritdoc/>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                // TimeSlot 選択ショートカットボタンのテンプレートを返す。
                return ShortcutButtonTemplate;
            }

            // null のときは、空白のテンプレートを返す。
            return FillerTemplate;
        }
    }
}
