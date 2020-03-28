using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Input;
using System.Windows.Media.Media3D;

// ReSharper disable once CheckNamespace
namespace WpfApplication1.Tests
{
    [TestClass]
    public class SimpleSolverTests
    {
        public const string Startcube = " 1  2  3  4  5  6  7  8  9 " +
                                        "10 11 12 13 14 15 16 17 18 " +
                                        "19 20 21 22 23 24 25 26 27 " +
                                        "28 29 30 31 32 33 34 35 36 " +
                                        "37 38 39 40 41 42 43 44 45 " +
                                        "46 47 48 49 50 51 52 53 54";
        public ICube Rc { get; set; }
        public SimpleSolver Solver { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            if (Rc == null) Rc = new Cube();
            if (Solver == null) Solver = new SimpleSolver(Rc);
        }

        [TestMethod, TestCategory("Solver"), Owner("Paul Baxter"), Priority(3)]
        public void SimpleSolverTest()
        {
            Assert.IsNotNull(Rc, "Cube not instantiated.");
            Assert.IsNotNull(Solver, "Solver not instantiated.");
        }

        [TestMethod, TestCategory("Solver"), Owner("Paul Baxter"), Priority(3)]
        public void solve_position_edgesTest()
        {
            var tests = new[]
            {
                "O O O O O O O O O Y G Y Y Y Y Y Y Y G B G G G G G G G W W W W W W W W W B Y B B B B B B B R R R R R R R R R",
                "B B B B B B B B B O Y O O O O O O O W R W W W W W W W R W R R R R R R R Y O Y Y Y Y Y Y Y G G G G G G G G G"
            };

            var negativetests = new[]
            {
                "B B B B B B B B B G G O O O O O O O W R W W W W W W W R W R R R R R R R Y O Y Y Y Y Y Y Y G G G G G G G G G"
            };

            foreach (var test in tests)
            {
                Rc.set_cube(test);
                Solver.solve_position_edges();
                Assert.IsTrue(Rc.issolved(), "solve_position_edgesTest FAILED");
            }
            foreach (var test in negativetests)
            {
                Rc.set_cube(test);
                try
                {
                    Solver.solve_position_edges();
                    Assert.IsFalse(validate_up_corner_positions(), "solve_position_edgesTest FAILED");
                }
                catch
                {
                    Assert.IsFalse(validate_up_corner_positions(), "solve_position_edgesTest FAILED");
                }
            }
        }

        [TestMethod, TestCategory("Solver"), Owner("Paul Baxter"), Priority(3)]
        public void solve_position_cornerTest()
        {
            var tests = new[]
            {
                "B B B B B B B B B R O R Y Y Y Y Y Y Y R O O O O O O O W W Y W W W W W W O Y W R R R R R R G G G G G G G G G",
                "R R R R R R R R R G B Y B B B B B B B W B W W W W W W W G G G G G G G G Y Y W Y Y Y Y Y Y O O O O O O O O O",
                "B B B B B B B B B Y R Y O O O O O O O Y W W W W W W W R O O R R R R R R W W R Y Y Y Y Y Y G G G G G G G G G"
            };

            var negativetests = new[]
            {
                "B B B B B B B B B Y R Y O O O O O O O Y W W W W W W W R O R R R R R R R W W R Y Y Y Y Y Y G G G G G G G G G"
            };
            foreach (var test in tests)
            {
                Rc.set_cube(test);
                Solver.solve_position_corner();
                Assert.IsTrue(validate_up_corner_positions(), "solve_position_cornerTest FAILED");
            }
            foreach (var test in negativetests)
            {
                Rc.set_cube(test);
                try
                {
                    Solver.solve_position_corner();
                    Assert.IsFalse(validate_up_corner_positions(), "solve_position_cornerTest FAILED");
                }
                catch
                {
                    Assert.IsFalse(validate_up_corner_positions(), "solve_position_cornerTest FAILED");
                }
            }
        }

        [TestMethod, TestCategory("Solver"), Owner("Paul Baxter"), Priority(3)]
        public void solve_cornerTest()
        {
            var tests = new[]
            {
                "G R R R R R G R R W G Y G G G G G G R B B Y Y Y Y Y Y W Y Y B B B B B B B W R W W W W W W O O O O O O O O O",
                "Y O G O O O O O W B G W B B B B B B B B O Y Y Y Y Y Y G Y O G G G G G G Y W O W W W W W W R R R R R R R R R",
                "B Y G Y Y Y R Y R O B G R R R R R R Y G Y G G G G G G B R O O O O O O O Y O Y B B B B B B W W W W W W W W W"
            };

            foreach (var test in tests)
            {
                Rc.set_cube(test);
                Solver.solve_corner(Rc.Up);
                Assert.IsTrue(validate_corners(), "solve_cornerTest FAILED");
            }
        }

        [TestMethod, TestCategory("Solver"), Owner("Paul Baxter"), Priority(3)]
        public void solve_crossTest()
        {
            var tests = new[]
            {
                "O O G W W W W B R B R R R R R R R R B W W B B B B B B G G W O O O O O O O W W G G G G G G Y Y Y Y Y Y Y Y Y",
                "W W W W W W W W W R G R B B B B B B B O B O O O O O O O B O G G G G G G G R G R R R R R R Y Y Y Y Y Y Y Y Y",
                "Y O W O O G W B Y B W O W W W W W W G O G B B B B B B O O B Y Y Y Y Y Y O Y O G G G G G G R R R R R R R R R",
                "Y Y O O Y G Y Y O G Y B B B B B B B R R B R R R R R R Y Y Y G G G G G G G B R O O O O O O W W W W W W W W W"
            };

            foreach (var test in tests)
            {
                Rc.set_cube(test);
                Solver.solve_cross(Rc.Up);
                Assert.IsTrue(validate_cross(), "solve_crossTest FAILED");
            }
        }

