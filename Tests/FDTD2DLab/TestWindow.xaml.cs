using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FDTD2DLab
{
    public partial class TestWindow
    {
        public TestWindow() => InitializeComponent();

        private void OnNumberTextChanged(object Sender, TextCompositionEventArgs E)
        {
            if(E.Source is not TextBox { Text: {Length: > 0} text }) return;
            E.Handled = !IsDouble(text + E.Text);
        }

        private static bool IsDouble(string str)
        {
            var is_fraction = false;
            IFormatProvider provider = CultureInfo.CurrentCulture;
            var s = NumberFormatInfo.GetInstance(provider).NumberDecimalSeparator[0];
            for (var i = 0; i < str.Length; i++)
            {
                if (is_fraction)
                {
                    if (!char.IsDigit(str, i))
                        return false;
                }
                else
                {
                    var c = str[i];
                    if (char.IsDigit(c)) continue;
                    if (c == s) 
                        is_fraction = true;
                    else
                        switch (str[i])
                        {
                            default: return false;
                            case '+' when i == 0:
                            case '-' when i == 0:
                                break;
                        }
                }
            }

            return true;
        }
    }

    public static class Handlers
    {
        public static void OnNumberTextChanged(object Sender, TextCompositionEventArgs E)
        {
            if (E.Source is not TextBox { Text: { Length: > 0 } text }) return;
            E.Handled = !IsDouble(text + E.Text);
        }

        private static bool IsDouble(string str)
        {
            var is_fraction = false;
            IFormatProvider provider = CultureInfo.CurrentCulture;
            var s = NumberFormatInfo.GetInstance(provider).NumberDecimalSeparator[0];
            for (var i = 0; i < str.Length; i++)
            {
                if (is_fraction)
                {
                    if (!char.IsDigit(str, i))
                        return false;
                }
                else
                {
                    var c = str[i];
                    if (char.IsDigit(c)) continue;
                    if (c == s)
                        is_fraction = true;
                    else
                        switch (str[i])
                        {
                            default: return false;
                            case '+' when i == 0:
                            case '-' when i == 0:
                                break;
                        }
                }
            }

            return true;
        }
    }
}
