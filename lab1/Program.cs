using System.Numerics;

using Raylib_cs;
using rl = Raylib_cs.Raylib;
using System.Security.Cryptography;
using System.Data;

namespace Game;

// Available colors: `Red`, `Green`, `Yellow`, `Blue`
public class Circle(Color Color) {
    public static readonly float RADIUS = 50;
    public  Color Color = Color;
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

        Vector2 offset = Rotation switch {
            0   => Vector2.Zero,
            90  => new Vector2(1, 0),
            180 => new Vector2(1, 1),
            -90 => new Vector2(0, 1),
        } * 100;
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

        this._oneStepFromWinningInit();

        var state = new State(this._circles);
        Console.WriteLine(state);
        var widthSearch = new WidthFirstSearch(state);
        var depthSerch = new DepthFirstSearch(state);

        var goalState = widthSearch.Search();
        if (goalState != null) Console.WriteLine(goalState);

        var goalState2 = depthSerch.Search();
        if (goalState2 != null) Console.WriteLine(goalState2);
    }

    public void Update() {
        while(!rl.WindowShouldClose()) {
            rl.BeginDrawing();

                this._update();
                this.Draw();

                // Console.WriteLine(_circles[0, 0].Color.ToString());

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

    private void _normalInit() {
        this._setCells();
        this._setCircles();
        this._setButtons();
    }

    private void _oneStepFromWinningInit() {
        this._setCells();
        this._setCirclesInWinState();
        this.MoveCol(0, Direction.UP);
        this._setButtons();
    }


    private void _setCells() {
        var startPos = new Vector2(1080 / 2 - Cell.SIZE.X * 4 / 2, 840 / 2 - Cell.SIZE.Y * 4 / 2);
        var curPos = startPos;

        for (var y = 0; y < this._cells.GetLength(0); y++) {
            for (var x = 0; x < this._cells.GetLength(1); x++) {
                var cell = new Cell(curPos);
                this._cells[y, x] = cell;
                curPos.X += Cell.SIZE.X;
            }
            curPos = new Vector2(startPos.X, curPos.Y + Cell.SIZE.Y);
        }
    }

    private void _setCircles() {
        for (var row = 0; row < this._cells.GetLength(0); row++) {
            for (var col = 0; col < this._cells.GetLength(1); col++) {
                var circle = new Circle(this._getRandomColor());
                this._cells[row, col].AttachCircle(circle);
                this._circles[row, col] = circle;
            }
        }
    }
    
    private void _setCirclesInWinState() {
        var colors = new Color[] { Color.Red, Color.Green, Color.Yellow, Color.Blue };
        for (var row = 0; row < this._cells.GetLength(0); row++) {
            for (var col = 0; col < this._cells.GetLength(1); col++) {
                var circle = new Circle(colors[row]);
                this._cells[row, col].AttachCircle(circle);
                this._circles[row, col] = circle;
            }
        }
    }

    private void _setButtons() {
        var startPos = new Vector2(1080 / 2 - Cell.SIZE.X * 4 / 2, 840 / 2 - Cell.SIZE.Y * 4 / 2);
        for (var i = 0; i < 4; i++) {
            // hor up buttons
            this._buttons[0, i]= new Button(startPos + new Vector2(Cell.SIZE.X * i, -Cell.SIZE.Y), 0, i, Direction.UP);
            // hor down buttons
            this._buttons[1, i]= new Button(startPos + new Vector2(Cell.SIZE.X * i, Cell.SIZE.Y * 4), 180, i, Direction.DOWN);
            // ver left buttons
            this._buttons[2, i]= new Button(startPos + new Vector2(-Cell.SIZE.X, Cell.SIZE.Y * i), -90, i, Direction.LEFT);
            // ver right buttons
            this._buttons[3, i]= new Button(startPos + new Vector2(Cell.SIZE.X * 4, Cell.SIZE.Y * i), 90, i, Direction.RIGHT);
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
    private Color _getRandomColor() {
        if (_colorRest.All(el => el == 0)) throw new Exception("Rest colors are empty");

        int i;
        do {
            i = RandomNumberGenerator.GetInt32(0, 4);
        } while (_colorRest[i] == 0);
        _colorRest[i]--;

        return i switch {
            0 => Color.Red,
            1 => Color.Green,
            2 => Color.Yellow,
            3 => Color.Blue,
        };
    }
}

class Program {
    public static void Main(String[] args) {
        var game = new Game();
        game.Update();
    }
}