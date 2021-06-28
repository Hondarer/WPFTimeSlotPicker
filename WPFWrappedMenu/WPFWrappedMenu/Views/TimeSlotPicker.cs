using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using WPFWrappedMenu.ViewModels;

namespace WPFWrappedMenu.Views
{
    [TemplatePart(Name = "PART_selectedStartTimeTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_timeSpans", Type = typeof(UIElement))]
    public class TimeSlotPicker : Control
    {
        #region 依存関係プロパティ

        /// <summary>
        /// SelectedStartTime 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty SelectedTimeSlotStartTimeProperty =
            DependencyProperty.Register(nameof(SelectedTimeSlotStartTime), typeof(DateTime?), typeof(TimeSlotPicker), new PropertyMetadata(null));

        /// <summary>
        /// 選択された時間帯の開始時刻を取得または設定します。
        /// </summary>
        public DateTime? SelectedTimeSlotStartTime
        {
            get
            {
                return (DateTime?)GetValue(SelectedTimeSlotStartTimeProperty);
            }
            set
            {
                SetValue(SelectedTimeSlotStartTimeProperty, value);
            }
        }

        /// <summary>
        /// UserDefinedTimeSlotStartTime 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty UserDefinedTimeSlotStartTimeProperty =
            DependencyProperty.Register(nameof(UserDefinedTimeSlotStartTime), typeof(DateTime?), typeof(TimeSlotPicker), new PropertyMetadata(null));

        /// <summary>
        /// ユーザー定義の時間帯の開始時刻を取得または設定します。
        /// </summary>
        public DateTime? UserDefinedTimeSlotStartTime
        {
            get
            {
                return (DateTime?)GetValue(UserDefinedTimeSlotStartTimeProperty);
            }
            set
            {
                SetValue(UserDefinedTimeSlotStartTimeProperty, value);
            }
        }

        /// <summary>
        /// UserDefinedTimeSlotDescription 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty UserDefinedTimeSlotDescriptionProperty =
            DependencyProperty.Register(nameof(UserDefinedTimeSlotDescription), typeof(string), typeof(TimeSlotPicker), new PropertyMetadata(null));

        /// <summary>
        /// ユーザー定義の時間帯の説明を取得または設定します。
        /// </summary>
        public string UserDefinedTimeSlotDescription
        {
            get
            {
                return (string)GetValue(UserDefinedTimeSlotDescriptionProperty);
            }
            set
            {
                SetValue(UserDefinedTimeSlotDescriptionProperty, value);
            }
        }

        #endregion

        /// <summary>
        /// <see cref="TimeSlotPicker"/> クラスの静的な初期化をします。
        /// </summary>
        static TimeSlotPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeSlotPicker), new FrameworkPropertyMetadata(typeof(TimeSlotPicker)));
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            // デザイン時は以下の処理を行わない。
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                // 部品の配置されたウインドウの Closed イベントを購読する
                Window.GetWindow(this).Closed -= ParentWindow_Closed; // 複数回の呼び出しで購読が多重登録されないように
                Window.GetWindow(this).Closed += ParentWindow_Closed;

                // 時刻の変化イベントを購読する
                DateTimeManager.Instance.CurrentDateTimeChanged -= DateTimeManager_CurrentDateTimeChanged; // 複数回の呼び出しで購読が多重登録されないように
                DateTimeManager.Instance.CurrentDateTimeChanged += DateTimeManager_CurrentDateTimeChanged;
            }

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

        private void DateTimeManager_CurrentDateTimeChanged(object sender, CurrentDateTimeChangedEventArgs e)
        {
            if (DataContext is TimeSlotPickerViewModel timeSlotPickerViewModel)
            {
                timeSlotPickerViewModel.CurrentDateTimeChanged(sender,e);
            }
        }

        /// <summary>
        /// Occurs when the window is about to close.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An object that contains no event data.</param>
        private void ParentWindow_Closed(object sender, EventArgs e)
        {
            DateTimeManager.Instance.CurrentDateTimeChanged -= DateTimeManager_CurrentDateTimeChanged;
        }
    }
}