        [TestMethod, TestCategory("Solver"), Owner("Paul Baxter"), Priority(3)]
        public void solve_up_cornersTest()
        {
            var tests = new[]
            {
                "Y R W R R R O R O G B Y Y B W W Y R G W B B W O B Y W Y G B G G W G G B O Y R O Y G R O G Y B O O O W R B W",
                "Y R G R R R O R W B B G O B W B Y G W W B B W B W O R O G O O G Y B Y Y Y Y O O Y W G G W R G Y G O B R W R",
                "O B B B B B O B G G W G G W O R G W W R Y G R R O Y R R Y Y W Y O G R B O O Y W O R R W Y B O W Y G Y B G W",
            };

            foreach (var test in tests)
            {
                Rc.set_cube(test);
                Solver.solve_up_corners();
                Assert.IsTrue(validate_up_corners(), "solve_up_corners FAILED");
            }
        }

        [TestMethod, TestCategory("Solver"), Owner("Paul Baxter"), Priority(3)]
        public void solve_middle_layerTest()
        {
            var tests = new[]
            {
                "Y Y Y Y Y Y Y Y Y B B B G B W W B G R R R O R O W W O G G G G G R W W O O O O B O R B G R R B G O W R B W W",
                "G G G G G G G G G R R R W R R Y O Y W W W B W W B Y R O O O B O B W W W Y Y Y O Y R O B B O R B Y B O R Y B",
                "Y Y Y Y Y Y Y Y Y G G G R G O B W W O O O W O B R G W B B B R B B B O W R R R W R G G R O G O R G W B W W O"
            };

            foreach (var test in tests)
            {
                Rc.set_cube(test);
                Solver.solve_middle_layer();
                Assert.IsTrue(validate_middle_layer(), "solve_middle_layerTest FAILED");
            }
        }

        [TestMethod, TestCategory("Solver"), Owner("Paul Baxter"), Priority(3)]
        public void solve_quick_solveTest()
        {
            var tests = new[]
            {
                "O W W O W W Y G G G G R G G R O O R G W W G R R G R R O B B W B B W R R O O Y O O Y B B B W B B Y Y Y Y Y Y",
                "W R W W R W O G G G G Y G G B O O Y G R R Y Y Y G R R W B B G B B W R R O W O O W O B W B R B B Y O Y Y O Y",
                "W O W G Y G W O W G Y G O R O G Y G R W R G G G R W R B W B R O R B W B O Y O B B B O Y O Y R Y B W B Y R Y"
            };

            foreach (var test in tests)
            {
                Rc.set_cube(test);
                Solver.solve_quick_solve();
                Assert.IsTrue(Rc.issolved(), "solve_quick_solveTest FAILED");
            }
        }

        [TestMethod, TestCategory("Solver"), Owner("Paul Baxter"), Priority(3)]
        public void solve_up_crossTest()
        {
            var tests = new[]
            {
                "O G G B O G R Y W G W Y W W G W B B B O O O B R W B O G R R W Y G Y Y Y W Y Y W G O G B R O Y B R R R B O R",
                "G G R G O R O B B O R W O Y Y Y G W B W Y R G O R O R O W Y Y W W W Y G B W W O B G Y B R G B B Y R B G R O",
                "G W O G R Y B O B O R R W G R Y W R Y B W B Y O G O W R R B G B O O Y O Y R W W W G G Y R W Y B B O B G G Y"
            };

            foreach (var test in tests)
            {
                Rc.set_cube(test);
                Solver.solve_up_cross();
                Assert.IsTrue(validate_up_cross(), "solve_up_crossTest FAILED");
            }
        }

        [TestMethod, TestCategory("Solver"), Owner("Paul Baxter"), Priority(3)]
        public void solveTest()
        {
            var tests = new[]
            {
                "R B O B Y O Y W O B O G R B O R G O O O G Y R R Y Y W W G W Y G Y B B G B W Y B O G R R W B G R W W R G W Y",
                "R O B W O B W W R W O G B W R O B O R B G G B O B Y O Y Y R G Y G W G B Y Y B W G R O R Y W R G O R Y G W Y",
                "R R Y W O G G Y R W G Y Y Y R O W O R O W B G W Y O G G Y G O W Y O R Y O G B B B R R G W B B W B R W B O B",
                "W G Y R O W B R R R G W R G Y O R G R B G B W G Y O O Y B O Y B O W O Y B W G G Y Y R B W O Y G W R W B O B",
                "R G R B B Y W Y B Y Y O W R W R O R G G Y O Y G W W O O O Y O O B B B Y B R G W W R O Y W B G W B G R G R G",
                "Y W W G B O Y O G O O O R W R B R Y B Y R G R Y R R Y W B O B Y O G G B G B G W O Y R G O B W R B G Y W W W",
                "W W W W W W W W W G G G G G G G G G R R R R R R R R R B B B B B B B B B O O O O O O O O O Y Y Y Y Y Y Y Y Y",
                "G G O B Y W W O B R Y R W R W G R B B Y O G G W Y R B W B Y O O G O B W G Y Y O B R R B W R G Y Y W O O R G",
                "O W G R G G G R O Y B O Y O O R W W Y Y W W Y Y G Y B B O Y B R B Y R R R G B O W O W R W O G R B B G G W B",
                "Y W O R R O G W G R G W O B Y R G B O O O R W Y O R B Y G W B G W R O W B B B R Y Y R G G Y B W W O B Y Y G",
                "R B R R O Y B G G G W R O G B B Y Y Y O O R W Y B R B Y B G O B G W W G Y O W R Y W O W W O Y O G R G R B W",
                "B G O B B W W R G Y W G O Y W Y B B R G Y R O G W G O R O G W W B G R W W Y R R R B B Y O O O Y Y G Y B O R",
                "Y Y G B W G Y O R B O G Y G R R O G O G Y Y R Y O G W B R R B B B O W R W O O W O G Y B B W W B W Y R W R G",
            };

            var negativeTests = new[]
            {
                "B B O B Y O Y W O B O G R B O R G O O O G Y R R Y Y W W G W Y G Y B B G B W Y B O G R R W B G R W W R G W Y",
                "R B R R O Y B G G G W R G G B B Y Y Y Y O R W Y B R B Y B G O B G W W G Y O W R Y W O W W O Y O G R G R B W",
                "W W W W W W W W W G G G G G G G G G R R R R R R R R R B B B B B B B B B O O O O O O O O O Y Y Y Y Y N N N Y",
            };

            foreach (var test in tests)
            {
                Rc.set_cube(test);
                Solver.solve();
                Assert.IsTrue(Rc.issolved(), "solveTest FAILED");
            }

            foreach (var test in negativeTests)
            {
                Rc.set_cube(test);
                try
                {
                    Solver.solve();
                }
                catch (Exception)
                {
                    Assert.IsFalse(Rc.issolved(), "solveTest FAILED");
                }
                Assert.IsFalse(Rc.issolved(), "solveTest FAILED");
            }

        }

