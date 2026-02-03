using VDLaser.Core.Codes;
using VDLaser.Core.Grbl.Models;
using Xunit;

namespace VDLaser.Tests.UnitTests
{
    public class AlarmTests
    {
        //AlarmCodes Tests
        [Fact]
        public void Should_Have_Alarm_Dictionaries()
        {
            var alarms = new AlarmCodes();

            Assert.NotEmpty(alarms.AlarmDict09);
            Assert.NotEmpty(alarms.AlarmDict11);
        }

        [Fact]
        public void All_Alarms_Should_Have_Message_And_Severity()
        {
            var alarms = new AlarmCodes();

            foreach (var kv in alarms.AlarmDict11)
            {
                var alarm = kv.Value;
                Assert.False(string.IsNullOrWhiteSpace(alarm.Message));
                Assert.IsType<AlarmSeverity>(alarm.Severity);
            }

            foreach (var kv in alarms.AlarmDict09)
            {
                var alarm = kv.Value;
                Assert.False(string.IsNullOrWhiteSpace(alarm.Message));
                Assert.IsType<AlarmSeverity>(alarm.Severity);
            }
        }

        [Fact]
        public void Alarm_Codes_Should_Be_Within_Valid_Range()
        {
            var alarms = new AlarmCodes();

            foreach (var id in alarms.AlarmDict11.Keys)
                Assert.InRange(id, 1, 9); // GRBL alarm codes = 1 → 9

            foreach (var id in alarms.AlarmDict09.Keys)
                Assert.InRange(id, 1, 9);
        }

        [Fact]
        public void Should_Return_Correct_Alarm_By_Version()
        {
            var alarms = new AlarmCodes();

            // Example: Alarm 3 (Hard Limit)
            var alarm11 = alarms.GetAlarm(3, isVersion11: true);
            var alarm09 = alarms.GetAlarm(3, isVersion11: false);

            Assert.NotNull(alarm11);
            Assert.NotNull(alarm09);

            Assert.Equal(3, alarm11.Code);
            Assert.Equal(3, alarm09.Code);

            Assert.False(string.IsNullOrWhiteSpace(alarm11.Message));
            Assert.False(string.IsNullOrWhiteSpace(alarm09.Message));
        }

        [Fact]
        public void Unknown_Alarm_Should_Return_Null()
        {
            var alarms = new AlarmCodes();

            var alarm11 = alarms.GetAlarm(999, true);
            var alarm09 = alarms.GetAlarm(999, false);

            Assert.Null(alarm11);
            Assert.Null(alarm09);
        }

        //GrblAlarm Tests
        [Fact]
        public void Constructor_Should_Set_All_Fields()
        {
            var alarm = new GrblAlarm(3, "Hard limit triggered", AlarmSeverity.Critical);

            Assert.Equal(3, alarm.Code);
            Assert.Equal("Hard limit triggered", alarm.Message);
            Assert.Equal(AlarmSeverity.Critical, alarm.Severity);
        }

        [Fact]
        public void ToString_Should_Return_Formatted_String()
        {
            var alarm = new GrblAlarm(2, "Soft limit", AlarmSeverity.Warning);

            var text = alarm.ToString();

            Assert.Contains("2", text);
            Assert.Contains("Soft limit", text);
            Assert.Contains("Warning", text);
        }

        [Fact]
        public void Alarm_Should_Allow_Zero_Code()
        {
            var alarm = new GrblAlarm(0, "Unknown alarm", AlarmSeverity.Info);

            Assert.Equal(0, alarm.Code);
            Assert.Equal("Unknown alarm", alarm.Message);
            Assert.Equal(AlarmSeverity.Info, alarm.Severity);
        }

        [Fact]
        public void Alarm_Should_Accept_All_Severity_Levels()
        {
            var critical = new GrblAlarm(1, "Test", AlarmSeverity.Critical);
            var warn = new GrblAlarm(1, "Test", AlarmSeverity.Warning);
            var info = new GrblAlarm(1, "Test", AlarmSeverity.Info);

            Assert.Equal(AlarmSeverity.Critical, critical.Severity);
            Assert.Equal(AlarmSeverity.Warning, warn.Severity);
            Assert.Equal(AlarmSeverity.Info, info.Severity);
        }
    }
}

