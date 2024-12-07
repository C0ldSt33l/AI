using System.Data;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Raylib_cs;

namespace Game;

public class State {
    public static State TARGET_STATE = new(new char[4,4] {
        {'R', 'R', 'R', 'R'},
        {'G', 'G', 'G', 'G'},
        {'Y', 'Y', 'Y', 'Y'},
        {'B', 'B', 'B', 'B'},
    });
    
    public int Size;
    public  Color[,] Colors = new Color[4, 4];
    public  State? Parent = null;

    public State(int[] colorPos, Color c, State? parent = null) {
        this._initColors(
            colorPos.Length,
            (row, col) =>colorPos.Contains(row * this.Size + col + 1) ? c : Color.Black 
        );
        this.Parent = parent;
    }

    public State(string[] colors, State? parent = null) {
        var cols = colors.Select(el => el.Trim().Replace(" ", "").ToCharArray()).ToArray();
        this._initColors(
            colors.Length,
            (row, col) => this.CharToColor(cols[row][col])
        );
        this.Parent = parent;
    }
    public State(Circle[,] circles, State? parent = null) {
        this._initColors(
            circles.GetLength(0),
            (row, col) => circles[row, col].Color
        );
        this.Parent = parent;
    }
    public State(Color[,] colors, State? parent = null) {
        this.Size = colors.GetLength(0);
        this.Colors = colors;
        this.Parent = parent;
    }
    
    public State(char[,] chars, State? parent = null) {
        this._initColors(
            chars.GetLength(0),
            (row, col) => this.CharToColor(chars[row, col])
        );
        this.Parent = parent;
    }
    private void _initColors(int size, Func<int, int, Color> initer) {
        this.Size = size;
        this.Colors = new Color[size, size];
        for (var row = 0; row < this.Size; row++) {
            for (var col = 0; col < this.Size; col++) {
                this.Colors[row, col] = initer(row, col);
            }
        }
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
                var idx = rand(0, this.Size - 1);
                chaos = odd switch {
                    true => chaos.moveCol(idx, Direction.DOWN),
                    false => chaos.moveRow(idx, Direction.RIGHT),
                };
                odd = !odd;
            }
            chaos.Parent = null;

