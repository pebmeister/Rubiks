using System.Windows.Input;

namespace WpfApplication1
{
    public static class RoutedCommands
    {
        public static readonly RoutedUICommand ScrambleCommand = new RoutedUICommand
        (
            "Scramble",
            nameof(ScrambleCommand),
            typeof(MainWindow),
            new InputGestureCollection()
            {
                new KeyGesture(Key.S, ModifierKeys.Alt)
            }
        );

        public static readonly RoutedUICommand SolveCommand = new RoutedUICommand
        (
            "Solve",
            nameof(SolveCommand),
            typeof(MainWindow),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F1)
            }
        );

        public static readonly RoutedUICommand ExecuteSequenceCommand = new RoutedUICommand
        (
            "ExecuteSequence",
            nameof(ExecuteSequenceCommand),
            typeof(MainWindow)
        );

        public static readonly RoutedUICommand PauseCommand = new RoutedUICommand
        (
            "Pause",
            nameof(PauseCommand),
            typeof(MainWindow)
        );

    }
}
