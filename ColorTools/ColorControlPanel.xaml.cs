using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ColorTools
{
    /// <summary>
    /// Interaction logic for ColorControlPanel.xaml
    /// </summary>
    public partial class ColorControlPanel : UserControl
    {
        private Color iniColor = Colors.Black;
        private Color tmpColor = Colors.Black;
        private Color outColor = Colors.Black;
        public Color SelectedColor { get { return outColor; } }

        private double outColorH, outColorS, outColorV;

        private Color thumbRColor = Color.FromRgb(0, 0, 0);
        private Color thumbGColor = Color.FromRgb(0, 0, 0);
        private Color thumbBColor = Color.FromRgb(0, 0, 0);
        private Color thumbHColor = Color.FromRgb(0, 0, 0);

        private SolidColorBrush iniColorBrush = new SolidColorBrush();
        private SolidColorBrush outColorBrush = new SolidColorBrush();
        private SolidColorBrush thumbSVbrush = new SolidColorBrush();
        private SolidColorBrush thumbRbrush;
        private SolidColorBrush thumbGbrush;
        private SolidColorBrush thumbBbrush;
        private SolidColorBrush thumbHbrush;

        private LinearGradientBrush SaturationGradBrush;
        private LinearGradientBrush RgradBrush;
        private LinearGradientBrush GgardBrush;
        private LinearGradientBrush BgradBrush;
        private LinearGradientBrush AgradBrush;
        
        private void IniGradientBrushes()
        {
            SaturationGradBrush = SaturationGradient.Background as LinearGradientBrush;

            RgradBrush = sliderRed.Background as LinearGradientBrush;
            GgardBrush = sliderGreen.Background as LinearGradientBrush;
            BgradBrush = sliderBlue.Background as LinearGradientBrush;
            AgradBrush = sliderAlpha.Background as LinearGradientBrush;
        }

        /// <summary>
        /// Loaded event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IniThumbsBrushes(object sender, RoutedEventArgs e)
        {
            Thumb theThumb;
            Ellipse theEllipse;
            Rectangle theRectangle;

            // Red
            theThumb = sliderRed.Template.FindName("TrackThumb", sliderRed) as Thumb;
            theEllipse = theThumb.Template.FindName("Ellipse", theThumb) as Ellipse;
            thumbRbrush = theEllipse.Fill as SolidColorBrush;
            
            // Green
            theThumb = sliderGreen.Template.FindName("TrackThumb", sliderGreen) as Thumb;
            theEllipse = theThumb.Template.FindName("Ellipse", theThumb) as Ellipse;
            thumbGbrush = theEllipse.Fill as SolidColorBrush;

            // Blue
            theThumb = sliderBlue.Template.FindName("TrackThumb", sliderBlue) as Thumb;
            theEllipse = theThumb.Template.FindName("Ellipse", theThumb) as Ellipse;
            thumbBbrush = theEllipse.Fill as SolidColorBrush;

            // Hue
            theThumb = sliderSpectrum.Template.FindName("TrackThumb", sliderSpectrum) as Thumb;
            theRectangle = theThumb.Template.FindName("Ellipse", theThumb) as Rectangle;
            thumbHbrush = theRectangle.Fill as SolidColorBrush;

            // Output color
            thumbSV.Fill = thumbSVbrush;

            if (setIniFlag) AdjustThumbs(iniColor);
            else SetInitialColor(Color.FromRgb(0, 0, 0));
        }

        // Algorithm taken from Wikipedia https://en.wikipedia.org/wiki/HSL_and_HSV
        public static void ConvertRgbToHsv(Color color, out double hue, out double saturation, out double value)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double chroma, min = r, max = r;

            if (g > max) max = g;
            else if (g < min) min = g;

            if (b > max) max = b;
            else if (b < min) min = b;

            value = max;
            chroma = max - min;

            if (value == 0) saturation = 0;
            else saturation = chroma / max;

            if (saturation == 0) hue = 0;
            else if (max == r) hue = (g - b) / chroma;
            else if (max == g) hue = 2 + (b - r) / chroma;
            else hue = 4 + (r - g) / chroma;

            hue *= 60;
            if (hue < 0) hue += 360;
        }

        // Algorithm taken from Wikipedia https://en.wikipedia.org/wiki/HSL_and_HSV
        public static Color ConvertHsvToRgb(double hue, double saturation, double value)
        {
            double chroma = value * saturation;

            if (hue == 360) hue = 0;

            double hueTag = hue / 60;
            double x = chroma * (1 - Math.Abs(hueTag % 2 - 1));
            double m = value - chroma;

            double R, G, B;

            switch ((int)hueTag)
            {
                case 0:
                    R = chroma; G = x; B = 0;
                    break;
                case 1:
                    R = x; G = chroma; B = 0;
                    break;
                case 2:
                    R = 0; G = chroma; B = x;
                    break;
                case 3:
                    R = 0; G = x; B = chroma;
                    break;
                case 4:
                    R = x; G = 0; B = chroma;
                    break;
                default:
                    R = chroma; G = 0; B = x;
                    break;
            }

            R += m; G += m; B += m;
            R *= 255; G *= 255; B *= 255;

            return Color.FromRgb((byte)R, (byte)G, (byte)B);
        }

        public static double GetBrightness(byte R, byte G, byte B)
        {
            // Value = Max(R,G,B)
            byte max = R;

            if (G > max) max = G;
            if (B > max) max = B;

            return max / 255.0;
        }

        public static double GetSaturation(byte R, byte G, byte B)
        {
            double r = R / 255.0;
            double g = G / 255.0;
            double b = B / 255.0;

            double chroma, value, saturation;
            double min = r, max = r;

            if (g > max) max = g;
            else if (g < min) min = g;

            if (b > max) max = b;
            else if (b < min) min = b;

            value = max;
            chroma = max - min;

            if (value == 0) saturation = 0;
            else saturation = chroma / max;

            return saturation;
        }
        
        private bool ColorCodeParser(string hexcode, out Color color)
        {
            color = Color.FromArgb(0, 0, 0, 0);
            bool success = false;
            
            if (!string.IsNullOrWhiteSpace(hexcode.Trim()))
            {
                if (hexcode.Substring(0, 1) == "#") hexcode = hexcode.Substring(1);

                if (hexcode.Length == 8)
                {
                    byte numeric;
                    string strByte;

                    // Alpha
                    strByte = hexcode.Substring(0, 2);

                    if (!byte.TryParse(strByte, NumberStyles.HexNumber, null as IFormatProvider, out numeric)) return false; // >>> FAILED >>>
                    color.A = numeric;

                    // Red
                    strByte = hexcode.Substring(2, 2);

                    if (!byte.TryParse(strByte, NumberStyles.HexNumber, null as IFormatProvider, out numeric)) return false; // >>> FAILED >>>
                    color.R = numeric;

                    // Green
                    strByte = hexcode.Substring(4, 2);

                    if (!byte.TryParse(strByte, NumberStyles.HexNumber, null as IFormatProvider, out numeric)) return false; // >>> FAILED >>>
                    color.G = numeric;

                    // Blue
                    strByte = hexcode.Substring(6, 2);

                    if (!byte.TryParse(strByte, NumberStyles.HexNumber, null as IFormatProvider, out numeric)) return false; // >>> FAILED >>>
                    color.B = numeric;

                    success = true;
                }
            }

            return success;
        }

        private void InputComponent(TextBox inputBox)
        {
            string input = inputBox.Text;
            byte numeric;

            switch (inputBox.Name)
            {
                case "txtAvalue":
                    if (byte.TryParse(input, out numeric) && outColor.A != numeric)
                    {
                        outColor.A = numeric;
                        AdjustThumbs(outColor);
                    }

                    txtAvalue.Text = outColor.A.ToString();
                    break;

                case "txtRvalue":
                    if (byte.TryParse(input, out numeric) && outColor.R != numeric)
                    {
                        outColor.R = numeric;
                        AdjustThumbs(outColor);
                    }

                    txtRvalue.Text = outColor.R.ToString();
                    break;

                case "txtGvalue":
                    if (byte.TryParse(input, out numeric) && outColor.G != numeric)
                    {
                        outColor.G = numeric;
                        AdjustThumbs(outColor);
                    }

                    txtGvalue.Text = outColor.G.ToString();
                    break;

                case "txtBvalue":
                    if (byte.TryParse(input, out numeric) && outColor.B != numeric)
                    {
                        outColor.B = numeric;
                        AdjustThumbs(outColor);
                    }

                    txtBvalue.Text = outColor.B.ToString();
                    break;

                case "txtColorCode":
                    Color buffColor;

                    if (ColorCodeParser(input, out buffColor) && outColor != buffColor)
                    {
                        AdjustThumbs(buffColor);
                    }
                    else txtColorCode.Text = outColor.ToString();

                    break;
            }
        }
        
        private void AdjustThumbs(Color theColor)
        {
            SwithHandlers(false);

            // --- ARGB ---
            byte A = theColor.A;
            byte R = theColor.R;
            byte G = theColor.G;
            byte B = theColor.B;

            outColor = theColor;
            outColorBrush.Color = theColor;
            txtColorCode.Text = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", A, R, G, B);

            // Alpha
            sliderAlpha.Value = A;
            txtAvalue.Text = A.ToString();

            theColor.A = 0;
            AgradBrush.GradientStops[0].Color = theColor;
            theColor.A = 255;
            AgradBrush.GradientStops[1].Color = theColor;
            //theColor.A = A; the alpha will be setted after the SV thumb adjusting

            // Red
            sliderRed.Value = R;
            txtRvalue.Text = R.ToString();

            theColor.R = 0;
            RgradBrush.GradientStops[0].Color = theColor;
            theColor.R = 255;
            RgradBrush.GradientStops[1].Color = theColor;
            theColor.R = R;

            thumbRColor.R = R;
            thumbRbrush.Color = thumbRColor;

            // Green
            sliderGreen.Value = G;
            txtGvalue.Text = G.ToString();

            theColor.G = 0;
            GgardBrush.GradientStops[0].Color = theColor;
            theColor.G = 255;
            GgardBrush.GradientStops[1].Color = theColor;
            theColor.G = G;

            thumbGColor.G = G;
            thumbGbrush.Color = thumbGColor;

            // Blue
            sliderBlue.Value = B;
            txtBvalue.Text = B.ToString();

            theColor.B = 0;
            BgradBrush.GradientStops[0].Color = theColor;
            theColor.B = 255;
            BgradBrush.GradientStops[1].Color = theColor;
            theColor.B = B;

            thumbBColor.B = B;
            thumbBbrush.Color = thumbBColor;

            // --- HSV ---
            ConvertRgbToHsv(theColor, out outColorH, out outColorS, out outColorV);
            
            thumbHColor = ConvertHsvToRgb(outColorH, 1, 1);

            // Hue
            thumbHbrush.Color = thumbHColor;
            sliderSpectrum.Value = outColorH;

            // SV thumb
            outColor.A = 255;
            thumbSVbrush.Color = outColor;
            outColor.A = A;

            // Saturation gradient
            SaturationGradBrush.GradientStops[1].Color = thumbHColor;

            // Saturation and value to canvas coords
            Canvas.SetLeft(thumbSV, outColorS * SaturationGradient.ActualWidth - 0.5 * thumbSV.ActualWidth);
            Canvas.SetTop(thumbSV, (1 - outColorV) * SaturationGradient.ActualHeight - 0.5 * thumbSV.ActualHeight);

            // RISE EVENT
            if (tmpColor != outColor)
            {
                ColorChanged?.Invoke(this, new ColorChangedEventArgs(iniColor, tmpColor, outColor));
                tmpColor = outColor;
            } 

            SwithHandlers(true);
        }

        private void AdjustThumbs(double H, double S, double V)
        {
            SwithHandlers(false);

            // --- HSV ---
            thumbHColor = ConvertHsvToRgb(H, 1, 1);

            // Hue
            thumbHbrush.Color = thumbHColor;
            sliderSpectrum.Value = H;

            // Saturation gradient
            SaturationGradBrush.GradientStops[1].Color = thumbHColor;

            // Saturation and value to canvas coords
            Canvas.SetLeft(thumbSV, S * SaturationGradient.ActualWidth - 0.5 * thumbSV.ActualWidth);
            Canvas.SetTop(thumbSV, (1 - V) * SaturationGradient.ActualHeight - 0.5 * thumbSV.ActualHeight);
            
            byte A = outColor.A;
            outColor = ConvertHsvToRgb(H, S, V);

            // SV thumb
            thumbSVbrush.Color = outColor;

            // --- ARGB ---
            outColor.A = A;
            byte R = outColor.R;
            byte G = outColor.G;
            byte B = outColor.B;

            outColorBrush.Color = outColor;
            txtColorCode.Text = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", A, R, G, B);

            // Alpha
            sliderAlpha.Value = A;
            txtRvalue.Text = A.ToString();

            outColor.A = 0;
            AgradBrush.GradientStops[0].Color = outColor;
            outColor.A = 255;
            AgradBrush.GradientStops[1].Color = outColor;
            //outColor.A = A; // the alpha will be setted after RGB adjusting

            // Red
            sliderRed.Value = R;
            txtRvalue.Text = R.ToString();

            outColor.R = 0;
            RgradBrush.GradientStops[0].Color = outColor;
            outColor.R = 255;
            RgradBrush.GradientStops[1].Color = outColor;
            outColor.R = R;

            thumbRColor.R = R;
            thumbRbrush.Color = thumbRColor;

            // Green
            sliderGreen.Value = G;
            txtGvalue.Text = G.ToString();

            outColor.G = 0;
            GgardBrush.GradientStops[0].Color = outColor;
            outColor.G = 255;
            GgardBrush.GradientStops[1].Color = outColor;
            outColor.G = G;

            thumbGColor.G = G;
            thumbGbrush.Color = thumbGColor;

            // Blue
            sliderBlue.Value = B;
            txtBvalue.Text = B.ToString();

            outColor.B = 0;
            BgradBrush.GradientStops[0].Color = outColor;
            outColor.B = 255;
            BgradBrush.GradientStops[1].Color = outColor;
            outColor.B = B;

            thumbBColor.B = B;
            thumbBbrush.Color = thumbBColor;

            outColor.A = A;

            // RISE EVENT
            if (tmpColor != outColor)
            {
                ColorChanged?.Invoke(this, new ColorChangedEventArgs(iniColor, tmpColor, outColor));
                tmpColor = outColor;
            }

            SwithHandlers(true);
        }

        private void MLBdownOverSVsquare(object sender, MouseButtonEventArgs e)
        {
            SaturationGradient.MouseLeftButtonDown -= MLBdownOverSVsquare;
            SaturationGradient.MouseLeftButtonUp += MLBupSVsquare;

            SaturationGradient.MouseMove += SVthumbMove;

            SaturationGradient.CaptureMouse();

            SVthumbMove(e.GetPosition(SaturationGradient));
        }

        private void MLBupSVsquare(object sender, MouseButtonEventArgs e)
        {
            SaturationGradient.ReleaseMouseCapture();

            SaturationGradient.MouseMove -= SVthumbMove;

            SaturationGradient.MouseLeftButtonDown += MLBdownOverSVsquare;
            SaturationGradient.MouseLeftButtonUp -= MLBupSVsquare;
        }

        private void SVthumbMove(Point point)
        {
            double X = point.X;
            double Y = point.Y;

            if (X < 0) outColorS = 0;
            else if (X > SaturationGradient.ActualWidth) outColorS = 1;
            else outColorS = (float)(X / SaturationGradient.ActualWidth);

            if (Y < 0) outColorV = 1;
            else if (Y > SaturationGradient.ActualHeight) outColorV = 0;
            else outColorV = (float)(1 - Y / SaturationGradient.ActualHeight);

            AdjustThumbs(outColorH, outColorS, outColorV);
        }

        private void SVthumbMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(SaturationGradient);
            double X = point.X;
            double Y = point.Y;

            if (X < 0) outColorS = 0;
            else if (X > SaturationGradient.ActualWidth) outColorS = 1;
            else outColorS = (float)(X / SaturationGradient.ActualWidth);

            if (Y < 0) outColorV = 1;
            else if (Y > SaturationGradient.ActualHeight) outColorV = 0;
            else outColorV = (float)(1 - Y / SaturationGradient.ActualHeight);

            AdjustThumbs(outColorH, outColorS, outColorV);
        }

        private void HueThumbMove(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            outColorH = (float)e.NewValue;
            AdjustThumbs(outColorH, outColorS, outColorV);
        }

        private void RedThumbMove(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            outColor.R = (byte)e.NewValue;
            AdjustThumbs(outColor);
        }

        private void GreenThumbMove(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            outColor.G = (byte)e.NewValue;
            AdjustThumbs(outColor);
        }

        private void BlueThumbMove(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            outColor.B = (byte)e.NewValue;
            AdjustThumbs(outColor);
        }

        private void AlphaThumbMove(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            outColor.A = (byte)e.NewValue;

            outColorBrush.Color = outColor;
            txtAvalue.Text = outColor.A.ToString();
            txtColorCode.Text = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", outColor.A, outColor.R, outColor.G, outColor.B);

            // RISE EVENT
            if (tmpColor != outColor)
            {
                ColorChanged?.Invoke(this, new ColorChangedEventArgs(iniColor, tmpColor, outColor));
                tmpColor = outColor;
            }
        }

        private void LostKeyFocus_RGBApanel(object sender, RoutedEventArgs e)
        {
            TextBox inputBox = e.Source as TextBox;

            if (inputBox != null)
            {
                InputComponent(inputBox);
                e.Handled = true;
            }
        }

        private void KeyDown_RGBApanel(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox inputBox = e.Source as TextBox;

                if (inputBox != null)
                {
                    InputComponent(inputBox);
                    e.Handled = true;
                }
            }
        }

        private void RevertIniColor(object sender, MouseEventArgs e)
        {
            outColor = iniColor;
            AdjustThumbs(outColor);
        }

        private void SwithHandlers(bool ON)
        {
            if (ON)
            {
                sliderSpectrum.ValueChanged += HueThumbMove;
                sliderRed.ValueChanged += RedThumbMove;
                sliderGreen.ValueChanged += GreenThumbMove;
                sliderBlue.ValueChanged += BlueThumbMove;
                sliderAlpha.ValueChanged += AlphaThumbMove;
            }
            else // OFF
            {
                sliderSpectrum.ValueChanged -= HueThumbMove;
                sliderRed.ValueChanged -= RedThumbMove;
                sliderGreen.ValueChanged -= GreenThumbMove;
                sliderBlue.ValueChanged -= BlueThumbMove;
                sliderAlpha.ValueChanged -= AlphaThumbMove;
            }
        }

        private bool setIniFlag = false;
        public void SetInitialColor(Color incolor)
        {
            iniColor = incolor;
            tmpColor = incolor;
            outColor = incolor;

            iniColorBrush.Color = iniColor;
            outColorBrush.Color = iniColor;

            rectInitialColor.Background = iniColorBrush;
            rectSelectedColor.Background = outColorBrush;

            if (IsLoaded)
            {
                AdjustThumbs(iniColor);
            }
            else
            {
                setIniFlag = true;
            }  
        }

        public class ColorChangedEventArgs : EventArgs
        {
            private Color iniColor;
            private Color previousColor;
            private Color currentColor;

            public Color InitialColor { get { return iniColor; } }
            public Color PreviousColor { get { return previousColor; } }
            public Color CurrentColor { get { return currentColor; } }

            public ColorChangedEventArgs(Color iniC, Color preC, Color curC)
            {
                iniColor = iniC;
                previousColor = preC;
                currentColor = curC;
            }
        }

        public event EventHandler<ColorChangedEventArgs> ColorChanged;

        public ColorControlPanel()
        {
            InitializeComponent();
            IniGradientBrushes();

            Loaded += IniThumbsBrushes;

            // Subscribe on events
            SaturationGradient.MouseLeftButtonDown += MLBdownOverSVsquare;

            // The following handlers are added in the SwithHandlers method
            //sliderSpectrum.ValueChanged += HueThumbMove;
            //sliderRed.ValueChanged += RedThumbMove;
            //sliderGreen.ValueChanged += GreenThumbMove;
            //sliderBlue.ValueChanged += BlueThumbMove;
            //sliderAlpha.ValueChanged += AlphaThumbMove;

            RGBAdock.LostKeyboardFocus += LostKeyFocus_RGBApanel;
            RGBAdock.KeyDown += KeyDown_RGBApanel;

            rectInitialColor.MouseLeftButtonDown += RevertIniColor;
        }
    }
}
