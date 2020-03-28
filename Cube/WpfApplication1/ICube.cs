namespace WpfApplication1
{
    public interface ICube
    {
        string Moves { get; }
        string OptimizedMoves { get; }
        string ScrambleSequence { get; }

        /// <summary>
        /// Up face of the cube
        /// </summary>
        Face Up { get; }

        /// <summary>
        /// Front face of the cube
        /// </summary>
        Face Front { get; }

        /// <summary>
        /// Left face of the cube
        /// </summary>
        Face Left { get; }

        /// <summary>
        /// Right face of teh cube
        /// </summary>
        Face Right { get; }

        /// <summary>
        /// Down face of the cube
        /// </summary>
        Face Down { get; }

        /// <summary>
        /// Back face of the cube
        /// </summary>
        Face Back { get; }

        /// <summary>
        /// Initialize the cube to a solved state
        /// Clear moves and scamblesequence
        /// </summary>
        void init_cube();

        /// <summary>
        /// Set the value of the cubies
        /// </summary>
        /// <param name="cubeValues">string containing cubie values</param>
        void set_cube(string cubeValues);

        /// <summary>
        /// Make a copy of the cube
        /// Only copies values not record, moves or scramble
        /// </summary>
        /// <returns>Clone of cube</returns>
        Cube clone();

        /// <summary>
        /// Determine if the cube is solved
        /// </summary>
        /// <returns>True if the cube is solved</returns>
        bool issolved();

        /// <summary>
        /// Rotate the front face clockwise
        /// </summary>
        void f();

        /// <summary>
        /// Rotate the front face counter clockwise
        /// </summary>
        void fi();

        /// <summary>
        /// Rotate the up face clockwise
        /// </summary>
        void u();

        /// <summary>
        /// Rotate the up face counter clockwise
        /// </summary>
        void ui();

        /// <summary>
        /// Rotate the back face clockwise
        /// </summary>
        void b();

        /// <summary>
        /// Rotate the back face counter clockwise
        /// </summary>
        void bi();

        /// <summary>
        /// Rotate the left face clockwise
        /// </summary>
        void l();

        /// <summary>
        /// Rotate the left face counter clockwise
        /// </summary>
        void li();

        /// <summary>
        /// Rotate the right face clockwise
        /// </summary>
        void r();

        /// <summary>
        /// Rotate the right face counter clockwise
        /// </summary>
        void ri();

        /// <summary>
        /// Rotate the down face clockwise
        /// </summary>
        void d();

        /// <summary>
        /// Rotate the down face counter clockwise
        /// </summary>
        void di();

        /// <summary>
        /// Rotate the up slice clockwise
        /// </summary>
        void us();

        /// <summary>
        /// Rotate the down slice clockwise
        /// </summary>
        void ds();

        /// <summary>
        /// Rotate the left slice clockwise
        /// </summary>
        void ls();

        /// <summary>
        /// Rotate the right slice clockwise
        /// </summary>
        void rs();

        /// <summary>
        /// Rotate the front slice clockwise
        /// </summary>
        void fs();

        /// <summary>
        /// Rotate the back slice clockwise
        /// </summary>
        void bs();

        /// <summary>
        /// Rotate the cube up
        /// </summary>
        void cu();

        /// <summary>
        /// Rotate the cube down
        /// </summary>
        void cd();

        /// <summary>
        /// Rotate the cube left
        /// </summary>
        void cl();

        /// <summary>
        /// Rotate the cube right
        /// </summary>
        void cr();

        /// <summary>
        /// Reverse a sequence
        /// </summary>
        /// <param name="sequence">Sequence to reverse</param>
        /// <returns>Reversed sequence</returns>
        string reverse_sequence(string sequence);

        /// <summary>
        /// Execute a sequence
        /// </summary>
        /// <param name="sequence">Sequence to execute</param>
        void execute_sequence(string sequence);

        /// <summary>
        /// Scramble the cube
        /// Put the sequence in _scrambleSequence
        /// </summary>
        /// <param name="scramblecount">Number of moves to generate</param>
        void scramble_cube(int scramblecount = 10);

        /// <summary>
        /// Optimzie a sequence
        /// </summary>
        /// <param name="sequence">Sequence to optimize</param>
        /// <returns>Optimize a sequence</returns>
        string optimize_sequence(string sequence);

        /// <summary>
        /// Positions the cube so the given color is the front face
        /// </summary>
        /// <param name="color">Color of front face</param>
        void set_front_face(FaceVal color);

        /// <summary>
        /// Positions the cube so the given color is the up face
        /// </summary>
        /// <param name="color">Color of top face</param>
        void set_up_face(FaceVal color);

        /// <summary>
        /// Representation of the Cube colors
        /// </summary>
        /// <returns></returns>
        string ToString();
    }
}