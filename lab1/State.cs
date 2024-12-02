using System.Data;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Raylib_cs;

namespace Game;

public class State {
    public static  State TARGET_STATE = new(new char[4,4] {
        {'R', 'R', 'R', 'R'},
        {'G', 'G', 'G', 'G'},
        {'Y', 'Y', 'Y', 'Y'},
        {'B', 'B', 'B', 'B'},
    }, null);
    public  Color[,] Colors = new Color[4, 4];
    public  State? Parent = null;

    public State(int[] colorPos, Color c, State? parent = null) {
        for (var row = 0; row < 4; row++) {
            for (var col = 0; col < 4; col++) {
                this.Colors[row, col] =
                    colorPos.Contains(row * 4 + col + 1) ?
                        c : Color.Black;
            }
        }
        this.Parent = parent;
    }

    public State(string[] colors, State? parent = null) {
        for (var i = 0; i < 4; i++) {
            var chars = colors[i].Trim().Replace(" ", "").ToCharArray();
            for (int j = 0; j < 4; j++) {
                this.Colors[i, j] = this.CharToColor(chars[j]);
            }
        }
    }
    public State(Circle[,] circles, State? parent = null) {
        for (var row = 0; row < circles.GetLength(0); row++) {
            for (var col = 0; col < circles.GetLength(1); col++) {
                this.Colors[row, col] = circles[row, col].Color;
            }
        }
        this.Parent = parent;
    }
    public State(Color[,] colors, State? parent = null) {
        this.Colors = colors;
        this.Parent = parent;
    }
    
    public State(char[,] chars, State? parent = null) {
        for (var row = 0; row < 4; row++) {
            for (var col = 0; col < 4; col++) {
                this.Colors[row, col] = this.CharToColor(chars[row, col]);
            }
        }
        this.Parent = parent;
    }

    public State AddSomeChaos(uint depth) {
        var rand = (int min, int max) => RandomNumberGenerator.GetInt32(min, max + 1);

        State chaos = this;
        int pathLength = 0;
        bool odd = true;
        do {
            Console.WriteLine("depth: " + depth);
            var diff = depth - pathLength;
            for (var i = 0; i < diff; i++) {
                var idx = rand(0, 3);
                chaos = odd switch {
                    true => chaos.moveCol(idx, (Direction)rand(0, 1)),
                    false => chaos.moveRow(idx, (Direction)rand(2, 3)),
                };
                odd = !odd;
            }
            chaos.Parent = null;

            pathLength = new BiDirectionalSearch(
                chaos, State.TARGET_STATE,
                State.FullDiscovery, State.FullDiscovery
            ).Search().Count - 1;
            Console.WriteLine("path length: " + pathLength);
            Console.WriteLine("diff :" + (depth - pathLength));
            if (pathLength > depth) {
                chaos = this;
                pathLength = 0;
            }
        } while (pathLength != depth);

        return chaos;
    }

    public static List<State> FullDiscovery(State state) {
        var states = new List<State>();
        for (var i = 0; i < 4; i++) {
            states.Add(state.moveRow(i, Direction.LEFT));
            states.Add(state.moveCol(i, Direction.UP));
            states.Add(state.moveRow(i, Direction.RIGHT));
            states.Add(state.moveCol(i, Direction.DOWN));
        }

        return states;
    }
    public static List<State> Discovery(State state) {
        var states = new List<State>();
        for (var i = 0; i < 4; i++) {
            states.Add(state.moveRow(i, Direction.LEFT));
            states.Add(state.moveCol(i, Direction.UP));
        }

        return states;
    }
    public static List<State> ReverseDiscovery(State state) {
        var states = new List<State>();
        for (var i = 0; i < 4; i++) {
            states.Add(state.moveRow(i, Direction.RIGHT));
            states.Add(state.moveCol(i, Direction.DOWN));
        }

        return states;
    }