        /// <summary>
        /// Validate the up corners after the cube has been flipped
        /// </summary>
        /// <returns>True if the up corners are valid</returns>
        private bool validate_corners()
        {
            return
                Rc.Down.Issolved() &&
                Rc.Left[2, 0] == Rc.Left.Color &&
                Rc.Left[2, 1] == Rc.Left.Color &&
                Rc.Left[2, 2] == Rc.Left.Color &&
                Rc.Front[2, 0] == Rc.Front.Color &&
                Rc.Front[2, 1] == Rc.Front.Color &&
                Rc.Front[2, 2] == Rc.Front.Color &&
                Rc.Right[2, 0] == Rc.Right.Color &&
                Rc.Right[2, 1] == Rc.Right.Color &&
                Rc.Right[2, 2] == Rc.Right.Color &&
                Rc.Back[2, 0] == Rc.Back.Color &&
                Rc.Back[2, 1] == Rc.Back.Color &&
                Rc.Back[2, 2] == Rc.Back.Color &&
                Rc.Left[1, 0] == Rc.Left.Color &&
                Rc.Left[1, 2] == Rc.Left.Color &&
                Rc.Front[1, 0] == Rc.Front.Color &&
                Rc.Front[1, 2] == Rc.Front.Color &&
                Rc.Right[1, 0] == Rc.Right.Color &&
                Rc.Right[1, 2] == Rc.Right.Color &&
                Rc.Back[1, 0] == Rc.Back.Color &&
                Rc.Back[1, 2] == Rc.Back.Color &&
                Rc.Up.Issolved();
        }

        /// <summary>
        /// Validate the up corners correctly positioned after the cube has been flipped
        /// </summary>
        /// <returns>True if the up corners correctly positioned</returns>
        private bool validate_up_corner_positions()
        {
            return
                Rc.Down.Issolved() &&
                Rc.Left[2, 0] == Rc.Left.Color &&
                Rc.Left[2, 1] == Rc.Left.Color &&
                Rc.Left[2, 2] == Rc.Left.Color &&
                Rc.Front[2, 0] == Rc.Front.Color &&
                Rc.Front[2, 1] == Rc.Front.Color &&
                Rc.Front[2, 2] == Rc.Front.Color &&
                Rc.Right[2, 0] == Rc.Right.Color &&
                Rc.Right[2, 1] == Rc.Right.Color &&
                Rc.Right[2, 2] == Rc.Right.Color &&
                Rc.Back[2, 0] == Rc.Back.Color &&
                Rc.Back[2, 1] == Rc.Back.Color &&
                Rc.Back[2, 2] == Rc.Back.Color &&
                Rc.Left[1, 0] == Rc.Left.Color &&
                Rc.Left[1, 2] == Rc.Left.Color &&
                Rc.Front[1, 0] == Rc.Front.Color &&
                Rc.Front[1, 2] == Rc.Front.Color &&
                Rc.Right[1, 0] == Rc.Right.Color &&
                Rc.Right[1, 2] == Rc.Right.Color &&
                Rc.Back[1, 0] == Rc.Back.Color &&
                Rc.Back[1, 2] == Rc.Back.Color &&
                Rc.Up.Issolved() &&
                Rc.Left[0, 0] == Rc.Left.Color &&
                Rc.Left[0, 2] == Rc.Left.Color &&
                Rc.Front[0, 0] == Rc.Front.Color &&
                Rc.Front[0, 2] == Rc.Front.Color &&
                Rc.Right[0, 0] == Rc.Right.Color &&
                Rc.Right[0, 2] == Rc.Right.Color &&
                Rc.Back[0, 0] == Rc.Back.Color &&
                Rc.Back[0, 2] == Rc.Back.Color;
        }

        /// <summary>
        /// Validate the up cross after cube is flipped
        /// </summary>
        /// <returns>True is the up cross is valid</returns>
        private bool validate_cross()
        {
            return
                Rc.Down.Issolved() &&
                Rc.Left[2, 0] == Rc.Left.Color &&
                Rc.Left[2, 1] == Rc.Left.Color &&
                Rc.Left[2, 2] == Rc.Left.Color &&
                Rc.Front[2, 0] == Rc.Front.Color &&
                Rc.Front[2, 1] == Rc.Front.Color &&
                Rc.Front[2, 2] == Rc.Front.Color &&
                Rc.Right[2, 0] == Rc.Right.Color &&
                Rc.Right[2, 1] == Rc.Right.Color &&
                Rc.Right[2, 2] == Rc.Right.Color &&
                Rc.Back[2, 0] == Rc.Back.Color &&
                Rc.Back[2, 1] == Rc.Back.Color &&
                Rc.Back[2, 2] == Rc.Back.Color &&
                Rc.Left[1, 0] == Rc.Left.Color &&
                Rc.Left[1, 2] == Rc.Left.Color &&
                Rc.Front[1, 0] == Rc.Front.Color &&
                Rc.Front[1, 2] == Rc.Front.Color &&
                Rc.Right[1, 0] == Rc.Right.Color &&
                Rc.Right[1, 2] == Rc.Right.Color &&
                Rc.Back[1, 0] == Rc.Back.Color &&
                Rc.Back[1, 2] == Rc.Back.Color &&
                Rc.Up[0, 1] == Rc.Up.Color &&
                Rc.Up[1, 0] == Rc.Up.Color &&
                Rc.Up[1, 2] == Rc.Up.Color &&
                Rc.Up[2, 1] == Rc.Up.Color;
        }

