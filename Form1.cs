namespace winforms;

public partial class Form1 : System.Windows.Forms.Form
{

    DoubleBufferedTableLayoutPanel dynamicTableLayoutPanel = new DoubleBufferedTableLayoutPanel();
    ToolStrip toolStrip;
    int [] connections;

    private const int EMPTY = 0;
    private const int UP = 1;
    private const int DOWN = 2;
    private const int LEFT = 4;
    private const int RIGHT = 8;
    private const int NOCON = 16;

    public Form1()
    {
        InitializeComponent();

        toolStrip = new ToolStrip();
        toolStrip.AutoSize = false;
        toolStrip.Size = new Size(32,32);
        toolStrip.Dock = DockStyle.Top;

        ToolStripButton toolNoConButton = new ToolStripButton();
        toolNoConButton.Name = "NOCON";
        toolNoConButton.AutoSize = false;
        toolNoConButton.Size = new Size(32,32);
        toolNoConButton.CheckOnClick = true;
        toolNoConButton.Image = Image.FromFile(@"nocon.ico");
        toolNoConButton.ImageScaling = ToolStripItemImageScaling.None;
        toolNoConButton.Click += toolStripMenuItem_Click;

        ToolStripButton toolCoilButton = new ToolStripButton();
        toolCoilButton.Name = "COIL";
        toolCoilButton.AutoSize = false;
        toolCoilButton.Size = new Size(32,32);
        toolCoilButton.CheckOnClick = true;
        toolCoilButton.Image = Image.FromFile(@"coil.ico");
        toolCoilButton.ImageScaling = ToolStripItemImageScaling.None;
        toolCoilButton.Click += toolStripMenuItem_Click;

        toolStrip.Items.Add(toolNoConButton);
        toolStrip.Items.Add(toolCoilButton);

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
        var topLeft = e.CellBounds.Location;
        var topRight = new Point(e.CellBounds.Right, e.CellBounds.Top);
        var bottomRight= new Point(e.CellBounds.Right, e.CellBounds.Bottom);

        Pen p = new Pen(Color.Black);
        p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
        e.Graphics.DrawLine(p, topLeft, topRight);
        e.Graphics.DrawLine(p, topRight, bottomRight);

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
        }
    }

    private bool canConnect(Point cellPos){
        /*if(cellPos.Y == 0)
            return true;
        if(cellPos.Y >=1 && connections[(cellPos.Y-1)*11] != EMPTY)
            return true;*/
        return true;
    }

    private void connect(Point cellPos, Point mousePos){

        if(cellPos.X % 2 == 0){
            int[] heights = dynamicTableLayoutPanel.GetRowHeights();

            if(connections[cellPos.Y*11+cellPos.X] != EMPTY){
                //connections[cellPos.Y*11+cellPos.X] = LEFT | RIGHT;
                if(mousePos.Y<(heights[0]/2)){

                    //if(connections[cellPos.Y*11+cellPos.X-1] != EMPTY || connections[cellPos.Y*11+cellPos.X+1] != EMPTY){
                        Console.WriteLine(cellPos.Y);
                        for(int i = cellPos.Y-1; i>=0; i--){
                            
                            if(connections[i*11+cellPos.X] != EMPTY){
                                Console.WriteLine(i);
                                connections[i*11+cellPos.X] |= DOWN;
                                int j;
                                for(j = i+1; j<cellPos.Y; j++){
                                    Console.WriteLine(j);
                                    connections[j*11+cellPos.X] |= UP | DOWN;
                                }
                                connections[j*11+cellPos.X] |= UP;
                                break;
                            }
                        }
                    //}
                    /*if(cellPos.Y>0 && cellPos.Y<5 && connections[(cellPos.Y-1)*11+cellPos.X] != EMPTY){
                        connections[cellPos.Y*11+cellPos.X] |= UP;
                        connections[(cellPos.Y-1)*11+cellPos.X] |= DOWN;
                    }*/
                }else{
                    /*if(cellPos.Y>=0 && cellPos.Y+1<5 && connections[(cellPos.Y+1)*11+cellPos.X] != EMPTY){
                        connections[cellPos.Y*11+cellPos.X] |= DOWN;
                        connections[(cellPos.Y+1)*11+cellPos.X] |= UP;
                    }*/
                    Console.WriteLine(cellPos.Y);
                    for(int i = cellPos.Y+1; i<5/*!!!*/; i++){
                            
                        if(connections[i*11+cellPos.X] != EMPTY){
                            Console.WriteLine(i);
                            connections[cellPos.Y*11+cellPos.X] |= DOWN;
                            int j;
                            for(j = cellPos.Y+1; j<i; j++){
                                Console.WriteLine(j);
                                connections[j*11+cellPos.X] |= UP | DOWN;
                            }
                            connections[j*11+cellPos.X] |= UP;
                            break;
                        }
                    }    
                }
            }
        }
        
         if(cellPos.X % 2 != 0){
            //if(cellPos.Value.Y == 0 && cellPos.Value.X == 1)
            /*for(int i=0; i<cellPos.X; i++){
                if(connections[cellPos.Y*11+i] < NOCON){
                    connections[cellPos.Y*11+i] |= LEFT | RIGHT;
                }
                
            }*/
            connections[cellPos.Y*11+cellPos.X-1] |= RIGHT;
            connections[cellPos.Y*11+cellPos.X] = NOCON;
            connections[cellPos.Y*11+cellPos.X+1] |= LEFT;
        }                
    }

    private void tableLayout_MouseClick(object sender, MouseEventArgs e){

        var cellPos = GetRowColIndex(dynamicTableLayoutPanel, dynamicTableLayoutPanel.PointToClient(Cursor.Position));
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
                    ContextMenuStrip m = new ContextMenuStrip();
                    m.Items.Add(new ToolStripMenuItem("Ladder"));
                    m.Show((Control)(sender), e.Location);
                }
            }
        }
        else if(e.Button == MouseButtons.Left){
            if (cellPos.HasValue && cellPos.Value.X > 0 && cellPos.Value.X < 10) 
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
        //Point p = dynamicTableLayoutPanel.PointToClient(Cursor.Position);
        //e.Graphics.DrawLine(new Pen(Color.Black, 1), new PointF(p.X, p.Y), new PointF(p.X+10f,p.Y+112f));
    }

    /*protected override void OnPaint(PaintEventArgs e){
        base.OnPaint(e);
        e.Graphics.DrawLine(new Pen(Color.Black, 3), new PointF(100f,100f), new PointF(10f,112f));
    }*/

    private void Form1_MouseMove(object sender, MouseEventArgs e){
        var cellPos = GetRowColIndex(dynamicTableLayoutPanel, dynamicTableLayoutPanel.PointToClient(Cursor.Position));

        /*if (cellPos.HasValue && cellPos.Value.X > 0 && cellPos.Value.X < 10) 
            {
                if(cellPos.Value.X % 2 == 0){
                    if(Cursor.Current != Cursors.Cross)
                        Cursor.Current = Cursors.Cross;
                }
                else{
                    if(Cursor.Current != Cursors.Arrow)
                        Cursor.Current = Cursors.Arrow;
                }
            }*/
            
    }
}
