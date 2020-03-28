
// ReSharper disable UnusedMember.Global

namespace WpfApplication1
{
    public abstract class Solver
    {
        private const int CubeSize = 3;

        // names of edges and orientation
        public enum FaceName
        {
            FaceNone = 0,
            TopFaceUp,              // 1
            TopFaceUpReverse,       // 2
            TopFaceLeft,            // 3
            TopFaceLeftReverse,     // 4
            TopFaceDown,            // 5
            TopFaceDownReverse,     // 6
            TopFaceRight,           // 7
            TopFaceRightReverse,    // 8
            MiddleFaceLeft,         // 9
            MiddleFaceLeftReverse,  // 10
            MiddleFaceFront,        // 11
            MiddleFaceFrontReverse, // 12
            MiddleFaceRight,        // 13
            MiddleFaceRightReverse, // 14
            MiddleFaceBack,         // 15
            MiddleFaceBackReverse,  // 16
            BottomFaceUp,           // 17
            BottomFaceUpReverse,    // 18
            BottomFaceLeft,         // 19
            BottomFaceLeftReverse,  // 20
            BottomFaceDown,         // 21
            BottomFaceDownReverse,  // 22
            BottomFaceRight,        // 23
            BottomFaceRightReverse  // 24
        };

        public enum FaceSide
        {
            BackFace = 0,
            LeftFace,
            RightFace,
            FrontFace
        };

        // the first letters of the returned enum are the name of the cubie
        // the numbers are the orientation of the cubie
        public enum CornerName
        {
            CornerNone = 0,

            CornerUlf,
            CornerUlf123 = CornerUlf, // 1
            CornerUlf132, // 2
            CornerUlf213, // 3
            CornerUlf231, // 4
            CornerUlf312, // 5
            CornerUlf321, // 6

            CornerUlb,
            CornerUlb123 = CornerUlb, // 7
            CornerUlb132, // 8
            CornerUlb213, // 9
            CornerUlb231, // 10
            CornerUlb312, // 11
            CornerUlb321, // 12

            CornerUrf,
            CornerUrf123 = CornerUrf, // 13
            CornerUrf132, // 14
            CornerUrf213, // 15
            CornerUrf231, // 16
            CornerUrf312, // 17
            CornerUrf321, // 18

            CornerUrb,
            CornerUrb123 = CornerUrb, // 19
            CornerUrb132, // 20
            CornerUrb213, // 21
            CornerUrb231, // 22
            CornerUrb312, // 23
            CornerUrb321, // 24

            CornerDlf,
            CornerDlf123 = CornerDlf, // 25
            CornerDlf132, // 26
            CornerDlf213, // 27
            CornerDlf231, // 28
            CornerDlf312, // 29
            CornerDlf321, // 30

            CornerDlb,
            CornerDlb123 = CornerDlb, // 31
            CornerDlb132, // 32
            CornerDlb213, // 33
            CornerDlb231, // 34
            CornerDlb312, // 35
            CornerDlb321, // 36

            CornerDrf,
            CornerDrf123 = CornerDrf, // 37
            CornerDrf132, // 38
            CornerDrf213, // 39
            CornerDrf231, // 40
            CornerDrf312, // 41
            CornerDrf321, // 42

            CornerDrb,
            CornerDrb123 = CornerDrb, // 43
            CornerDrb132, // 44
            CornerDrb213, // 45
            CornerDrb231, // 46
            CornerDrb312, // 47
            CornerDrb321, // 48
        };

        /// <summary>
        /// Cube to solve
        /// </summary>
        public ICube Cube { get; set; }

        /// <summary>
        /// Constructor that take the Cube to be solved
        /// </summary>
        /// <param name="cube">cube to solve</param>
        protected Solver(ICube cube)
        {
            Cube = cube;
        }