        /// <summary>
        /// Validate that the top layer is solved
        /// </summary>
        /// <returns>True if the top layer is solved</returns>
        private bool validate_up_corners()
        {
            return
                Rc.Up.Issolved() &&
                Rc.Left[0, 0] == Rc.Left.Color &&
                Rc.Left[0, 1] == Rc.Left.Color &&
                Rc.Left[0, 2] == Rc.Left.Color &&
                Rc.Front[0, 0] == Rc.Front.Color &&
                Rc.Front[0, 1] == Rc.Front.Color &&
                Rc.Front[0, 2] == Rc.Front.Color &&
                Rc.Right[0, 0] == Rc.Right.Color &&
                Rc.Right[0, 1] == Rc.Right.Color &&
                Rc.Right[0, 2] == Rc.Right.Color &&
                Rc.Back[0, 0] == Rc.Back.Color &&
                Rc.Back[0, 1] == Rc.Back.Color &&
                Rc.Back[0, 2] == Rc.Back.Color;
        }

        /// <summary>
        /// Validate the up layer and middle layer are solved 
        /// </summary>
        /// <returns>True if up layer and middle layer are solved</returns>
        private bool validate_middle_layer()
        {
            return
                Rc.Up.Issolved() &&
                Rc.Left[0, 0] == Rc.Left.Color &&
                Rc.Left[0, 1] == Rc.Left.Color &&
                Rc.Left[0, 2] == Rc.Left.Color &&
                Rc.Front[0, 0] == Rc.Front.Color &&
                Rc.Front[0, 1] == Rc.Front.Color &&
                Rc.Front[0, 2] == Rc.Front.Color &&
                Rc.Right[0, 0] == Rc.Right.Color &&
                Rc.Right[0, 1] == Rc.Right.Color &&
                Rc.Right[0, 2] == Rc.Right.Color &&
                Rc.Back[0, 0] == Rc.Back.Color &&
                Rc.Back[0, 1] == Rc.Back.Color &&
                Rc.Back[0, 2] == Rc.Back.Color &&
                Rc.Left[1, 0] == Rc.Left.Color &&
                Rc.Left[1, 2] == Rc.Left.Color &&
                Rc.Front[1, 0] == Rc.Front.Color &&
                Rc.Front[1, 2] == Rc.Front.Color &&
                Rc.Right[1, 0] == Rc.Right.Color &&
                Rc.Right[1, 2] == Rc.Right.Color &&
                Rc.Back[1, 0] == Rc.Back.Color &&
                Rc.Back[1, 2] == Rc.Back.Color;
        }