            pathLength = new BiDirectionalSearch(
                chaos, State.TARGET_STATE,
                State.Discovery, State.ReverseDiscovery
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

    public static List<State> FullDiscovery(State parent) {
        var children = new List<State>();
        for (var i = 0; i < parent.Size; i++) {
            children.Add(parent.moveRow(i, Direction.LEFT));
            children.Add(parent.moveCol(i, Direction.UP));
            children.Add(parent.moveRow(i, Direction.RIGHT));
            children.Add(parent.moveCol(i, Direction.DOWN));
        }
        children.RemoveAll(it => it.Equals(parent));

        return children;
    }
    public static List<State> Discovery(State parent) {
        var children = new List<State>();
        for (var i = 0; i < parent.Size; i++) {
            children.Add(parent.moveRow(i, Direction.LEFT));
            children.Add(parent.moveCol(i, Direction.UP));
        }
        children.RemoveAll(it => it.Equals(parent));

        return children;
    }
    public static List<State> ReverseDiscovery(State child) {
        var parents = new List<State>();
        for (var i = 0; i < child.Size; i++) {
            parents.Add(child.moveRow(i, Direction.RIGHT));
            parents.Add(child.moveCol(i, Direction.DOWN));
        }
        parents.RemoveAll(it => it.Equals(child));

        return parents;
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
        var pos = new uint[this.Size];

        for (int count = 0, i = 0; count < this.Size; i++) {
            int row = i / this.Size, col = i % this.Size; 
            if (this.Colors[row, col].Equals(c)) {
                pos[count++] = (uint)(i + 1);
            }
        }

        return String.Join(" ",pos);
    }

    // Search how many colors not on its own places
    public static uint Heuristics1(State state, State target) {
        float value = 0;
        for (var row = 0; row < state.Size; row++) {
            for (var col = 0; col < state.Size; col++) {
                if (!state.Colors[row, col].Equals(target.Colors[row, col])) value++;
            }
        }

        return (uint)Math.Floor(value / (float)state.Size);
    }

    // Manhattan distance (Mosany)
    public static uint Heuristics2(State state, State target) {
        float value = 0;

        for (var targetRow = 0; targetRow < target.Size; targetRow++) {
            for (var targetCol = 0; targetCol < target.Size; targetCol++) {
                var targetColor = target.Colors[targetRow, targetCol];
                var pos = new int[target.Size];
                var curPos = 0;
                for (var row = 0; row < state.Size; row++) {
                    for (var col = 0; col < state.Size; col++) {
                        if (curPos == state.Size) {
                            value += pos.Min();
                            goto Skip;
                        }
                        if (state.Colors[row, col].Equals(targetColor)) {
                            (int x, int y) = (Math.Abs(targetRow - row), Math.Abs(targetCol - col));
                            pos[curPos] = x + y;
                            curPos++;
                        }
                    }
                }
                Skip: {}
            }
        }

        return (uint)Math.Floor(value / (float)state.Size);
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

        for (var i = 0; i < state.Size; i++) {
            for (var j = 0; j < state.Size; j++) {
                if (!rows[i][j].Equals(targetRows[i][j])) {
                    rowsNotInPlace++;
                    break;
                }
            }
        }

        for (var i = 0; i < state.Size; i++) {
            for (var j = 0; j < state.Size; j++) {
                if (!cols[i][j].Equals(targetCols[i][j])) {
                    colsNotInPlace++;
                    break;
                }
            }
        }

        return rowsNotInPlace + colsNotInPlace - (uint)(state.Size + 1);
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

    public Color[][] _getRows() {
        var rows = new Color[this.Size][];
        for (var row = 0; row < this.Size; row++) {
            rows[row] = new Color[this.Size];
            for (var col = 0; col < this.Size; col++) {
                rows[row][col] = this.Colors[row, col];
            }
        }

        return rows;
    }

    public Color[][] _getCols() {
        var cols = new Color[this.Size][];
        for (var i = 0; i < this.Size; i++) {
            cols[i] = new Color[this.Size];
        }
        for (var col = 0; col < this.Size; col++) {
            for (var row = 0; row < this.Size; row++) {
                cols[row][col] = this.Colors[col, row];
            }
        }

        return cols;
    }

    private State moveRow(int row, Direction dir) {
        var rowColors = new LinkedList<Color>();
        for (var i = 0; i < this.Size; i++) {
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
        for (var i = 0; i < this.Size; i++) {
            colors[row, i] = rowColors.First.Value;
            rowColors.RemoveFirst();
        }

        return new State(colors, this);
    }

    private State moveCol(int col, Direction dir) {
        var colColors = new LinkedList<Color>();
        for (var i = 0; i < this.Size; i++) {
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
        for (var i = 0; i < this.Size; i++) {
            colors[i, col] = colColors.First.Value;
            colColors.RemoveFirst();
        }

        return new State(colors, this);
    }

    public override string ToString() {
        var builder = new StringBuilder(4 * 4 * 2 + 4);
        for (var row = 0; row < this.Size; row++) {
            for (var col = 0; col < this.Size; col++) {
                builder.Append(this.ColorToChar(this.Colors[row, col])).Append(' ');
            }
            if (row < this.Size - 1) builder.Append('\n');
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
        if (this.Size != state.Size) return false;
        for (var row = 0; row < this.Size; row++) {
            for (var col = 0; col < this.Size; col++) {
                if (!this.Colors[row, col].Equals(state.Colors[row, col]))  return false;
            }
        }

        return true;
    }

    public override int GetHashCode() {
        var str = "";
        var i = 1;
        foreach (var color in this.Colors) {
            str += i.ToString() + this.ColorToChar(color);
        }

        return str.GetHashCode();
    }

    public bool EqualsByColor(State other, Color c) {
        if (this.Size != other.Size) return false;
        var col_count = 0;
        for (var row = 0; row < this.Size; row++) {
            for (var col = 0; col < this.Size; col++) {
                if (col_count == this.Size) break;
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