using VDLaser.Core.Codes;
using VDLaser.Core.Gcode.Models;
using VDLaser.Core.Grbl.Models;
using Xunit;

namespace VDLaser.Tests.UnitTests
{
    public class ErrorTests
    {
        //ErrorCodes Tests
        [Fact]
        public void Should_Have_Error_Dictionaries()
        {
            var codes = new ErrorCodes();

            Assert.NotEmpty(codes.ErrorDict09);
            Assert.NotEmpty(codes.ErrorDict11);
        }

        [Fact]
        public void Should_Have_Error_Code_Descriptions()
        {
            var codes = new ErrorCodes();

            foreach (var kv in codes.ErrorDict11)
            {
                Assert.False(string.IsNullOrWhiteSpace(kv.Value.Message));
                Assert.False(string.IsNullOrWhiteSpace(kv.Value.Description));
            }
        }

        [Fact]
        public void Errors09_And_11_Should_Use_Valid_Codes()
        {
            var codes = new ErrorCodes();

            foreach (var id in codes.ErrorDict09.Keys)
                Assert.InRange(id, 1, 38);

            foreach (var id in codes.ErrorDict11.Keys)
                Assert.InRange(id, 1, 38);
        }
        //GrblError tests
        [Fact]
        public void Constructor_Should_Set_All_Fields()
        {
            var err = new GrblError(22, "Feed undefined", "Feed rate must be specified");

            Assert.Equal(22, err.Code);
            Assert.Equal("Feed undefined", err.Message);
            Assert.Equal("Feed rate must be specified", err.Description);
            Assert.NotEqual(default, err.Timestamp);
        }

        [Fact]
        public void ToString_Should_Return_Formatted_String()
        {
            var err = new GrblError(7, "EEPROM read fail", "EEPROM read error — defaults restored.");

            var text = err.ToString();

            Assert.Contains("7", text);
            Assert.Contains("EEPROM", text);
        }

        [Fact]
        public void Error_Should_Accept_Null_Description()
        {
            var err = new GrblError(1, "Letter missing", null);

            Assert.Equal("Letter missing", err.Message);
            Assert.Null(err.Description);
        }

        [Fact]
        public void Error_Should_Allow_Zero_Code()
        {
            var err = new GrblError(0, "Unknown", "Should not happen");

            Assert.Equal(0, err.Code);
        }
    }
}
