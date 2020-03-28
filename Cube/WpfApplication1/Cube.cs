using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace WpfApplication1
{
    public class Cube : ICube
    {
        private const int CubeSize = 3;

        private static readonly Random Rnd = new Random((int) DateTime.Now.Ticks);
        private static readonly char[] Whitechars = {' ', '\t', '\n', '\r', '\v', '\f'};

        private bool _record;
        private readonly StringBuilder _moves = new StringBuilder();
        private readonly StringBuilder _scrambleSequence = new StringBuilder();

        public string Moves => _moves.ToString().Trim();

        public string OptimizedMoves => optimize_sequence(_moves.ToString());
        public string ScrambleSequence => _scrambleSequence.ToString();

        /// <summary>
        /// Up face of the cube
        /// </summary>
        public Face Up { get; } = new Face();

        /// <summary>
        /// Front face of the cube
        /// </summary>
        public Face Front { get; } = new Face();

        /// <summary>
        /// Left face if the cube
        /// </summary>
        public Face Left { get; } = new Face();

        /// <summary>
        /// Right face of teh cube
        /// </summary>
        public Face Right { get; } = new Face();

        /// <summary>
        /// Down face of the cube
        /// </summary>
        public Face Down { get; } = new Face();

        /// <summary>
        /// Back face of the cube
        /// </summary>
        public Face Back { get; } = new Face();

        /// <summary>
        /// Cube constructor
        /// </summary>
        public Cube()
        {
            init_cube();
        }

        /// <summary>
        /// Initialize the cube to a solved state
        /// Clear moves and scamblesequence
        /// </summary>
        public void init_cube()
        {
            set_cube("W W W W W W W W W " +
                     "G G G G G G G G G " +
                     "R R R R R R R R R " +
                     "B B B B B B B B B " +
                     "O O O O O O O O O " +
                     "Y Y Y Y Y Y Y Y Y "
            );

            _moves.Clear();
            _scrambleSequence.Clear();
            _record = true;
        }

        /// <summary>
        /// Get the FaceVal for a cubie from a string
        /// </summary>
        /// <param name="val">String cubie value</param>
        /// <returns>Face_val of cubie</returns>
        private static FaceVal get_value(string val)
        {
            var result = 0;
            var shift = (val.Length - 1) * 8;
            foreach (var v in val)
            {
                result |= v << shift;
                shift -= 8;
            }

            return result;
        }

        /// <summary>
        /// Set the value of the cubies
        /// </summary>
        /// <param name="cubeValues">string containing cubie values</param>
        public void set_cube(string cubeValues)
        {
            var vals = cubeValues.Split(Whitechars, StringSplitOptions.RemoveEmptyEntries);

            var index = 0;

            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                Up[row, col] = get_value(vals[index++]);

            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                Left[row, col] = get_value(vals[index++]);

            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                Front[row, col] = get_value(vals[index++]);

            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                Right[row, col] = get_value(vals[index++]);

            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                Back[row, col] = get_value(vals[index++]);

            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                Down[row, col] = get_value(vals[index++]);
        }

        /// <summary>
        /// Make a copy of the cube
        /// Only copies values not record, moves or scramble
        /// </summary>
        /// <returns>Clone of cube</returns>
        public Cube clone()
        {
            var cloneCube = new Cube();
            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
            {
                cloneCube.Up[row, col] = Up[row, col];
                cloneCube.Left[row, col] = Left[row, col];
                cloneCube.Front[row, col] = Front[row, col];
                cloneCube.Right[row, col] = Right[row, col];
                cloneCube.Back[row, col] = Back[row, col];
                cloneCube.Down[row, col] = Down[row, col];
            }

            return cloneCube;
        }

        /// <summary>
        /// Determine if the cube is solved
        /// </summary>
        /// <returns>True if the cube is solved</returns>
        public bool issolved()
        {
            return Up.Issolved() && Left.Issolved() &&
                   Front.Issolved() && Right.Issolved() &&
                   Back.Issolved() && Down.Issolved();
        }

        /// <summary>
        /// Rotate the front face clockwise
        /// </summary>
        public void f()
        {
            Front.rotate_clockwise();
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Up[CubeSize - 1, CubeSize - 1 - index];
                Up[CubeSize - 1, CubeSize - 1 - index] = Left[index, CubeSize - 1];
                Left[index, CubeSize - 1] = Down[0, index];
                Down[0, index] = Right[CubeSize - 1 - index, 0];
                Right[CubeSize - 1 - index, 0] = temp;
            }

            if (_record)
                _moves.Append("F ");
        }

        /// <summary>
        /// Rotate the front face counter clockwise
        /// </summary>
        public void fi()
        {
            Front.rotate_counter_clockwise();
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Up[CubeSize - 1, CubeSize - 1 - index];
                Up[CubeSize - 1, CubeSize - 1 - index] = Right[CubeSize - 1 - index, 0];
                Right[CubeSize - 1 - index, 0] = Down[0, index];
                Down[0, index] = Left[index, CubeSize - 1];
                Left[index, CubeSize - 1] = temp;
            }

            if (_record)
                _moves.Append("Fi ");
        }

        /// <summary>
        /// Rotate the up face clockwise
        /// </summary>
        public void u()
        {
            Up.rotate_clockwise();
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Front[0, index];
                Front[0, index] = Right[0, index];
                Right[0, index] = Back[0, index];
                Back[0, index] = Left[0, index];
                Left[0, index] = temp;
            }

            if (_record)
                _moves.Append("U ");
        }

        /// <summary>
        /// Rotate the up face counter clockwise
        /// </summary>
        public void ui()
        {
            Up.rotate_counter_clockwise();
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Front[0, index];
                Front[0, index] = Left[0, index];
                Left[0, index] = Back[0, index];
                Back[0, index] = Right[0, index];
                Right[0, index] = temp;
            }

            if (_record)
                _moves.Append("Ui ");
        }

        /// <summary>
        /// Rotate the back face clockwise
        /// </summary>
        public void b()
        {
            Back.rotate_clockwise();
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Up[0, index];
                Up[0, index] = Right[index, CubeSize - 1];
                Right[index, CubeSize - 1] = Down[CubeSize - 1, CubeSize - 1 - index];
                Down[CubeSize - 1, CubeSize - 1 - index] = Left[CubeSize - 1 - index, 0];
                Left[CubeSize - 1 - index, 0] = temp;
            }

            if (_record)
                _moves.Append("B ");
        }

        /// <summary>
        /// Rotate the back face counter clockwise
        /// </summary>
        public void bi()
        {
            Back.rotate_counter_clockwise();
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Up[0, index];
                Up[0, index] = Left[CubeSize - 1 - index, 0];
                Left[CubeSize - 1 - index, 0] = Down[CubeSize - 1, CubeSize - 1 - index];
                Down[CubeSize - 1, CubeSize - 1 - index] = Right[index, CubeSize - 1];
                Right[index, CubeSize - 1] = temp;
            }

            if (_record)
                _moves.Append("Bi ");
        }

        /// <summary>
        /// Rotate the left face clockwise
        /// </summary>
        public void l()
        {
            Left.rotate_clockwise();
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Up[index, 0];
                Up[index, 0] = Back[CubeSize - 1 - index, CubeSize - 1];
                Back[CubeSize - 1 - index, CubeSize - 1] = Down[index, 0];
                Down[index, 0] = Front[index, 0];
                Front[index, 0] = temp;
            }

            if (_record)
                _moves.Append("L ");
        }

        /// <summary>
        /// Rotate the left face counter clockwise
        /// </summary>
        public void li()
        {
            Left.rotate_counter_clockwise();
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Up[index, 0];
                Up[index, 0] = Front[index, 0];
                Front[index, 0] = Down[index, 0];
                Down[index, 0] = Back[CubeSize - 1 - index, CubeSize - 1];
                Back[CubeSize - 1 - index, CubeSize - 1] = temp;
            }

            if (_record)
                _moves.Append("Li ");
        }

        /// <summary>
        /// Rotate the right face clockwise
        /// </summary>
        public void r()
        {
            Right.rotate_clockwise();
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Up[index, CubeSize - 1];
                Up[index, CubeSize - 1] = Front[index, CubeSize - 1];
                Front[index, CubeSize - 1] = Down[index, CubeSize - 1];
                Down[index, CubeSize - 1] = Back[CubeSize - 1 - index, 0];
                Back[CubeSize - 1 - index, 0] = temp;
            }

            if (_record)
                _moves.Append("R ");
        }

        /// <summary>
        /// Rotate the right face counter clockwise
        /// </summary>
        public void ri()
        {
            Right.rotate_counter_clockwise();
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Up[CubeSize - 1 - index, CubeSize - 1];
                Up[CubeSize - 1 - index, CubeSize - 1] = Back[index, 0];
                Back[index, 0] = Down[CubeSize - 1 - index, CubeSize - 1];
                Down[CubeSize - 1 - index, CubeSize - 1] = Front[CubeSize - 1 - index, CubeSize - 1];
                Front[CubeSize - 1 - index, CubeSize - 1] = temp;
            }

            if (_record)
                _moves.Append("Ri ");
        }

        /// <summary>
        /// Rotate the down face clockwise
        /// </summary>
        public void d()
        {
            Down.rotate_clockwise();
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Front[CubeSize - 1, index];
                Front[CubeSize - 1, index] = Left[CubeSize - 1, index];
                Left[CubeSize - 1, index] = Back[CubeSize - 1, index];
                Back[CubeSize - 1, index] = Right[CubeSize - 1, index];
                Right[CubeSize - 1, index] = temp;
            }

            if (_record)
                _moves.Append("D ");
        }

        /// <summary>
        /// Rotate the down face counter clockwise
        /// </summary>
        public void di()
        {
            Down.rotate_counter_clockwise();
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Front[CubeSize - 1, index];
                Front[CubeSize - 1, index] = Right[CubeSize - 1, index];
                Right[CubeSize - 1, index] = Back[CubeSize - 1, index];
                Back[CubeSize - 1, index] = Left[CubeSize - 1, index];
                Left[CubeSize - 1, index] = temp;
            }

            if (_record)
                _moves.Append("Di ");
        }

        /// <summary>
        /// Rotate the up slice clockwise
        /// </summary>
        public void us()
        {
            for (var col = 0; col < CubeSize; col++)
            {
                var temp = Front[1, col];
                Front[1, col] = Right[1, col];
                Right[1, col] = Back[1, col];
                Back[1, col] = Left[1, col];
                Left[1, col] = temp;
            }

            if (_record)
                _moves.Append("Us ");
        }

        /// <summary>
        /// Rotate the down slice clockwise
        /// </summary>
        public void ds()
        {
            for (var col = 0; col < CubeSize; col++)
            {
                var temp = Front[1, col];
                Front[1, col] = Left[1, col];
                Left[1, col] = Back[1, col];
                Back[1, col] = Right[1, col];
                Right[1, col] = temp;
            }

            if (_record)
                _moves.Append("Ds ");
        }

        /// <summary>
        /// Rotate the left slice clockwise
        /// </summary>
        public void ls()
        {
            for (var row = 0; row < CubeSize; row++)
            {
                var temp = Up[row, 1];
                Up[row, 1] = Back[CubeSize - row - 1, 1];
                Back[CubeSize - row - 1, 1] = Down[row, 1];
                Down[row, 1] = Front[row, 1];
                Front[row, 1] = temp;
            }

            if (_record)
                _moves.Append("Ls ");
        }

        /// <summary>
        /// Rotate the right slice clockwise
        /// </summary>
        public void rs()
        {
            for (var row = 0; row < CubeSize; row++)
            {
                var temp = Up[row, 1];
                Up[row, 1] = Front[row, 1];
                Front[row, 1] = Down[row, 1];
                Down[row, 1] = Back[CubeSize - row - 1, 1];
                Back[CubeSize - row - 1, 1] = temp;
            }

            if (_record)
                _moves.Append("Rs ");
        }

        /// <summary>
        /// Rotate the front slice clockwise
        /// </summary>
        public void fs()
        {
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Up[1, index];
                Up[1, index] = Left[CubeSize - index - 1, 1];
                Left[CubeSize - index - 1, 1] = Down[1, CubeSize - index - 1];
                Down[1, CubeSize - index - 1] = Right[index, 1];
                Right[index, 1] = temp;
            }

            if (_record)
                _moves.Append("Fs ");
        }

        /// <summary>
        /// Rotate the back slice clockwise
        /// </summary>
        public void bs()
        {
            for (var index = 0; index < CubeSize; index++)
            {
                var temp = Up[1, index];
                Up[1, index] = Right[index, 1];
                Right[index, 1] = Down[1, CubeSize - index - 1];
                Down[1, CubeSize - index - 1] = Left[CubeSize - index - 1, 1];
                Left[CubeSize - index - 1, 1] = temp;
            }

            if (_record)
                _moves.Append("Bs ");
        }

        /// <summary>
        /// Rotate the cube up
        /// </summary>
        public void cu()
        {
            Left.rotate_counter_clockwise();
            Right.rotate_clockwise();
            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
            {
                var temp = Up[row, col];
                Up[row, col] = Front[row, col];
                Front[row, col] = Down[row, col];
                Down[row, col] = Back[CubeSize - 1 - row, CubeSize - 1 - col];
                Back[CubeSize - 1 - row, CubeSize - 1 - col] = temp;
            }

            if (_record)
                _moves.Append("Cu ");
        }

        /// <summary>
        /// Rotate the cube down
        /// </summary>
        public void cd()
        {
            Left.rotate_clockwise();
            Right.rotate_counter_clockwise();
            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
            {
                var temp = Down[row, col];
                Down[row, col] = Front[row, col];
                Front[row, col] = Up[row, col];
                Up[row, col] = Back[CubeSize - 1 - row, CubeSize - 1 - col];
                Back[CubeSize - 1 - row, CubeSize - 1 - col] = temp;
            }

            if (_record)
                _moves.Append("Cd ");
        }

        /// <summary>
        /// Rotate the cube left
        /// </summary>
        public void cl()
        {
            Down.rotate_counter_clockwise();
            Up.rotate_clockwise();
            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
            {
                var temp = Front[row, col];
                Front[row, col] = Right[row, col];
                Right[row, col] = Back[row, col];
                Back[row, col] = Left[row, col];
                Left[row, col] = temp;
            }

            if (_record)
                _moves.Append("Cl ");
        }

        /// <summary>
        /// Rotate the cube right
        /// </summary>
        public void cr()
        {
            Down.rotate_clockwise();
            Up.rotate_counter_clockwise();
            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
            {
                var temp = Front[row, col];
                Front[row, col] = Left[row, col];
                Left[row, col] = Back[row, col];
                Back[row, col] = Right[row, col];
                Right[row, col] = temp;
            }

            if (_record)
                _moves.Append("Cr ");
        }

        /// <summary>
        /// Reverse a sequence
        /// </summary>
        /// <param name="sequence">Sequence to reverse</param>
        /// <returns>Reversed sequence</returns>
        public string reverse_sequence(string sequence)
        {
            var buffer = new StringBuilder();
            var strs = sequence.Split(Whitechars, StringSplitOptions.RemoveEmptyEntries);

            foreach (var str in strs)
            {
                var lowerstr = str.ToLower(CultureInfo.CurrentCulture);
                if (buffer.Length > 0)
                    buffer.Insert(0, ' ');

                switch (lowerstr)
                {
                    case "u":
                        buffer.Insert(0, 'i');
                        buffer.Insert(0, str[0]);
                        break;
                    case "ui":
                        buffer.Insert(0, str[0]);
                        break;
                    case "l":
                        buffer.Insert(0, 'i');
                        buffer.Insert(0, str[0]);
                        break;
                    case "li":
                        buffer.Insert(0, str[0]);
                        break;
                    case "f":
                        buffer.Insert(0, 'i');
                        buffer.Insert(0, str[0]);
                        break;
                    case "fi":
                        buffer.Insert(0, str[0]);
                        break;
                    case "r":
                        buffer.Insert(0, 'i');
                        buffer.Insert(0, str[0]);
                        break;
                    case "ri":
                        buffer.Insert(0, str[0]);
                        break;
                    case "b":
                        buffer.Insert(0, 'i');
                        buffer.Insert(0, str[0]);
                        break;
                    case "bi":
                        buffer.Insert(0, str[0]);
                        break;
                    case "d":
                        buffer.Insert(0, 'i');
                        buffer.Insert(0, str[0]);
                        break;
                    case "di":
                        buffer.Insert(0, 'd');
                        break;
                    case "us":
                        buffer.Insert(0, str[1]);
                        buffer.Insert(0, 'D');
                        break;
                    case "ds":
                        buffer.Insert(0, str[1]);
                        buffer.Insert(0, 'U');
                        break;
                    case "ls":
                        buffer.Insert(0, str[1]);
                        buffer.Insert(0, 'R');
                        break;
                    case "rs":
                        buffer.Insert(0, str[1]);
                        buffer.Insert(0, 'L');
                        break;
                    case "fs":
                        buffer.Insert(0, str[1]);
                        buffer.Insert(0, 'B');
                        break;
                    case "bs":
                        buffer.Insert(0, str[1]);
                        buffer.Insert(0, 'F');
                        break;
                    case "cu":
                        buffer.Insert(0, 'd');
                        buffer.Insert(0, str[0]);
                        break;
                    case "cd":
                        buffer.Insert(0, 'u');
                        buffer.Insert(0, str[0]);
                        break;
                    case "cl":
                        buffer.Insert(0, 'r');
                        buffer.Insert(0, str[0]);
                        break;
                    case "cr":
                        buffer.Insert(0, 'l');
                        buffer.Insert(0, str[0]);
                        break;
                }
            }

            return optimize_sequence(buffer.ToString());
        }

        /// <summary>
        /// Execute a single move
        /// </summary>
        /// <param name="move">Move to execute</param>
        private void execute_move(string move)
        {
            switch (move.ToLower())
            {
                case "u":
                    u();
                    break;
                case "ui":
                    ui();
                    break;
                case "d":
                    d();
                    break;
                case "di":
                    di();
                    break;
                case "l":
                    l();
                    break;
                case "li":
                    li();
                    break;
                case "r":
                    r();
                    break;
                case "ri":
                    ri();
                    break;
                case "f":
                    f();
                    break;
                case "fi":
                    fi();
                    break;
                case "b":
                    b();
                    break;
                case "bi":
                    bi();
                    break;
                case "us":
                    us();
                    break;
                case "ds":
                    ds();
                    break;
                case "rs":
                    rs();
                    break;
                case "ls":
                    ls();
                    break;
                case "fs":
                    fs();
                    break;
                case "bs":
                    bs();
                    break;
                case "cu":
                    cu();
                    break;
                case "cd":
                    cd();
                    break;
                case "cl":
                    cl();
                    break;
                case "cr":
                    cr();
                    break;
            }
        }

        /// <summary>
        /// Execeute a sequence
        /// </summary>
        /// <param name="sequence">Sequence to execute</param>
        public void execute_sequence(string sequence)
        {
            if (string.IsNullOrEmpty(sequence)) return;

            var tokens = sequence.Split(Whitechars, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
                execute_move(token);
        }

        /// <summary>
        /// Scramble the cube
        /// Put the sequence in _scrambleSequence
        /// </summary>
        /// <param name="scramblecount">Number of moves to generate</param>
        public void scramble_cube(int scramblecount = 200)
        {
            var temp = _record;
            _record = false;

            var count = scramblecount;
            string[] scramblemoves =
            {
                "U", "Ui", "D", "Di", "L", "Li", "R", "Ri", "B", "Bi", "F", "Fi",
                "Us", "Ds", "Ls", "Rs", "Fs", "Bs"
            };

            var sequence = new StringBuilder();
            for (var scrambleloop = 0; scrambleloop < count; scrambleloop++)
            {
                var rnd = Rnd.Next(0, scramblemoves.Length);
                sequence.Append(scramblemoves[rnd] + ' ');
            }

            _scrambleSequence.Clear();
            _scrambleSequence.Append(optimize_sequence(sequence.ToString()));
            execute_sequence(_scrambleSequence.ToString());
            _record = temp;
        }

        /// <summary>
        /// Optimize a sequence
        /// </summary>
        /// <param name="sequence">Sequence to optimize</param>
        /// <returns>Optimize a sequence</returns>
        public string optimize_sequence(string sequence)
        {
            string outstr;
            var temp = sequence;
            int len1;
            int len2;
            do
            {
                outstr = optimize_sequence_recursion(temp);
                len1 = temp.Length;
                len2 = outstr.Length;
                temp = outstr;
            } while (len2 < len1);

            return outstr;
        }

        /// <summary>
        /// Optimize a sequence
        ///
        /// The basic algorithm is to track 
        /// how many turns there are in a given direction
        /// and opposite direction while keeping track of which moves can be ignored.
        /// Optimize the ignored moves via recursion
        /// </summary>
        /// <param name="sequence">Sequence to optimize</param>
        /// <returns>Optimized sequence</returns>
        private static string optimize_sequence_recursion(string sequence)
        {

            const string endMarker = "****";

            sequence += ' ' + endMarker;
            var tokens = sequence.Split(Whitechars);

            var index = 0u;
            var count = 0;
            var ignore = new List<string>();
            var add = new List<string>();
            var subtract = new List<string>();
            var newsequence = true;
            var ignoreString = "";
            var outstr = "";
            var addString = "";
            var subtractString = "";

            // loop through all tokens
            while (index < tokens.Length)
            {
                // get the current token
                var token = tokens[index++].ToLower();

                // new sequence
                if (newsequence)
                {
                    newsequence = false;
                    count = 0;
                    ignore.Clear();
                    add.Clear();
                    subtract.Clear();
                    ignoreString = "";
                    addString = "";
                    subtractString = "";

                    switch (token)
                    {
                        // left and left inverse
                        case "l":
                        case "li":
                            add.Add("l");
                            subtract.Add("li");
                            ignore.AddRange(new[] {"r", "ri", "ls", "rs"});
                            addString = "L";
                            subtractString = "Li";
                            break;

                        // right and right inverse
                        case "r":
                        case "ri":
                            add.Add("r");
                            subtract.Add("ri");
                            ignore.AddRange(new[] {"l", "li", "ls", "rs"});
                            addString = "R";
                            subtractString = "Ri";
                            break;

                        // front and front inverse
                        case "f":
                        case "fi":
                            add.Add("f");
                            subtract.Add("fi");
                            ignore.AddRange(new[] {"b", "bi", "fs", "bs"});
                            addString = "F";
                            subtractString = "Fi";
                            break;

                        // back and back inverse
                        case "b":
                        case "bi":
                            add.Add("b");
                            subtract.Add("bi");
                            ignore.AddRange(new[] {"f", "fi", "fs", "bs"});
                            addString = "B";
                            subtractString = "Bi";
                            break;

                        // up and up inverse
                        case "u":
                        case "ui":
                            add.Add("u");
                            subtract.Add("ui");
                            ignore.AddRange(new[] {"d", "di", "us", "ds"});
                            addString = "U";
                            subtractString = "Ui";
                            break;

                        // down and down inverse
                        case "d":
                        case "di":
                            add.Add("d");
                            subtract.Add("di");
                            ignore.AddRange(new[] {"u", "ui", "us", "ds"});
                            addString = "D";
                            subtractString = "Di";
                            break;

                        // cube up and cube down
                        case "cu":
                        case "cd":
                            add.Add("cu");
                            subtract.Add("cd");
                            addString = "Cu";
                            subtractString = "Cd";
                            break;

                        // cube left and cub3+e right
                        case "cl":
                        case "cr":
                            add.Add("cl");
                            subtract.Add("cr");
                            addString = "Cl";
                            subtractString = "Cr";
                            break;

                        // up slice and down slice
                        case "us":
                        case "ds":
                            add.Add("us");
                            subtract.Add("ds");
                            ignore.AddRange(new[] {"u", "ui", "d", "di"});
                            addString = "Us";
                            subtractString = "Ds";
                            break;

                        // left slice and right slice
                        case "ls":
                        case "rs":
                            add.Add("ls");
                            subtract.Add("rs");
                            ignore.AddRange(new[] {"l", "li", "r", "ri"});
                            addString = "Ls";
                            subtractString = "Rs";
                            break;

                        // front slice and back slice
                        case "fs":
                        case "bs":
                            add.Add("fs");
                            subtract.Add("bs");
                            ignore.AddRange(new[] {"f", "fi", "b", "bi"});
                            addString = "Fs";
                            subtractString = "Bs";
                            break;

                        // if (tok == end_marker)
                        default:
                            add.Add(token);
                            break;
                    }
                }

                // At this point add, substract and ignore lists are set

                // add
                if (add.Contains(token))
                {
                    count++;
                    continue;
                }

                // subtract
                if (subtract.Contains(token))
                {
                    count--;
                    continue;
                }

                // ignore
                if (ignore.Contains(token))
                {
                    ignoreString += ' ' + token;
                    continue;
                }

                // check for end of sequence
                // recurse over ignore string
                if (ignoreString.Length > 0)
                {
                    var opt = optimize_sequence_recursion(ignoreString);
                    if (opt.Length > 0)
                        outstr += ' ' + opt;
                }

                // the numbers of moves in any direction must be mod 4
                count %= 4;
                if (count > 0)
                {
                    switch (count)
                    {
                        case 1:
                            outstr += ' ' + addString;
                            break;
                        case 2:
                            outstr += ' ' + addString;
                            outstr += ' ' + addString;
                            break;
                        case 3: // 3 add == 1 substract
                            outstr += ' ' + subtractString;
                            break;
                    }
                }
                else if (count < 0)
                {
                    switch (count)
                    {
                        case -1:
                            outstr += ' ' + subtractString;
                            break;
                        case -2: // 2 subtracts == 2 adds for simplicity
                            outstr += ' ' + addString;
                            outstr += ' ' + addString;
                            break;
                        case -3: // 3 subtracts == 1 add
                            outstr += ' ' + addString;
                            break;
                    }
                }

                // trigger a new sequence
                newsequence = true;

                // move 1 token backwards
                index--;
            }

            return outstr.Trim();
        }

        /// <summary>
        /// Positions the cube so the given color is the up face
        /// </summary>
        /// <param name="color">Color of top face</param>
        public void set_up_face(FaceVal color)
        {
            if (Front.Color == color)
                cu();
            else if (Left.Color == color)
            {
                cr();
                cu();
            }
            else if (Right.Color == color)
            {
                cl();
                cu();
            }
            else if (Back.Color == color)
                cd();
            else if (Down.Color == color)
            {
                cd();
                cd();
            }
        }

        /// <summary>
        /// Positions the cube so the given color is the front face
        /// </summary>
        /// <param name="color">Color of front face</param>
        public void set_front_face(FaceVal color)
        {
            if (Up.Color == color)
                cd();
            else if (Left.Color == color)
                cr();
            else if (Right.Color == color)
                cl();
            else if (Back.Color == color)
            {
                cl();
                cl();
            }
            else if (Down.Color == color)
                cu();
        }

        /// <summary>
        /// String representation of the cube
        /// </summary>
        /// <returns>String of the Cube colors</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                sb.Append(Up[row, col] + " ");

            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                sb.Append(Left[row, col] + " ");

            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                sb.Append(Front[row, col] + " ");

            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                sb.Append(Right[row, col] + " ");

            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                sb.Append(Back[row, col] + " ");

            for (var row = 0; row < CubeSize; row++)
            for (var col = 0; col < CubeSize; col++)
                sb.Append(Down[row, col] + " ");

            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }
}
