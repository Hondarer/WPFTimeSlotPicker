using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using WPFWrappedMenu.Commands;

namespace WPFWrappedMenu.ViewModels
{
    public class TimeSlotPickerViewModel : BindableBase
    {
        public class TimeSlotViewModel : BindableBase
        {
            public string Description { get; set; }

            public DateTime? SpecifyTimeSlotStartTime { get; set; }

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

        private DateTime _currentTimeSlotStartTime;

        public DateTime CurrentTimeSlotStartTime

        {
            get
            {
                return _currentTimeSlotStartTime;
            }
            set
            {
                if (SetProperty(ref _currentTimeSlotStartTime, TruncateDateTime(value)) == true)
                {
                    RefreshTimeSlotsViewModel();
                }
            }
        }

        private DateTime? _selectedTimeSlotStartTime;

        public DateTime? SelectedTimeSlotStartTime
        {
            get
            {
                return _selectedTimeSlotStartTime;
            }
            set
            {
                if (SetProperty(ref _selectedTimeSlotStartTime, TruncateDateTime(value)) == true)
                {
                    OnPropertyChanged(nameof(SelectedTimeSlotStartTimeString));
                }
            }
        }

        public string SelectedTimeSlotStartTimeString
        {
            get
            {
                return string.Format("{0:HH:mm}", SelectedTimeSlotStartTime);
            }
            set
            {
                // 入力された文字列を DateTime として評価する
                if (DateTime.TryParseExact(value,
                    Resources.StringResource.StartTimeParseFormats.Split(','),
                    CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsedDateTime) == false)
                {
                    // 無効な値の場合にはソースを更新しない
                    // View を元の値に戻すために、PropertyChanged イベントを発行する
                    OnPropertyChanged();
                    return;
                }

                parsedDateTime = TruncateDateTime(parsedDateTime);

                if (StartTimeSlotStartTime <= EndTimeSlotStartTime)
                {
                    if ((StartTimeSlotStartTime > parsedDateTime) || (parsedDateTime > EndTimeSlotStartTime))
                    {
                        // 無効な値の場合にはソースを更新しない
                        // View を元の値に戻すために、PropertyChanged イベントを発行する
                        OnPropertyChanged();
                        return;
                    }
                }
                else
                {
                    if ((parsedDateTime > EndTimeSlotStartTime) && (StartTimeSlotStartTime > parsedDateTime))
                    {
                        // 無効な値の場合にはソースを更新しない
                        // View を元の値に戻すために、PropertyChanged イベントを発行する
                        OnPropertyChanged();
                        return;
                    }
                }

                // 有意であれば設定
                ChangeSelectedTimeSlotCore(parsedDateTime);
            }
        }

        private DateTime _startTimeSlotStartTime = new DateTime(1, 1, 1, 0, 0, 0);

        public DateTime StartTimeSlotStartTime
        {
            get
            {
                return _startTimeSlotStartTime;
            }
            set
            {
                if (SetProperty(ref _startTimeSlotStartTime, TruncateDateTime(value)) == true)
                {
                    RefreshTimeSlotsViewModel();
                }
            }
        }

        private DateTime _endTimeSlotStartTime = new DateTime(1, 1, 1, 23, 30, 0);

        public DateTime EndTimeSlotStartTime
        {
            get
            {
                return _endTimeSlotStartTime;
            }
            set
            {
                if (SetProperty(ref _endTimeSlotStartTime, TruncateDateTime(value)) == true)
                {
                    RefreshTimeSlotsViewModel();
                }
            }
        }

        /// <summary>
        /// ユーザー定義の時間帯の開始時刻を保持します。
        /// </summary>
        private DateTime? _userDefinedTimeSlotStartTime = null;

        /// <summary>
        /// ユーザー定義の時間帯の開始時刻を取得または設定します。
        /// </summary>
        public DateTime? UserDefinedTimeSlotStartTime
        {
            get
            {
                return _userDefinedTimeSlotStartTime;
            }
            set
            {
                DateTime? oldValue = _userDefinedTimeSlotStartTime;
                if (SetProperty(ref _userDefinedTimeSlotStartTime, TruncateDateTime(value)) == true)
                {
                    RefreshTimeSlotsViewModel();

                    // null からの設定時、選択中 TimeSlot を変更する。
                    // 非null からの設定時は変更しない。
                    if (oldValue == null)
                    {
                        ChangeSelectedTimeSlotCore(UserDefinedTimeSlotStartTime);
                    }
                }
            }
        }