        /// <summary>
        /// Searches for edge that matches the given face and color edge
        /// </summary>
        /// <param name="faceColor">Color of the face to search</param>
        /// <param name="color">Color of the edge to search</param>
        /// <returns>Position of edge</returns>
        public FaceName search_edge(FaceVal faceColor, FaceVal color)
        {
            if (Cube.Up[0, 1] == faceColor && Cube.Back[0, 1] == color) return FaceName.TopFaceUp;
            if (Cube.Up[0, 1] == color && Cube.Back[0, 1] == faceColor) return FaceName.TopFaceUpReverse;
            if (Cube.Up[1, 0] == faceColor && Cube.Left[0, 1] == color) return FaceName.TopFaceLeft;
            if (Cube.Up[1, 0] == color && Cube.Left[0, 1] == faceColor) return FaceName.TopFaceLeftReverse;
            if (Cube.Up[1, 2] == faceColor && Cube.Right[0, 1] == color) return FaceName.TopFaceRight;
            if (Cube.Up[1, 2] == color && Cube.Right[0, 1] == faceColor) return FaceName.TopFaceRightReverse;
            if (Cube.Up[2, 1] == faceColor && Cube.Front[0, 1] == color) return FaceName.TopFaceDown;
            if (Cube.Up[2, 1] == color && Cube.Front[0, 1] == faceColor) return FaceName.TopFaceDownReverse;

            if (Cube.Left[1, 2] == faceColor && Cube.Front[1, 0] == color) return FaceName.MiddleFaceLeft;
            if (Cube.Left[1, 2] == color && Cube.Front[1, 0] == faceColor) return FaceName.MiddleFaceLeftReverse;
            if (Cube.Front[1, 2] == faceColor && Cube.Right[1, 0] == color) return FaceName.MiddleFaceFront;
            if (Cube.Front[1, 2] == color && Cube.Right[1, 0] == faceColor) return FaceName.MiddleFaceFrontReverse;
            if (Cube.Right[1, 2] == faceColor && Cube.Back[1, 0] == color) return FaceName.MiddleFaceRight;
            if (Cube.Right[1, 2] == color && Cube.Back[1, 0] == faceColor) return FaceName.MiddleFaceRightReverse;
            if (Cube.Back[1, 2] == faceColor && Cube.Left[1, 0] == color) return FaceName.MiddleFaceBack;
            if (Cube.Back[1, 2] == color && Cube.Left[1, 0] == faceColor) return FaceName.MiddleFaceBackReverse;

            if (Cube.Down[0, 1] == faceColor && Cube.Front[2, 1] == color) return FaceName.BottomFaceUp;
            if (Cube.Down[0, 1] == color && Cube.Front[2, 1] == faceColor) return FaceName.BottomFaceUpReverse;
            if (Cube.Down[1, 0] == faceColor && Cube.Left[2, 1] == color) return FaceName.BottomFaceLeft;
            if (Cube.Down[1, 0] == color && Cube.Left[2, 1] == faceColor) return FaceName.BottomFaceLeftReverse;
            if (Cube.Down[1, 2] == faceColor && Cube.Right[2, 1] == color) return FaceName.BottomFaceRight;
            if (Cube.Down[1, 2] == color && Cube.Right[2, 1] == faceColor) return FaceName.BottomFaceRightReverse;
            if (Cube.Down[2, 1] == faceColor && Cube.Back[2, 1] == color) return FaceName.BottomFaceDown;
            if (Cube.Down[2, 1] == color && Cube.Back[2, 1] == faceColor) return FaceName.BottomFaceDownReverse;

            return FaceName.FaceNone;
        }

