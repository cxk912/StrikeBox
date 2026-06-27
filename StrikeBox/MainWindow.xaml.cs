using StrikeBox.ViewModels;
using System.Windows;

namespace StrikeBox;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    public MainWindow(MainWindowViewModel viewModel, Services.NavigationService navigationService)
    {
        InitializeComponent();
        DataContext = viewModel;

        ApplyAdaptiveSize();
    }

    /// <summary>
    /// 根据屏幕分辨率与缩放比例计算窗口初始大小，
    /// 确保工具卡片区域正好放下 5×5 列卡片。
    /// 卡片尺寸: 180px 宽 + 12px 边距 = 192px/列；100px 高 + 12px 边距 = 112px/行
    /// 5 列: 192×5 = 960px；加上左导航 240px + 边距 48px ≈ 1250px
    /// 5 行: 112×5 = 560px；加上标题栏等 ≈ 750px
    /// </summary>
    private void ApplyAdaptiveSize()
    {
        const double leftNavWidth = 240;
        const double contentMargin = 48;       // ToolsView Grid Margin="24" × 2
        const double cardSlotWidth = 192;      // 180 card + 6+6 margin
        const int targetColumns = 5;
        const double targetWidth = leftNavWidth + contentMargin + cardSlotWidth * targetColumns + 2;

        const double titleBarHeight = 48;
        const double toolbarHeight = 56;
        const double countLabelHeight = 28;
        const double cardSlotHeight = 112;     // 100 card + 12 margin
        const int targetRows = 5;
        const double targetHeight = titleBarHeight + contentMargin + toolbarHeight
                                    + countLabelHeight + cardSlotHeight * targetRows + 12;

        var workArea = SystemParameters.WorkArea;

        Width = System.Math.Min(targetWidth, workArea.Width * 0.88);
        Height = System.Math.Min(targetHeight, workArea.Height * 0.85);

        // 窗口居中
        Left = (workArea.Width - Width) / 2 + workArea.Left;
        Top = (workArea.Height - Height) / 2 + workArea.Top;
    }
}