        /// <summary>
        /// ユーザー定義の時間帯の説明を保持します。
        /// </summary>
        private string _userDefinedTimeSlotDescription = null;

        /// <summary>
        /// ユーザー定義の時間帯の説明を取得または設定します。
        /// </summary>
        public string UserDefinedTimeSlotDescription
        {
            get
            {
                return _userDefinedTimeSlotDescription;
            }
            set
            {
                if (SetProperty(ref _userDefinedTimeSlotDescription, value) == true)
                {
                    RefreshTimeSlotsViewModel();
                }
            }
        }

        /// <summary>
        /// 現在の TimeSlot を判断するオフセットを保持します。
        /// </summary>
        private TimeSpan _currentTimeSlotOffset = TimeSpan.Zero;

        /// <summary>
        /// 現在の TimeSlot を判断するオフセットを取得または設定します。
        /// </summary>
        public TimeSpan CurrentTimeSlotOffset
        {
            get
            {
                return _currentTimeSlotOffset;
            }
            set
            {
                if (SetProperty(ref _currentTimeSlotOffset, value) == true)
                {
                    InvalidateTimeSlot();
                }
            }
        }

        /// <summary>
        /// 既定の選択 TimeSlot のオフセットを保持します。
        /// </summary>
        private int? _defaultSelectTimeSlotOffset = 0;

        /// <summary>
        /// 既定の選択 TimeSlot のオフセットを取得または設定します。
        /// </summary>
        public int? DefaultSelectTimeSlotOffset
        {
            get
            {
                return _defaultSelectTimeSlotOffset;
            }
            set
            {
                if (SetProperty(ref _defaultSelectTimeSlotOffset, value) == true)
                {
                    if (DefaultSelectTimeSlotOffset is int defaultSelectTimeSlotOffset)
                    {
                        ChangeSelectedTimeSlotCore(CurrentTimeSlotStartTime.AddMinutes(30 * defaultSelectTimeSlotOffset));
                    }
                    else
                    {
                        ChangeSelectedTimeSlotCore(null);
                    }
                }
            }
        }

        private List<TimeSlotViewModel> _timeSpans;

        public List<TimeSlotViewModel> TimeSpans
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

        private List<TimeSlotViewModel> _shortcuts;

        public List<TimeSlotViewModel> Shortcuts
        {
            get
            {
                return _shortcuts;
            }
            set
            {
                SetProperty(ref _shortcuts, value);
            }
        }

        public DelegateCommand SpecifyTimeSlotCommamnd { get; }

        public DelegateCommand PreviousTimeSlotCommamnd { get; }

        public DelegateCommand NextTimeSlotCommamnd { get; }

