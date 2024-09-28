using System.Numerics;

using Raylib_cs;
using rl = Raylib_cs.Raylib;
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

public enum Direction {
    UP,
    DOWN,
    LEFT,
    RIGHT,
}

public class Button {
    private static readonly Texture2D _texture;

    public Rectangle Rect;
    public float Rotation;
    public int Index;
    public Direction Dir;

    private Vector2 _offset_pos;

    static Button() {
        var image = rl.LoadImage("arrow.png");
        // rl.ImageCrop(ref image, new Rectangle(Vector2.Zero, new Vector2(100)));
        rl.ImageResize(ref image, 100, 100);
        _texture = rl.LoadTextureFromImage(image);
    }

    public Button(Vector2 Pos, float Rotation, int Index, Direction Dir) {
        this.Rect = new Rectangle(Pos, new Vector2(100));
        this.Rotation = Rotation;

        Vector2 offset = Vector2.Zero;
        switch (Rotation) {
            case 0:
                offset = Vector2.Zero;
                break;
            case 90:
                offset = new Vector2(1, 0);
                break;
            case 180:
                offset = new Vector2(1, 1);
                break;
            case -90:
                offset = new Vector2(0, 1);
                break;
        }

        offset *= 100;
        this._offset_pos = this.Rect.Position + offset;

        this.Index = Index;
        this.Dir = Dir;
    }

    public void Draw() {
        rl.DrawTextureEx(_texture, this._offset_pos, this.Rotation, 1, Color.White);
    }
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
            // var hor_up_button = new Button(startPos + new Vector2(Cell.SIZE.X * i, -Cell.SIZE.Y));
            // var hor_down_button = new Button(startPos + new Vector2(Cell.SIZE.X * i, Cell.SIZE.Y * 4));

            // var ver_left_button = new Button(startPos + new Vector2(-Cell.SIZE.X, Cell.SIZE.Y * i));
            // var ver_right_button = new Button(startPos + new Vector2(Cell.SIZE.X * 4, Cell.SIZE.Y * i));

            this._buttons[0, i]= new Button(startPos + new Vector2(Cell.SIZE.X * i, -Cell.SIZE.Y), 0, i, Direction.UP);
            this._buttons[1, i]= new Button(startPos + new Vector2(Cell.SIZE.X * i, Cell.SIZE.Y * 4), 180, i, Direction.DOWN);
            this._buttons[2, i]= new Button(startPos + new Vector2(-Cell.SIZE.X, Cell.SIZE.Y * i), -90, i, Direction.LEFT);
            this._buttons[3, i]= new Button(startPos + new Vector2(Cell.SIZE.X * 4, Cell.SIZE.Y * i), 90, i, Direction.RIGHT);
        }
    }

    public void Update() {
        while(!rl.WindowShouldClose()) {
            rl.BeginDrawing();

                this._update();
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
        foreach (var button in this._buttons) {
            button.Draw();
        }
    }

    private void _update() {
        if (rl.IsMouseButtonPressed(MouseButton.Left)) {
            var mousePos = rl.GetMousePosition();
            foreach (var button in this._buttons) {
                if (rl.CheckCollisionPointRec(mousePos, button.Rect)) {
                    switch (button.Dir) {
                        case Direction.UP:
                        case Direction.DOWN:
                            this.MoveCol(button.Index, button.Dir);
                            break;
                        case Direction.LEFT:
                        case Direction.RIGHT:
                            this.MoveRow(button.Index, button.Dir);
                            break;
                    }
                    break;
                }
            }
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

        switch (Dir) {
            case Direction.RIGHT: {
                var node = circles.Last;
                circles.RemoveLast();
                circles.AddFirst(node);
                break;
            }

            case Direction.LEFT: {
                var node = circles.First;
                circles.RemoveFirst();
                circles.AddLast(node);
                break;
            }

            default:
                throw new Exception("wrong dir");
        }

        for (var i = 0; i < 4; i++) {
            this._circles[Row, i] = circles.First.Value;
            this._circles[Row, i].Pos = pos[i];
            circles.RemoveFirst();
        }
    }

    public void MoveCol(int Col, Direction Dir) {
        var pos = new Vector2[4] {
            this._circles[0, Col].Pos,
            this._circles[1, Col].Pos,
            this._circles[2, Col].Pos,
            this._circles[3, Col].Pos,
        };
        var circles = new LinkedList<Circle>();
        for (var i = 0; i < 4; i++) {
            circles.AddLast(this._circles[i, Col]);
        }

        switch (Dir) {
            case Direction.DOWN: {
                var node = circles.Last;
                circles.RemoveLast();
                circles.AddFirst(node);
                break;
            }

            case Direction.UP: {
                var node = circles.First;
                circles.RemoveFirst();
                circles.AddLast(node);
                break;
            }

            default:
                throw new Exception("wrong dir");
        }

        for (var i = 0; i < 4; i++) {
            this._circles[i, Col] = circles.First.Value;
            this._circles[i, Col].Pos = pos[i];

            circles.RemoveFirst();
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