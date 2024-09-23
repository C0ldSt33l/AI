using System.Numerics;
using System.Linq;

using Raylib_cs;
using rl = Raylib_cs.Raylib;
using Microsoft.VisualBasic;
using System.Security.Cryptography;

namespace Game;

// Available colors: `Red`, `Green`, `Yellow`, `Blue`
public class Circle(Color Color) {
    public static readonly float RADIUS = 50;
    public Vector2 Pos = Vector2.Zero;

    public void Draw() {
        rl.DrawCircleV(
            this.Pos,
            50,
            Color
        );
    }
}

public class Cell(Vector2 Pos) {
    public static readonly Vector2 SIZE = new Vector2(100);
    public Rectangle Rect = new Rectangle(Pos, SIZE);

    public void AttachCircle(Circle circle) {
        var pos = this.Rect.Position + this.Rect.Size / 2;
        circle.Pos = pos;
    }

    public void Draw() {
        rl.DrawRectangleRec(this.Rect, Color.Beige);
        rl.DrawRectangleLinesEx(this.Rect, 10, Color.Black);
    }
}

public class Button(Vector2 Pos) {
    public static readonly Texture2D Texture;
    public Rectangle Rect = new Rectangle(Pos, new Vector2(100));

    public void Draw() {

    }
}

public enum Direction {
    FORWARD,
    BACK,
}

public class Game {
    private readonly Vector2 WINDOW_SIZE = new Vector2(1080, 840);
    private readonly string TITLE = "Move balls";
    public Color Background = Color.Gray;
    private Circle[,] _circles = new Circle[4, 4];
    private Cell[,] _cells = new Cell[4, 4];
    private Button[,] _buttons = new Button[4, 4];

    public Game() {
        rl.InitWindow((int)this.WINDOW_SIZE.X, (int)this.WINDOW_SIZE.Y, this.TITLE);

        var startPos = new Vector2(1080 / 2 - Cell.SIZE.X * 4 / 2, 840 / 2 - Cell.SIZE.Y * 4 / 2);
        var curPos = startPos;

        for (var y = 0; y < this._cells.GetLength(0); y++) {
            for (var x = 0; x < this._cells.GetLength(1); x++) {
                var cell = new Cell(curPos);
                this._cells[y, x] = cell;
                curPos.X += Cell.SIZE.X;

                var circle = new Circle(this._getColor());
                cell.AttachCircle(circle);
                this._circles[y, x] = circle;
            }
            curPos = new Vector2(startPos.X, curPos.Y + Cell.SIZE.Y);
        }

        for (var i = 0; i < 4; i++) {
        }
    }

    public void Update() {
        while(!rl.WindowShouldClose()) {
            rl.BeginDrawing();

                // this.Update();
                this.Draw();

            rl.EndDrawing();
        }

        rl.CloseWindow();
    }

    public void Draw() {
        rl.ClearBackground(this.Background);

        foreach (var cell in this._cells) {
            cell.Draw();
        }
        foreach (var circle in this._circles) {
            circle.Draw();
        }
    }

    public void MoveRow(int Row, Direction Dir) {
        var pos = new Vector2[4] {
            this._circles[Row, 0].Pos,
            this._circles[Row, 1].Pos,
            this._circles[Row, 2].Pos,
            this._circles[Row, 3].Pos,
        };

        var circles = new LinkedList<Circle>();
        for (var i = 0; i < 4; i++) {
            circles.AddLast(this._circles[Row, i]);
        }

        if (Dir == Direction.FORWARD) {
            var node = circles.Last;
            circles.RemoveLast();
            circles.AddFirst(node);
        }
        else {
            var node = circles.First;
            circles.RemoveFirst();
            circles.AddLast(node);
        }

        for (var i = 0; i < 4; i++) {
            this._circles[Row, i] = circles.Last.Value;
            this._circles[Row, i].Pos = pos[i];
        }
    }

    public void MoveCol(int Col, Direction Dir) {
        var cells = new Cell[4];
        var pos = new Vector2[4] {
            this._cells[0, Col].Rect.Position,
            this._cells[1, Col].Rect.Position,
            this._cells[2, Col].Rect.Position,
            this._cells[3, Col].Rect.Position,
        };

        for (var i = 0; i < 4; i++) {
            this._cells[i, Col] = cells[i];
            this._cells[i, Col].Rect.Position = pos[i];
        }
    }


    private static int[] _colorRest = new int[4] { 4, 4, 4, 4 };
    private Color _getColor() {
        int i;
        do {
            i = RandomNumberGenerator.GetInt32(0, 4);
        } while (_colorRest[i] == 0);
        _colorRest[i]--;

        switch (i) {
            case 0: return Color.Red;
            case 1: return Color.Green;
            case 2: return Color.Yellow;
            case 3: return Color.Blue;

            default: return Color.Black;
        }
    }
}

class Program {
    public static void Main(String[] args) {
        var game = new Game();
        game.Update();
    }
}