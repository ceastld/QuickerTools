using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuickerTools.Utilities
{
    public static class CommandBindingsHelper
    {
        public static void AddKeyGesture(
          this CommandBindingCollection bindings,
          KeyGesture keyGesture,
          ExecutedRoutedEventHandler handler)
        {
            bindings.Add(new CommandBinding((ICommand)new RoutedCommand()
            {
                InputGestures = {
          (InputGesture) keyGesture
        }
            }, (ExecutedRoutedEventHandler)((sender, args) => handler(sender, args))));
        }
    }
}
