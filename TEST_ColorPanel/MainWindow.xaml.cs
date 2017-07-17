using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ColorTools;

namespace TEST_ColorPanel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Color[] ButtonsColors;

        private ColorControlPanel colorPanel;
        private SetColorWin ccpWindow = new SetColorWin();

        private int BttIndex = 0;

        public MainWindow()
        {
            InitializeComponent();

            ButtonsColors = new Color[3];
            ButtonsColors[0] = (button0.Foreground as SolidColorBrush).Color;
            ButtonsColors[1] = (button1.Background as LinearGradientBrush).GradientStops[1].Color;
            ButtonsColors[2] = (button2.Background as SolidColorBrush).Color;
        }

        private void openColorControls()
        {
            ccpWindow = new SetColorWin();
            colorPanel = ccpWindow.ColorControls;
            colorPanel.ColorChanged += buttons_ColorChanged;

            ccpWindow.Show();
        }

        private void button0_Click(object sender, RoutedEventArgs e)
        {
            if (ccpWindow == null || !ccpWindow.IsVisible) openColorControls();

            BttIndex = 0;
            colorPanel.SetInitialColor(ButtonsColors[0]);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (ccpWindow == null || !ccpWindow.IsVisible) openColorControls();

            BttIndex = 1;
            colorPanel.SetInitialColor(ButtonsColors[1]);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (ccpWindow == null || !ccpWindow.IsVisible) openColorControls();

            BttIndex = 2;
            colorPanel.SetInitialColor(ButtonsColors[2]);
        }
        
        private void updateBttColor()
        {
            (button0.Foreground as SolidColorBrush).Color = ButtonsColors[0];
            (button1.Background as LinearGradientBrush).GradientStops[1].Color = ButtonsColors[1];
            (button2.Background as SolidColorBrush).Color = ButtonsColors[2];
        }

        private void buttons_ColorChanged(object sender, ColorControlPanel.ColorChangedEventArgs e)
        {
            ButtonsColors[BttIndex] = e.CurrentColor;
            updateBttColor();
        }
    }
}
