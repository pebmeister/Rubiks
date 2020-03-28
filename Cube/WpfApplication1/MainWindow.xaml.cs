using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using WpfApplication1.Annotations;

// ReSharper disable once UnusedMember.Global
namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        /// <summary>
        /// sequence being executed
        /// </summary>
        public string Sequence
        {
            get { return _sequence; }
            set
            {
                _sequence = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Index of sequence being executed
        /// </summary>
        public int CurrentSequenceIndex
        {
            get { return _currentSequenceIndex; }
            set
            {
                _currentSequenceIndex = value;
                OnPropertyChanged();

                if (Sequence.Length == 0) return;

                Dispatcher?.BeginInvoke(new Action(() =>
                {
                    var moves = Sequence.Split(new[] {' ', '\t', '\v', '\r', '\n'},
                        StringSplitOptions.RemoveEmptyEntries);

                    var flowdoc = new FlowDocument();
                    var p1 = new Paragraph();
                    for (var i = 0; i < _currentSequenceIndex; i++)
                        p1.Inlines.Add(new Span(new Run(" " + moves[i]) {Foreground = Brushes.Blue, FontSize = 14}));

                    p1.Inlines.Add(new Span(new Run(" " + moves[_currentSequenceIndex])
                        {Foreground = Brushes.Black, Background = Brushes.White, FontSize = 14}));
                    for (var i = _currentSequenceIndex + 1; i < moves.Length; i++)
                        p1.Inlines.Add(new Span(new Run(" " + moves[i]) {Foreground = Brushes.Blue, FontSize = 14}));

                    flowdoc.Blocks.Add(p1);
                    SequenceText.Document = flowdoc;
                }));
            }
        }

        /// <summary>
        /// Cuurent move being executed
        /// </summary>
        public string CurrentSequenceMove
        {
            get { return _currentSequenceMove; }
            set
            {
                _currentSequenceMove = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Radius of the Camera
        /// </summary>
        public string R
        {
            get
            {
                if (string.IsNullOrEmpty(_r))
                {
                    _r = $"{Cube.CameraR:0.00}";
                }

                return _r;
            }
            set
            {
                _r = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Theta of the Camera
        /// </summary>
        public string Theta
        {
            get
            {
                if (string.IsNullOrEmpty(_theta))
                {
                    _theta = $"{Cube.CameraTheta:0.00}";
                }

                return _theta;
            }
            set
            {
                _theta = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Phi of the Camera
        /// </summary>
        public string Phi
        {
            get
            {
                if (string.IsNullOrEmpty(_phi))
                {
                    _phi = $"{Cube.CameraPhi:0.00}";
                }

                return _phi;
            }
            set
            {
                _phi = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Width of the camera
        /// </summary>
        public string CameraWidth
        {
            get
            {
                if (string.IsNullOrEmpty(_cameraWidth))
                {
                    _cameraWidth = $"{Cube.CameraWidth:0.00}";
                }

                return _cameraWidth;
            }
            set
            {
                _cameraWidth = value;
                OnPropertyChanged();
            }
        }

        private int _currentSequenceIndex;
        private string _cameraWidth;
        private string _sequence;
        private string _currentSequenceMove;
        private string _r;
        private string _theta;
        private string _phi;
        private readonly double _rDefault;
        private readonly double _thetaDefault;
        private readonly double _phiDefault;
        private readonly double _widthDefault;

        public MainWindow()
        {
            InitializeComponent();

            // get camera defaults
            _rDefault = Cube.CameraR;
            _thetaDefault = Cube.CameraTheta;
            _phiDefault = Cube.CameraPhi;
            _widthDefault = Cube.CameraWidth;

            // subscribe to property changes
            Cube.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(Cube.CameraR):
                        R = $"{Cube.CameraR:0.00}";
                        break;

                    case nameof(Cube.CameraTheta):
                        Theta = $"{Cube.CameraTheta:0.00}";
                        break;

                    case nameof(Cube.CameraPhi):
                        Phi = $"{Cube.CameraPhi:0.00}";
                        break;

                    case nameof(Cube.CameraWidth):
                        CameraWidth = $"{Cube.CameraWidth:0.00}";
                        break;
                }
            };

            // subscribe to move
            Cube.MoveProcessEvent += (sender, args) =>
            {
                CurrentSequenceIndex = args.Index;
                CurrentSequenceMove = args.Move;
            };

            // subscribe to pause/resume
            Cube.PauseEvent += (sender, args) =>
            {
                Dispatcher?.BeginInvoke(new Action(() => { RoutedCommands.PauseCommand.SetIsRunning(false); }));
            };

            // subscribe to sequence start
            Cube.SequenceStart += (sender, args) => { Sequence = args.Sequence; };

            // subscribe to sequence end
            Cube.SequenceEnd += (sender, args) =>
            {
                Dispatcher?.BeginInvoke(new Action(() =>
                {
                    Sequence = args.Sequence;
                    if (RoutedCommands.ScrambleCommand.IsExecuting())
                        RoutedCommands.ScrambleCommand.SetIsRunning(false);
                    else if (RoutedCommands.SolveCommand.IsExecuting())
                        RoutedCommands.SolveCommand.SetIsRunning(false);
                    else if (RoutedCommands.ExecuteSequenceCommand.IsExecuting())
                        RoutedCommands.ExecuteSequenceCommand.SetIsRunning(false);
                }));
            };
        }

        private static bool AnyCommandIsRunning()
        {
            return
                RoutedCommands.ScrambleCommand.IsExecuting() ||
                RoutedCommands.SolveCommand.IsExecuting() ||
                RoutedCommands.ExecuteSequenceCommand.IsExecuting() ||
                ApplicationCommands.Save.IsExecuting() ||
                ApplicationCommands.Open.IsExecuting();
        }

        private void ScrambleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !AnyCommandIsRunning();
        }

        private async void ScrambleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Command.SetIsRunning(true);
            await Task.Run(() => Cube.Scramble());
        }

        private void SolveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Cube.Solved && !AnyCommandIsRunning();
        }

        private async void SolveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Command.SetIsRunning(true);
            await Task.Run(() => Cube.Solve());
        }

        private void ExecuteSequenceCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !AnyCommandIsRunning();
        }

        private async void ExecuteSequenceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Command.SetIsRunning(true);
            await Task.Run(() => Cube.ExecuteSequence(e.Parameter.ToString()));
        }

        private void PauseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ApplicationCommands.Save.IsExecuting() || ApplicationCommands.Open.IsExecuting())
            {
                e.CanExecute = false;
                return;
            }

            bool pause;
            if (e.Parameter == null || !bool.TryParse(e.Parameter.ToString(), out pause))
                pause = false;

            if (pause &&
                !RoutedCommands.ScrambleCommand.IsExecuting() &&
                !RoutedCommands.SolveCommand.IsExecuting() &&
                !RoutedCommands.ExecuteSequenceCommand.IsExecuting()
                )
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = !e.Command.IsExecuting() && Cube.Paused != pause;
        }

        private async void PauseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Command.SetIsRunning(true);
            await Task.Run(() =>
            {
                bool pause;
                if (e.Parameter == null || !bool.TryParse(e.Parameter.ToString(), out pause))
                    pause = false;

                Cube.Pause(pause);
            });
        }

        private void OpenCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !AnyCommandIsRunning();
        }

        private async void OpenCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Command.SetIsRunning(true);
            await Task.Run(() =>
            {
                var openFileDialog = new OpenFileDialog();
                try
                {
                    if (openFileDialog.ShowDialog().HasValue)
                    {
                        var text = File.ReadAllText(openFileDialog.FileName);
                        Cube.SetCube(text);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                Dispatcher?.BeginInvoke(new Action(() => e.Command.SetIsRunning(false)));
            });
        }

        private void SaveCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!AnyCommandIsRunning())
            {
                e.CanExecute = true;
                return;
            }
            if (ApplicationCommands.Save.IsExecuting() || ApplicationCommands.Open.IsExecuting())
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = !RoutedCommands.PauseCommand.IsExecuting() && Cube.Paused;
        }

        private async void SaveCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Command.SetIsRunning(true);
            await Task.Run(() =>
            {
                var saveFileDialog = new SaveFileDialog();
                try
                {
                    saveFileDialog.Filter = "Text file (*.txt)|*.txt|Any (*.*)|.*";
                    saveFileDialog.FileName = "Rubiks.txt";
                    if (saveFileDialog.ShowDialog().HasValue)
                    {
                        var buffer = Cube.GetCube();
                        File.WriteAllText(saveFileDialog.FileName, buffer);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                Dispatcher?.BeginInvoke(new Action(() => e.Command.SetIsRunning(false)));
            });
        }


        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            // The change in CameraPhi.
            const double cameraDPhi = 0.025;

            // The change in CameraTheta.
            const double cameraDTheta = 0.025;

            // The change in CameraR
            const double cameraDr = 0.025;

            // The change in Camera width 
            const double cameraDw = 0.025;

            double angle;
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (e.Key)
            {
                default:
                    e.Handled = false;
                    break;

                case Key.Enter:
                    Cube.CameraR = _rDefault;
                    Cube.CameraTheta = _thetaDefault;
                    Cube.CameraPhi = _phiDefault;
                    Cube.CameraWidth = _widthDefault;
                    e.Handled = true;
                    break;

                case Key.Up:
                    angle = (Cube.CameraPhi + cameraDPhi) % (2 * Math.PI);
                    Cube.CameraPhi = angle;
                    e.Handled = true;
                    break;

                case Key.Down:
                    angle = (Cube.CameraPhi - cameraDPhi) % (2 * Math.PI);
                    Cube.CameraPhi = angle;
                    e.Handled = true;
                    break;

                case Key.Left:
                    angle = (Cube.CameraTheta + cameraDTheta) % (2 * Math.PI);
                    Cube.CameraTheta = angle;
                    e.Handled = true;
                    break;

                case Key.Right:
                    angle = (Cube.CameraTheta - cameraDTheta) % (2 * Math.PI);
                    Cube.CameraTheta = angle;
                    e.Handled = true;
                    break;

                case Key.Add:
                case Key.OemPlus:
                    Cube.CameraR -= cameraDr;
                    e.Handled = true;
                    break;

                case Key.Subtract:
                case Key.OemMinus:
                    Cube.CameraR += cameraDr;
                    e.Handled = true;
                    break;

                case Key.Z:
                    Cube.CameraWidth += cameraDw;
                    e.Handled = true;
                    break;

                case Key.X:
                    Cube.CameraWidth -= cameraDw;
                    e.Handled = true;
                    break;

                case Key.Space:
                    var pt3d = Cube.CameraPosition.ToPoint3D();
                    pt3d.X = -pt3d.X;
                    pt3d.Y = -pt3d.Y;
                    pt3d.Z = -pt3d.Z;
                    var t = pt3d.ToSphere();
                    Cube.CameraR = t.Item1;
                    Cube.CameraTheta = t.Item2;
                    Cube.CameraPhi = t.Item3;
                    e.Handled = true;
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Cube.Width = e.NewSize.Width - ButtonGrid.ActualWidth;
            Cube.Height = e.NewSize.Height;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Cube.StopProcessing();
        }

    }
}
