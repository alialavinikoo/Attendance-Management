using System;

namespace AttendanceManagement.Models
{
    public class ShiftSchedule
    {
        public int ScheduleID { get; set; }
        public int ShiftID { get; set; }

        private byte _dayOfWeek;
        public byte DayOfWeek
        {
            get { return _dayOfWeek; }
            set
            {
                if (value < 1 || value > 7)
                    throw new ArgumentOutOfRangeException(nameof(DayOfWeek), "روز هفته باید بین 1 تا 7 باشد.");

                _dayOfWeek = value;
            }
        }

        private TimeSpan _startTime;
        public TimeSpan StartTime
        {
            get { return _startTime; }
            set
            {
                if (_finishTime != TimeSpan.Zero && value >= _finishTime)
                    throw new ArgumentException("زمان شروع باید قبل از زمان پایان باشد.");

                _startTime = value;
            }
        }

        private TimeSpan _finishTime;
        public TimeSpan FinishTime
        {
            get { return _finishTime; }
            set
            {
                if (_startTime != TimeSpan.Zero && value <= _startTime)
                    throw new ArgumentException("زمان پایان باید بعد از زمان شروع باشد.");

                _finishTime = value;
            }
        }

        // UI HELPERS
        public string WorkingHours => $"{StartTime:hh\\:mm} تا {FinishTime:hh\\:mm}";

        public string PersianDayName
        {
            get
            {
                return DayOfWeek switch
                {
                    1 => "شنبه",
                    2 => "یکشنبه",
                    3 => "دوشنبه",
                    4 => "سه‌شنبه",
                    5 => "چهارشنبه",
                    6 => "پنج‌شنبه",
                    7 => "جمعه",
                    _ => "نامشخص"
                };
            }
        }
    }
}