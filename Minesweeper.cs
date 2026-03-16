// Minesweeper.cs
using System;
using System.Drawing;
using System.Windows.Forms;

public class Minesweeper : Form
{
    const int CELL = 32;

    int cols, rows, totalMines;
    int[,] board;      // -1=地雷, 0-8=周囲の地雷数
    bool[,] revealed;
    bool[,] flagged;
    int flagsLeft;
    bool gameOver, gameWon, firstClick;

    Panel menuPanel;
    Panel gamePanel;
    Label flagLabel;
    Label statusLabel;

    public Minesweeper()
    {
        Text = "Minesweeper";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.FromArgb(30, 30, 30);
        ShowMenu();
    }

    void ShowMenu()
    {
        Controls.Clear();
        ClientSize = new Size(320, 260);

        menuPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30) };
        Controls.Add(menuPanel);

        var title = new Label {
            Text = "MINESWEEPER",
            Font = new Font("Arial", 20, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };
        title.Location = new Point((320 - 220) / 2, 30);
        menuPanel.Controls.Add(title);

        string[] labels = { "Easy  (9×9, 10mines)", "Normal (16×16, 40mines)", "Hard  (30×16, 99mines)" };
        int[][] configs = new int[][] {
            new int[] { 9,  9,  10 },
            new int[] { 16, 16, 40 },
            new int[] { 30, 16, 99 }
        };
        Color[] btnColors = { Color.ForestGreen, Color.DodgerBlue, Color.Crimson };

        for (int i = 0; i < 3; i++)
        {
            var btn = new Button {
                Text = labels[i],
                Font = new Font("Arial", 11),
                ForeColor = Color.White,
                BackColor = btnColors[i],
                FlatStyle = FlatStyle.Flat,
                Size = new Size(240, 40),
                Location = new Point(40, 90 + i * 52),
                Tag = configs[i]
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => {
                var cfg = (int[])((Button)s).Tag;
                StartGame(cfg[0], cfg[1], cfg[2]);
            };
            menuPanel.Controls.Add(btn);
        }
    }

    void StartGame(int c, int r, int mines)
    {
        cols = c; rows = r; totalMines = mines;
        board    = new int[cols, rows];
        revealed = new bool[cols, rows];
        flagged  = new bool[cols, rows];
        flagsLeft = mines;
        gameOver = false; gameWon = false; firstClick = true;

        Controls.Clear();

        int panelW = cols * CELL;
        int panelH = rows * CELL;
        int topH = 44;
        ClientSize = new Size(panelW, panelH + topH);

        // 上部バー
        var topBar = new Panel {
            Size = new Size(panelW, topH),
            Location = new Point(0, 0),
            BackColor = Color.FromArgb(45, 45, 45)
        };
        Controls.Add(topBar);

        flagLabel = new Label {
            Text = "🚩 " + flagsLeft,
            Font = new Font("Arial", 14, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(10, 10)
        };
        topBar.Controls.Add(flagLabel);

        statusLabel = new Label {
            Text = "",
            Font = new Font("Arial", 11),
            ForeColor = Color.Yellow,
            AutoSize = true,
            Location = new Point(panelW / 2 - 60, 12)
        };
        topBar.Controls.Add(statusLabel);

        var menuBtn = new Button {
            Text = "Menu",
            Font = new Font("Arial", 9),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(80, 80, 80),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(55, 28),
            Location = new Point(panelW - 65, 8)
        };
        menuBtn.FlatAppearance.BorderSize = 0;
        menuBtn.Click += (s, e) => ShowMenu();
        topBar.Controls.Add(menuBtn);

        gamePanel = new Panel {
            Size = new Size(panelW, panelH),
            Location = new Point(0, topH),
            BackColor = Color.FromArgb(50, 50, 50)
        };
        gamePanel.Paint += OnGamePaint;
        gamePanel.MouseClick += OnGameClick;
        Controls.Add(gamePanel);
    }

    void PlaceMines(int safeX, int safeY)
    {
        var rng = new Random();
        int placed = 0;
        while (placed < totalMines)
        {
            int x = rng.Next(cols), y = rng.Next(rows);
            if (board[x, y] == -1) continue;
            if (Math.Abs(x - safeX) <= 1 && Math.Abs(y - safeY) <= 1) continue;
            board[x, y] = -1;
            placed++;
        }
        for (int x = 0; x < cols; x++)
            for (int y = 0; y < rows; y++)
                if (board[x, y] != -1)
                    board[x, y] = CountAdjMines(x, y);
    }

    int CountAdjMines(int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && nx < cols && ny >= 0 && ny < rows && board[nx, ny] == -1)
                    count++;
            }
        return count;
    }

    void Reveal(int x, int y)
    {
        if (x < 0 || x >= cols || y < 0 || y >= rows) return;
        if (revealed[x, y] || flagged[x, y]) return;
        revealed[x, y] = true;
        if (board[x, y] == 0)
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    Reveal(x + dx, y + dy);
    }

    void CheckWin()
    {
        for (int x = 0; x < cols; x++)
            for (int y = 0; y < rows; y++)
                if (board[x, y] != -1 && !revealed[x, y]) return;
        gameWon = true;
        statusLabel.Text = "🎉 Clear!";
    }

    void OnGameClick(object sender, MouseEventArgs e)
    {
        if (gameOver || gameWon) return;
        int x = e.X / CELL, y = e.Y / CELL;
        if (x < 0 || x >= cols || y < 0 || y >= rows) return;

        if (e.Button == MouseButtons.Right)
        {
            if (revealed[x, y]) return;
            if (!flagged[x, y] && flagsLeft <= 0) return;
            flagged[x, y] = !flagged[x, y];
            flagsLeft += flagged[x, y] ? -1 : 1;
            flagLabel.Text = "🚩 " + flagsLeft;
        }
        else if (e.Button == MouseButtons.Left)
        {
            if (flagged[x, y] || revealed[x, y]) return;
            if (firstClick) { firstClick = false; PlaceMines(x, y); }
            if (board[x, y] == -1)
            {
                revealed[x, y] = true;
                gameOver = true;
                // 全地雷を表示
                for (int i = 0; i < cols; i++)
                    for (int j = 0; j < rows; j++)
                        if (board[i, j] == -1) revealed[i, j] = true;
                statusLabel.Text = "💥 Game Over";
            }
            else
            {
                Reveal(x, y);
                CheckWin();
            }
        }
        gamePanel.Invalidate();
    }

    static Color[] numColors = new Color[] {
        Color.Empty,
        Color.DeepSkyBlue,
        Color.LimeGreen,
        Color.Tomato,
        Color.MediumPurple,
        Color.Crimson,
        Color.Cyan,
        Color.Black,
        Color.Gray
    };

    void OnGamePaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                int px = x * CELL, py = y * CELL;
                var rect = new Rectangle(px + 1, py + 1, CELL - 2, CELL - 2);

                if (revealed[x, y])
                {
                    if (board[x, y] == -1)
                    {
                        // 地雷
                        g.FillRectangle(new SolidBrush(Color.FromArgb(180, 40, 40)), rect);
                        g.DrawString("💣", new Font("Arial", 14), Brushes.White, px + 4, py + 4);
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(70, 70, 70)), rect);
                        if (board[x, y] > 0)
                        {
                            var numColor = numColors[board[x, y]];
                            g.DrawString(board[x, y].ToString(),
                                new Font("Arial", 13, FontStyle.Bold),
                                new SolidBrush(numColor),
                                px + (CELL - 14) / 2, py + (CELL - 18) / 2);
                        }
                    }
                }
                else if (flagged[x, y])
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 60)), rect);
                    g.DrawString("🚩", new Font("Arial", 14), Brushes.White, px + 4, py + 4);
                }
                else
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(90, 90, 90)), rect);
                }

                g.DrawRectangle(new Pen(Color.FromArgb(40, 40, 40)), px, py, CELL, CELL);
            }
        }
    }

    [STAThread]
    static void Main()
    {
        Application.Run(new Minesweeper());
    }
}
