namespace winforms;

public partial class Form1 : System.Windows.Forms.Form
{

    DoubleBufferedTableLayoutPanel dynamicTableLayoutPanel = new DoubleBufferedTableLayoutPanel();
    ToolStripButton toolNoConButton;
    ToolStripButton toolCoilButton;
    ToolStripButton toolConButton;
    ToolStripButton toolDelButton;

    ToolStrip toolStrip;
    int [] connections;
    int ladObjectToDrop = -1;

    Point mouseClickedPos;

    private const int EMPTY = 0;
    private const int UP = 1;
    private const int DOWN = 2;
    private const int LEFT = 4;
    private const int RIGHT = 8;
    private const int CONN = 16;
    private const int NOCON = 32;
    private const int COIL = 64;

    public Form1()
    {
        InitializeComponent();

        toolStrip = new ToolStrip();
        toolStrip.AutoSize = false;
        toolStrip.Size = new Size(32,32);
        toolStrip.Dock = DockStyle.Top;

        toolNoConButton = new ToolStripButton();
        toolNoConButton.Name = "NOCON";
        toolNoConButton.AutoSize = false;
        toolNoConButton.Size = new Size(32,32);
        toolNoConButton.CheckOnClick = true;
        toolNoConButton.Image = Image.FromFile(@"nocon.ico");
        toolNoConButton.ImageScaling = ToolStripItemImageScaling.None;
        toolNoConButton.Click += toolStripMenuItem_Click;

        toolCoilButton = new ToolStripButton();
        toolCoilButton.Name = "COIL";
        toolCoilButton.AutoSize = false;
        toolCoilButton.Size = new Size(32,32);
        toolCoilButton.CheckOnClick = true;
        toolCoilButton.Image = Image.FromFile(@"coil.ico");
        toolCoilButton.ImageScaling = ToolStripItemImageScaling.None;
        toolCoilButton.Click += toolStripMenuItem_Click;

        toolConButton = new ToolStripButton();
        toolConButton.Name = "CON";
        toolConButton.AutoSize = false;
        toolConButton.Size = new Size(32,32);
        toolConButton.CheckOnClick = true;
        toolConButton.Image = Image.FromFile(@"conn.ico");
        toolConButton.ImageScaling = ToolStripItemImageScaling.None;
        toolConButton.Click += toolStripMenuItem_Click;

        toolDelButton = new ToolStripButton();
        toolDelButton.Name = "DEL";
        toolDelButton.AutoSize = false;
        toolDelButton.Size = new Size(32,32);
        toolDelButton.CheckOnClick = true;
        toolDelButton.Image = Image.FromFile(@"del.ico");
        toolDelButton.ImageScaling = ToolStripItemImageScaling.None;
        toolDelButton.Click += toolStripMenuItem_Click;

        toolStrip.Items.Add(toolNoConButton);
        toolStrip.Items.Add(toolCoilButton);
        toolStrip.Items.Add(toolConButton);
        toolStrip.Items.Add(toolDelButton);

        //MenuStrip menuStrip = new MenuStrip();
        //menuStrip.Dock = DockStyle.Top;
        this.Controls.Add(toolStrip);
        //menuStrip.Items.Add(item);

        dynamicTableLayoutPanel.Dock = DockStyle.Fill;
        dynamicTableLayoutPanel.ColumnCount = 11;
        dynamicTableLayoutPanel.RowCount = 5;
        dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2.5f));
        dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17f));
        dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2.5f));
        dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17f));
        dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2.5f));
        dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17f));
        dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2.5f));
        dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17f));
        dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2.5f));
        dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17f));
        dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2.5f));
        dynamicTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        dynamicTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        dynamicTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        dynamicTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        dynamicTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

        connections = new int[55];

        for(int i=0; i<55; i++){
            connections[i] = EMPTY;
        }

        for(int i=0; i<5; i++){
            connections[i*11] = UP | DOWN;
            connections[i*11+10] = UP | DOWN;
        }
        
        
        dynamicTableLayoutPanel.CellPaint += tableLayoutPanel_CellPaint;
        dynamicTableLayoutPanel.MouseClick += tableLayout_MouseClick;
        this.Controls.Add(dynamicTableLayoutPanel);
        dynamicTableLayoutPanel.BringToFront();
        dynamicTableLayoutPanel.MouseMove += Form1_MouseMove;

        dynamicTableLayoutPanel.Paint += Form1_Paint;

        this.BackColor = Color.White;
    }

    private void toolStripMenuItem_Click(object sender, EventArgs e){
        if(sender == toolNoConButton){
            ladObjectToDrop = NOCON;
        }else if(sender == toolCoilButton){
            ladObjectToDrop = COIL;
        }else if(sender == toolConButton){
            ladObjectToDrop = CONN;
        }
        else if(sender == toolDelButton){
            ladObjectToDrop = EMPTY;
        }
        UncheckOtherToolStripMenuItems((ToolStripItem)sender);
    }

    private void UncheckOtherToolStripMenuItems(object selectedMenuItem){
        foreach(ToolStripItem tb in toolStrip.Items){
            if(tb is ToolStripButton){
                ToolStripButton tbs = ((ToolStripButton)tb);
                if(tbs != ((ToolStripButton)selectedMenuItem))
                    tbs.Checked = false;
            }
        }
    }

    Point? GetRowColIndex(TableLayoutPanel tlp, Point point)
    {
        if (point.X > tlp.Width || point.Y > tlp.Height)
            return null;

        int w = tlp.Width;
        int h = tlp.Height;
        int[] widths = tlp.GetColumnWidths();

        int i;
        for (i = widths.Length - 1; i >= 0 && point.X < w; i--)
            w -= widths[i];
        int col = i + 1;

        int[] heights = tlp.GetRowHeights();
        for (i = heights.Length - 1; i >= 0 && point.Y < h; i--)
            h -= heights[i];

        int row = i + 1;

        return new Point(col, row);
    }

    Rectangle? GetCellConnectionBounds(TableLayoutPanel tlp, Point point)
    {
        if (point.X > tlp.Width || point.Y > tlp.Height)
            return null;

        int w = tlp.Width;
        int h = tlp.Height;
        int[] widths = tlp.GetColumnWidths();

        int i;
        for (i = widths.Length - 1; i >= 0 && point.X < w; i--)
            w -= widths[i];
        int col = i + 1;

        int[] heights = tlp.GetRowHeights();
        for (i = heights.Length - 1; i >= 0 && point.Y < h; i--)
            h -= heights[i];

        int row = i + 1;

        if(col % 2 == 0){
            if(col>0)
                return new Rectangle(w-widths[col-1], h, widths[col]+widths[col-1], heights[row]);
        }
        else{
            return new Rectangle(w, h, widths[col]+widths[col+1], heights[row]);
        }

        return null;
    }

    Point? GetCellLocalPos(TableLayoutPanel tlp, Point point)
    {
        if (point.X > tlp.Width || point.Y > tlp.Height)
            return null;

        int w = tlp.Width;
        int h = tlp.Height;
        int[] widths = tlp.GetColumnWidths();

        int i;
        for (i = widths.Length - 1; i >= 0 && point.X < w; i--)
            w -= widths[i];
        //int col = i + 1;

        int[] heights = tlp.GetRowHeights();
        for (i = heights.Length - 1; i >= 0 && point.Y < h; i--)
            h -= heights[i];

        //int row = i + 1;

        return new Point(point.X-w, point.Y-h);
    }

    private void tableLayoutPanel_CellPaint(object sender, TableLayoutCellPaintEventArgs e){
        int[] widths = dynamicTableLayoutPanel.GetColumnWidths();
        int[] heights = dynamicTableLayoutPanel.GetRowHeights();
        //var clickedCellPos = GetRowColIndex(dynamicTableLayoutPanel, dynamicTableLayoutPanel.PointToClient(Cursor.Position));
        var cellPos = GetRowColIndex(dynamicTableLayoutPanel, e.CellBounds.Location);
        /*var topLeft = e.CellBounds.Location;
        var topRight = new Point(e.CellBounds.Right, e.CellBounds.Top);
        var bottomRight = new Point(e.CellBounds.Right, e.CellBounds.Bottom);
        var bottomLeft = new Point(e.CellBounds.Left, e.CellBounds.Bottom);
        Pen p = new Pen(Color.Black);
        p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;*/

        if(e.CellBounds.Contains(mouseClickedPos)){
            var rect = GetCellConnectionBounds(dynamicTableLayoutPanel, e.CellBounds.Location);
            if(rect.HasValue){
                Pen p = new Pen(Color.Black);
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawRectangle(p, rect.Value);
            }
        }

        

        //if (clickedCellPos.HasValue && clickedCellPos.Value.X > 0 && clickedCellPos.Value.X < 10 &&
        if(cellPos.HasValue){//} && cellPos.Value.X == clickedCellPos.Value.X && cellPos.Value.Y == clickedCellPos.Value.Y) {

            int conn = connections[cellPos.Value.Y*11+cellPos.Value.X];
            int cellPosX = (int)cellPos.Value.X;
            int cellPosY = (int)cellPos.Value.Y;
            if((conn & UP) == UP){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]/2f,e.CellBounds.Top), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]/2f,e.CellBounds.Top+heights[cellPosY]/2f));
            }
            if((conn & DOWN) == DOWN){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]/2f,e.CellBounds.Top+heights[cellPosY]/2f), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]/2f,e.CellBounds.Top+heights[cellPosY]));
            }
            if((conn & LEFT) == LEFT){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left,e.CellBounds.Top+heights[cellPosY]/2f), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]/2f,e.CellBounds.Top+heights[cellPosY]/2f));
            }
            if((conn & RIGHT) == RIGHT){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]/2f,e.CellBounds.Top+heights[cellPosY]/2f), 
                                    new PointF(e.CellBounds.Right,e.CellBounds.Top+heights[cellPosY]/2f));
            }
            if((conn & NOCON) == NOCON){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left,e.CellBounds.Top+heights[cellPosY]*0.5f), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]*0.44f,e.CellBounds.Top+heights[cellPosY]*0.5f));
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]*0.56f,e.CellBounds.Top+heights[cellPosY]*0.5f), 
                                    new PointF(e.CellBounds.Right,e.CellBounds.Top+heights[cellPosY]*0.5f));
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]*0.44f,e.CellBounds.Top+heights[cellPosY]*0.44f), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]*0.44f,e.CellBounds.Top+heights[cellPosY]*0.56f));
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]*0.56f,e.CellBounds.Top+heights[cellPosY]*0.44f), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]*0.56f,e.CellBounds.Top+heights[cellPosY]*0.56f));                    
            }
            if((conn & COIL) == COIL){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left,e.CellBounds.Top+heights[cellPosY]*0.5f), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]*0.44f,e.CellBounds.Top+heights[cellPosY]*0.5f));
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]*0.56f,e.CellBounds.Top+heights[cellPosY]*0.5f), 
                                    new PointF(e.CellBounds.Right,e.CellBounds.Top+heights[cellPosY]*0.5f));

                Rectangle rect = new Rectangle((int)(e.CellBounds.Left+widths[cellPosX]*0.44f), (int)(e.CellBounds.Top+heights[cellPosY]*0.44f),
                                                (int)(widths[cellPosX]*0.08f),(int)(heights[cellPosY]*0.12f));

                e.Graphics.DrawArc(Pens.Blue, rect, 90f, 180f);

                rect = new Rectangle((int)(e.CellBounds.Left+widths[cellPosX]*0.48f), (int)(e.CellBounds.Top+heights[cellPosY]*0.44f),
                                                (int)(widths[cellPosX]*0.08f),(int)(heights[cellPosY]*0.12f));    

                e.Graphics.DrawArc(Pens.Blue, rect, 90f, -180f);             
                /*e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]*0.44f,e.CellBounds.Top+heights[cellPosY]*0.44f), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]*0.44f,e.CellBounds.Top+heights[cellPosY]*0.56f));
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]*0.56f,e.CellBounds.Top+heights[cellPosY]*0.44f), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]*0.56f,e.CellBounds.Top+heights[cellPosY]*0.56f));*/               
            }
            if((conn & CONN) == CONN){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left,e.CellBounds.Top+heights[cellPosY]*0.5f), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX],e.CellBounds.Top+heights[cellPosY]*0.5f));
            }
        }
    }

    private bool canConnect(Point cellPos){
        /*if(cellPos.Y == 0)
            return true;
        if(cellPos.Y >=1 && connections[(cellPos.Y-1)*11] != EMPTY)
            return true;*/
        return true;
    }

    private void connectAbove(int x, int y1, int y2){
        if(y2<0)return;
        if((((connections[y2*11+x] & LEFT) == LEFT) || ((connections[y2*11+x] & RIGHT) == RIGHT))  && y1 != y2){
            connections[y2*11+x] |= DOWN;
            return;
        }
        if(y1 != y2){
            connections[y2*11+x] |= UP;
            connections[y2*11+x] |= DOWN;
        }else{
            connections[y2*11+x] |= UP;
        }
        connectAbove(x, y1, y2-1);
    }

    private void connectBelow(int x, int y1, int y2){
        if(y2>(5-1))return;
        if((((connections[y2*11+x] & LEFT) == LEFT) || ((connections[y2*11+x] & RIGHT) == RIGHT))  && y1 != y2){
            connections[y2*11+x] |= UP;
            return;
        }
        if(y1 != y2){
            connections[y2*11+x] |= UP;
            connections[y2*11+x] |= DOWN;
        }else{
            connections[y2*11+x] |= DOWN;
        }
        connectBelow(x, y1, y2+1);
    }

    private void disconnectAbove(int x, int y1, int y2){
        if(y2<0)return;
        if((((connections[y2*11+x] & LEFT) == LEFT) || ((connections[y2*11+x] & RIGHT) == RIGHT))  && y1 != y2){
            connections[y2*11+x] &= ~DOWN;
            return;
        }
        connections[y2*11+x] &= ~UP;
        connections[y2*11+x] &= ~DOWN;
        disconnectAbove(x, y1, y2-1);
    }

    private void disconnectBelow(int x, int y1, int y2){
        if(y2>(5-1))return;
        if((((connections[y2*11+x] & LEFT) == LEFT) || ((connections[y2*11+x] & RIGHT) == RIGHT)) && y1 != y2){
            connections[y2*11+x] &= ~UP;
            return;
        }
        connections[y2*11+x] &= ~UP;
        connections[y2*11+x] &= ~DOWN;
        disconnectBelow(x, y1, y2+1);
    }

    private void connect(Point cellPos, Point mousePos){

        if(ladObjectToDrop >= 0){
            if(ladObjectToDrop == COIL){//or in table of objects that need to be last in line
                
                for(int i=8; i>=0; i--){
                    if(connections[cellPos.Y*11+i] == EMPTY){
                         if(i % 2 == 0){
                             connections[cellPos.Y*11+i] |= RIGHT | LEFT;
                         }else{
                             connections[cellPos.Y*11+i] |= CONN;
                         }
                    }else{
                        connections[cellPos.Y*11+i] |= RIGHT;
                        break;
                    }
                }
                
                connections[cellPos.Y*11+9] = ladObjectToDrop;
                connections[cellPos.Y*11+10] |= LEFT;
            }else{
                int x = cellPos.X;
                int[] heights = dynamicTableLayoutPanel.GetRowHeights();

                if(x % 2 == 0){
                    if(ladObjectToDrop == CONN){
                        if(connections[cellPos.Y*11+x] != EMPTY){
                            if(mousePos.Y<(heights[0]/2)){
                                connectAbove(cellPos.X, cellPos.Y, cellPos.Y);
                            }else{
                                connectBelow(cellPos.X, cellPos.Y, cellPos.Y);
                            }
                        }
                        return;
                    }else if(ladObjectToDrop == EMPTY){
                        disconnectAbove(cellPos.X, cellPos.Y, cellPos.Y);
                        disconnectBelow(cellPos.X, cellPos.Y, cellPos.Y);
                    }
                }
                
                if(cellPos.X % 2 == 0){
                    x -=1;
                }

                if((connections[cellPos.Y*11+x] == EMPTY || connections[cellPos.Y*11+x] == CONN) && ladObjectToDrop != EMPTY){

                    if(ladObjectToDrop == CONN && connections[cellPos.Y*11+x-1] == EMPTY && connections[cellPos.Y*11+x+1] == EMPTY){
                        return;
                    }
                    connections[cellPos.Y*11+x-1] |= RIGHT;
                    connections[cellPos.Y*11+x+1] |= LEFT;
                    connections[cellPos.Y*11+x] = ladObjectToDrop;
                }

                if(ladObjectToDrop == EMPTY && cellPos.X % 2 != 0){
                    connections[cellPos.Y*11+cellPos.X-1] &= ~RIGHT;
                    connections[cellPos.Y*11+cellPos.X+1] &= ~LEFT;
                    connections[cellPos.Y*11+cellPos.X] = EMPTY;
                }

            }
        }
    }

    private void tableLayout_MouseClick(object sender, MouseEventArgs e){

        mouseClickedPos = dynamicTableLayoutPanel.PointToClient(Cursor.Position);
        var cellPos = GetRowColIndex(dynamicTableLayoutPanel, mouseClickedPos);

        int[] widths = dynamicTableLayoutPanel.GetColumnWidths();
        int[] heights = dynamicTableLayoutPanel.GetRowHeights();

        //draw clicked cell dashed
        
        var mouseCellPos = GetCellLocalPos(dynamicTableLayoutPanel, dynamicTableLayoutPanel.PointToClient(Cursor.Position));
        if(e.Button == MouseButtons.Right)
        {
            if (cellPos.HasValue && cellPos.Value.X > 0 && cellPos.Value.X < 10) 
            {
                if(cellPos.Value.X % 2 == 0){
                    //var localMousePos = dynamicTableLayoutPanel.GetControlFromPosition(cellPos.Value.X, cellPos.Value.Y).PointToClient(Cursor.Position);
                    
                    /*ContextMenuStrip m = new ContextMenuStrip();
                    m.Items.Add(new ToolStripMenuItem("Connection "+mouseCellPos.Value.X+","+mouseCellPos.Value.Y));
                    m.Show((Control)(sender), e.Location);*/
                }
                else{
                    /*ContextMenuStrip m = new ContextMenuStrip();
                    m.Items.Add(new ToolStripMenuItem("Ladder"));
                    m.Show((Control)(sender), e.Location);*/
                }
            }
        }
        else if(e.Button == MouseButtons.Left){
            if (cellPos.HasValue && cellPos.Value.X > 0 && cellPos.Value.X <= 10) 
            {
                if(canConnect(cellPos.Value) && mouseCellPos.HasValue)
                    connect(cellPos.Value, mouseCellPos.Value);
            }
        }

        dynamicTableLayoutPanel.Invalidate();
    }

    private void Form1_Paint(object sender, PaintEventArgs e){
        //e.Graphics.DrawImage(bmp, 10,10);
        //base.OnPaint(e);

        if(ladObjectToDrop == CONN){

            Point p = dynamicTableLayoutPanel.PointToClient(Cursor.Position);

            var cellPos = GetRowColIndex(dynamicTableLayoutPanel, p);
            var mouseCellPos = GetCellLocalPos(dynamicTableLayoutPanel, dynamicTableLayoutPanel.PointToClient(Cursor.Position));
            int[] heights = dynamicTableLayoutPanel.GetRowHeights();

            if(cellPos.HasValue && cellPos.Value.X-1>=0)
            {
                if(cellPos.Value.X % 2 == 0){
                    if(mouseCellPos.HasValue && connections[cellPos.Value.Y*11+cellPos.Value.X-1] != EMPTY){
                        bool isConnection = false;
                        if(mouseCellPos.Value.Y<(heights[0]/2)){
                            for(int i = cellPos.Value.Y-1; i>=0; i--)
                                if(connections[i*11+cellPos.Value.X] != EMPTY){isConnection=true;break;}
                        }else{
                            for(int i = cellPos.Value.Y+1; i<5/*!!!*/; i++)
                                if(connections[i*11+cellPos.Value.X] != EMPTY){isConnection=true;break;}
                        }

                        if(isConnection)
                            e.Graphics.DrawLine(new Pen(Color.Black, 1), new PointF(p.X+Cursor.Size.Width / 2, p.Y+Cursor.Size.Height / 2), 
                                new PointF(p.X+Cursor.Size.Width / 2,p.Y+Cursor.Size.Height));
                    }
                }else if(cellPos.Value.X % 2 != 0 && connections[cellPos.Value.Y*11+cellPos.Value.X-1] != EMPTY){
                    e.Graphics.DrawLine(new Pen(Color.Black, 1), new PointF(p.X, p.Y+Cursor.Size.Height), 
                        new PointF(p.X+Cursor.Size.Width/2,p.Y+Cursor.Size.Height));
                }
            }
            
        }
    }

    /*protected override void OnPaint(PaintEventArgs e){
        base.OnPaint(e);
        e.Graphics.DrawLine(new Pen(Color.Black, 3), new PointF(100f,100f), new PointF(10f,112f));
    }*/

    private void Form1_MouseMove(object sender, MouseEventArgs e){
        dynamicTableLayoutPanel.Invalidate();
    }
}
