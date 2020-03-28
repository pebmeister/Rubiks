using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using WpfApplication1.Annotations;
// ReSharper disable LocalizableElement

namespace WpfApplication1
{
    /// <summary>
    /// Move being processed
    /// </summary>
    public class MoveProcessingArgs : EventArgs
    {
        public string Move { get; set; }
        public int Index { get; set; }
        public MoveProcessingArgs(string move, int index)
        {
            Move = move;
            Index = index;
        }
    }

    /// <summary>
    /// class for sequencestart and sequenceend event arguments
    /// </summary>
    public class PauseArgs : EventArgs
    {
        public bool Pause { get; set; }

        public PauseArgs(bool pause)
        {
            Pause = pause;
        }
    }

    /// <summary>
    /// class for sequencestart and sequenceend event arguments
    /// </summary>
    public class SequenceArgs : EventArgs
    {
        public string Sequence { get; set; }

        public SequenceArgs(string sequence)
        {
            Sequence = sequence;
        }
    }

    /// <summary>
    /// Interaction logic for CubeUserControl.xaml
    /// </summary>
    public partial class CubeUserControl : INotifyPropertyChanged, IDisposable
    {
        private delegate void Rotator();

        private ICube Rc { get; } = new Cube();

        private bool Scrambling
        {
            get { return _scrambling; }
            set
            {
                _scrambling = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// property to determine if the cube os solved
        /// </summary>
        public bool Solved
        {
            get
            {
                var temp = _solved;
                _solved = Rc.issolved();
                if (temp != _solved)
                    OnPropertyChanged();
                return _solved;
            }
        }

        /// <summary>
        /// sequence that is being executed
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
        /// Camera R spherical coordinates
        /// </summary>
        public double CameraR
        {
            get { return _cameraR; }
            set
            {
                _cameraR = value;
                OnPropertyChanged();

                CameraPosition = new Tuple<double, double, double>(CameraR, CameraTheta, CameraPhi);
            }
        }

        /// <summary>
        /// Camera Theta spherical coordinates
        /// </summary>
        public double CameraTheta
        {
            get { return _cameraTheta; }
            set
            {
                _cameraTheta = value;
                OnPropertyChanged();

                CameraPosition = new Tuple<double, double, double>(CameraR, CameraTheta, CameraPhi);
            }
        }

        /// <summary>
        /// Camera Phi spherical coordinates
        /// </summary>
        public double CameraPhi
        {
            get { return _cameraPhi; }
            set
            {
                _cameraPhi = value;
                OnPropertyChanged();

                CameraPosition = new Tuple<double, double, double>(CameraR, CameraTheta, CameraPhi);
            }
        }

        /// <summary>
        /// Camera spherical coordinates
        /// </summary>
        public Tuple<double, double, double> CameraPosition
        {
            get { return _cameraPosition; }
            set
            {
                _cameraPosition = value;
                OnPropertyChanged();

                CubeCamera.Position = CameraPosition.ToPoint3D();
                CubeCamera.LookDirection =
                    new Vector3D(-CubeCamera.Position.X, -CubeCamera.Position.Y, -CubeCamera.Position.Z);
            }
        }

        /// <summary>
        /// Width of camera lens
        /// </summary>
        public double CameraWidth
        {
            get { return _cameraWidth; }
            set
            {
                _cameraWidth = value;
                OnPropertyChanged();
                CubeCamera.Width = value;
            }
        }

        /// <summary>
        /// Camera
        /// </summary>
        private OrthographicCamera CubeCamera
        {
            get { return _cubeCamera; }
            set
            {
                _cubeCamera = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Pause processing moves
        /// </summary>
        public bool Paused
        {
            get { return _paused; }
            private set
            {
                _paused = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// normal speed for processing moves
        /// </summary>
        public int NormalSpeed
        {
            get { return _normalSpeed; }
            set
            {
                _normalSpeed = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Fast speed for processing moves
        /// </summary>
        public int FastSpeed
        {
            get { return _fastSpeed; }
            set
            {
                _fastSpeed = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan AnimationDuration { get; set; } = TimeSpan.FromMilliseconds(500);
        private AutoResetEvent MoveEvent { get; } = new AutoResetEvent(false);
        private Queue<string> MoveQueue { get; } = new Queue<string>();
        private Thread ProcessMovesThread { get; set; }

        public string CubeColors
        {
            get { return _cubeColors; }
            set
            {
                if (string.Compare(value, _cubeColors, StringComparison.CurrentCultureIgnoreCase) == 0) return;

                _cubeColors = value;
                OnPropertyChanged();
                UpdateCubeColors();
            }
        }

        /// <summary>
        /// Gemometery of Cube
        /// </summary>
        private List<List<GeometryModel3D>> GeometryLists
        {
            get { return _geometryLists; }
            set
            {
                _geometryLists = value; 
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Bounding Rect3D of Cube
        /// </summary>
        public Rect3D Bounds
        {
            get
            {
                var rect3d = new Rect3D();
                var minx = double.MaxValue;
                var miny = double.MaxValue;
                var minz = double.MaxValue;
                var maxx = double.MinValue;
                var maxy = double.MinValue;
                var maxz = double.MinValue;

                foreach (var geometryModel3D in GeometryLists.SelectMany(meshList => meshList))
                {
                    if (geometryModel3D.Bounds.X < minx)
                        minx = geometryModel3D.Bounds.X;

                    if (geometryModel3D.Bounds.X + geometryModel3D.Bounds.SizeX > maxx)
                        maxx = geometryModel3D.Bounds.X + geometryModel3D.Bounds.SizeX;

                    if (geometryModel3D.Bounds.Y < miny)
                        miny = geometryModel3D.Bounds.Y;

                    if (geometryModel3D.Bounds.Y + geometryModel3D.Bounds.SizeY > maxy)
                        maxy = geometryModel3D.Bounds.Y + geometryModel3D.Bounds.SizeY;

                    if (geometryModel3D.Bounds.Z < minz)
                        minz = geometryModel3D.Bounds.Z;

                    if (geometryModel3D.Bounds.Z + geometryModel3D.Bounds.SizeZ > maxz)
                        maxz = geometryModel3D.Bounds.Z + geometryModel3D.Bounds.SizeZ;
                }

                rect3d.X = minx;
                rect3d.SizeX = maxx - minx;
                rect3d.Y = miny;
                rect3d.SizeY = maxy - miny;
                rect3d.Z = minz;
                rect3d.SizeZ = maxz - minz;

                return rect3d;
            }
        }

        /// <summary>
        /// Center point of Cube
        /// </summary>
        public Point3D Center
        {
            get
            {
                var point3d = new Point3D();
                var bounds = Bounds;
                point3d.X = bounds.X + bounds.SizeX / 2.0;
                point3d.Y = bounds.Y + bounds.SizeY / 2.0;
                point3d.Z = bounds.Z + bounds.SizeZ / 2.0;
                return point3d;
            }
        }

        /// <summary>
        /// Dictionary of colors
        /// </summary>
        private readonly Dictionary<string, Color> _brushDict =
            new Dictionary<string, Color>()
            {
                {"W", Colors.White},
                {"Y", Colors.Yellow},
                {"R", Colors.Red},
                {"O", Colors.Orange},
                {"B", Colors.Blue},
                {"G", Colors.Green}
            };

        private static readonly Point3D CameraStartPos = new Point3D(2, 1.5, 4);
        private static readonly Tuple<double, double, double> CameraStartPosSphere = CameraStartPos.ToSphere();

        private OrthographicCamera _cubeCamera;
        private bool _paused;
        private int _normalSpeed = 500;
        private int _fastSpeed = 300;
        private double _cameraR = CameraStartPosSphere.Item1;
        private double _cameraTheta = CameraStartPosSphere.Item2;
        private double _cameraPhi = CameraStartPosSphere.Item3;
        private double _cameraWidth = 11.0;
        private string _sequence;
        private bool _scrambling;
        private bool _solved;
        private bool _disposed;
        private Tuple<double, double, double> _cameraPosition = CameraStartPosSphere;
        private List<List<GeometryModel3D>> _geometryLists;
        private string _cubeColors =
            "W W W W W W W W W G G G G G G G G G R R R R R R R R R B B B B B B B B B O O O O O O O O O Y Y Y Y Y Y Y Y Y";


        /// <summary>
        /// Faces of the entire cube
        ///
        /// WARNNG
        /// Must match order in BuildCube
        /// and CreateCubie
        /// </summary>
        private enum CubeFace
        {
            Front = 1,
            Back = 3,
            Right = 5,
            Left = 7,
            Up = 9,
            Down = 11
        }

        /// <summary>
        /// Faces of each cubie
        ///
        /// WARNNG
        /// Must match order in BuildCube
        /// and CreateCubie
        /// </summary>
        private enum Face
        {
            UpFaceTopLeft = 0,
            UpFaceTopMiddle,
            UpFaceTopRight,
            UpFaceMiddleLeft,
            UpFaceMiddleMiddle,
            UpFaceMiddleRight,
            UpFaceBottomLeft,
            UpFaceBottomMiddle,
            UpFaceBottomRight,
            LeftFaceTopLeft,
            LeftFaceTopMiddle,
            LeftFaceTopRight,
            LeftFaceMiddleLeft,
            LeftFaceMiddleMiddle,
            LeftFaceMiddleRight,
            LeftFaceBottomLeft,
            LeftFaceBottomMiddle,
            LeftFaceBottomRight,
            FrontFaceTopLeft,
            FrontFaceTopMiddle,
            FrontFaceTopRight,
            FrontFaceMiddleLeft,
            FrontFaceMiddleMiddle,
            FrontFaceMiddleRight,
            FrontFaceBottomLeft,
            FrontFaceBottomMiddle,
            FrontFaceBottomRight,
            RightFaceTopLeft,
            RightFaceTopMiddle,
            RightFaceTopRight,
            RightFaceMiddleLeft,
            RightFaceMiddleMiddle,
            RightFaceMiddleRight,
            RightFaceBottomLeft,
            RightFaceBottomMiddle,
            RightFaceBottomRight,
            BackFaceTopLeft,
            BackFaceTopMiddle,
            BackFaceTopRight,
            BackFaceMiddleLeft,
            BackFaceMiddleMiddle,
            BackFaceMiddleRight,
            BackFaceBottomLeft,
            BackFaceBottomMiddle,
            BackFaceBottomRight,
            DownFaceTopLeft,
            DownFaceTopMiddle,
            DownFaceTopRight,
            DownFaceMiddleLeft,
            DownFaceMiddleMiddle,
            DownFaceMiddleRight,
            DownFaceBottomLeft,
            DownFaceBottomMiddle,
            DownFaceBottomRight
        }

        /// <summary>
        /// Index of Cubie Geometery meshes
        ///
        /// WARNNG
        /// Must match order in BuildCube
        /// and CreateCubie
        /// </summary>
        private enum CubeGeomertyIndex
        {
            FrontUpLeft,
            FrontUpMiddle,
            FrontUpRight,
            FrontMiddleLeft,
            FrontMiddleMiddle,
            FrontMiddleRight,
            FrontDownLeft,
            FrontDownMiddle,
            FrontDownRight,
            BackUpLeft,
            BackUpMiddle,
            BackUpRight,
            BackMiddleLeft,
            BackMiddleMiddle,
            BackMiddleRight,
            BackDownLeft,
            BackDownMiddle,
            BackDownRight,
            UpMiddleLeft,
            UpMiddleMiddle,
            UpMiddleRight,
            DownMiddleLeft,
            DownMiddleMiddle,
            DownMiddleRight,
            RightMiddleMiddle,
            LeftMiddleMiddle
        };

        /// <summary>
        /// This contains the material mapping of each cubie
        /// </summary>
        private static readonly Dictionary<CubeGeomertyIndex, IEnumerable<Tuple<CubeFace, Face>>> CubeColorDict =
            new Dictionary<CubeGeomertyIndex, IEnumerable<Tuple<CubeFace, Face>>>
            {
                {
                    CubeGeomertyIndex.FrontUpLeft, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Front, Face.FrontFaceTopLeft),
                        new Tuple<CubeFace, Face>(CubeFace.Left, Face.LeftFaceTopRight),
                        new Tuple<CubeFace, Face>(CubeFace.Up, Face.UpFaceBottomLeft)
                    }
                },
                {
                    CubeGeomertyIndex.FrontUpMiddle, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Front, Face.FrontFaceTopMiddle),
                        new Tuple<CubeFace, Face>(CubeFace.Up, Face.UpFaceBottomMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.FrontUpRight, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Front, Face.FrontFaceTopRight),
                        new Tuple<CubeFace, Face>(CubeFace.Right, Face.RightFaceTopLeft),
                        new Tuple<CubeFace, Face>(CubeFace.Up, Face.UpFaceBottomRight)
                    }
                },
                {
                    CubeGeomertyIndex.FrontMiddleLeft, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Front, Face.FrontFaceMiddleLeft),
                        new Tuple<CubeFace, Face>(CubeFace.Left, Face.LeftFaceMiddleRight)
                    }
                },
                {
                    CubeGeomertyIndex.FrontMiddleMiddle, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Front, Face.FrontFaceMiddleMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.FrontMiddleRight, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Front, Face.FrontFaceMiddleRight),
                        new Tuple<CubeFace, Face>(CubeFace.Right, Face.RightFaceMiddleLeft)
                    }
                },
                {
                    CubeGeomertyIndex.FrontDownLeft, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Front, Face.FrontFaceBottomLeft),
                        new Tuple<CubeFace, Face>(CubeFace.Left, Face.LeftFaceBottomRight),
                        new Tuple<CubeFace, Face>(CubeFace.Down, Face.DownFaceTopLeft)
                    }
                },
                {
                    CubeGeomertyIndex.FrontDownMiddle, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Front, Face.FrontFaceBottomMiddle),
                        new Tuple<CubeFace, Face>(CubeFace.Down, Face.DownFaceTopMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.FrontDownRight, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Front, Face.FrontFaceBottomRight),
                        new Tuple<CubeFace, Face>(CubeFace.Right, Face.RightFaceBottomLeft),
                        new Tuple<CubeFace, Face>(CubeFace.Down, Face.DownFaceTopRight)
                    }
                },
                {
                    CubeGeomertyIndex.BackUpLeft, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Back, Face.BackFaceTopLeft),
                        new Tuple<CubeFace, Face>(CubeFace.Right, Face.RightFaceTopRight),
                        new Tuple<CubeFace, Face>(CubeFace.Up, Face.UpFaceTopRight)
                    }
                },
                {
                    CubeGeomertyIndex.BackUpMiddle, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Back, Face.BackFaceTopMiddle),
                        new Tuple<CubeFace, Face>(CubeFace.Up, Face.UpFaceTopMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.BackUpRight, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Back, Face.BackFaceTopRight),
                        new Tuple<CubeFace, Face>(CubeFace.Left, Face.LeftFaceTopLeft),
                        new Tuple<CubeFace, Face>(CubeFace.Up, Face.UpFaceTopLeft)
                    }
                },
                {
                    CubeGeomertyIndex.BackMiddleLeft, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Back, Face.BackFaceMiddleLeft),
                        new Tuple<CubeFace, Face>(CubeFace.Right, Face.RightFaceMiddleRight)
                    }
                },
                {
                    CubeGeomertyIndex.BackMiddleMiddle, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Back, Face.BackFaceMiddleMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.BackMiddleRight, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Back, Face.BackFaceMiddleRight),
                        new Tuple<CubeFace, Face>(CubeFace.Left, Face.LeftFaceMiddleLeft)
                    }
                },
                {
                    CubeGeomertyIndex.BackDownLeft, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Back, Face.BackFaceBottomLeft),
                        new Tuple<CubeFace, Face>(CubeFace.Right, Face.RightFaceBottomRight),
                        new Tuple<CubeFace, Face>(CubeFace.Down, Face.DownFaceBottomRight)
                    }
                },
                {
                    CubeGeomertyIndex.BackDownMiddle, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Back, Face.BackFaceBottomMiddle),
                        new Tuple<CubeFace, Face>(CubeFace.Down, Face.DownFaceBottomMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.BackDownRight, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Back, Face.BackFaceBottomRight),
                        new Tuple<CubeFace, Face>(CubeFace.Left, Face.LeftFaceBottomLeft),
                        new Tuple<CubeFace, Face>(CubeFace.Down, Face.DownFaceBottomLeft)
                    }
                },
                {
                    CubeGeomertyIndex.UpMiddleLeft, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Up, Face.UpFaceMiddleLeft),
                        new Tuple<CubeFace, Face>(CubeFace.Left, Face.LeftFaceTopMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.UpMiddleMiddle, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Up, Face.UpFaceMiddleMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.UpMiddleRight, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Up, Face.UpFaceMiddleRight),
                        new Tuple<CubeFace, Face>(CubeFace.Right, Face.RightFaceTopMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.DownMiddleLeft, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Down, Face.DownFaceMiddleLeft),
                        new Tuple<CubeFace, Face>(CubeFace.Left, Face.LeftFaceBottomMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.DownMiddleMiddle, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Down, Face.DownFaceMiddleMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.DownMiddleRight, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Down, Face.DownFaceMiddleRight),
                        new Tuple<CubeFace, Face>(CubeFace.Right, Face.RightFaceBottomMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.RightMiddleMiddle, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Right, Face.RightFaceMiddleMiddle)
                    }
                },
                {
                    CubeGeomertyIndex.LeftMiddleMiddle, new List<Tuple<CubeFace, Face>>
                    {
                        new Tuple<CubeFace, Face>(CubeFace.Left, Face.LeftFaceMiddleMiddle)
                    }
                }
            };

        public CubeUserControl()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Update the colors of the Cube
        /// </summary>
        private void UpdateCubeColors()
        {
            CubeColors = Rc.ToString();

            var colors = CubeColors.ToUpper().Split(new[] {"\t", "\n", "\r", "\v", "\f", " "},
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var cubieindex in (CubeGeomertyIndex[])Enum.GetValues(typeof(CubeGeomertyIndex)))
            {
                IEnumerable<Tuple<CubeFace, Face>> colorList;
                if (!CubeColorDict.TryGetValue(cubieindex, out colorList)) continue;

                var cubie = GeometryLists[(int)cubieindex];
                foreach (var tuple in colorList)
                {
                    Color color;
                    if (_brushDict.TryGetValue(colors[(int)tuple.Item2], out color))
                        cubie[(int)tuple.Item1].Material = new DiffuseMaterial(new SolidColorBrush(color));
                }
            }
        }

        public void Pause(bool pause)
        {
            lock (CubeUserControl.QueueLock)
            {
                MoveQueue.EnqueTop(pause ? "pause" : "resume");
            }
        }

        /// <summary>
        /// Execute a squence
        /// add the moves to the MoveQue
        /// </summary>
        /// <param name="sequence"></param>
        public void ExecuteSequence(string sequence)
        {
            Sequence = sequence;
            var strs = sequence.Split(new[] {"\t", "\n", "\r", "\v", "\f", " "}, StringSplitOptions.RemoveEmptyEntries);

            lock (QueueLock)
            {
                MoveQueue.Enqueue("SequenceStart");
                foreach (var str in strs)
                {
                    MoveQueue.Enqueue(str);
                }
                MoveQueue.Enqueue("SequenceEnd");
            }
            MoveEvent.Set();
        }

        /// <summary>
        /// Scramble Cube
        /// </summary>
        public void Scramble()
        {
            if (MoveQueue.Count != 0) return;

            Dispatcher?.BeginInvoke(new Action(() =>
            {
                    Scrambling = true;
                    var tempCube = Rc.clone();

                    tempCube.scramble_cube();
                    var scrambleBuffer = tempCube.ScrambleSequence;
                    ExecuteSequence(scrambleBuffer);
            }));
        }

        /// <summary>
        /// Get the colors of the cube
        /// </summary>
        /// <returns>colors of the cube</returns>
        public string GetCube()
        {
            return Rc.ToString();
        }

        /// <summary>
        /// Solve the Cube
        /// </summary>
        public void Solve()
        {
            // IMPORTANT
            // make a copy of the cube before solving it
            //
            var tempCube = Rc.clone();
            ISolver solver = new SimpleSolver(tempCube);
            try
            {
                var buffer = solver.solve();
                ExecuteSequence(buffer);
            }
            catch (Exception e)
            {
                Dispatcher?.BeginInvoke(new Action(() =>
                {
                    UpdateCubeColors();
                    MessageBox.Show(e.Message);
                    ExecuteSequence("");
                }));
            }
        }

        /// <summary>
        /// Enums for CubeGemoeteryIndices
        /// This is used to animate the Cube
        /// 
        /// WARNNG
        /// Must match order in BuildCube
        /// and CreateCubie
        /// </summary>
        private static IEnumerable<int> AllCubeieIndices { get; } =
            (int[]) Enum.GetValues(typeof(CubeGeomertyIndex));

        private static IEnumerable<int> FrontCubieIndices { get; } = new[]
        {
            (int) CubeGeomertyIndex.FrontUpLeft,
            (int) CubeGeomertyIndex.FrontUpMiddle,
            (int) CubeGeomertyIndex.FrontUpRight,
            (int) CubeGeomertyIndex.FrontMiddleLeft,
            (int) CubeGeomertyIndex.FrontMiddleMiddle,
            (int) CubeGeomertyIndex.FrontMiddleRight,
            (int) CubeGeomertyIndex.FrontDownLeft,
            (int) CubeGeomertyIndex.FrontDownMiddle,
            (int) CubeGeomertyIndex.FrontDownRight
        };

        private static IEnumerable<int> BackCubieIndices { get; } = new[]
        {
            (int) CubeGeomertyIndex.BackUpLeft,
            (int) CubeGeomertyIndex.BackUpMiddle,
            (int) CubeGeomertyIndex.BackUpRight,
            (int) CubeGeomertyIndex.BackMiddleLeft,
            (int) CubeGeomertyIndex.BackMiddleMiddle,
            (int) CubeGeomertyIndex.BackMiddleRight,
            (int) CubeGeomertyIndex.BackDownLeft,
            (int) CubeGeomertyIndex.BackDownMiddle,
            (int) CubeGeomertyIndex.BackDownRight
        };

        private static IEnumerable<int> LeftCubieIndices { get; } = new[]
        {
            (int) CubeGeomertyIndex.FrontUpLeft,
            (int) CubeGeomertyIndex.FrontMiddleLeft,
            (int) CubeGeomertyIndex.FrontDownLeft,
            (int) CubeGeomertyIndex.BackUpRight,
            (int) CubeGeomertyIndex.BackMiddleRight,
            (int) CubeGeomertyIndex.BackDownRight,
            (int) CubeGeomertyIndex.UpMiddleLeft,
            (int) CubeGeomertyIndex.DownMiddleLeft,
            (int) CubeGeomertyIndex.LeftMiddleMiddle
        };

        private static IEnumerable<int> RightCubieIndices { get; } = new[]
        {
            (int) CubeGeomertyIndex.FrontUpRight,
            (int) CubeGeomertyIndex.FrontMiddleRight,
            (int) CubeGeomertyIndex.FrontDownRight,
            (int) CubeGeomertyIndex.BackUpLeft,
            (int) CubeGeomertyIndex.BackMiddleLeft,
            (int) CubeGeomertyIndex.BackDownLeft,
            (int) CubeGeomertyIndex.UpMiddleRight,
            (int) CubeGeomertyIndex.DownMiddleRight,
            (int) CubeGeomertyIndex.RightMiddleMiddle
        };

        private static IEnumerable<int> UpCubieIndices { get; } = new[]
        {
            (int) CubeGeomertyIndex.FrontUpLeft,
            (int) CubeGeomertyIndex.FrontUpMiddle,
            (int) CubeGeomertyIndex.FrontUpRight,
            (int) CubeGeomertyIndex.BackUpLeft,
            (int) CubeGeomertyIndex.BackUpMiddle,
            (int) CubeGeomertyIndex.BackUpRight,
            (int) CubeGeomertyIndex.UpMiddleLeft,
            (int) CubeGeomertyIndex.UpMiddleMiddle,
            (int) CubeGeomertyIndex.UpMiddleRight
        };

        private static IEnumerable<int> DownCubieIndices { get; } = new[]
        {
            (int) CubeGeomertyIndex.FrontDownLeft,
            (int) CubeGeomertyIndex.FrontDownMiddle,
            (int) CubeGeomertyIndex.FrontDownRight,
            (int) CubeGeomertyIndex.BackDownLeft,
            (int) CubeGeomertyIndex.BackDownMiddle,
            (int) CubeGeomertyIndex.BackDownRight,
            (int) CubeGeomertyIndex.DownMiddleLeft,
            (int) CubeGeomertyIndex.DownMiddleMiddle,
            (int) CubeGeomertyIndex.DownMiddleRight
        };

        private static IEnumerable<int> UpDownSliceCubieIndices { get; } = new[]
        {
            (int) CubeGeomertyIndex.FrontMiddleLeft,
            (int) CubeGeomertyIndex.FrontMiddleMiddle,
            (int) CubeGeomertyIndex.FrontMiddleRight,
            (int) CubeGeomertyIndex.BackMiddleLeft,
            (int) CubeGeomertyIndex.BackMiddleMiddle,
            (int) CubeGeomertyIndex.BackMiddleRight,
            (int) CubeGeomertyIndex.LeftMiddleMiddle,
            (int) CubeGeomertyIndex.RightMiddleMiddle
        };

        private static IEnumerable<int> LeftRightSliceCubieIndices { get; } = new[]
        {
            (int) CubeGeomertyIndex.FrontUpMiddle,
            (int) CubeGeomertyIndex.FrontMiddleMiddle,
            (int) CubeGeomertyIndex.FrontDownMiddle,
            (int) CubeGeomertyIndex.BackUpMiddle,
            (int) CubeGeomertyIndex.BackMiddleMiddle,
            (int) CubeGeomertyIndex.BackDownMiddle,
            (int) CubeGeomertyIndex.UpMiddleMiddle,
            (int) CubeGeomertyIndex.DownMiddleMiddle
        };

        private static IEnumerable<int> FrontBackSliceCubieIndices { get; } = new[]
        {
            (int) CubeGeomertyIndex.UpMiddleLeft,
            (int) CubeGeomertyIndex.UpMiddleMiddle,
            (int) CubeGeomertyIndex.UpMiddleRight,
            (int) CubeGeomertyIndex.DownMiddleLeft,
            (int) CubeGeomertyIndex.DownMiddleMiddle,
            (int) CubeGeomertyIndex.DownMiddleRight,
            (int) CubeGeomertyIndex.LeftMiddleMiddle,
            (int) CubeGeomertyIndex.RightMiddleMiddle
        };

        /// <summary>
        /// Build the Cube
        /// </summary>
        /// <param name="startposition">Position to place cube</param>
        /// <param name="size">size of 1 cubie</param>
        /// <param name="edgesize">edgesize of 1 cubie</param>
        /// <returns>List of Gemometry list of the Cube that is constructed</returns>
        private static List<List<GeometryModel3D>> BuildCube(Point3D startposition, double size, double edgesize)
        {
            var cube = new List<List<GeometryModel3D>>();

            // FrontUpLeft
            var pos = startposition;
            cube.Insert((int)CubeGeomertyIndex.FrontUpLeft, CreateCubie(size, pos, edgesize));

            // FrontUpMiddle
            pos.X += size;
            cube.Insert((int)CubeGeomertyIndex.FrontUpMiddle, CreateCubie(size, pos, edgesize));

            // FrontUpRight
            pos.X += size;
            cube.Insert((int)CubeGeomertyIndex.FrontUpRight, CreateCubie(size, pos, edgesize));

            // FrontMiddleLeft
            pos.X = startposition.X;
            pos.Y -= size;
            cube.Insert((int)CubeGeomertyIndex.FrontMiddleLeft, CreateCubie(size, pos, edgesize));

            // FrontMiddleMiddle
            pos.X += size;
            cube.Insert((int)CubeGeomertyIndex.FrontMiddleMiddle, CreateCubie(size, pos, edgesize));

            // FrontMiddleRight
            pos.X += size;
            cube.Insert((int)CubeGeomertyIndex.FrontMiddleRight, CreateCubie(size, pos, edgesize));

            // FrontDownLeft
            pos.X = startposition.X;
            pos.Y -= size;
            cube.Insert((int)CubeGeomertyIndex.FrontDownLeft, CreateCubie(size, pos, edgesize));

            // FrontDownMiddle
            pos.X += size;
            cube.Insert((int)CubeGeomertyIndex.FrontDownMiddle, CreateCubie(size, pos, edgesize));

            // FrontDownRight
            pos.X += size;
            cube.Insert((int)CubeGeomertyIndex.FrontDownRight, CreateCubie(size, pos, edgesize));

            // BackUpLeft
            pos = startposition;
            pos.Z -= (size * 2);
            pos.X += (size * 2);
            cube.Insert((int)CubeGeomertyIndex.BackUpLeft, CreateCubie(size, pos, edgesize));

            // BackUpMiddle
            pos.X -= size;
            cube.Insert((int)CubeGeomertyIndex.BackUpMiddle, CreateCubie(size, pos, edgesize));

            // BackUpRight
            pos.X -= size;
            cube.Insert((int)CubeGeomertyIndex.BackUpRight, CreateCubie(size, pos, edgesize));

            // BackMiddleLeft
            pos.X += (size * 2);
            pos.Y -= size;
            cube.Insert((int)CubeGeomertyIndex.BackMiddleLeft, CreateCubie(size, pos, edgesize));

            // BackMiddleMiddle
            pos.X -= size;
            cube.Insert((int)CubeGeomertyIndex.BackMiddleMiddle, CreateCubie(size, pos, edgesize));

            // BackMiddleRight
            pos.X -= size;
            cube.Insert((int)CubeGeomertyIndex.BackMiddleRight, CreateCubie(size, pos, edgesize));

            // BackDownLeft
            pos.X += (size * 2);
            pos.Y -= size;
            cube.Insert((int)CubeGeomertyIndex.BackDownLeft, CreateCubie(size, pos, edgesize));

            // BackDownMiddle
            pos.X -= size;
            cube.Insert((int)CubeGeomertyIndex.BackDownMiddle, CreateCubie(size, pos, edgesize));

            // BackDownRight
            pos.X -= size;
            cube.Insert((int)CubeGeomertyIndex.BackDownRight, CreateCubie(size, pos, edgesize));

            // UpMiddleLeft
            pos.Y += (size * 2);
            pos.Z += size;
            cube.Insert((int)CubeGeomertyIndex.UpMiddleLeft, CreateCubie(size, pos, edgesize));

            // UpMiddleMiddle
            pos.X += size;
            cube.Insert((int)CubeGeomertyIndex.UpMiddleMiddle, CreateCubie(size, pos, edgesize));

            // UpMiddleRight
            pos.X += size;
            cube.Insert((int)CubeGeomertyIndex.UpMiddleRight, CreateCubie(size, pos, edgesize));

            // DownMiddleLeft
            pos.Y -= (size * 2);
            pos.X -= (size * 2);
            cube.Insert((int)CubeGeomertyIndex.DownMiddleLeft, CreateCubie(size, pos, edgesize));

            // DownMiddleMiddle
            pos.X += size;
            cube.Insert((int)CubeGeomertyIndex.DownMiddleMiddle, CreateCubie(size, pos, edgesize));

            // DownMiddleRight
            pos.X += size;
            cube.Insert((int)CubeGeomertyIndex.DownMiddleRight, CreateCubie(size, pos, edgesize));

            // RightMiddleMiddle
            pos.Y += size;
            cube.Insert((int)CubeGeomertyIndex.RightMiddleMiddle, CreateCubie(size, pos, edgesize));

            // LeftMiddleMiddle
            pos.X -= (size * 2);
            cube.Insert((int)CubeGeomertyIndex.LeftMiddleMiddle, CreateCubie(size, pos, edgesize));

            return cube;
        }

        /// <summary>
        /// Create cubie geometery
        /// </summary>
        /// <param name="size">Size of the cubie.</param>
        /// <param name="offset">Offset of cubie.</param>
        /// <param name="edgesize">Size of frame for cubie.</param>
        private static List<GeometryModel3D> CreateCubie(double size, Point3D offset, double edgesize)
        {
            var x0 = 0.0;
            var x1 = edgesize;
            var x2 = size - edgesize;
            var x3 = size;

            var y0 = size;
            var y1 = size - edgesize;
            var y2 = edgesize;
            var y3 = 0.0;

            var z0 = 0.0;
            var z1 = -edgesize;
            var z2 = -(size - edgesize);
            var z3 = -size;

            x0 += offset.X;
            x1 += offset.X;
            x2 += offset.X;
            x3 += offset.X;

            y0 += offset.Y;
            y1 += offset.Y;
            y2 += offset.Y;
            y3 += offset.Y;

            z0 += offset.Z;
            z1 += offset.Z;
            z2 += offset.Z;
            z3 += offset.Z;

            var frontIndices = new Int32Collection
            {
                0, 12, 13,
                0, 13, 1,
                1, 5, 7,
                1, 7, 3,
                6, 10, 11,
                6, 11, 7,
                9, 13, 15,
                9, 15, 11
            };

            var backIndices = new Int32Collection
            {
                1, 13, 12,
                1, 12, 0,
                3, 7, 5,
                3, 5, 1,
                7, 15, 14,
                7, 14, 6,
                10, 14, 13,
                10, 13, 9
            };

            var cubie = new List<GeometryModel3D>
            {
                new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection
                        {
                            // top row front face left to right
                            new Point3D(x0, y0, z0),    // 0
                            new Point3D(x1, y0, z0),    // 1
                            new Point3D(x2, y0, z0),    // 2
                            new Point3D(x3, y0, z0),    // 3

                            // second row front face left to right
                            new Point3D(x0, y1, z0),    // 4
                            new Point3D(x1, y1, z0),    // 5
                            new Point3D(x2, y1, z0),    // 6
                            new Point3D(x3, y1, z0),    // 7

                            // third row front face left to right
                            new Point3D(x0, y2, z0),    // 8
                            new Point3D(x1, y2, z0),    // 9
                            new Point3D(x2, y2, z0),    // 10
                            new Point3D(x3, y2, z0),    // 11

                            // bottom row front face left to right
                            new Point3D(x0, y3, z0),    // 12
                            new Point3D(x1, y3, z0),    // 13
                            new Point3D(x2, y3, z0),    // 14
                            new Point3D(x3, y3, z0)     // 15
                        },
                        TriangleIndices = frontIndices
                    },
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black))
                },
                new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection
                        {
                            new Point3D(x1, y1, z0),    // 5
                            new Point3D(x2, y1, z0),    // 6
                            new Point3D(x1, y2, z0),    // 9
                            new Point3D(x2, y2, z0)     // 10
                        },
                        TriangleIndices = new Int32Collection
                        {
                            0, 2, 3,
                            0, 3, 1
                        }
                    },
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black))
                },
                new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection
                        {
                            // top row back face left to right
                            new Point3D(x0, y0, z3),    // 0
                            new Point3D(x1, y0, z3),    // 1
                            new Point3D(x2, y0, z3),    // 2
                            new Point3D(x3, y0, z3),    // 3

                            // second row back face left to right
                            new Point3D(x0, y1, z3),    // 4
                            new Point3D(x1, y1, z3),    // 5
                            new Point3D(x2, y1, z3),    // 6
                            new Point3D(x3, y1, z3),    // 7

                            // third row back face left to right
                            new Point3D(x0, y2, z3),    // 8
                            new Point3D(x1, y2, z3),    // 9
                            new Point3D(x2, y2, z3),    // 10
                            new Point3D(x3, y2, z3),    // 11

                            // bottom row back face left to right
                            new Point3D(x0, y3, z3),    // 12
                            new Point3D(x1, y3, z3),    // 13
                            new Point3D(x2, y3, z3),    // 14
                            new Point3D(x3, y3, z3)     // 15
                        },
                        TriangleIndices = backIndices
                    },
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black))
                },
                new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection
                        {
                            new Point3D(x1, y1, z3),    // 5
                            new Point3D(x2, y1, z3),    // 6
                            new Point3D(x1, y2, z3),    // 9
                            new Point3D(x2, y2, z3)     // 10
                        },
                        TriangleIndices = new Int32Collection
                        {
                            1, 3, 2,
                            1, 2, 0
                        }
                    },
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black))
                },
                new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection
                        {
                            // top row right face left to right
                            new Point3D(x3, y0, z0),    // 0
                            new Point3D(x3, y0, z1),    // 1
                            new Point3D(x3, y0, z2),    // 2
                            new Point3D(x3, y0, z3),    // 3

                            // second row right face left to right
                            new Point3D(x3, y1, z0),    // 4
                            new Point3D(x3, y1, z1),    // 5
                            new Point3D(x3, y1, z2),    // 6
                            new Point3D(x3, y1, z3),    // 7

                            // third row right face left to right
                            new Point3D(x3, y2, z0),    // 8
                            new Point3D(x3, y2, z1),    // 9
                            new Point3D(x3, y2, z2),    // 10
                            new Point3D(x3, y2, z3),    // 11

                            // bottom row right face left to right
                            new Point3D(x3, y3, z0),    // 12
                            new Point3D(x3, y3, z1),    // 13
                            new Point3D(x3, y3, z2),    // 14
                            new Point3D(x3, y3, z3)     // 15
                        },
                        TriangleIndices = frontIndices
                    },
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black))
                },
                new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection
                        {
                            new Point3D(x3, y1, z1),    // 5
                            new Point3D(x3, y1, z2),    // 6
                            new Point3D(x3, y2, z1),    // 9
                            new Point3D(x3, y2, z2)     // 10
                        },
                        TriangleIndices = new Int32Collection
                        {
                            0, 2, 3,
                            0, 3, 1
                        }
                    },
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black))
                },
                new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection
                        {
                            // top row left face left to right
                            new Point3D(x0, y0, z0),    // 0
                            new Point3D(x0, y0, z1),    // 1
                            new Point3D(x0, y0, z2),    // 2
                            new Point3D(x0, y0, z3),    // 3

                            // second row left face left to right
                            new Point3D(x0, y1, z0),    // 4
                            new Point3D(x0, y1, z1),    // 5
                            new Point3D(x0, y1, z2),    // 6
                            new Point3D(x0, y1, z3),    // 7

                            // third row left face left to right
                            new Point3D(x0, y2, z0),    // 8
                            new Point3D(x0, y2, z1),    // 9
                            new Point3D(x0, y2, z2),    // 10
                            new Point3D(x0, y2, z3),    // 11

                            // bottom row left face left to right
                            new Point3D(x0, y3, z0),    // 12
                            new Point3D(x0, y3, z1),    // 13
                            new Point3D(x0, y3, z2),    // 14
                            new Point3D(x0, y3, z3)     // 15
                        },
                        TriangleIndices = backIndices
                    },
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black))
                },
                new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection
                        {
                            new Point3D(x0, y1, z1),    // 5
                            new Point3D(x0, y1, z2),    // 6
                            new Point3D(x0, y2, z1),    // 9
                            new Point3D(x0, y2, z2)     // 10
                        },
                        TriangleIndices = new Int32Collection
                        {
                            1, 3, 2,
                            1, 2, 0
                        }
                    },
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black))
                },
                new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection
                        {
                            // top row top face left to right
                            new Point3D(x0, y0, z3),    // 0
                            new Point3D(x1, y0, z3),    // 1
                            new Point3D(x2, y0, z3),    // 2
                            new Point3D(x3, y0, z3),    // 3

                            // second row front face left to right
                            new Point3D(x0, y0, z2),    // 4
                            new Point3D(x1, y0, z2),    // 5
                            new Point3D(x2, y0, z2),    // 6
                            new Point3D(x3, y0, z2),    // 7

                            // third row front face left to right
                            new Point3D(x0, y0, z1),    // 8
                            new Point3D(x1, y0, z1),    // 9
                            new Point3D(x2, y0, z1),    // 10
                            new Point3D(x3, y0, z1),    // 11

                            // bottom row front face left to right
                            new Point3D(x0, y0, z0),    // 12
                            new Point3D(x1, y0, z0),    // 13
                            new Point3D(x2, y0, z0),    // 14
                            new Point3D(x3, y0, z0)     // 15
                        },
                        TriangleIndices = frontIndices
                    },
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black))
                },
                new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection
                        {
                            new Point3D(x1, y0, z2),    // 5
                            new Point3D(x2, y0, z2),    // 6
                            new Point3D(x1, y0, z1),    // 9
                            new Point3D(x2, y0, z1)     // 10
                        },
                        TriangleIndices = new Int32Collection
                        {
                            0, 2, 3,
                            0, 3, 1
                        }
                    },
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black))
                },
                new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection
                        {
                            // top row bottom face left to right
                            new Point3D(x0, y3, z3),    // 0
                            new Point3D(x1, y3, z3),    // 1
                            new Point3D(x2, y3, z3),    // 2
                            new Point3D(x3, y3, z3),    // 3

                            // second row bottom face left to right
                            new Point3D(x0, y3, z2),    // 4
                            new Point3D(x1, y3, z2),    // 5
                            new Point3D(x2, y3, z2),    // 6
                            new Point3D(x3, y3, z2),    // 7

                            // third row bottom face left to right
                            new Point3D(x0, y3, z1),    // 8
                            new Point3D(x1, y3, z1),    // 9
                            new Point3D(x2, y3, z1),    // 10
                            new Point3D(x3, y3, z1),    // 11

                            // bottom row bottom face left to right
                            new Point3D(x0, y3, z0),    // 12
                            new Point3D(x1, y3, z0),    // 13
                            new Point3D(x2, y3, z0),    // 14
                            new Point3D(x3, y3, z0)     // 15
                        },
                        TriangleIndices = backIndices
                    },
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black))
                },
                new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection
                        {
                            new Point3D(x1, y3, z2),    // 5
                            new Point3D(x2, y3, z2),    // 6
                            new Point3D(x1, y3, z1),    // 9
                            new Point3D(x2, y3, z1)     // 10
                        },
                        TriangleIndices = new Int32Collection
                        {
                            1, 3, 2,
                            1, 2, 0
                        }
                    },
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black))
                }
            };
            return cubie;
        }

        private int _sequenceIndex;

        public static object QueueLock = new object();
        /// <summary>
        /// Process the move queue
        /// </summary>
        public void ProcessMoves()
        {
            do
            {
                lock (QueueLock)
                {
                    if (MoveQueue.Count > 0)
                    {
                        var peekmove = MoveQueue.Peek().ToLower();
                        if (peekmove == "pause" || peekmove == "resume")
                        {
                            MoveQueue.Dequeue();
                            Paused = peekmove == "pause";
                            OnPause(Paused);
                            MoveEvent.Set();
                            continue;
                        }
 
                        if (Paused)
                        {
                            Thread.Sleep(1);
                            continue;
                        }

                        MoveEvent.WaitOne(-1);
                        var element = MoveQueue.Dequeue();

                        // adjust speed
                        if (Scrambling)
                        {
                            AnimationDuration = TimeSpan.FromMilliseconds(1);
                        }
                        else if (AnimationDuration.TotalMilliseconds > FastSpeed && MoveQueue.Count > 5)
                        {
                            AnimationDuration = TimeSpan.FromMilliseconds(FastSpeed);
                        }
                        else if (AnimationDuration.TotalMilliseconds < NormalSpeed && MoveQueue.Count < 3)
                        {
                            AnimationDuration = TimeSpan.FromMilliseconds(NormalSpeed);
                        }

                        // check moves and to trigger events
                        if (string.Compare(element, "sequencestart", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            OnSequenceStart(Sequence);
                            MoveEvent.Set();
                            _sequenceIndex = 0;
                        }
                        else if (string.Compare(element, "sequenceend", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            OnSequenceEnd(Sequence);
                            Scrambling = false;
                            AnimationDuration = TimeSpan.FromMilliseconds(NormalSpeed);
                            MoveEvent.Set();
                        }
                        else
                        {
                            OnProcessingMove(element, _sequenceIndex++);
                        }

                        switch (element.ToLower())
                        {
                            case "cu":
                                Rotate(1, 0, 0, AllCubeieIndices, -90,  Rc.cu);
                                break;

                            case "cd":
                                Rotate(1, 0, 0, AllCubeieIndices, 90, Rc.cd);
                                break;

                            case "cr":
                                Rotate(0, 1, 0, AllCubeieIndices, 90, Rc.cr);
                                break;

                            case "cl":
                                Rotate(0, 1, 0, AllCubeieIndices, -90, Rc.cl);
                                break;

                            case "f":
                                Rotate(0, 0, 1, FrontCubieIndices, -90, Rc.f);
                                break;

                            case "fi":
                                Rotate(0, 0, 1, FrontCubieIndices, 90, Rc.fi);
                                break;

                            case "b":
                                Rotate(0, 0, 1, BackCubieIndices, 90, Rc.b);
                                break;

                            case "bi":
                                Rotate(0, 0, 1, BackCubieIndices, -90, Rc.bi);
                                break;

                            case "l":
                                Rotate(1, 0, 0, LeftCubieIndices, 90, Rc.l);
                                break;

                            case "li":
                                Rotate(1, 0, 0, LeftCubieIndices, -90, Rc.li);
                                break;

                            case "u":
                                Rotate(0, 1, 0, UpCubieIndices, -90, Rc.u);
                                break;

                            case "ui":
                                Rotate(0, 1, 0, UpCubieIndices, 90, Rc.ui);
                                break;

                            case "r":
                                Rotate(1, 0, 0, RightCubieIndices, -90, Rc.r);
                                break;

                            case "ri":
                                Rotate(1, 0, 0, RightCubieIndices, 90, Rc.ri);
                                break;

                            case "d":
                                Rotate(0, 1, 0, DownCubieIndices, 90, Rc.d);
                                break;

                            case "di":
                                Rotate(0, 1, 0, DownCubieIndices, -90, Rc.di);
                                break;

                            case "us":
                                Rotate(0, 1, 0, UpDownSliceCubieIndices, -90, Rc.us);
                                break;

                            case "ds":
                                Rotate(0, 1, 0, UpDownSliceCubieIndices, 90, Rc.ds);
                                break;

                            case "ls":
                                Rotate(1, 0, 0, LeftRightSliceCubieIndices, 90, Rc.ls);
                                break;

                            case "rs":
                                Rotate(1, 0, 0, LeftRightSliceCubieIndices, -90, Rc.rs);
                                break;

                            case "fs":
                                Rotate(0, 0, 1, FrontBackSliceCubieIndices, -90, Rc.fs);
                                break;

                            case "bs":
                                Rotate(0, 0, 1, FrontBackSliceCubieIndices, 90, Rc.bs);
                                break;

                            default:
                                // this is needed so we can process the next move in the sequene
                                MoveEvent.Set();
                                break;
                        }
                    }
                }

                // Sleep for a short time
                Thread.Sleep( Scrambling ? 1 : 10);
            } while (true);

            // ReSharper disable once FunctionNeverReturns
        }

        /// <summary>
        /// Rotate cubies
        /// </summary>
        /// <param name="x">x rotation</param>
        /// <param name="y">y rotation</param>
        /// <param name="z">z rotation</param>
        /// <param name="cubieIndexList">list of cubie indices  to rotate</param>
        /// <param name="by">rotation in degrees. negative is counter clockwise</param>
        /// <param name="rot">rotation function</param>
        private void Rotate(double x, double y, double z, IEnumerable<int> cubieIndexList, double by, Rotator rot)
        {
            // dont allow another move to process
            // until we are done with the animation
            MoveEvent.Reset();

            Dispatcher?.BeginInvoke(new Action(() =>
            {
                var axis = new AxisAngleRotation3D(new Vector3D(x, y, z), 0);
                var rotate = new RotateTransform3D(axis, Center);

                // add the rotation to the cubies
                foreach (var cubieIndex in cubieIndexList.AsParallel())
                {
                    foreach (var face in GeometryLists[cubieIndex].AsParallel())
                    {
                        face.Transform = rotate;
                    }
                }

                // create the animation
                var rotationAngleAnimation =
                    new DoubleAnimation
                    {
                        By = by,
                        Duration = AnimationDuration
                    };

                NameScope.SetNameScope(CubeViewport3D, new NameScope());
                CubeViewport3D.RegisterName(nameof(axis), axis);
                Storyboard.SetTargetName(rotationAngleAnimation, nameof(axis));
                Storyboard.SetTargetProperty(rotationAngleAnimation, new PropertyPath(AxisAngleRotation3D.AngleProperty));
                var rotCube = new Storyboard();
                rotCube.Completed += (sender, args) =>
                {
                    // when done remove the transformation from the cubies
                    foreach (var face in GeometryLists.SelectMany(faces => faces).AsParallel())
                        face.Transform = null;

                    // call the  supplied function for the internal rotation
                    rot();
                    // draw the new colors
                    UpdateCubeColors();
                    // allow next move to process
                    MoveEvent.Set();
                };
                // add rotation to storyboard
                rotCube.Children.Add(rotationAngleAnimation);
                // begin the animation
                rotCube.Begin(CubeViewport3D);
            }));
        }

        /// <summary>
        /// Stop the processMoves thread
        /// </summary>
        public void StopProcessing()
        {
            ProcessMovesThread.Abort();
        }

        /// <summary>
        /// Set the cube colors
        /// </summary>
        /// <param name="buffer"></param>
        public void SetCube(string buffer)
        {
            Rc.set_cube(buffer);
            Dispatcher?.BeginInvoke(new Action(UpdateCubeColors));
        }

        /// <summary>
        /// Initialize the Cube
        /// </summary>
        private void InitializeCube()
        {
            ProcessMovesThread = new Thread(ProcessMoves);

            // build the cube geometery
            var postart = new Point3D(-1.5, .5, 1.5);
            GeometryLists = BuildCube(postart, 1, .06);
            // set cube colors
            UpdateCubeColors();

            // Create model group
            var modelGroup = new Model3DGroup();

            // add cubes to model
            foreach (var geometryModel3D in
                from geometrylist in GeometryLists
                from geometryModel3D in geometrylist
                select geometryModel3D)
                modelGroup.Children.Add(geometryModel3D);

            // add lighting to model
            modelGroup.Children.Add(new AmbientLight(Color.FromRgb(50, 50, 50)));
            modelGroup.Children.Add(
                new DirectionalLight {Color = Colors.White, Direction = new Vector3D(-20, -20, -20)});
            modelGroup.Children.Add(new DirectionalLight {Color = Colors.White, Direction = new Vector3D(20, 20, -20)});
            modelGroup.Children.Add(new DirectionalLight
                {Color = Colors.White, Direction = new Vector3D(-20, -20, 20)});
            modelGroup.Children.Add(new DirectionalLight {Color = Colors.White, Direction = new Vector3D(20, 20, 20)});
            modelGroup.Children.Add(new DirectionalLight {Color = Colors.White, Direction = new Vector3D(0, 20, 1.5)});
            modelGroup.Children.Add(new DirectionalLight
                {Color = Colors.White, Direction = new Vector3D(0, -20, -1.5)});

            var w = ActualWidth < 1 ? ActualHeight : ActualWidth;
            var h = ActualHeight < 1 ? ActualWidth : ActualHeight;

            CubeViewport3D.Width = w;
            CubeViewport3D.Height = h;

            var pos = CameraPosition.ToPoint3D();
            // set camera
            CubeCamera =
                new OrthographicCamera(
                    pos,
                    new Vector3D(-pos.X, -pos.Y, -pos.Z),
                    new Vector3D(0, 1, 0),
                    CameraWidth);

            CubeViewport3D = new Viewport3D
            {
                Camera = CubeCamera,
                Width = w,
                Height = h
            };

            // add the model group to the viewport
            CubeViewport3D.Children.Add(new ModelVisual3D {Content = modelGroup});

            // add the viewport to the WPF element
            Canvas1.Children.Add(CubeViewport3D);
            Canvas.SetTop(CubeViewport3D, 0);
            Canvas.SetLeft(CubeViewport3D, 0);

            UpdateCubeColors();

            if (ProcessMovesThread.IsAlive) return;

            ProcessMovesThread.IsBackground = true;
            ProcessMovesThread.Start();

            InvalidateVisual();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Microsoft.Usage", "CA2213")]
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            _disposed = true;
            if (disposing)
            {
                // free managed resources
                MoveEvent.Dispose();
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~CubeUserControl()
        {
            Dispose(false);
        }

        /// <summary>
        /// Cube control loaded
        /// we need to initialize the cube
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CubeUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeCube();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<SequenceArgs> SequenceStart;
        public event EventHandler<SequenceArgs> SequenceEnd;
        public event EventHandler<PauseArgs> PauseEvent;
        public event EventHandler<MoveProcessingArgs> MoveProcessEvent;

        protected virtual void OnPause(bool pause)
        {
            PauseEvent?.Invoke(this, new PauseArgs(pause));
        }

        protected virtual void OnSequenceStart(string sequence)
        {
            SequenceStart?.Invoke(this, new SequenceArgs(sequence));
        }

        protected virtual void OnSequenceEnd(string sequence)
        {
            SequenceEnd?.Invoke(this, new SequenceArgs(sequence));
        }

        protected virtual void OnProcessingMove(string move, int index)
        {
            MoveProcessEvent?.Invoke(this, new MoveProcessingArgs(move, index));
        }

        private void CubeUserControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CubeViewport3D.Width = e.NewSize.Width;
            CubeViewport3D.Height = e.NewSize.Height;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