        public TimeSlotPickerViewModel()
        {
            SpecifyTimeSlotCommamnd = new DelegateCommand(
                parameter =>
                {
                    if (parameter == null)
                    {
                        ChangeSelectedTimeSlotCore(null);
                    }

                    if (parameter is DateTime specifyTimeSlotStartTime)
                    {
                        ChangeSelectedTimeSlotCore(specifyTimeSlotStartTime);
                    }

                    // ポップアップを閉じる
                    PopupOpen = false;
                },
                parameter =>
                {
                    if (parameter == null)
                    {
                        return true;
                    }

                    if (parameter is DateTime specifyTimeSlotStartTime)
                    {
                        specifyTimeSlotStartTime = TruncateDateTime(specifyTimeSlotStartTime);

                        if (StartTimeSlotStartTime <= EndTimeSlotStartTime)
                        {
                            if ((StartTimeSlotStartTime <= specifyTimeSlotStartTime) && (specifyTimeSlotStartTime <= EndTimeSlotStartTime))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if ((specifyTimeSlotStartTime <= EndTimeSlotStartTime) || (StartTimeSlotStartTime <= specifyTimeSlotStartTime))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                });

            PreviousTimeSlotCommamnd = new DelegateCommand(
                parameter =>
                {
                    if (parameter is not DateTime specifyTimeSlotStartTime)
                    {
                        specifyTimeSlotStartTime = CurrentTimeSlotStartTime;
                    }
                    // 1/1 00:00 からさかのぼることはできない。翌日基準で 30 分さかのぼることで時間帯を算出する。
                    SpecifyTimeSlotCommamnd.Execute(specifyTimeSlotStartTime.AddDays(1).AddMinutes(-30));
                },
                parameter =>
                {
                    if (parameter is not DateTime specifyTimeSlotStartTime)
                    {
                        specifyTimeSlotStartTime = CurrentTimeSlotStartTime;
                    }
                    // 1/1 00:00 からさかのぼることはできない。翌日基準で 30 分さかのぼることで時間帯を算出する。
                    return SpecifyTimeSlotCommamnd.CanExecute(specifyTimeSlotStartTime.AddDays(1).AddMinutes(-30));
                });

            NextTimeSlotCommamnd = new DelegateCommand(
                parameter =>
                {
                    if (parameter is not DateTime specifyTimeSlotStartTime)
                    {
                        specifyTimeSlotStartTime = CurrentTimeSlotStartTime;
                    }
                    SpecifyTimeSlotCommamnd.Execute(specifyTimeSlotStartTime.AddMinutes(30));
                },
                parameter =>
                {
                    if (parameter is not DateTime specifyTimeSlotStartTime)
                    {
                        specifyTimeSlotStartTime = CurrentTimeSlotStartTime;
                    }
                    return SpecifyTimeSlotCommamnd.CanExecute(specifyTimeSlotStartTime.AddMinutes(30));
                });

            List<TimeSlotViewModel> timeSpans = new List<TimeSlotViewModel>();
            for (int i = 0; i < 48; i++)
            {
                TimeSlotViewModel timeViewModel = new TimeSlotViewModel() { SpecifyTimeSlotStartTime = new DateTime().AddMinutes(30 * i) };
                timeSpans.Add(timeViewModel);
            }

            TimeSpans = timeSpans;

            InvalidateTimeSlot();

            if (DefaultSelectTimeSlotOffset is int defaultSelectTimeSlotOffset)
            {
                ChangeSelectedTimeSlotCore(CurrentTimeSlotStartTime.AddMinutes(30 * defaultSelectTimeSlotOffset));
            }
            else
            {
                ChangeSelectedTimeSlotCore(null);
            }
        }

        public static DateTime? TruncateDateTime(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }

            return TruncateDateTime((DateTime)dateTime);
        }

        public static DateTime TruncateDateTime(DateTime dateTime)
        {
            // 日付情報があれば削除する
            DateTime timeOfDay = new DateTime().Add(dateTime.TimeOfDay);

            // 30 分単位で切り捨てる
            return timeOfDay.AddTicks(-(timeOfDay.Ticks % TimeSpan.FromMinutes(30).Ticks));
        }

        public void CurrentDateTimeChanged(object sender, EventArgs e)
        {
            InvalidateTimeSlot();
        }

        private void InvalidateTimeSlot()
        {
            CurrentTimeSlotStartTime = DateTimeManager.Instance.CurrentDateTime.Add(CurrentTimeSlotOffset);
        }

        private void ChangeSelectedTimeSlotCore(DateTime? specifyDate)
        {
            SelectedTimeSlotStartTime = specifyDate;

            foreach (object vm in TimeSpans.Union(Shortcuts))
            {
                if (vm is TimeSlotViewModel dateViewModel)
                {
                    dateViewModel.IsSelected = dateViewModel.SpecifyTimeSlotStartTime == SelectedTimeSlotStartTime;
                }
            }
        }

        private void RefreshTimeSlotsViewModel()
        {
            #region ショートカットの作成

            List<TimeSlotViewModel> shortcuts = new List<TimeSlotViewModel>()
            {
                new TimeSlotViewModel(){SpecifyTimeSlotStartTime=CurrentTimeSlotStartTime.AddMinutes(30), Description="next of current timeslot" },
                new TimeSlotViewModel(){SpecifyTimeSlotStartTime=CurrentTimeSlotStartTime, Description="current timeslot" },
                // 1/1 00:00 からさかのぼることはできない。翌日基準で 30 分さかのぼることで時間帯を算出する。
                new TimeSlotViewModel(){SpecifyTimeSlotStartTime=CurrentTimeSlotStartTime.AddDays(1).AddMinutes(-30), Description="previous of current timeslot" }
            };

            if (!string.IsNullOrEmpty(UserDefinedTimeSlotDescription))
            {
                shortcuts.Add(null);
                shortcuts.Add(new TimeSlotViewModel() { SpecifyTimeSlotStartTime = UserDefinedTimeSlotStartTime, Description = UserDefinedTimeSlotDescription });
            }

            Shortcuts = shortcuts;

            #endregion

            // 選択状態の更新
            ChangeSelectedTimeSlotCore(SelectedTimeSlotStartTime);

            // コマンドの実行可否再評価を依頼
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
