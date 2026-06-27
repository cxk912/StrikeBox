using System.Windows.Data;

namespace StrikeBox.Converters;

public sealed class ToolTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is Models.ToolType type)
        {
            return type switch
            {
                Models.ToolType.GUIEXE => "Gui exe",
                Models.ToolType.GUIJAVA => "Gui Java",
                Models.ToolType.TerminalEXE => "Terminal exe",
                Models.ToolType.TerminalPYTHON => "Terminal Python",
                Models.ToolType.TerminalJAVA => "Terminal Java",
                Models.ToolType.WEB => "Web link",
                _ => type.ToString()
            };
        }
        return "-";
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
