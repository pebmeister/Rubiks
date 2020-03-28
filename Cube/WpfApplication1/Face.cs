using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public class Face
    {
        private const int CubeSize = 3;

        /// <summary>
        /// Holds the 3x3 FaceVals of the cubie
        /// </summary>
        private readonly FaceVal[,] _square =
        {
            { new FaceVal(0), new FaceVal(0),new FaceVal(0) },
            { new FaceVal(0), new FaceVal(0),new FaceVal(0) },
            { new FaceVal(0), new FaceVal(0),new FaceVal(0) }
        };

        /// <summary>
        /// Color of the face
        /// Center cubie
        /// </summary>
        /// <returns>Color of the center cubie</returns>
        public FaceVal Color => this[1, 1];

        /// <summary>
        /// Determine if all colors of the face are correct
        /// </summary>
        /// <returns>True if all colors of the face are correct</returns>
        public bool Issolved()
        {
            return _square.Cast<FaceVal>().All(faceVal => faceVal == Color);
        }

        /// <summary>
        /// Rotate the face clockwise
        /// </summary>
        public void rotate_clockwise()
        {
            var temp = new[,]
            {
                { this[2,0], this[1,0], this[0,0] },
                { this[2,1], this[1,1], this[0,1] },
                { this[2,2], this[1,2], this[0,2] }
            };

            for (var row = 0; row < CubeSize; row++)
                for (var col = 0; col < CubeSize; col++)
                    this[row, col] = temp[row, col];
        }

        /// <summary>
        /// Rotate the face counter clockwise
        /// </summary>
        public void rotate_counter_clockwise()
        {
            var temp = new[,]
            {
                { this[0,2], this[1,2], this[2,2] },
                { this[0,1], this[1,1], this[2,1] },
                { this[0,0], this[1,0], this[2,0] }
            };

            for (var row = 0; row < CubeSize; row++)
                for (var col = 0; col < CubeSize; col++)
                    this[row, col] = temp[row, col];
        }

        /// <summary>
        /// Array accessor
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns>cubie at the given row and column</returns>
        public FaceVal this[int row, int col]
        {
            get { return _square[row, col]; }
            set { _square[row, col] = value; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                sb.Append(this[row, col]);
            return sb.ToString();
        }
    }
}
