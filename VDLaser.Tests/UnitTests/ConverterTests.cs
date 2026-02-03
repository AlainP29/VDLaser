using Xunit;
using VDLaser.Converters;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using static VDLaser.Core.Grbl.Models.GrblState;

namespace VDLaser.Tests.UnitTests
{
    public class ConverterTests
    {
        // Tests existants pour InverseBooleanToVisibilityConverter
        [Fact]
        public void InverseBooleanToVisibilityConverter_True_ReturnsCollapsed()
        {
            var converter = new InverseBooleanToVisibilityConverter();
            var result = converter.Convert(true, typeof(Visibility), null!, null!);
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void InverseBooleanToVisibilityConverter_False_ReturnsVisible()
        {
            var converter = new InverseBooleanToVisibilityConverter();
            var result = converter.Convert(false, typeof(Visibility), null!, null!);
            Assert.Equal(Visibility.Visible, result);
        }

        // Nouveau : Test pour BoolToVisibilityConverter
        [Fact]
        public void BoolToVisibilityConverter_True_ReturnsVisible()
        {
            var converter = new BoolToVisibilityConverter();
            var result = converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void BoolToVisibilityConverter_False_ReturnsCollapsed()
        {
            var converter = new BoolToVisibilityConverter();
            var result = converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.Equal(Visibility.Collapsed, result);
        }

        // Nouveau : Test pour StatusToColorConverter
        [Fact]
        public void StatusToColorConverter_Idle_ReturnsBeige()
        {
            var converter = new StatusToColorConverter();
            var result = converter.Convert(MachState.Idle, typeof(Brush), null!, CultureInfo.InvariantCulture);
            Assert.Equal(Brushes.Beige, result);
        }

        [Fact]
        public void StatusToColorConverter_Unknown_ReturnsDarkGray()
        {
            var converter = new StatusToColorConverter();
            var result = converter.Convert((MachState)999, typeof(Brush), null!, CultureInfo.InvariantCulture);  // Valeur inconnue
            Assert.Equal(Brushes.DarkGray, result);
        }

        // Nouveau : Test pour StringToIntConverter
        [Fact]
        public void StringToIntConverter_ValidString_ReturnsInt()
        {
            var converter = new StringToIntConverter();
            var result = converter.Convert("123.45", typeof(int), null!, CultureInfo.InvariantCulture);
            Assert.Equal(123, result);
        }

        [Fact]
        public void StringToIntConverter_InvalidString_ReturnsZero()
        {
            var converter = new StringToIntConverter();
            var result = converter.Convert("invalid", typeof(int), null!, CultureInfo.InvariantCulture);
            Assert.Equal(0, result);
        }

        // Nouveau : Test pour PositionToStringConverter
        [Fact]
        public void PositionToStringConverter_ValidDouble_ReturnsFormattedString()
        {
            var converter = new PositionToStringConverter();
            var result = converter.Convert(12.345, typeof(string), "X:", CultureInfo.InvariantCulture);
            Assert.Equal("X:12,35 mm", result);  // Arrondi à 2 décimales
        }

        [Fact]
        public void PositionToStringConverter_Invalid_ReturnsNA()
        {
            var converter = new PositionToStringConverter();
            var result = converter.Convert("not a double", typeof(string), null!, CultureInfo.InvariantCulture);
            Assert.Equal("N/A", result);
        }

        // Nouveau : Test pour InverseBooleanConverter
        [Fact]
        public void InverseBooleanConverter_True_ReturnsFalse()
        {
            var converter = new InverseBooleanConverter();
            var result = converter.Convert(true, typeof(bool), null!, CultureInfo.InvariantCulture);
            Assert.NotNull(result);
            Assert.False((bool)result);
        }

        [Fact]
        public void InverseBooleanConverter_NonBool_ReturnsFalse()
        {
            var converter = new InverseBooleanConverter();
            var result = converter.Convert("not bool", typeof(bool), null!, CultureInfo.InvariantCulture);
            Assert.NotNull(result);
            Assert.False((bool)result);
        }

        // Nouveau : Test pour BrushColorConverter
        [Fact]
        public void BrushColorConverter_True_ReturnsLightGreen()
        {
            var converter = new BrushColorConverter();
            var result = converter.Convert(true, typeof(SolidColorBrush), null!, CultureInfo.InvariantCulture);
            Assert.Equal(new SolidColorBrush(Colors.LightGreen).Color, ((SolidColorBrush)result!).Color);
        }

        [Fact]
        public void BrushColorConverter_False_ReturnsOrangeRed()
        {
            var converter = new BrushColorConverter();
            var result = converter.Convert(false, typeof(SolidColorBrush), null!, CultureInfo.InvariantCulture);
            Assert.Equal(new SolidColorBrush(Colors.OrangeRed).Color, ((SolidColorBrush)result!).Color);
        }

        // Nouveau : Test pour DotToCommaConverter
        [Fact]
        public void DotToCommaConverter_DotString_ReturnsCommaString()
        {
            var converter = new DotToCommaConverter();
            var result = converter.Convert("1.23", typeof(string), null!, CultureInfo.InvariantCulture);
            Assert.Equal("1,23", result);
        }

        [Fact]
        public void DotToCommaConverter_Back_CommaToDot()
        {
            var converter = new DotToCommaConverter();
            var result = converter.ConvertBack("1,23", typeof(string), null!, CultureInfo.InvariantCulture);
            Assert.Equal("1.23", result);
        }

        // Nouveau : Test pour IntToTimeConverter
        [Fact]
        public void IntToTimeConverter_PositiveInt_ReturnsTimeSpan()
        {
            var converter = new IntToTimeConverter();
            var result = converter.Convert(3600, typeof(TimeSpan), null!, CultureInfo.InvariantCulture);
            Assert.Equal(TimeSpan.FromSeconds(3600), result);
        }

        [Fact]
        public void IntToTimeConverter_Zero_ReturnsZeroTimeSpan()
        {
            var converter = new IntToTimeConverter();
            var result = converter.Convert(0, typeof(TimeSpan), null!, CultureInfo.InvariantCulture);
            Assert.Equal(TimeSpan.Zero, result);
        }

        // Nouveau : Test pour PercentageConverter
        [Fact]
        public void PercentageConverter_Double_ReturnsDividedBy100()
        {
            var converter = new PercentageConverter();
            var result = converter.Convert(50.0, typeof(double), null!, CultureInfo.InvariantCulture);
            Assert.Equal(0.5, result);
        }

        [Fact]
        public void PercentageConverter_WithFactor_ReturnsDividedBy100TimesFactor()
        {
            var converter = new PercentageConverter();
            var result = converter.Convert(50.0, typeof(double), "2", CultureInfo.InvariantCulture);  // Factor 2
            Assert.Equal(0.25, result);  // 50 / (100 * 2) = 0.25
        }
    }
}