    public List<State> GetPath() {
        var path = new List<State>();
        var node = this;
        
        while (node.Parent != null) {
            path.Add(node);
            node = node.Parent;
        }
        path.Add(node);
        path.Reverse();

        return path;
    }

    public bool IsTargetState() {
        return this.Equals(TARGET_STATE);
    }

    public string GetColorPositions(Color c) {
        var pos = new uint[4] { 0, 0, 0, 0 };

        for (int count = 0, i = 0; count < 4; i++) {
            int row = i / 4, col = i % 4; 
            if (this.Colors[row, col].Equals(c)) {
                pos[count++] = (uint)(i + 1);
            }
        }

        return String.Join(" ",pos);
    }

    // Search how many colors not on its own places
    public static uint Heuristics1(State state, State target) {
        float value = 0;
        for (var row = 0; row < state.Colors.GetLength(0); row++) {
            for (var col = 0; col < state.Colors.GetLength(1); col++) {
                if (!state.Colors[row, col].Equals(target.Colors[row, col])) value++;
            }
        }

        return (uint)Math.Floor(value / 4.0f);
    }

    // Manhattan distance (Mosany)
    public static uint Heuristics2(State state, State target) {
        float value = 0;

        for (var targetRow = 0; targetRow < 4; targetRow++) {
            for (var targetCol = 0; targetCol < 4; targetCol++) {
                var targetColor = target.Colors[targetRow, targetCol];
                var pos = new int[4];
                var curPos = 0;
                for (var row = 0; row < 4; row++) {
                    for (var col = 0; col < 4; col++) {
                        if (curPos == 4) {
                            value += pos.Min();
                            goto Skip;
                        }
                        if (state.Colors[row, col].Equals(targetColor)) {
                            (int x, int y) = (Math.Abs(targetRow - row), Math.Abs(targetCol - col));
                            pos[curPos] = (x == 3 ? 1 : x) + (y == 3 ? 1 : y);
                            curPos++;
                        }
                    }
                }
                Skip: {}
            }
        }

        return (uint)Math.Floor(value / 4.0f);
    }
        

    // Count how many rows and cols in right color?
    public static uint TheMostFoolishHeuristics(State state, State target) {
        uint
            rowsNotInPlace = 0,
            colsNotInPlace = 0;

        var rows = state._getRows();
        var cols = state._getCols();
        var targetRows = target._getRows();
        var targetCols = target._getCols();

        for (var i = 0; i < 4; i++) {
            for (var j = 0; j < 4; j++) {
                if (!rows[i][j].Equals(targetRows[i][j])) {
                    rowsNotInPlace++;
                    break;
                }
            }
        }

        for (var i = 0; i < 4; i++) {
            for (var j = 0; j < 4; j++) {
                if (!cols[i][j].Equals(targetCols[i][j])) {
                    colsNotInPlace++;
                    break;
                }
            }
        }

        return (uint)Math.Floor((rowsNotInPlace + colsNotInPlace) / 4.0f);
    }

    // Extra task
    // Using subtask DB to evaluate approximated rest of path
    public static Dictionary<string, uint> redDB = _createPathDB("db//red_subtask.txt");
    public static Dictionary<string, uint> greenDB = _createPathDB("db//green_subtask.txt");
    public static Dictionary<string, uint> yellowDB = _createPathDB("db//yellow_subtask.txt");
    public static Dictionary<string, uint> blueDB = _createPathDB("db//blue_subtask.txt");
    private static Dictionary<string, uint> _createPathDB(string path) {
        var db = new Dictionary<string, uint>();
        foreach (var line in File.ReadAllLines(path)) {
            var tmp = line.Trim().Split(":");
            db[tmp[0]] = UInt32.Parse(tmp[1]);
        }

        return db;
    }
    public static uint DBHeuristics(State state, State target) {
        return new uint[4] {
            State.redDB[state.GetColorPositions(Color.Red)],
            State.greenDB[state.GetColorPositions(Color.Green)],
            State.yellowDB[state.GetColorPositions(Color.Yellow)],
            State.blueDB[state.GetColorPositions(Color.Blue)],
        }.Max();
    }

