#nullable enable
using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using Microsoft.Xaml.Behaviors;

namespace FDTD2DLab.Infrastructure.Behaviors
{
    public class SelectAll : Behavior<TextBoxBase>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanged += OnTextChanged;
        }

        private static void OnTextChanged(object? Sender, EventArgs E)
        {
            if (Sender is not TextBox { Text: { Length: > 0 } } text_box) return;
            text_box.Initialized -= OnTextChanged;
            text_box.SelectAll();
        }
    }
}
