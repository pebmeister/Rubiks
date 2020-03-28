using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace WpfApplication1
{
    public static class Extensions
    {
        /// <summary>
        /// dictionary to hold executing routed commands
        /// </summary>
        private static readonly Dictionary<ICommand, bool> CommandDict = new Dictionary<ICommand, bool>();

        public static bool IsExecuting(this ICommand command)
        {
            if (!CommandDict.ContainsKey(command)) return false;

            bool executing;
            CommandDict.TryGetValue(command, out executing);
            return executing;
        }

        /// <summary>
        /// Set the state of a routed command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="isExecuting"></param>
        public static void SetIsRunning(this ICommand command, bool isExecuting)
        {
            if (CommandDict.ContainsKey(command))
                CommandDict.Remove(command);

            CommandDict.Add(command, isExecuting);
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Conversion from spherical to cartesian coordinates
        /// </summary>
        /// <param name="sphere"></param>
        /// <returns>Cartesian 3d coordinates</returns>
        public static Point3D ToPoint3D(this Tuple<double, double, double> sphere)
        {
            var r = sphere.Item1;
            var theta = sphere.Item2;
            var phi = sphere.Item3;

            var x = r * Math.Sin(theta) * Math.Cos(phi);
            var y = r * Math.Sin(theta) * Math.Sin(phi);
            var z = r * Math.Cos(theta);
            return new Point3D(x, y, z);
        }

        /// <summary>
        /// Conversion from cartesian to spherical coordinates
        /// </summary>
        /// <param name="point3d"></param>
        /// <returns>Spherical coordinates</returns>
        public static Tuple<double, double, double> ToSphere(this Point3D point3d)
        {
            const double tolerance = 1E-20;
            var r = Math.Sqrt(point3d.X * point3d.X + point3d.Y * point3d.Y + point3d.Z * point3d.Z);
            var x = point3d.X;
            if (Math.Abs(x) < tolerance) x = tolerance;

            if (Math.Abs(r) < tolerance) r = tolerance;
            var theta = Math.Acos(point3d.Z / r);
            var phi = Math.Atan2(point3d.Y, x);

            return new Tuple<double, double, double>(r, theta, phi);
        }

        /// <summary>
        /// Add an item to the top of a queue
        /// call should do locking round this if needed
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="newFirstItem"></param>
        public static void EnqueTop(this Queue<string> queue, string newFirstItem)
        {
            var items = queue.ToArray();
            queue.Clear();
            queue.Enqueue(newFirstItem);
            foreach (var item in items)
                queue.Enqueue(item);
        }

        public static bool IsSet(this uint flags, uint mask)
        {
            return (flags & mask) == mask;
        }

        public static uint Set(this uint flags, uint mask)
        {
            return flags | mask;
        }

        /// <summary>
        /// Test if a cubie face is the correct color
        /// </summary>
        /// <param name="side">Side of the cube</param>
        /// <param name="row">Row of face</param>
        /// <param name="col">Column of face</param>
        /// <returns></returns>
        public static bool IsCorrect(this Face side, int row, int col)
        {
            return side[row, col] == side.Color;
        }
    }
}