        /// <summary>
        /// Search for one corner and orientation
        /// </summary>
        /// <param name="color1">First color of where to search</param>
        /// <param name="color2">Second color of where search</param>
        /// <param name="color3">Third color of where to search</param>
        /// <param name="a">First color to search for</param>
        /// <param name="b">Second color to search for</param>
        /// <param name="c">Third color to search for</param>
        /// <param name="resultBase">Enum name base</param>
        /// <returns>Found corner.
        /// The first letters of the returned enum are the name of the cubie
        /// The numbers are the orientation of the cubie</returns>
        private static CornerName find_corner(FaceVal color1, FaceVal color2, FaceVal color3, FaceVal a, FaceVal b, FaceVal c,
            CornerName resultBase)
        {
            if (a == color1)
            {
                if (b == color2)
                {
                    if (c == color3)
                        return resultBase + 0;
                }
                else if (b == color3)
                {
                    if (c == color2)
                        return resultBase + 1;
                }
            }
            else if (a == color2)
            {
                if (b == color1)
                {
                    if (c == color3)
                        return resultBase + 2;
                }
                else if (b == color3)
                {
                    if (c == color1)
                        return resultBase + 3;
                }
            }
            else if (a == color3)
            {
                if (b == color1)
                {
                    if (c == color2)
                        return resultBase + 4;
                }
                else if (b == color2)
                {
                    if (c == color1)
                        return resultBase + 5;
                }
            }

            return CornerName.CornerNone;
        }

        /// <summary>
        /// Search for a corner
        /// The first letters of the returned enum are the name of the cubie
        /// The numbers are the orientation of the cubie
        /// </summary>
        /// <param name="color1">First color of corner</param>
        /// <param name="color2">Second color of corner</param>
        /// <param name="color3">Third color of corner</param>
        /// <returns>Found corner and orientation or cubie</returns>
        public CornerName search_corner(FaceVal color1, FaceVal color2, FaceVal color3)
        {
            // CUBIE Up Left Front
            var pos = find_corner(color1, color2, color3,
                Cube.Up[CubeSize - 1, 0], Cube.Left[0, CubeSize - 1], Cube.Front[0, 0], CornerName.CornerUlf);
            if (pos != CornerName.CornerNone)
                return pos;

            // CUBIE Up Left Back
            pos = find_corner(color1, color2, color3,
                Cube.Up[0, 0], Cube.Left[0, 0], Cube.Back[0, CubeSize - 1], CornerName.CornerUlb);
            if (pos != CornerName.CornerNone)
                return pos;

            // CUBIE Up Right Front
            pos = find_corner(color1, color2, color3,
                Cube.Up[CubeSize - 1, CubeSize - 1], Cube.Right[0, 0], Cube.Front[0, CubeSize - 1],
                CornerName.CornerUrf);
            if (pos != CornerName.CornerNone)
                return pos;

            // CUBIE Up Right Back
            pos = find_corner(color1, color2, color3,
                Cube.Up[0, CubeSize - 1], Cube.Right[0, CubeSize - 1], Cube.Back[0, 0], CornerName.CornerUrb);
            if (pos != CornerName.CornerNone)
                return pos;

            // CUBIE Down Left Front
            pos = find_corner(color1, color2, color3,
                Cube.Down[0, 0], Cube.Left[CubeSize - 1, CubeSize - 1], Cube.Front[CubeSize - 1, 0],
                CornerName.CornerDlf);
            if (pos != CornerName.CornerNone)
                return pos;

            // CUBIE Down Left Back
            pos = find_corner(color1, color2, color3,
                Cube.Down[CubeSize - 1, 0], Cube.Left[CubeSize - 1, 0], Cube.Back[CubeSize - 1, CubeSize - 1],
                CornerName.CornerDlb);
            if (pos != CornerName.CornerNone)
                return pos;

            // CUBIE Down Right Front
            pos = find_corner(color1, color2, color3,
                Cube.Down[0, CubeSize - 1], Cube.Right[CubeSize - 1, 0], Cube.Front[CubeSize - 1, CubeSize - 1],
                CornerName.CornerDrf);
            if (pos != CornerName.CornerNone)
                return pos;

            // CUBIE Down Right Back
            pos = find_corner(color1, color2, color3,
                Cube.Down[CubeSize - 1, CubeSize - 1], Cube.Right[CubeSize - 1, CubeSize - 1],
                Cube.Back[CubeSize - 1, 0], CornerName.CornerDrb);

            return pos;
        }
    }
}
