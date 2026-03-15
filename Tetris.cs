// Tetris.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class TetrisForm : Form
{
    const int COLS = 10, ROWS = 20, CELL = 30;
    const int BOARD_X = 160;
    const int PANEL_W = COLS * CELL;
    const int SIDE_W = 155;
    const int NC = 18;
    const int HC = 22;

    int[,] board = new int[ROWS, COLS];
    int[][] piece;
    int pieceX, pieceY, pieceType, pieceRot;
    int holdType = -1;
    bool holdUsed = false;
    bool gameOver = false;
    bool paused = false;
    int score = 0, level = 1, lines = 0;
    Queue<int> nextQueue = new Queue<int>();
    Random rng = new Random();

    System.Windows.Forms.Timer gameTimer     = new System.Windows.Forms.Timer();
    System.Windows.Forms.Timer lockTimer     = new System.Windows.Forms.Timer();
    System.Windows.Forms.Timer dasTimer      = new System.Windows.Forms.Timer();
    System.Windows.Forms.Timer arrTimer      = new System.Windows.Forms.Timer();
    System.Windows.Forms.Timer softDropTimer = new System.Windows.Forms.Timer();
    System.Windows.Forms.Timer retryTimer    = new System.Windows.Forms.Timer();
    int dasDir = 0;
    int retryHoldMs = 0;
    const int RETRY_HOLD_MS = 1000;

    static Color[] colors = new Color[] {
        Color.Cyan, Color.Yellow, Color.Green, Color.Red,
        Color.Blue, Color.Orange, Color.Purple
    };

    static int[,,] wallKickJLSTZ;
    static int[,,] wallKickI;
    static int[][][][] shapes;

    static TetrisForm()
    {
        wallKickJLSTZ = new int[4, 5, 2];
        wallKickJLSTZ[0,0,0]= 0; wallKickJLSTZ[0,0,1]= 0;
        wallKickJLSTZ[0,1,0]=-1; wallKickJLSTZ[0,1,1]= 0;
        wallKickJLSTZ[0,2,0]=-1; wallKickJLSTZ[0,2,1]= 1;
        wallKickJLSTZ[0,3,0]= 0; wallKickJLSTZ[0,3,1]=-2;
        wallKickJLSTZ[0,4,0]=-1; wallKickJLSTZ[0,4,1]=-2;
        wallKickJLSTZ[1,0,0]= 0; wallKickJLSTZ[1,0,1]= 0;
        wallKickJLSTZ[1,1,0]= 1; wallKickJLSTZ[1,1,1]= 0;
        wallKickJLSTZ[1,2,0]= 1; wallKickJLSTZ[1,2,1]=-1;
        wallKickJLSTZ[1,3,0]= 0; wallKickJLSTZ[1,3,1]= 2;
        wallKickJLSTZ[1,4,0]= 1; wallKickJLSTZ[1,4,1]= 2;
        wallKickJLSTZ[2,0,0]= 0; wallKickJLSTZ[2,0,1]= 0;
        wallKickJLSTZ[2,1,0]= 1; wallKickJLSTZ[2,1,1]= 0;
        wallKickJLSTZ[2,2,0]= 1; wallKickJLSTZ[2,2,1]= 1;
        wallKickJLSTZ[2,3,0]= 0; wallKickJLSTZ[2,3,1]=-2;
        wallKickJLSTZ[2,4,0]= 1; wallKickJLSTZ[2,4,1]=-2;
        wallKickJLSTZ[3,0,0]= 0; wallKickJLSTZ[3,0,1]= 0;
        wallKickJLSTZ[3,1,0]=-1; wallKickJLSTZ[3,1,1]= 0;
        wallKickJLSTZ[3,2,0]=-1; wallKickJLSTZ[3,2,1]=-1;
        wallKickJLSTZ[3,3,0]= 0; wallKickJLSTZ[3,3,1]= 2;
        wallKickJLSTZ[3,4,0]=-1; wallKickJLSTZ[3,4,1]= 2;

        wallKickI = new int[4, 5, 2];
        wallKickI[0,0,0]= 0; wallKickI[0,0,1]= 0;
        wallKickI[0,1,0]=-2; wallKickI[0,1,1]= 0;
        wallKickI[0,2,0]= 1; wallKickI[0,2,1]= 0;
        wallKickI[0,3,0]=-2; wallKickI[0,3,1]=-1;
        wallKickI[0,4,0]= 1; wallKickI[0,4,1]= 2;
        wallKickI[1,0,0]= 0; wallKickI[1,0,1]= 0;
        wallKickI[1,1,0]=-1; wallKickI[1,1,1]= 0;
        wallKickI[1,2,0]= 2; wallKickI[1,2,1]= 0;
        wallKickI[1,3,0]=-1; wallKickI[1,3,1]= 2;
        wallKickI[1,4,0]= 2; wallKickI[1,4,1]=-1;
        wallKickI[2,0,0]= 0; wallKickI[2,0,1]= 0;
        wallKickI[2,1,0]= 2; wallKickI[2,1,1]= 0;
        wallKickI[2,2,0]=-1; wallKickI[2,2,1]= 0;
        wallKickI[2,3,0]= 2; wallKickI[2,3,1]= 1;
        wallKickI[2,4,0]=-1; wallKickI[2,4,1]=-2;
        wallKickI[3,0,0]= 0; wallKickI[3,0,1]= 0;
        wallKickI[3,1,0]= 1; wallKickI[3,1,1]= 0;
        wallKickI[3,2,0]=-2; wallKickI[3,2,1]= 0;
        wallKickI[3,3,0]= 1; wallKickI[3,3,1]=-2;
        wallKickI[3,4,0]=-2; wallKickI[3,4,1]= 1;

        shapes = new int[][][][]
        {
            new int[][][] {
                new int[][] { new int[]{0,1}, new int[]{1,1}, new int[]{2,1}, new int[]{3,1} },
                new int[][] { new int[]{2,0}, new int[]{2,1}, new int[]{2,2}, new int[]{2,3} },
                new int[][] { new int[]{0,2}, new int[]{1,2}, new int[]{2,2}, new int[]{3,2} },
                new int[][] { new int[]{1,0}, new int[]{1,1}, new int[]{1,2}, new int[]{1,3} }
            },
            new int[][][] {
                new int[][] { new int[]{1,0}, new int[]{2,0}, new int[]{1,1}, new int[]{2,1} },
                new int[][] { new int[]{1,0}, new int[]{2,0}, new int[]{1,1}, new int[]{2,1} },
                new int[][] { new int[]{1,0}, new int[]{2,0}, new int[]{1,1}, new int[]{2,1} },
                new int[][] { new int[]{1,0}, new int[]{2,0}, new int[]{1,1}, new int[]{2,1} }
            },
            new int[][][] {
                new int[][] { new int[]{1,0}, new int[]{2,0}, new int[]{0,1}, new int[]{1,1} },
                new int[][] { new int[]{1,0}, new int[]{2,1}, new int[]{1,1}, new int[]{2,2} },
                new int[][] { new int[]{1,1}, new int[]{2,1}, new int[]{0,2}, new int[]{1,2} },
                new int[][] { new int[]{0,0}, new int[]{1,1}, new int[]{0,1}, new int[]{1,2} }
            },
            new int[][][] {
                new int[][] { new int[]{0,0}, new int[]{1,0}, new int[]{1,1}, new int[]{2,1} },
                new int[][] { new int[]{2,0}, new int[]{1,1}, new int[]{2,1}, new int[]{1,2} },
                new int[][] { new int[]{0,1}, new int[]{1,1}, new int[]{1,2}, new int[]{2,2} },
                new int[][] { new int[]{1,0}, new int[]{0,1}, new int[]{1,1}, new int[]{0,2} }
            },
            new int[][][] {
                new int[][] { new int[]{0,0}, new int[]{0,1}, new int[]{1,1}, new int[]{2,1} },
                new int[][] { new int[]{1,0}, new int[]{2,0}, new int[]{1,1}, new int[]{1,2} },
                new int[][] { new int[]{0,1}, new int[]{1,1}, new int[]{2,1}, new int[]{2,2} },
                new int[][] { new int[]{1,0}, new int[]{1,1}, new int[]{0,2}, new int[]{1,2} }
            },
            new int[][][] {
                new int[][] { new int[]{2,0}, new int[]{0,1}, new int[]{1,1}, new int[]{2,1} },
                new int[][] { new int[]{1,0}, new int[]{1,1}, new int[]{1,2}, new int[]{2,2} },
                new int[][] { new int[]{0,1}, new int[]{1,1}, new int[]{2,1}, new int[]{0,2} },
                new int[][] { new int[]{0,0}, new int[]{1,0}, new int[]{1,1}, new int[]{1,2} }
            },
            new int[][][] {
                new int[][] { new int[]{1,0}, new int[]{0,1}, new int[]{1,1}, new int[]{2,1} },
                new int[][] { new int[]{1,0}, new int[]{1,1}, new int[]{2,1}, new int[]{1,2} },
                new int[][] { new int[]{0,1}, new int[]{1,1}, new int[]{2,1}, new int[]{1,2} },
                new int[][] { new int[]{1,0}, new int[]{0,1}, new int[]{1,1}, new int[]{1,2} }
            }
        };
    }

    public TetrisForm()
    {
        Text = "Tetris";
        ClientSize = new Size(BOARD_X + PANEL_W + SIDE_W, ROWS * CELL);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        DoubleBuffered = true;
        BackColor = Color.Black;

        InitGame();

        gameTimer.Tick += (s, e) => { if (!gameOver && !paused) { MoveDown(); Invalidate(); } };

        lockTimer.Interval = 1000;
        lockTimer.Tick += (s, e) => { lockTimer.Stop(); LockPiece(); Invalidate(); };

        dasTimer.Interval = 130;
        dasTimer.Tick += (s, e) => { dasTimer.Stop(); arrTimer.Start(); MoveHorizontal(dasDir); Invalidate(); };

        arrTimer.Interval = 50;
        arrTimer.Tick += (s, e) => { MoveHorizontal(dasDir); Invalidate(); };

        softDropTimer.Interval = 150;
        softDropTimer.Tick += (s, e) => {
            if (gameOver || paused) return;
            if (IsValid(piece, pieceX, pieceY + 1))
            {
                pieceY++;
                lockTimer.Stop();
            }
            else
            {
                if (!lockTimer.Enabled) lockTimer.Start();
            }
            Invalidate();
        };

        retryTimer.Interval = 100;
        retryTimer.Tick += (s, e) => {
            retryHoldMs += 100;
            Invalidate();
            if (retryHoldMs >= RETRY_HOLD_MS) { retryTimer.Stop(); retryHoldMs = 0; InitGame(); Invalidate(); }
        };

        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
    }

    void InitGame()
    {
        board = new int[ROWS, COLS];
        holdType = -1; holdUsed = false;
        gameOver = false; paused = false;
        score = 0; level = 1; lines = 0;
        nextQueue.Clear();
        gameTimer.Stop(); lockTimer.Stop(); softDropTimer.Stop();
        FillBag(); SpawnPiece();
        gameTimer.Interval = 800;
        gameTimer.Start();
    }

    void FillBag()
    {
        var bag = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
        while (bag.Count > 0) { int i = rng.Next(bag.Count); nextQueue.Enqueue(bag[i]); bag.RemoveAt(i); }
    }

    void SpawnPiece()
    {
        if (nextQueue.Count < 6) FillBag();
        pieceType = nextQueue.Dequeue();
        pieceRot = 0; pieceX = 3; pieceY = 0;
        piece = shapes[pieceType][pieceRot];
        holdUsed = false; lockTimer.Stop();
        if (!IsValid(piece, pieceX, pieceY))
        {
            gameOver = true;
            gameTimer.Stop(); lockTimer.Stop(); softDropTimer.Stop();
        }
    }

    bool IsValid(int[][] cells, int x, int y)
    {
        foreach (var c in cells)
        {
            int nx = x + c[0], ny = y + c[1];
            if (nx < 0 || nx >= COLS || ny >= ROWS) return false;
            if (ny >= 0 && board[ny, nx] != 0) return false;
        }
        return true;
    }

    int GhostY()
    {
        int gy = pieceY;
        while (IsValid(piece, pieceX, gy + 1)) gy++;
        return gy;
    }

    void MoveHorizontal(int dir)
    {
        if (gameOver || paused) return;
        if (IsValid(piece, pieceX + dir, pieceY))
        {
            pieceX += dir;
            if (!IsValid(piece, pieceX, pieceY + 1)) { lockTimer.Stop(); lockTimer.Start(); }
            else lockTimer.Stop();
        }
    }

    void MoveDown()
    {
        if (IsValid(piece, pieceX, pieceY + 1)) { pieceY++; lockTimer.Stop(); }
        else { if (!lockTimer.Enabled) lockTimer.Start(); }
    }

    void LockPiece()
    {
        foreach (var c in piece)
        {
            int nx = pieceX + c[0], ny = pieceY + c[1];
            if (ny >= 0) board[ny, nx] = pieceType + 1;
        }
        ClearLines(); SpawnPiece();
    }

    void ClearLines()
    {
        int cleared = 0;
        for (int y = ROWS - 1; y >= 0; y--)
        {
            bool full = true;
            for (int x = 0; x < COLS; x++) if (board[y, x] == 0) { full = false; break; }
            if (full)
            {
                for (int yy = y; yy > 0; yy--)
                    for (int x = 0; x < COLS; x++) board[yy, x] = board[yy - 1, x];
                for (int x = 0; x < COLS; x++) board[0, x] = 0;
                cleared++; y++;
            }
        }
        int[] pts = new int[] { 0, 100, 300, 500, 800 };
        if (cleared > 0)
        {
            score += pts[Math.Min(cleared, 4)] * level;
            lines += cleared; level = lines / 10 + 1;
            gameTimer.Interval = Math.Max(150, 800 - (level - 1) * 50);
        }
    }

    bool TryRotate(int dir)
    {
        int newRot = (pieceRot + dir + 4) % 4;
        int[][] newCells = shapes[pieceType][newRot];
        if (pieceType == 1) { pieceRot = newRot; piece = newCells; return true; }
        int[,,] kicks = (pieceType == 0) ? wallKickI : wallKickJLSTZ;
        int kickRow = (dir == 1) ? pieceRot : newRot;
        for (int k = 0; k < 5; k++)
        {
            int dx = kicks[kickRow, k, 0] * dir;
            int dy = -kicks[kickRow, k, 1] * dir;
            if (IsValid(newCells, pieceX + dx, pieceY + dy))
            {
                pieceX += dx; pieceY += dy;
                pieceRot = newRot; piece = newCells;
                if (!IsValid(piece, pieceX, pieceY + 1)) { lockTimer.Stop(); lockTimer.Start(); }
                else lockTimer.Stop();
                return true;
            }
        }
        return false;
    }

    void HardDrop() { pieceY = GhostY(); lockTimer.Stop(); LockPiece(); }

    void Hold()
    {
        if (holdUsed) return;
        holdUsed = true; lockTimer.Stop();
        if (holdType == -1) { holdType = pieceType; SpawnPiece(); }
        else
        {
            int tmp = holdType; holdType = pieceType; pieceType = tmp;
            pieceRot = 0; pieceX = 3; pieceY = 0;
            piece = shapes[pieceType][pieceRot];
        }
    }

    void TogglePause()
    {
        if (gameOver) return;
        paused = !paused;
        if (paused) { gameTimer.Stop(); lockTimer.Stop(); softDropTimer.Stop(); dasTimer.Stop(); arrTimer.Stop(); }
        else gameTimer.Start();
        Invalidate();
    }

    void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.R) { if (!retryTimer.Enabled) retryTimer.Start(); return; }
        if (e.KeyCode == Keys.E) { TogglePause(); return; }
        if (gameOver || paused) return;
        switch (e.KeyCode)
        {
            case Keys.Left:  dasDir = -1; MoveHorizontal(-1); dasTimer.Stop(); arrTimer.Stop(); dasTimer.Start(); break;
            case Keys.Right: dasDir =  1; MoveHorizontal( 1); dasTimer.Stop(); arrTimer.Stop(); dasTimer.Start(); break;
            case Keys.Down:  MoveDown(); if (!softDropTimer.Enabled) softDropTimer.Start(); break;
            case Keys.Up:    TryRotate(1); break;
            case Keys.Z:     TryRotate(-1); break;
            case Keys.Space: HardDrop(); break;
            case Keys.C:     Hold(); break;
        }
        Invalidate();
    }

    void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.R) { retryTimer.Stop(); retryHoldMs = 0; Invalidate(); }
        if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right) { dasTimer.Stop(); arrTimer.Stop(); dasDir = 0; }
        if (e.KeyCode == Keys.Down) softDropTimer.Stop();
    }

    void DrawMino(Graphics g, int type, int boxX, int boxY, int boxW, int boxH, int cs)
    {
        var cells = shapes[type][0];
        int minX = 99, maxX = -1, minY = 99, maxY = -1;
        foreach (var c in cells)
        {
            if (c[0] < minX) minX = c[0]; if (c[0] > maxX) maxX = c[0];
            if (c[1] < minY) minY = c[1]; if (c[1] > maxY) maxY = c[1];
        }
        int minoW = (maxX - minX + 1) * cs;
        int minoH = (maxY - minY + 1) * cs;
        int offX = boxX + (boxW - minoW) / 2 - minX * cs;
        int offY = boxY + (boxH - minoH) / 2 - minY * cs;
        foreach (var c in cells)
            DrawCell(g, offX + c[0] * cs, offY + c[1] * cs, colors[type], cs);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Color.Black);
        int bx = BOARD_X;

        for (int y = 0; y < ROWS; y++)
            for (int x = 0; x < COLS; x++)
            {
                if (board[y, x] != 0)
                    DrawCell(g, bx + x * CELL, y * CELL, colors[board[y, x] - 1], CELL);
                else
                    g.DrawRectangle(Pens.DimGray, bx + x * CELL, y * CELL, CELL, CELL);
            }

        if (paused)
        {
            g.FillRectangle(new SolidBrush(Color.Black), bx, 0, PANEL_W, ROWS * CELL);
            g.DrawRectangle(new Pen(Color.DimGray), bx, 0, PANEL_W - 1, ROWS * CELL - 1);
            var pf = new Font("Arial", 28, FontStyle.Bold);
            var ps = new Font("Arial", 12);
            SizeF sz  = g.MeasureString("PAUSED", pf);
            SizeF sz2 = g.MeasureString("E: 再開", ps);
            g.DrawString("PAUSED", pf, Brushes.White, bx + (PANEL_W - sz.Width)  / 2, ROWS * CELL / 2 - 30);
            g.DrawString("E: 再開", ps, Brushes.Gray,  bx + (PANEL_W - sz2.Width) / 2, ROWS * CELL / 2 + 10);
        }
        else
        {
            int gy = GhostY();
            foreach (var c in piece)
            {
                int gx2 = pieceX + c[0], gyy = gy + c[1];
                if (gyy >= 0)
                {
                    Color gc = colors[pieceType];
                    g.FillRectangle(new SolidBrush(Color.FromArgb(60, gc)),  bx + gx2 * CELL + 1, gyy * CELL + 1, CELL - 2, CELL - 2);
                    g.DrawRectangle(new Pen(Color.FromArgb(200, gc), 2),      bx + gx2 * CELL + 1, gyy * CELL + 1, CELL - 2, CELL - 2);
                }
            }
            foreach (var c in piece)
            {
                int px = pieceX + c[0], py = pieceY + c[1];
                if (py >= 0) DrawCell(g, bx + px * CELL, py * CELL, colors[pieceType], CELL);
            }
        }

        var f9  = new Font("Arial", 9);
        var f11 = new Font("Arial", 11, FontStyle.Bold);
        var borderPen = new Pen(Color.Gray, 1);

        int lx = 6, lw = BOARD_X - 12, cy = 4;
        g.DrawString("HOLD", f9, Brushes.White, lx, cy); cy += 14;
        int holdBoxH = HC * 3 + 4;
        g.DrawRectangle(borderPen, lx, cy, lw, holdBoxH);
        if (holdType != -1) DrawMino(g, holdType, lx, cy, lw, holdBoxH, HC);
        cy += holdBoxH + 12;
        g.DrawString("SCORE", f9, Brushes.White, lx, cy); cy += 15;
        g.DrawString(score.ToString(), f11, Brushes.Yellow, lx, cy); cy += 26;
        g.DrawString("LEVEL", f9, Brushes.White, lx, cy); cy += 15;
        g.DrawString(level.ToString(), f11, Brushes.Yellow, lx, cy); cy += 26;
        g.DrawString("LINES", f9, Brushes.White, lx, cy); cy += 15;
        g.DrawString(lines.ToString(), f11, Brushes.Yellow, lx, cy); cy += 30;
        g.DrawString("R長押: リトライ", new Font("Arial", 7), Brushes.DimGray, lx, cy); cy += 13;
        g.DrawString("E: 一時停止",     new Font("Arial", 7), Brushes.DimGray, lx, cy);

        int rx = bx + PANEL_W + 6, rw = SIDE_W - 12, ry = 4;
        g.DrawString("NEXT", f9, Brushes.White, rx, ry); ry += 14;
        int nextBoxH = NC * 3 * 5 + 4;
        g.DrawRectangle(borderPen, rx, ry, rw, nextBoxH);
        var nextArr = nextQueue.ToArray();
        for (int n = 0; n < Math.Min(5, nextArr.Length); n++)
            DrawMino(g, nextArr[n], rx, ry + n * NC * 3, rw, NC * 3, NC);

        if (retryHoldMs > 0)
        {
            float prog = (float)retryHoldMs / RETRY_HOLD_MS;
            g.FillRectangle(Brushes.DimGray,  lx, ROWS * CELL - 18, lw, 8);
            g.FillRectangle(Brushes.OrangeRed, lx, ROWS * CELL - 18, (int)(lw * prog), 8);
            g.DrawString("RETRY...", new Font("Arial", 7), Brushes.OrangeRed, lx, ROWS * CELL - 30);
        }

        if (gameOver)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(170, 0, 0, 0)), bx, 0, PANEL_W, ROWS * CELL);
            var gof = new Font("Arial", 24, FontStyle.Bold);
            var gos = new Font("Arial", 11);
            SizeF s1 = g.MeasureString("GAME OVER", gof);
            SizeF s2 = g.MeasureString("R長押しでリトライ", gos);
            g.DrawString("GAME OVER",       gof, Brushes.Red,   bx + (PANEL_W - s1.Width) / 2, ROWS * CELL / 2 - 30);
            g.DrawString("R長押しでリトライ", gos, Brushes.White, bx + (PANEL_W - s2.Width) / 2, ROWS * CELL / 2 + 10);
        }
    }

    void DrawCell(Graphics g, int x, int y, Color c, int size)
    {
        g.FillRectangle(new SolidBrush(c), x + 1, y + 1, size - 2, size - 2);
        g.DrawRectangle(new Pen(Color.FromArgb(180, 255, 255, 255)), x + 1, y + 1, size - 2, size - 2);
    }

    [STAThread]
    static void Main()
    {
        Application.Run(new TetrisForm());
    }
}
