// TODO: 多分 TimeSlot が適切
// TODO: 初期の TimeSlot を外部から与えることはあるか

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace WPFWrappedMenu.Views
{
    [TemplatePart(Name = "PART_selectedStartTimeTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_timeSpans", Type = typeof(UIElement))]
    public class TimeSpanPicker : Control
    {
        #region 依存関係プロパティ

        /// <summary>
        /// SelectedStartTime 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty SelectedStartTimeProperty =
            DependencyProperty.Register(nameof(SelectedStartTime), typeof(DateTime?), typeof(TimeSpanPicker), new PropertyMetadata(null));

        /// <summary>
        /// 選択された開始時刻を取得または設定します。
        /// </summary>
        public DateTime? SelectedStartTime
        {
            get
            {
                return (DateTime?)GetValue(SelectedStartTimeProperty);
            }
            set
            {
                SetValue(SelectedStartTimeProperty, value);
            }
        }

        #endregion

        /// <summary>
        /// <see cref="TimeSpanPicker"/> クラスの静的な初期化をします。
        /// </summary>
        static TimeSpanPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeSpanPicker), new FrameworkPropertyMetadata(typeof(TimeSpanPicker)));
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            if (Template.FindName("PART_popup", this) is Popup part_popup)
            {
                // ポップアップの位置を、起点オブジェクトの右下基準にする
                part_popup.CustomPopupPlacementCallback += (Size popupSize, Size targetSize, Point offset) =>
                {
                    return new CustomPopupPlacement[] { new CustomPopupPlacement() { Point = new Point(targetSize.Width - popupSize.Width, targetSize.Height) } };
                };

                // ポップアップを開いたときに、ポップアップ内にフォーカスを移動する
                part_popup.Opened += (sender, e) =>
                {
                    if (sender is Popup popup)
                    {
                        if (Template.FindName("PART_timeSpans", this) is UIElement uiElement)
                        {
                            uiElement.Focus();
                        }
                    }
                };
            }

            if (Template.FindName("PART_selectedStartTimeTextBox", this) is TextBox part_selectedStartTimeTextBox)
            {
                #region フォーカスを得たときに全選択する

                // MEMO: Popup を開いた状態で TextBox にフォーカスを与えると全選択されない。
                //       このとき、PreviewMouseLeftButtonDown が呼ばれないためだが、詳細メカニズム未調査。
                //       機能に支障がないため、現状通りとしたい。

                part_selectedStartTimeTextBox.MouseDoubleClick += (sender, e) =>
                {
                    if (sender is TextBox textBox)
                    {
                        textBox.SelectAll();
                    }
                };

                part_selectedStartTimeTextBox.GotKeyboardFocus += (sender, e) =>
                {
                    if (sender is TextBox textBox)
                    {
                        textBox.SelectAll();
                    }
                };

                part_selectedStartTimeTextBox.PreviewMouseLeftButtonDown += (sender, e) =>
                {
                    if (sender is TextBox textBox)
                    {
                        if (textBox.IsKeyboardFocusWithin == false)
                        {
                            e.Handled = true;
                            textBox.Focus();
                        }
                    }
                };

                #endregion

                // Enter キーを入力した際に、ソースを更新する
                part_selectedStartTimeTextBox.KeyDown += (sender, e) =>
                {
                    if (e.Key != Key.Enter)
                    {
                        return;
                    }

                    BindingExpression be = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();

                    // フォーカスを移動する
                    TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Previous)
                    {
                        Wrapped = true
                    };
                    ((TextBox)sender).MoveFocus(request);

                    e.Handled = true;
                };
            }

            base.OnApplyTemplate();
        }
    }
}
