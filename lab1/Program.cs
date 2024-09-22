using Raylib_cs;
using rl = Raylib_cs.Raylib;

class Program {
    public static void Main(String[] args) {
        rl.InitWindow(800, 480, "Hello World");

        while (!rl.WindowShouldClose()) {
            rl.BeginDrawing();
            rl.ClearBackground(Color.White);

            rl.DrawText(
                "Hello, world!",
                12, 12,
                20, Color.Black
            );

            rl.EndDrawing();
        }

        rl.CloseWindow();
    }
}