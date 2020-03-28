using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public class SimpleSolver : Solver, ISolver
    {
        private const int NumCubeSides = 4;

        /// <summary>
        /// corner_state
        /// </summary>
        private enum CornerState
        {
            CornerStateOne,
            CornerStateTwo,
            CornerStateThree,
            CornerStateFour,
        };

        /// <summary>
        /// Cross state
        /// </summary>
        private enum CrossState
        {
            CrossStateUnknown,
            CrossStateOne,
            CrossStateTwo,
            CrossStateThreeA,
            CrossStateThreeB,
            CrossStateThreeC,
            CrossStateThreeD,
            CrossStateFourA,
            CrossStateFourB
        };

        /// <summary>
        /// Moves allowed
        /// </summary>
        private static readonly string[] PossibleMoves =
        {
            "U", "Ui", "D", "Di", "L", "Li", "R", "Ri", "B", "Bi", "F", "Fi",
            "Us", "Ds", "Ls", "Rs", "Fs", "Bs"
        };

        /// <summary>
        /// Solve sequences for TopEdges 
        /// </summary>
        private static readonly string[][] TopEdgeMoves =
        {
            new[]                           // face_none
            {
                "",                         // back
                "",                         // left
                "",                         // right
                ""                          // front
            },
            new[]                           // top_face_up
            {
                "",                         // back
                "b b d l l",                // left
                "b b di r r",               // right
                "b b d d f f"               // front
            },
            new[]                           // top_face_up_reverse
            {
                "b b di ri b r",            // back
                "b b d d f li fi",          // left
                "b b d d fi r f",           // right
                "b b d li f l"              // front
            },
            new[]                           // top_face_left
            {
                "l l di b b",               // back
                "",                         // left
                "l l d d r r",              // right
                "l l d f f"                 // front
            },
            new[]                           // top_face_left_reverse
            {
                "li bi",                    // back
                "l l d f li fi",            // left
                "l l d fi r f",             // right
                "l f"                       // front
            },
            new[]                           // top_face_down
            {
                "f f d d b b",              // back
                "f f di l l",               // left
                "f f d r r",                // right
                ""                          // front
            },
            new[]                           // top_face_down_reverse
            {
                "f f di l bi li",           // back
                "fi li",                    // left
                "f r",                      // right
                "f f d r fi ri"             // front
            },
            new[]                           // top_face_right
            {
                "r r d b b",                // back
                "r r d d l l",              // left
                "",                         // right
                "r r di f f"                // front
            },
            new[]                           // top_face_right_reverse
            {
                "r b",                      // back
                "r r di f li fi",           // left
                "r r di fi r f",            // right
                "ri fi"                     // front
            },
            new[]                           // middle_face_left
            {
                "l l bi l l",               // back
                "fi d d f d l l",           // left
                "li f l fi d r r",          // right
                "f"                         // front
            },
            new[]                           // middle_face_left_reverse
            {
                "l d li d d b b",           // back
                "li",                       // left
                "u f f ui r",               // right
                "fi ri d r fi"              // front
            },
            new[]                           // middle_face_front
            {
                "ri d d r di b b",          // back
                "ri di r di l l",           // left
                "r",                        // right
                "f ri d r fi"               // front
            },
            new[]                           // middle_face_front_reverse
            {
                "fi r r f r r b",           // back
                "f di l l fi",              // left
                "ri di fi r f",             // right
                "fi"                        // front
            },
            new[]                           // middle_face_right
            {
                "b",                        // back
                "bi d d b di l l",          // left
                "bi di b r r",              // right
                "bi d d b f f"              // front
            },
            new[]                           // middle_face_right_reverse
            {
                "bi r di ri b",             // back
                "r d d ri l l",             // left
                "ri",                       // right
                "b ri bi r di f f"          // front
            },
            new[]                           // middle_face_back
            {
                "li d d l d b b",           // back
                "l",                        // left
                "li d d l r r",             // right
                "li d l f f"                // front
            },
            new[]                           // middle_face_back_reverse
            {
                "bi",                       // back
                "b d bi l l",               // left
                "b di bi r r",              // right
                "b d d bi f f"              // front
            },
            new[]                           // bottom_face_up
            {
                "d d b b",                  // back
                "di l l",                   // left
                "d r r",                    // right
                "f f"                       // front
            },
            new[]                           // bottom_face_up_reverse
            {
                "r d ri b",                 // back
                "f li fi",                  // left
                "fi r f",                   // right
                "ri d r fi"                 // front
            },
            new[]                           // bottom_face_left
            {
                "di b b",                   // back
                "l l",                      // left
                "d d r r",                  // right
                "d f f"                     // front
            },
            new[]                           // bottom_face_left_reverse
            {
                "l bi li",                  // back
                "d d fi di f li",           // left
                "f d fi r",                 // right
                "li f l"                    // front
            },
            new[]                           // bottom_face_down
            {
                "b b",                      // back
                "d l l",                    // left
                "di r r",                   // right
                "d d f f"                   // front
            },
            new[]                           // bottom_face_down_reverse
            {
                "r di ri b",                // back
                "di b d bi l",              // left
                "d bi di b ri",             // right
                "ri di r fi"                // front
            },
            new[]                           // bottom_face_right
            {
                "d b b",                    // back
                "d d l l",                  // left
                "r r",                      // right
                "di f f"                    // front
            },
            new[]                           // bottom_face_right_reverse
            {
                "ri b r",                   // back
                "fi di f li",               // left
                "f di fi r",                // right
                "di ri d r fi"              // front
            }
        };

        /// <summary>
        /// Constructor for the SimpleSolver
        /// </summary>
        /// <param name="cube">Cube to solve</param>
        public SimpleSolver(ICube cube) : base(cube)
        {
        }

        /// <summary>
        /// Get the face that matches the given color
        /// </summary>
        /// <param name="face">Color of face</param>
        /// <returns>Face index</returns>
        private int face_val_to_index(FaceVal face)
        {
            if (face == Cube.Back.Color)
                return (int) FaceSide.BackFace;
            if (face == Cube.Left.Color)
                return (int) FaceSide.LeftFace;
            if (face == Cube.Right.Color)
                return (int) FaceSide.RightFace;
            
            return (int) FaceSide.FrontFace;
        }

        /// <summary>
        /// Positions the up edge so it is the given color
        /// </summary>
        /// <param name="color">Color of Edge</param>
        private void position_up_edge(FaceVal color)
        {
            var upColor = Cube.Up.Color;
            var foundEdgePosition = search_edge(upColor, color);
            if (foundEdgePosition == FaceName.FaceNone)
                return;  //  should NEVER happen

            var face = face_val_to_index(color);

            var sequence = TopEdgeMoves[(int) foundEdgePosition][face];
            Cube.execute_sequence(sequence);
        }

        /// <summary>
        /// Position the upper left front corner
        /// </summary>
        /// <param name="color1">First color of corner</param>
        /// <param name="color2">Second color of corner</param>
        /// <param name="color3">Third color of corner</param>
        private void position_up_left_front_corner(FaceVal color1, FaceVal color2, FaceVal color3)
        {
            var pos = search_corner(color1, color2, color3);
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (pos)
            {
                default:
                // ReSharper disable once RedundantCaseLabel
                case CornerName.CornerUlf123:
                    break;

                case CornerName.CornerUlf231:
                    Cube.execute_sequence("l di li d l di li");
                    break;

                case CornerName.CornerUlf312:
                    Cube.execute_sequence("fi d f di fi d f");
                    break;

                case CornerName.CornerUlb132:
                    Cube.execute_sequence("b d bi d l di li");
                    break;

                case CornerName.CornerUlb213:
                    Cube.execute_sequence("b di bi d d l di li");
                    break;

                case CornerName.CornerUlb321:
                    Cube.execute_sequence("b fi d bi f");
                    break;

                case CornerName.CornerUrf132:
                    Cube.execute_sequence("ri d d r fi d f");
                    break;

                case CornerName.CornerUrf213:
                    Cube.execute_sequence("ri l di r li");
                    break;

                case CornerName.CornerUrf321:
                    Cube.execute_sequence("f d d f f d f");
                    break;

                case CornerName.CornerUrb123:
                    Cube.execute_sequence("bi fi d d f b");
                    break;

                case CornerName.CornerUrb231:
                    Cube.execute_sequence("bi di l di li b");
                    break;

                case CornerName.CornerUrb312:
                    Cube.execute_sequence("r d d ri di fi d f");
                    break;

                case CornerName.CornerDlf132:
                    Cube.execute_sequence("di fi d d f di fi d f");
                    break;

                case CornerName.CornerDlf213:
                    Cube.execute_sequence("di fi d f");
                    break;

                case CornerName.CornerDlf321:
                    Cube.execute_sequence("d l di li");
                    break;

                case CornerName.CornerDlb123:
                    Cube.execute_sequence("fi d d f di fi d f");
                    break;

                case CornerName.CornerDlb231:
                    Cube.execute_sequence("fi d f");
                    break;

                case CornerName.CornerDlb312:
                    Cube.execute_sequence("d d l di li");
                    break;

                case CornerName.CornerDrf123:
                    Cube.execute_sequence("l d d li d l di li");
                    break;

                case CornerName.CornerDrf231:
                    Cube.execute_sequence("d d fi d f");
                    break;

                case CornerName.CornerDrf312:
                    Cube.execute_sequence("l di li");
                    break;

                case CornerName.CornerDrb132:
                    Cube.execute_sequence("l d li d l di li");
                    break;

                case CornerName.CornerDrb213:
                    Cube.execute_sequence("fi d d f");
                    break;

                case CornerName.CornerDrb321:
                    Cube.execute_sequence("l d d li");
                    break;
            }
        }

        /// <summary>
        /// Position the middle front right edge of the cube
        /// </summary>
        /// <param name="side">Color of face. Should always be right face color</param>
        private void position_middle_front_right_edge(FaceVal side)
        {
            var pos = search_edge(Cube.Front.Color, side);
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (pos)
            {
                case FaceName.MiddleFaceLeft:
                    Cube.execute_sequence("l di li di fi d f d ri d r d f di fi");
                    break;

                case FaceName.MiddleFaceLeftReverse:
                    Cube.execute_sequence("l di li di fi d f f di fi di ri d r");
                    break;

                default:
                // ReSharper disable once RedundantCaseLabel
                case FaceName.MiddleFaceFront:
                    break;

                case FaceName.MiddleFaceFrontReverse:
                    Cube.execute_sequence("ri d r d f di fi d ri d r d f di fi");
                    break;

                case FaceName.MiddleFaceRight:
                    Cube.execute_sequence("bi d b d r di ri di f di fi di ri d r");
                    break;

                case FaceName.MiddleFaceRightReverse:
                    Cube.execute_sequence("bi d b d r di ri ri d r d f di fi");
                    break;

                case FaceName.MiddleFaceBack:
                    Cube.execute_sequence("b di bi di li d l d d ri d r d f di fi");
                    break;

                case FaceName.MiddleFaceBackReverse:
                    Cube.execute_sequence("b di bi di li d l d f di fi di ri d r");
                    break;

                case FaceName.BottomFaceUp:
                    Cube.execute_sequence("d d f di fi di ri d r");
                    break;

                case FaceName.BottomFaceUpReverse:
                    Cube.execute_sequence("di ri d r d f di fi");
                    break;

                case FaceName.BottomFaceLeft:
                    Cube.execute_sequence("di f di fi di ri d r");
                    break;

                case FaceName.BottomFaceLeftReverse:
                    Cube.execute_sequence("ri d r d f di fi");
                    break;

                case FaceName.BottomFaceDown:
                    Cube.execute_sequence("f di fi di ri d r");
                    break;

                case FaceName.BottomFaceDownReverse:
                    Cube.execute_sequence("d ri d r d f di fi");
                    break;

                case FaceName.BottomFaceRight:
                    Cube.execute_sequence("d f di fi di ri d r");
                    break;

                case FaceName.BottomFaceRightReverse:
                    Cube.execute_sequence("d d ri d r d f di fi");
                    break;
            }
        }

        /// <summary>
        /// Get the state of the last layer
        /// Count how many corner colors are correct
        /// return retur corner state based on number correct
        /// </summary>
        /// <param name="side">Face to check</param>
        /// <returns>corner state</returns>
        private static CornerState get_corner_state(Face side)
        {
            var count = 0;
            if (side.IsCorrect(0, 0))
                count++;

            if (side.IsCorrect(0, 2))
                count++;

            if (side.IsCorrect(2, 0))
                count++;

            if (side.IsCorrect(2, 2))
                count++;

            switch (count)
            {
                default:
                // ReSharper disable once RedundantCaseLabel
                case 0:
                    return CornerState.CornerStateOne;
                case 1:
                    return CornerState.CornerStateTwo;
                case 2:
                case 3:
                    return CornerState.CornerStateThree;
                case 4:
                    return CornerState.CornerStateFour;
            }
        }

        /// <summary>
        /// Get the edge pattern of the side
        /// </summary>
        /// <param name="side"></param>
        /// <returns>patter of the side</returns>
        private static string edge_pattern(Face side)
        {
            var s = side.ToString();
            var sb = new StringBuilder();
            var c = side.Color.ToString()[0];

            // Top row
            var x = '-';
            var y = s[1] == c ? 'X' : '-';
            var z = '-';
            sb.Append($"{x}{y}{z}");

            // Middle row
            x = s[3] == c ? 'X' : '-';
            y = s[4] == c ? 'X' : '-';
            z = s[5] == c ? 'X' : '-';
            sb.Append($"{x}{y}{z}");

            // Bottom row
            x = '-';
            y = s[7] == c ? 'X' : '-';
            z = '-';
            sb.Append($"{x}{y}{z}");
            return sb.ToString();
        }

        /// <summary>
        /// Get the cross state of the last layer
        /// </summary>
        /// <param name="side">Face to count</param>
        /// <returns>State of the side</returns>
        private static CrossState get_cross_state(Face side)
        {
            var sidepattern = edge_pattern(side);
            switch (sidepattern)
            {
                default:
                    return CrossState.CrossStateUnknown;

                //   | X |
                // X | X | X 
                //   | X |
                case "-X-XXX-X-":
                    return CrossState.CrossStateOne;

                //   |   |
                //   | X |   
                //   |   |
                case "----X----":
                    return CrossState.CrossStateTwo;

                //   | X |
                // X | X | 
                //   |   |
                case "-X-XX----":
                    return CrossState.CrossStateThreeA;
                case "-X--XX---":
                    return CrossState.CrossStateThreeB;
                case "----XX-X-":
                    return CrossState.CrossStateThreeC;
                case "---XX--X-":
                    return CrossState.CrossStateThreeD;

                //   |   |
                // X | X | X
                //   |   |
                case "---XXX---":
                    return CrossState.CrossStateFourA;
                case "-X--X--X-":
                    return CrossState.CrossStateFourB;
            }
        }

        /// <summary>
        /// Solve the edges with correct position of the last layer
        /// </summary>
        public void solve_position_edges()
        {
            var count = 0;

            do
            {
                // determine number in correct position
                // if its 4 then we are done
                // if its 1 then move it to the back
                // excute clockwise or counter clockwise sequence 
                var correctCount = 0;
                var clockwise = true;

                if (Cube.Back.IsCorrect(0, 1))
                    correctCount++;

                if (Cube.Left.IsCorrect(0, 1))
                    correctCount++;

                if (Cube.Front.IsCorrect(0, 1))
                    correctCount++;

                if (Cube.Right.IsCorrect(0, 1))
                    correctCount++;

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (correctCount == NumCubeSides)
                    return;

                if (correctCount == 1)
                {
                    var rotateCount = 0;

                    while (rotateCount++ < NumCubeSides && Cube.Back[0, 1] != Cube.Back.Color)
                        Cube.execute_sequence("cr");

                    if (Cube.Left[0, 1] == Cube.Front.Color)
                        clockwise = false;
                }

                Cube.execute_sequence(clockwise ? "f f u l ri f f li r u f f" : "f f ui l ri f f li r ui f f");
            } while (count++ < NumCubeSides);

            // Should NEVER get here
            throw new Exception("Cube can't be solved.");
        }

        /// <summary>
        /// Solve the corners with correct position of the last layer
        /// </summary>
        // ReSharper disable once UnusedParameter.Local
        public bool solve_position_corner()
        {
            const uint a = 1u;
            const uint b = 1u << 1;
            const uint c = 1u << 2;
            const uint d = 1u << 3;

            var count = 0;
            do
            {
                var loopCount = 0;
                do
                {
                    var correctCount = 0;

                    if (Cube.Back.IsCorrect(0, 0) && Cube.Right.IsCorrect(0, 2))
                        correctCount++;

                    if (Cube.Back.IsCorrect(0, 2) && Cube.Left.IsCorrect(0, 0))
                        correctCount++;

                    if (Cube.Front.IsCorrect(0, 0) && Cube.Left.IsCorrect(0, 2))
                        correctCount++;

                    if (Cube.Front.IsCorrect(0, 2) && Cube.Right.IsCorrect(0, 0))
                        correctCount++;

                    if (correctCount == NumCubeSides)
                        return true;

                    if (correctCount < 2)
                        Cube.execute_sequence("u");
                    else
                        break;

                } while (loopCount++ < NumCubeSides);

                var rotateCount = 0;
                do
                {
                    var flags = 0u;
                    if (Cube.Back.IsCorrect(0, 2) && Cube.Left.IsCorrect(0, 0))
                        flags = flags.Set(a);
                    if (Cube.Back.IsCorrect(0, 0) && Cube.Right.IsCorrect(0, 2))
                        flags = flags.Set(b);
                    if (Cube.Front.IsCorrect(0, 0) && Cube.Left.IsCorrect(0, 2))
                        flags = flags.Set(c);
                    if (Cube.Front.IsCorrect(0, 2) && Cube.Right.IsCorrect(0, 0))
                        flags = flags.Set(d);

                    if (
                        flags.IsSet(a | b) ||
                        flags.IsSet(a | d) || 
                        flags.IsSet(b | c)
                        )
                        break;

                    Cube.execute_sequence("cr");

                } while (rotateCount++ <= NumCubeSides);

                Cube.execute_sequence("ri f ri b b r fi ri b b r r ui");
            } while (count++ < NumCubeSides);

            throw new Exception("Cube can't be solved.");
        }

        /// <summary>
        /// Solve the corners of the last layer
        /// </summary>
        /// <param name="side">Color of layer to solve</param>
        /// <returns>True if positions are correct</returns>
        public void solve_corner(Face side)
        {
            var count = 0;
            do
            {
                var state = get_corner_state(side);
                int rotateCount;
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (state)
                {
                    case CornerState.CornerStateOne:
                        // rotate until the top right of the right side is the same color as the side color
                        rotateCount = 0;
                        while (rotateCount++ < NumCubeSides && !Cube.Left.IsCorrect(0, 2))
                            Cube.execute_sequence("cr");
                        break;

                    case CornerState.CornerStateTwo:
                        // rotate the face until it looks like this
                        //   | X |
                        // X | X | X
                        // X | X |
                        rotateCount = 0;
                        while (rotateCount++ < NumCubeSides && !side.IsCorrect(2, 0))
                            Cube.execute_sequence("cr");
                        break;

                    case CornerState.CornerStateThree:
                        // rotate until the left top of the front side is the same color as the side color
                        rotateCount = 0;
                        while (rotateCount++ < NumCubeSides && Cube.Front[0, 0] != side.Color)
                            Cube.execute_sequence("cr");
                        break;

                    case CornerState.CornerStateFour:
                        return;
                }

                Cube.execute_sequence("r u ri u r u u ri");
                count++;
            } while (count <= NumCubeSides);

            // Should NEVER get here
            throw new Exception("Cube can't be solved.");
        }

        /// <summary>
        /// Solve the cross of the last layer
        /// </summary>
        /// <param name="side"></param>
        public void solve_cross(Face side)
        {
            var count = 0;
            do
            {
                var state = get_cross_state(side);
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (state)
                {
                    case CrossState.CrossStateUnknown:
                        throw new Exception("Cube can't be solved.");

                    case CrossState.CrossStateOne:
                        return;

                    case CrossState.CrossStateTwo:
                        Cube.execute_sequence("f u r ui ri fi");
                        break;

                    case CrossState.CrossStateThreeA:
                        Cube.execute_sequence("f u r ui ri fi");
                        break;

                    case CrossState.CrossStateThreeB:
                        Cube.execute_sequence("cl f u r ui ri fi");
                        break;

                    case CrossState.CrossStateThreeC:
                        Cube.execute_sequence("cr cr f u r ui ri fi");
                        break;

                    case CrossState.CrossStateThreeD:
                        Cube.execute_sequence("cr f u r ui ri fi");
                        break;

                    case CrossState.CrossStateFourA:
                        Cube.execute_sequence("f r u ri ui fi");
                        break;

                    case CrossState.CrossStateFourB:
                        Cube.execute_sequence("cr f r u ri ui fi");
                        break;
                }
                count++;

                // prevent infinite loop
            } while (count < NumCubeSides);

            throw new Exception("Cube can't be solved.");
        }

        /// <summary>
        /// Solve the up corners of the cube
        /// </summary>
        public void solve_up_corners()
        {
            for (var count = 0; count < NumCubeSides; count++)
            {
                position_up_left_front_corner(Cube.Up.Color, Cube.Left.Color, Cube.Front.Color);
                if (count < NumCubeSides - 1)
                    Cube.execute_sequence("cr");
            }
        }

        /// <summary>
        /// Solve the middle layer of the cube
        /// </summary>
        public void solve_middle_layer()
        {
            for (var count = 0; count < NumCubeSides; count++)
            {
                position_middle_front_right_edge(Cube.Right.Color);
                if (count < NumCubeSides - 1)
                    Cube.execute_sequence("cr");
            }
        }

        /// <summary>
        /// See if the cube can be solved with only a few moves
        /// </summary>
        public void solve_quick_solve()
        {
            const int maxSolveLevel = 3;

            var moves = new List<string>();
            var solution = "";

            solve_quick_solve_recursion(Cube, maxSolveLevel, 0, moves, ref solution);
            Cube.execute_sequence(solution);
        }

        /// <summary>
        /// See if the cube can be solved with only a few moves by brute force
        /// </summary>
        /// <param name="cube">Cube Instance</param>
        /// <param name="maxLevel">Max level of recursion</param>
        /// <param name="level">Currenct level of recursion</param>
        /// <param name="moves">Current moves</param>
        /// <param name="solution">Best solve sequence</param>
        /// <returns>True if solved</returns>
        private static bool solve_quick_solve_recursion(ICube cube, int maxLevel, int level, ICollection<string> moves,
            ref string solution)
        {
            if (cube.issolved())
                return true;

            if (level++ >= maxLevel)
                return false;

            var temp = "";

            foreach (var m1 in PossibleMoves)
            {
                moves.Add(m1);

                var clone = cube.clone();
                clone.execute_sequence(m1);
                var result = solve_quick_solve_recursion(clone, maxLevel, level, moves, ref solution);
                if (result)
                {
                    temp = moves.Aggregate(temp, (current, m) => current + (m + " "));

                    var buffer = clone.optimize_sequence(temp);
                    if (!string.IsNullOrEmpty(buffer))
                        solution = buffer;

                    moves.Remove(m1);
                    break;
                }

                moves.Remove(m1);
            }

            return false;
        }

        /// <summary>
        /// Solve the up cross
        /// </summary>
        /// <returns>True if the Cube is valid</returns>
        public void solve_up_cross()
        {
            position_up_edge(Cube.Left.Color);
            position_up_edge(Cube.Front.Color);
            position_up_edge(Cube.Right.Color);
            position_up_edge(Cube.Back.Color);
        }

        /// <summary>
        /// Solve the cube
        /// </summary>
        /// <returns>Solve sequence</returns>
        public string solve()
        {
            solve_quick_solve();
            if (Cube.issolved()) return Cube.OptimizedMoves;

            solve_up_cross();
            solve_up_corners();
            solve_middle_layer();
 
            // flip the cube upside down
            Cube.set_up_face(Cube.Down.Color);

            solve_cross(Cube.Up);
            solve_corner(Cube.Up);
            solve_position_corner();
            solve_position_edges();

            return Cube.OptimizedMoves;
        }
    }
}