        /// <summary>
        /// Validate the Up Cross is solved
        /// </summary>
        /// <returns>True if the Up Cross is solved</returns>
        private bool validate_up_cross()
        {
            return
                (Rc.Up[0, 1] == Rc.Up.Color) &&
                (Rc.Up[1, 0] == Rc.Up.Color) &&
                (Rc.Up[1, 2] == Rc.Up.Color) &&
                (Rc.Up[1, 2] == Rc.Up.Color) &&
                (Rc.Up[2, 1] == Rc.Up.Color) &&
                (Rc.Back[0, 1] == Rc.Back.Color) &&
                (Rc.Left[0, 1] == Rc.Left.Color) &&
                (Rc.Right[0, 1] == Rc.Right.Color) &&
                (Rc.Front[0, 1] == Rc.Front.Color);
        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void EqualsTest()
        {
            var a = new FaceVal(20);
            object b = a;
            var result = a.Equals(b);
            Assert.IsTrue(result, "EqualsTest FAIL");

            object c = new FaceVal(20);
            result = a.Equals(c);
            Assert.IsTrue(result, "EqualsTest FAIL");

            result = a.Equals((object)null);
            Assert.IsFalse(result, "EqualsTest FAIL");


        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void FaceValTest()
        {
            var a = new FaceVal(20);
            var result = a.Val == 20;
            Assert.IsTrue(result, "FaceValTest FAIL");

            var b = (FaceVal)22u;
            result = b.Val == 22;
            Assert.IsTrue(result, "FaceValTest FAIL");

            var c = (FaceVal)'A';
            result = c.Val == 'A';
            Assert.IsTrue(result, "FaceValTest FAIL");

            var d = new FaceVal(null);
            result = d.Val == 0;
            Assert.IsTrue(result, "FaceValTest FAIL");

            var e = new FaceVal();
            result = e.Val == 0;
            Assert.IsTrue(result, "FaceValTest FAIL");


        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void EqualsTest1()
        {
            var a = new FaceVal(20);
            var b = new FaceVal(20);
            var result = a.Equals(b);
            Assert.IsTrue(result, "EqualsTest1 FAIL");
        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void GetHashCodeTest()
        {
            var a = new FaceVal(20);
            var b = new FaceVal(20);
            var c = new FaceVal(90);
            var hash1 = a.GetHashCode();
            var hash2 = b.GetHashCode();
            var hash3 = c.GetHashCode();

            Assert.IsTrue(hash1 == hash2, "GetHashCodeTest1 FAIL");
            Assert.IsTrue(hash1 != hash3, "GetHashCodeTest2 FAIL");
        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void EqualsTest2()
        {
            var a = new FaceVal(20);
            var result = a.Equals(20);
            Assert.IsTrue(result, "EqualsTest2 FAIL");
        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void CompareToTest()
        {
            var a = new FaceVal(20);
            var result1 = a.CompareTo(10u);
            var result2 = a.CompareTo(20u);
            var result3 = a.CompareTo(30u);
            Assert.IsTrue(result1 > 0, "CompareToTest FAIL");
            Assert.IsTrue(result2 == 0, "CompareToTest FAIL");
            Assert.IsTrue(result3 < 0, "CompareToTest FAIL");
        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void CompareToTest1()
        {
            var a = new FaceVal(20);
            var result1 = a.CompareTo(10);
            var result2 = a.CompareTo(20);
            var result3 = a.CompareTo(30);
            Assert.IsTrue(result1 > 0, "CompareToTest FAIL");
            Assert.IsTrue(result2 == 0, "CompareToTest FAIL");
            Assert.IsTrue(result3 < 0, "CompareToTest FAIL");
        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void EqualsTest3()
        {
            var a = new FaceVal(20);
            var result = a.Equals(20);
            Assert.IsTrue(result, "EqualsTest3 FAIL");
        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void CompareToTest2()
        {
            var a = new FaceVal('A');
            var b = new FaceVal('B');
            var c = new FaceVal('C');

            var result = b.CompareTo(a) > 0;
            Assert.IsTrue(result, "EqualsTest4 FAIL");
            result = b.CompareTo(b) == 0;
            Assert.IsTrue(result, "EqualsTest4 FAIL");
            result = b.CompareTo(c) < 0;
            Assert.IsTrue(result, "EqualsTest4 FAIL");
        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void EqualsTest4()
        {
            var a = new FaceVal('B');
            var result = a.Equals('B');
            Assert.IsTrue(result, "EqualsTest4 FAIL");
        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void EqualsTest5()
        {
            var a = new FaceVal(22);
            var result = a.Equals(22u);
            Assert.IsTrue(result, "EqualsTest5 FAIL");
        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void CompareToTest3()
        {
            var a = new FaceVal('B');
            var result = a.CompareTo('B') == 0;
            Assert.IsTrue(result, "CompareToTest3 FAIL");

            result = a.CompareTo('A') > 0;
            Assert.IsTrue(result, "CompareToTest3 FAIL");

            result = a.CompareTo('C') < 0;
            Assert.IsTrue(result, "CompareToTest3 FAIL");

        }

        [TestMethod, TestCategory("FaceVal"), Owner("Paul Baxter"), Priority(3)]
        public void ToStringTest()
        {
            var a = new FaceVal('A');
            Assert.IsTrue(a.ToString() == "A");
        }

        [TestMethod, TestCategory("ExtensionMethods"), Owner("Paul Baxter"), Priority(3)]
        public void IsExecutingTest()
        {
            RoutedUICommand testCommand = new RoutedUICommand
            (
                "test",
                nameof(testCommand),
                typeof(MainWindow)
            );

            var result0 = testCommand.IsExecuting();
            testCommand.SetIsRunning(true);
            var result1 = testCommand.IsExecuting();
            testCommand.SetIsRunning(false);
            var result2 = testCommand.IsExecuting();

            Assert.IsFalse(result0, "IsExecutingTest FAIL");
            Assert.IsTrue(result1, "IsExecutingTest FAIL");
            Assert.IsFalse(result2, "IsExecutingTest FAIL");
        }

        [TestMethod, TestCategory("ExtensionMethods"), Owner("Paul Baxter"), Priority(3)]
        public void SetIsRunningTest()
        {
            RoutedUICommand testCommand = new RoutedUICommand
            (
                "test",
                nameof(testCommand),
                typeof(MainWindow)
            );

            testCommand.SetIsRunning(true);
            var result1 = testCommand.IsExecuting();
            testCommand.SetIsRunning(false);
            var result2 = testCommand.IsExecuting();

            Assert.IsTrue(result1, "SetIsRunningTest FAIL");
            Assert.IsFalse(result2, "SetIsRunningTest FAIL");
        }

        [TestMethod, TestCategory("ExtensionMethods"), Owner("Paul Baxter"), Priority(3)]
        public void ToPoint3DTest()
        {
            var sphere = new Tuple<double, double, double>(7.0710678118655, 0.92729521800161, 0.78539816339745);
            var pt3d = sphere.ToPoint3D();
            var sphere2 = pt3d.ToSphere();

            var temp1 = Math.Abs(pt3d.X - 3) +
                        Math.Abs(pt3d.Y - 4) +
                        Math.Abs(pt3d.Z - 5);

            var temp2 = Math.Abs(sphere.Item1 - sphere2.Item1) +
                        Math.Abs(sphere.Item2 - sphere2.Item2) +
                        Math.Abs(sphere.Item3 - sphere2.Item3);


            Assert.IsTrue(temp1 < 5, $"ToPoint3DTest A FAIL [{pt3d.X}, {pt3d.Y}, {pt3d.Z}] {temp1}");
            Assert.IsTrue(temp2 < 1E-10, $"ToPoint3DTest B FAIL {temp2}");
        }

        [TestMethod, TestCategory("ExtensionMethods"), Owner("Paul Baxter"), Priority(3)]
        public void ToSphereTest()
        {
            var pt3d = new Point3D(3, 4, 5);
            var sphere = pt3d.ToSphere();
            var pt23d = sphere.ToPoint3D();

            var temp1 = Math.Abs(sphere.Item1 - 7.0710678118655) +
                        Math.Abs(sphere.Item2 - 0.92729521800161) +
                        Math.Abs(sphere.Item3 - 0.78539816339745);

            var temp2 = Math.Abs(pt23d.X - pt3d.X) +
                       Math.Abs(pt23d.Y - pt3d.Y) +
                       Math.Abs(pt23d.Z - pt3d.Z);

            var temp3 = new Point3D(0, 0, 0);
            // ReSharper disable once UnusedVariable
            var sphere2 = temp3.ToSphere();

            Assert.IsTrue(temp1 < 1, "ToSphereTest A FAIL");
            Assert.IsTrue(temp2 < 1E-10, "ToSphereTest B FAIL");
        }

        [TestMethod, TestCategory("ExtensionMethods"), Owner("Paul Baxter"), Priority(3)]
        public void EnqueTopTest()
        {
            var queue = new Queue<string>();
            queue.Enqueue("1");
            queue.Enqueue("2");
            queue.Enqueue("3");
            queue.Enqueue("4");
            queue.Enqueue("5");

            queue.EnqueTop("0");

            var strs = queue.ToArray();
            for (var i = 0; i < queue.Count; i++)
            {
                Assert.IsTrue(strs[i] == i.ToString(), "EnqueTopTest FAIL");
            }
        }

        [TestMethod, TestCategory("ExtensionMethods"), Owner("Paul Baxter"), Priority(3)]
        public void IsSetTest()
        {
            for (var mask = 1u; mask > 0; mask *= 2)
            {
                Assert.IsTrue((~0u).IsSet(mask), "IsSetTest FAIL");
                Assert.IsFalse((0u).IsSet(mask), "IsSetTest FAIL");
            }
        }

        [TestMethod, TestCategory("ExtensionMethods"), Owner("Paul Baxter"), Priority(3)]
        public void SetTest()
        {
            for (var mask = 1u; mask > 0; mask *= 2)
            {
                var flags = 0u;
                flags = flags.Set(mask);
                Assert.IsTrue(flags.IsSet(  mask), "IsSetTest FAIL");
            }
        }

        [TestMethod, TestCategory("ExtensionMethods"), Owner("Paul Baxter"), Priority(3)]
        public void IsCorrectTest()
        {
            var face = new Face
            {
                [0, 0] = '1',
                [0, 1] = '1',
                [0, 2] = '1',
                [1, 0] = '1',
                [1, 1] = '1',
                [1, 2] = '1',
                [2, 0] = '1',
                [2, 1] = '1',
                [2, 2] = '1'
            };

            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < 3; col++)
                {
                    Assert.IsTrue(face.IsCorrect(row, col), "IsCorrectTest FAIL");
                }
            }

            face[1, 1] = '0';

            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < 3; col++)
                {
                    if (row == 1 && col == 1) continue;
                    Assert.IsTrue(!face.IsCorrect(row, col), "IsCorrectTest FAIL");
                }
            }

        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void CubeTest()
        {
            var cube = new Cube();
            Assert.IsTrue(cube.issolved(), "CubeTest FAIL");
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void init_cubeTest()
        {
            var cube = new Cube();
            cube.scramble_cube();
            Assert.IsFalse(cube.issolved(), "CubeTest FAIL");
            cube.init_cube();
            Assert.IsTrue(cube.issolved(), "CubeTest FAIL");
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void set_cubeTest()
        {
            var cube = new Cube();
            cube.scramble_cube();
            cube.set_cube(
                "R R R R R R R R R " +
                "G G G G G G G G G " +
                "B B B B B B B B B " +
                "A A A A A A A A A " +
                "Y Y Y Y Y Y Y Y Y " +
                "O O O O O O O O O "
            );
            Assert.IsTrue(cube.issolved(), "set_cubeTest FAIL");
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void cloneTest()
        {
            var cube = new Cube();
            cube.scramble_cube();
            var cube2 = cube.clone();

            var a = cube.ToString();
            var b = cube2.ToString();
            Assert.IsTrue(a.Equals(b), "cloneTest FAIL");
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void issolvedTest()
        {

            var cube = new Cube();
            var result1 = cube.issolved();
            cube.scramble_cube();
            var result2 = cube.issolved();
            Assert.IsTrue(result1, "issolvedTest FAIL");
            Assert.IsFalse(result2, "issolvedTest FAIL");
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void testRotation(ICube cube, string expected, Action action, [CallerMemberName]  string testName = null)
        {
            cube.set_cube(Startcube);
            action();

            var str = cube.ToString();
            var cubestrs = str.Split(new [] { ' ', '\t', '\f', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            var expectedstrs = expected.Split(new[] { ' ', '\t', '\f', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < 54; i++)
            {
                Assert.IsTrue(cubestrs[i] == expectedstrs[i], $"{testName} index {i}  '{cubestrs[i]}'  '{expectedstrs[i]}'  FAIL");
            }
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void fTest()
        {
            var cube = new Cube();
            const string expected =
                "1 2 3 4 5 6 18 15 12 10 11 46 13 14 47 16 17 48 25 22 19 26 23 20 27 24 21 7 29 30 8 32 33 9 35 36 37 38 39 40 41 42 43 44 45 34 31 28 49 50 51 52 53 54";
            testRotation(cube, expected, () => cube.f());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void fiTest()
        {
            var cube = new Cube();
            const string expected =
                "1 2 3 4 5 6 28 31 34 10 11 9 13 14 8 16 17 7 21 24 27 20 23 26 19 22 25 48 29 30 47 32 33 46 35 36 37 38 39 40 41 42 43 44 45 12 15 18 49 50 51 52 53 54";
            testRotation(cube, expected, () => cube.fi());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void uTest()
        {
            var cube = new Cube();
            const string expected =
                "7 4 1 8 5 2 9 6 3 19 20 21 13 14 15 16 17 18 28 29 30 22 23 24 25 26 27 37 38 39 31 32 33 34 35 36 10 11 12 40 41 42 43 44 45 46 47 48 49 50 51 52 53 54";
            testRotation(cube, expected, () => cube.u());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void uiTest()
        {
            var cube = new Cube();
            const string expected =
                "3 6 9 2 5 8 1 4 7 37 38 39 13 14 15 16 17 18 10 11 12 22 23 24 25 26 27 19 20 21 31 32 33 34 35 36 28 29 30 40 41 42 43 44 45 46 47 48 49 50 51 52 53 54";
            testRotation(cube, expected, () => cube.ui());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void bTest()
        {
            var cube = new Cube();
            const string expected =
                "30 33 36 4 5 6 7 8 9 3 11 12 2 14 15 1 17 18 19 20 21 22 23 24 25 26 27 28 29 54 31 32 53 34 35 52 43 40 37 44 41 38 45 42 39 46 47 48 49 50 51 10 13 16";
            testRotation(cube, expected, () => cube.b());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void biTest()
        {
            var cube = new Cube();
            const string expected =
                "16 13 10 4 5 6 7 8 9 52 11 12 53 14 15 54 17 18 19 20 21 22 23 24 25 26 27 28 29 1 31 32 2 34 35 3 39 42 45 38 41 44 37 40 43 46 47 48 49 50 51 36 33 30";
            testRotation(cube, expected, () => cube.bi());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void lTest()
        {
            var cube = new Cube();
            const string expected =
                "45 2 3 42 5 6 39 8 9 16 13 10 17 14 11 18 15 12 1 20 21 4 23 24 7 26 27 28 29 30 31 32 33 34 35 36 37 38 52 40 41 49 43 44 46 19 47 48 22 50 51 25 53 54";
            testRotation(cube, expected, () => cube.l());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void liTest()
        {
            var cube = new Cube();
            const string expected =
                "19 2 3 22 5 6 25 8 9 12 15 18 11 14 17 10 13 16 46 20 21 49 23 24 52 26 27 28 29 30 31 32 33 34 35 36 37 38 7 40 41 4 43 44 1 45 47 48 42 50 51 39 53 54";
            testRotation(cube, expected, () => cube.li());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void rTest()
        {
            var cube = new Cube();
            const string expected =
                "1 2 21 4 5 24 7 8 27 10 11 12 13 14 15 16 17 18 19 20 48 22 23 51 25 26 54 34 31 28 35 32 29 36 33 30 9 38 39 6 41 42 3 44 45 46 47 43 49 50 40 52 53 37";
            testRotation(cube, expected, () => cube.r());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void riTest()
        {
            var cube = new Cube();
            const string expected =
                "1 2 43 4 5 40 7 8 37 10 11 12 13 14 15 16 17 18 19 20 3 22 23 6 25 26 9 30 33 36 29 32 35 28 31 34 54 38 39 51 41 42 48 44 45 46 47 21 49 50 24 52 53 27";
            testRotation(cube, expected, () => cube.ri());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void dTest()
        {
            var cube = new Cube();
            const string expected =
                "1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 43 44 45 19 20 21 22 23 24 16 17 18 28 29 30 31 32 33 25 26 27 37 38 39 40 41 42 34 35 36 52 49 46 53 50 47 54 51 48";
            testRotation(cube, expected, () => cube.d());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void diTest()
        {
            var cube = new Cube();
            const string expected =
                "1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 25 26 27 19 20 21 22 23 24 34 35 36 28 29 30 31 32 33 43 44 45 37 38 39 40 41 42 16 17 18 48 51 54 47 50 53 46 49 52";
            testRotation(cube, expected, () => cube.di());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void usTest()
        {
            var cube = new Cube();
            const string expected =
                "1 2 3 4 5 6 7 8 9 10 11 12 22 23 24 16 17 18 19 20 21 31 32 33 25 26 27 28 29 30 40 41 42 34 35 36 37 38 39 13 14 15 43 44 45 46 47 48 49 50 51 52 53 54";
            testRotation(cube, expected, () => cube.us());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void dsTest()
        {
            var cube = new Cube();
            const string expected =
                "1 2 3 4 5 6 7 8 9 10 11 12 40 41 42 16 17 18 19 20 21 13 14 15 25 26 27 28 29 30 22 23 24 34 35 36 37 38 39 31 32 33 43 44 45 46 47 48 49 50 51 52 53 54";
            testRotation(cube, expected, () => cube.ds());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void lsTest()
        {
            var cube = new Cube();
            const string expected =
                "1 44 3 4 41 6 7 38 9 10 11 12 13 14 15 16 17 18 19 2 21 22 5 24 25 8 27 28 29 30 31 32 33 34 35 36 37 53 39 40 50 42 43 47 45 46 20 48 49 23 51 52 26 54";
            testRotation(cube, expected, () => cube.ls());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void rsTest()
        {
            var cube = new Cube();
            const string expected =
                "1 20 3 4 23 6 7 26 9 10 11 12 13 14 15 16 17 18 19 47 21 22 50 24 25 53 27 28 29 30 31 32 33 34 35 36 37 8 39 40 5 42 43 2 45 46 44 48 49 41 51 52 38 54";
            testRotation(cube, expected, () => cube.rs());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void fsTest()
        {
            var cube = new Cube();
            const string expected =
                "1 2 3 17 14 11 7 8 9 10 49 12 13 50 15 16 51 18 19 20 21 22 23 24 25 26 27 28 4 30 31 5 33 34 6 36 37 38 39 40 41 42 43 44 45 46 47 48 35 32 29 52 53 54";
            testRotation(cube, expected, () => cube.fs());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void bsTest()
        {
            var cube = new Cube();
            const string expected =
                "1 2 3 29 32 35 7 8 9 10 6 12 13 5 15 16 4 18 19 20 21 22 23 24 25 26 27 28 51 30 31 50 33 34 49 36 37 38 39 40 41 42 43 44 45 46 47 48 11 14 17 52 53 54";
            testRotation(cube, expected, () => cube.bs());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void cuTest()
        {
            var cube = new Cube();
            const string expected =
                "19 20 21 22 23 24 25 26 27 12 15 18 11 14 17 10 13 16 46 47 48 49 50 51 52 53 54  34 31 28 35 32 29 36 33 30 9 8 7 6 5 4 3 2 1 45 44 43 42 41 40 39 38 37";
            testRotation(cube, expected, () => cube.cu());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void cdTest()
        {
            var cube = new Cube();
            const string expected =
                "45 44 43 42 41 40 39 38 37 16 13 10 17 14 11 18 15 12 1 2 3 4 5 6 7 8 9 30 33 36 29 32 35 28 31 34 54 53 52 51 50 49 48 47 46 19 20 21 22 23 24 25 26 27";
            testRotation(cube, expected, () => cube.cd());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void clTest()
        {
            var cube = new Cube();
            const string expected =
                "7 4 1 8 5 2 9 6 3 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39 40 41 42 43 44 45 10 11 12 13 14 15 16 17 18 48 51 54 47 50 53 46 49 52";
            testRotation(cube, expected, () => cube.cl());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void crTest()
        {
            var cube = new Cube();
            const string expected =
                "3 6 9 2 5 8 1 4 7 37 38 39 40 41 42 43 44 45 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 52 49 46 53 50 47 54 51 48";
            testRotation(cube, expected, () => cube.cr());
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void reverse_sequenceTest()
        {
            var cube = new Cube();
            const string sequence = "u l f r d b ui li bi fi ri di ri ls fs us bs ds rs cu cl cd cr";
            cube.execute_sequence(sequence);
            Assert.IsFalse(cube.issolved(), "reverse_sequenceTest FAILED");

            var reverse = cube.reverse_sequence(sequence);
            cube.execute_sequence(reverse);
            Assert.IsTrue(cube.issolved(), "reverse_sequenceTest FAILED");
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void execute_sequenceTest()
        {
            var cube = new Cube();
            const string sequence = "F F D D R R L L U D";
            const string expected =
                "7 51 54 47 5 2 9 49 52 19 26 21 31 14 13 25 20 27 28 17 16 42 23 40 34 11 10 45 38 43 33 32 15 39 44 37 36 35 12 22 41 24 30 29 18 48 4 1 8 50 53 46 6 3";
            testRotation(cube, expected, () => cube.execute_sequence(sequence));
            var s = cube.Moves;
            var result = string.Compare(sequence, s, StringComparison.CurrentCultureIgnoreCase) == 0;
            Assert.IsTrue(result, "execute_sequenceTest FAIL");
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void scramble_cubeTest()
        {
            var cube = new Cube();
            Assert.IsTrue(cube.issolved(), "scramble_cubeTest FAIL");
            cube.scramble_cube();
            Assert.IsFalse(cube.issolved(), "scramble_cubeTest FAIL");
            var seq = cube.ScrambleSequence;
            Assert.IsTrue(seq.Length > 0, "scramble_cubeTest FAIL");
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void optimize_sequenceTest()
        {
            var cube = new Cube();
            const string sequence = "l l l r l r r";
            var optsequence = cube.optimize_sequence(sequence);
            Assert.IsTrue(sequence.Length >= optsequence.Length, "scramble_cubeTest FAIL");

            cube.execute_sequence(sequence);
            var str = cube.ToString();
            cube.init_cube();

            cube.execute_sequence(optsequence);
            var result = string.Compare(str, cube.ToString(), StringComparison.CurrentCultureIgnoreCase) == 0;
            Assert.IsTrue(result, "scramble_cubeTest FAIL");
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void set_up_faceTest()
        {
            var cube = new Cube();

            var c = cube.Left.Color;
            cube.set_up_face(c);
            Assert.IsTrue(c == cube.Up.Color, "set_up_faceTest FAIL");

            c = cube.Front.Color;
            cube.set_up_face(c);
            Assert.IsTrue(c == cube.Up.Color, "set_up_faceTest FAIL");

            c = cube.Right.Color;
            cube.set_up_face(c);
            Assert.IsTrue(c == cube.Up.Color, "set_up_faceTest FAIL");

            c = cube.Back.Color;
            cube.set_up_face(c);
            Assert.IsTrue(c == cube.Up.Color, "set_up_faceTest FAIL");

            c = cube.Down.Color;
            cube.set_up_face(c);
            Assert.IsTrue(c == cube.Up.Color, "set_up_faceTest FAIL");
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void set_front_faceTest()
        {
            var cube = new Cube();

            var c = cube.Up.Color;
            cube.set_front_face(c);
            Assert.IsTrue(c == cube.Front.Color, "set_front_faceTest FAIL");

            c = cube.Left.Color;
            cube.set_front_face(c);
            Assert.IsTrue(c == cube.Front.Color, "set_front_faceTest FAIL");

            c = cube.Right.Color;
            cube.set_front_face(c);
            Assert.IsTrue(c == cube.Front.Color, "set_up_faceTest FAIL");

            c = cube.Back.Color;
            cube.set_front_face(c);
            Assert.IsTrue(c == cube.Front.Color, "set_up_faceTest FAIL");

            c = cube.Down.Color;
            cube.set_front_face(c);
            Assert.IsTrue(c == cube.Front.Color, "set_up_faceTest FAIL");
        }

        [TestMethod, TestCategory("Cube"), Owner("Paul Baxter"), Priority(3)]
        public void ToStringTest1()
        {
            const string expected = "W W W W W W W W W " +
                                    "G G G G G G G G G " +
                                    "R R R R R R R R R " +
                                    "B B B B B B B B B " +
                                    "O O O O O O O O O " +
                                    "Y Y Y Y Y Y Y Y Y";
            var cube = new Cube();
            var s = cube.ToString();
            var result = string.Compare(s, expected, StringComparison.CurrentCultureIgnoreCase) == 0;
            Assert.IsTrue(result, "set_up_faceTest FAIL");
        }
    }
}