    private Color[][] _getRows() {
        var rows = new Color[4][] {
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
        };
        for (var row = 0; row < this.Colors.GetLength(0); row++) {
            for (var col = 0; col < this.Colors.GetLength(1); col++) {
                rows[row][col] = this.Colors[row, col];
            }
        }

        return rows;
    }

    private Color[][] _getCols() {
        var cols = new Color[4][] {
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
        };
        for (var col = 0; col < this.Colors.GetLength(0); col++) {
            for (var row = 0; row < this.Colors.GetLength(1); row++) {
                cols[row][col] = this.Colors[col, row];
            }
        }

        return cols;
    }

    private State moveRow(int row, Direction dir) {
        var rowColors = new LinkedList<Color>();
        for (var i = 0; i < 4; i++) {
            rowColors.AddLast(this.Colors[row, i]);
        }

        switch (dir) {
            case Direction.LEFT: {
                var node = rowColors.First;
                rowColors.RemoveFirst();
                rowColors.AddLast(node);
                break;
            }
            case Direction.RIGHT: {
                var node = rowColors.Last;
                rowColors.RemoveLast();
                rowColors.AddFirst(node);
                break;
            }
        }

        var colors = this.Colors.Clone() as Color[,];
        for (var i = 0; i < 4; i++) {
            colors[row, i] = rowColors.First.Value;
            rowColors.RemoveFirst();
        }

        return new State(colors, this);
    }

    private State moveCol(int col, Direction dir) {
        var colColors = new LinkedList<Color>();
        for (var i = 0; i < 4; i++) {
            colColors.AddLast(this.Colors[i, col]);
        }

        switch (dir) {
            case Direction.UP: {
                var node = colColors.First;
                colColors.RemoveFirst();
                colColors.AddLast(node);
                break;
            }
            case Direction.DOWN: {
                var node = colColors.Last;
                colColors.RemoveLast();
                colColors.AddFirst(node);
                break;
            }
        }

        var colors = this.Colors.Clone() as Color[,];
        for (var i = 0; i < 4; i++) {
            colors[i, col] = colColors.First.Value;
            colColors.RemoveFirst();
        }

        return new State(colors, this);
    }

    public override string ToString() {
        var builder = new StringBuilder(4 * 4 * 2 + 4);
        for (var row = 0; row < 4; row++) {
            for (var col = 0; col < 4; col++) {
                builder.Append(this.ColorToChar(this.Colors[row, col])).Append(' ');
            }
            if (row < 3) builder.Append('\n');
        }

        return builder.ToString();
    }

    public char ColorToChar(Color color) {
        return color switch {
            Color c when c.Equals(Color.Red) => 'R',
            Color c when c.Equals(Color.Green) => 'G',
            Color c when c.Equals(Color.Yellow) => 'Y',
            Color c when c.Equals(Color.Blue) => 'B',

            _ => '?',
        };
    }

    public Color CharToColor(char c) {
        return c switch {
            'R' => Color.Red,
            'G' => Color.Green,
            'Y' => Color.Yellow,
            'B' => Color.Blue,

            _ =>  Color.Black,
        };
    }

    public override bool Equals(object? obj) {
        if (obj == null || this.GetType() != obj.GetType()) return false;
        
        var state = obj as State;
        for (var row = 0; row < 4; row++) {
            for (var col = 0; col < 4; col++) {
                if (!this.Colors[row, col].Equals(state.Colors[row, col]))  return false;
            }
        }

        return true;
    }

    public bool EqualsByColor(State other, Color c) {
        var col_count = 0;
        for (var row = 0; row < this.Colors.GetLength(0); row++) {
            for (var col = 0; col < this.Colors.GetLength(1); col++) {
                if (col_count == 4) break;
                if (this.Colors[row, col].Equals(c)) {
                    if (other.Colors[row, col].Equals(c))
                        col_count++;
                    else
                        return false;
                }
            }
        }

        return true;
    }
}