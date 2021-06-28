using System;
using System.Collections.Generic;
using System.Globalization;
using WPFWrappedMenu.Commands;

namespace WPFWrappedMenu.ViewModels
{
    public class TimeSpanPickerViewModel : BindableBase
    {
        public class TimeViewModel : BindableBase
        {
            public DateTime SpecifyTime { get; set; }

            private bool _isSelected = false;

            public bool IsSelected
            {
                get
                {
                    return _isSelected;
                }
                set
                {
                    SetProperty(ref _isSelected, value);
                }
            }
        }

        private DateTime? _selectedStartTime;

        public DateTime? SelectedStartTime

        {
            get
            {
                return _selectedStartTime;
            }
            set
            {
                DateTime? selectedStartTime = value;

                // null でない場合に切り捨て処理を行う
                if (selectedStartTime is DateTime nonNullSelectedStartTime)
                {
                    // 日付情報があれば削除する
                    nonNullSelectedStartTime = new DateTime().Add(nonNullSelectedStartTime.TimeOfDay);

                    // 30 分単位で切り捨てる
                    selectedStartTime = nonNullSelectedStartTime.AddTicks(-(nonNullSelectedStartTime.Ticks % TimeSpan.FromMinutes(30).Ticks));
                }

                if (SetProperty(ref _selectedStartTime, selectedStartTime) == true)
                {
                    OnPropertyChanged(nameof(SelectedStartTimeString));
                }
            }
        }

        public string SelectedStartTimeString
        {
            get
            {
                return string.Format("{0:HH:mm}", SelectedStartTime);
            }
            set
            {
                // 入力された文字列を DateTime として評価する
                if (DateTime.TryParseExact(value,
                    Resources.StringResource.StartTimeParseFormats.Split(','),
                    CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsedDateTime) == false)
                {
                    // 無効な値の場合にはソースを更新しない
                    // View を元の値に戻すために、PropertyChanged を発行する
                    OnPropertyChanged();
                    return;
                }

                // 有意であれば設定
                ChangeStartTimeCore(parsedDateTime);
            }
        }

        private List<TimeViewModel> _timeSpans;

        public List<TimeViewModel> TimeSpans
        {
            get
            {
                return _timeSpans;
            }
            set
            {
                SetProperty(ref _timeSpans, value);
            }
        }

        private bool _popupOpen;

        public bool PopupOpen
        {
            get
            {
                return _popupOpen;
            }
            set
            {
                SetProperty(ref _popupOpen, value);
            }
        }

        public DelegateCommand SpecifyTimeCommamnd { get; }

        public TimeSpanPickerViewModel()
        {
            SpecifyTimeCommamnd = new DelegateCommand(
                parameter =>
                {
                    if (parameter is DateTime specifyDate)
                    {
                        ChangeStartTimeCore(specifyDate);
                    }

                    // ポップアップを閉じる
                    PopupOpen = false;

                    // TODO: 選択(指定)できない条件はあるか
                });

            List<TimeViewModel> timeSpans = new List<TimeViewModel>();
            for (int i = 0; i < 48; i++)
            {
                TimeViewModel timeViewModel = new TimeViewModel() { SpecifyTime = new DateTime().AddMinutes(30 * i) };
                timeSpans.Add(timeViewModel);
            }

            TimeSpans = timeSpans;

            // TODO: 初期選択は何か、空白あるいは?
            // まずはバインド元にまかせることにした。
            //ChangeStartTimeCore(SelectedStartTime);
        }

        private void ChangeStartTimeCore(DateTime specifyDate)
        {
            SelectedStartTime = specifyDate;

            foreach (object vm in TimeSpans)
            {
                if (vm is TimeViewModel dateViewModel)
                {
                    dateViewModel.IsSelected = dateViewModel.SpecifyTime == SelectedStartTime;
                }
            }
        }
    }
}
