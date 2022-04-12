namespace winforms;

using System.Text;

public partial class Form1 : System.Windows.Forms.Form
{

    DoubleBufferedTableLayoutPanel dynamicTableLayoutPanel = new DoubleBufferedTableLayoutPanel();
    ToolStripButton toolNoConButton;
    ToolStripButton toolCoilButton;
    ToolStripButton toolConButton;
    ToolStripButton toolDelButton;
    ToolStripButton toolBuildButton;

    ToolStrip toolStrip;
    int [] connections;
    int ladObjectToDrop = -1;

    List<List<int>> child = null;
    List<List<int>> parent = null;

    Point mouseClickedPos;

    private const int EMPTY = 0;
    private const int UP = 1;
    private const int DOWN = 2;
    private const int LEFT = 3;
    private const int RIGHT = 4;
    private const int CONN = 5;
    private const int NOCON = 6;
    private const int COIL = 7;

    private const int END = 32;

    private string [] objStr = {"EMPTY", "UP", "DOWN", "LEFT", "RIGHT", "CONN", "NOCON", "COIL"};

    private const int ROWS = 6;
    private const int COLS = 13;//must be odd

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

        toolBuildButton = new ToolStripButton();
        toolBuildButton.Name = "BUILD";
        toolBuildButton.AutoSize = false;
        toolBuildButton.Size = new Size(32,32);
        toolBuildButton.CheckOnClick = true;
        toolBuildButton.Image = Image.FromFile(@"build.ico");
        toolBuildButton.ImageScaling = ToolStripItemImageScaling.None;
        toolBuildButton.Click += build_Click;

        toolStrip.Items.Add(toolNoConButton);
        toolStrip.Items.Add(toolCoilButton);
        toolStrip.Items.Add(toolConButton);
        toolStrip.Items.Add(toolDelButton);
        toolStrip.Items.Add(toolBuildButton);

        //MenuStrip menuStrip = new MenuStrip();
        //menuStrip.Dock = DockStyle.Top;
        this.Controls.Add(toolStrip);
        //menuStrip.Items.Add(item);

        dynamicTableLayoutPanel.Dock = DockStyle.Fill;
        dynamicTableLayoutPanel.ColumnCount = COLS;
        dynamicTableLayoutPanel.RowCount = ROWS;

        for(int i=0; i<COLS/2; i++){
            dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100/COLS*0.1f));
            dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100/COLS*0.9f));
        }

        dynamicTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100/COLS*0.1f));

        for(int i=0; i<ROWS; i++){
            dynamicTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100/ROWS));
        }

        connections = new int[ROWS*COLS];
        child = new List<List<int>>();
        parent = new List<List<int>>();
        for(int i=0; i<ROWS*COLS; i++){
            connections[i] = EMPTY;
            child.Add(new List<int>());
            parent.Add(new List<int>());
        }

        for(int i=0; i<ROWS; i++){
            connections[i*COLS] = 1<<UP | 1<<DOWN;
            connections[i*COLS+COLS-1] = 1<<UP | 1<<DOWN;
        }

        connections[0] |= 1<<0;
        connections[12] |= 1<<END;
        
        
        dynamicTableLayoutPanel.CellPaint += tableLayoutPanel_CellPaint;
        dynamicTableLayoutPanel.MouseClick += tableLayout_MouseClick;
        this.Controls.Add(dynamicTableLayoutPanel);
        dynamicTableLayoutPanel.BringToFront();
        dynamicTableLayoutPanel.MouseMove += Form1_MouseMove;

        dynamicTableLayoutPanel.Paint += Form1_Paint;

        this.BackColor = Color.White;
        this.Resize += Form1_Resize;
    }

    private bool isBitSet(int val, int bit){
        return ((val & 1<<bit) == 1<<bit);
    }

    private void Form1_Resize(object sender, System.EventArgs e){
        //Console.WriteLine("resize");
        mouseClickedPos = new Point(0,0);
    }

    private void printConnectionInConsole(int c){
        
        StringBuilder sb = new StringBuilder("", 50);
        for(int i=0; i<8; i++){
            if(isBitSet(c, i)){
                sb.Append(objStr[i]);
                //sb.Append("|");
            }
        }
        Console.WriteLine(sb.ToString());
    }


    List<String> prog = new List<String>();

    void compile(int id){

        //Console.WriteLine(id);

        if(isBitSet(connections[id], COIL))
            prog.Add("= " + id);
        else if(isBitSet(connections[id], NOCON))
            prog.Add("A "+id);

        List<int> l = new List<int>();
        for(int i=0; i<ROWS*COLS; i++){
            if(parent[i].Contains(id)){
                l.Add(i);
            }
        } 

        if(l.Count() > 1)
            prog.Add(")");

        foreach(int c in l){
            compile(c);
        }

        if(l.Count() > 1)
            prog.Add("O(");
    }

    private void build_Click(object sender, EventArgs e){
        Console.WriteLine("build");

        for(int i=0; i<ROWS*COLS; i++){
            child[i].Clear();
            parent[i].Clear();
        }

        //for(int i=0; i<ROWS; i++){
            crawlLeft(COLS-1,0,COLS-1);
            //crawlDown(COLS-1,0,COLS-1);
        //}

        StringBuilder sb = new StringBuilder("", 50);

        for(int i=0; i<ROWS*COLS; i++){
            sb.Append(i + " p(" );

            foreach(int j in parent[i]){
                sb.Append(j + ",");
            }

            sb.Append( "): c[");

            foreach(int j in child[i]){
                sb.Append(j + ",");
            }
            sb.Append("]");
            Console.WriteLine(sb.ToString());
            sb.Clear();
        }

        Console.WriteLine();
        Console.WriteLine("Listing:");
        Console.WriteLine("-------------");
        prog.Clear();
        compile(12);
        prog.Reverse();
        foreach(String s in prog){
            Console.WriteLine(s);
        }
    }

    private void toolStripMenuItem_Click(object sender, EventArgs e){
        if(sender == toolNoConButton){
            ladObjectToDrop = 1<<NOCON;
        }else if(sender == toolCoilButton){
            ladObjectToDrop = 1<<COIL;
        }else if(sender == toolConButton){
            ladObjectToDrop = 1<<CONN;
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

        
        if(cellPos.HasValue)
        {
            int n = e.Column+e.Row*COLS;
            e.Graphics.DrawString(String.Format("{0}",n), new Font("Arial", 10), Brushes.Black, e.CellBounds.Location);
        }

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

            int conn = connections[cellPos.Value.Y*COLS+cellPos.Value.X];
            int cellPosX = (int)cellPos.Value.X;
            int cellPosY = (int)cellPos.Value.Y;
            if(isBitSet(conn, UP)){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]/2f,e.CellBounds.Top), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]/2f,e.CellBounds.Top+heights[cellPosY]/2f));
            }
            if(isBitSet(conn, DOWN)){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]/2f,e.CellBounds.Top+heights[cellPosY]/2f), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]/2f,e.CellBounds.Top+heights[cellPosY]));
            }
            if(isBitSet(conn, LEFT)){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left,e.CellBounds.Top+heights[cellPosY]/2f), 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]/2f,e.CellBounds.Top+heights[cellPosY]/2f));
            }
            if(isBitSet(conn, RIGHT)){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[cellPosX]/2f,e.CellBounds.Top+heights[cellPosY]/2f), 
                                    new PointF(e.CellBounds.Right,e.CellBounds.Top+heights[cellPosY]/2f));
            }
            if(isBitSet(conn, NOCON)){
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
            if(isBitSet(conn, COIL)){
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
            if(isBitSet(conn, CONN)){
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
        if((isBitSet(connections[y2*COLS+x], LEFT) || isBitSet(connections[y2*COLS+x],RIGHT))  && (y1 != y2)){
            connections[y2*COLS+x] |= 1<<DOWN;
            return;
        }
        if(y1 != y2){
            connections[y2*COLS+x] |= 1<<UP;
            connections[y2*COLS+x] |= 1<<DOWN;
        }else{
            connections[y2*COLS+x] |= 1<<UP;
        }
        connectAbove(x, y1, y2-1);
    }

    private void connectBelow(int x, int y1, int y2){
        if(y2>(ROWS-1))return;
        if((isBitSet(connections[y2*COLS+x], LEFT) || isBitSet(connections[y2*COLS+x],RIGHT))  && (y1 != y2)){
            connections[y2*COLS+x] |= 1<<UP;
            return;
        }
        if(y1 != y2){
            connections[y2*COLS+x] |= 1<<UP;
            connections[y2*COLS+x] |= 1<<DOWN;
        }else{
            connections[y2*COLS+x] |= 1<<DOWN;
        }
        connectBelow(x, y1, y2+1);
    }

    private void disconnectAbove(int x, int y1, int y2){
        if(y2<0)return;
        if((isBitSet(connections[y2*COLS+x], LEFT) || isBitSet(connections[y2*COLS+x],RIGHT))  && (y1 != y2)){
            connections[y2*COLS+x] &= ~(1<<DOWN);
            return;
        }
        connections[y2*COLS+x] &= ~(1<<UP);
        connections[y2*COLS+x] &= ~(1<<DOWN);
        disconnectAbove(x, y1, y2-1);
    }

    private void disconnectBelow(int x, int y1, int y2){
        if(y2>(ROWS-1))return;
        if((isBitSet(connections[y2*COLS+x], LEFT) || isBitSet(connections[y2*COLS+x],RIGHT))  && (y1 != y2)){
            connections[y2*COLS+x] &= ~(1<<UP);
            return;
        }
        connections[y2*COLS+x] &= ~(1<<UP);
        connections[y2*COLS+x] &= ~(1<<DOWN);
        disconnectBelow(x, y1, y2+1);
    }

    private void connect(Point cellPos, Point mousePos){

        if(ladObjectToDrop >= 0){
            if(ladObjectToDrop == 1<<COIL){//or in table of objects that need to be last in line
                
                for(int i=COLS-3; i>=0; i--){
                    if(connections[cellPos.Y*COLS+i] == EMPTY){
                         if(i % 2 == 0){
                             connections[cellPos.Y*COLS+i] |= (1<<RIGHT) | (1<<LEFT);
                         }else{
                             connections[cellPos.Y*COLS+i] |= 1<<CONN;
                         }
                    }else{
                        connections[cellPos.Y*COLS+i] |= 1<<RIGHT;
                        break;
                    }
                }
                
                connections[cellPos.Y*COLS+COLS-2] = ladObjectToDrop;
                connections[cellPos.Y*COLS+COLS-1] |= 1<<LEFT;
            }else{
                int x = cellPos.X;
                int[] heights = dynamicTableLayoutPanel.GetRowHeights();

                if(x % 2 == 0){
                    if(ladObjectToDrop == 1<<CONN){
                        if(connections[cellPos.Y*COLS+x] != EMPTY){
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

                if((connections[cellPos.Y*COLS+x] == EMPTY || connections[cellPos.Y*COLS+x] == 1<<CONN) && ladObjectToDrop != EMPTY){

                    if(ladObjectToDrop == 1<<CONN && connections[cellPos.Y*COLS+x-1] == EMPTY && connections[cellPos.Y*COLS+x+1] == EMPTY){
                        return;
                    }
                    connections[cellPos.Y*COLS+x-1] |= 1<<RIGHT;
                    connections[cellPos.Y*COLS+x+1] |= 1<<LEFT;
                    connections[cellPos.Y*COLS+x] = ladObjectToDrop;
                }

                if(ladObjectToDrop == EMPTY && cellPos.X % 2 != 0){
                    connections[cellPos.Y*COLS+cellPos.X-1] &= ~(1<<RIGHT);
                    connections[cellPos.Y*COLS+cellPos.X+1] &= ~(1<<LEFT);
                    connections[cellPos.Y*COLS+cellPos.X] = EMPTY;
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
            if (cellPos.HasValue && cellPos.Value.X > 0 && cellPos.Value.X < COLS-3) 
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
            if (cellPos.HasValue && cellPos.Value.X > 0 && cellPos.Value.X <= COLS-2) 
            {
                if(canConnect(cellPos.Value) && mouseCellPos.HasValue)
                    connect(cellPos.Value, mouseCellPos.Value);
            }
        }

        dynamicTableLayoutPanel.Invalidate();
    }

    private void Form1_Paint(object sender, PaintEventArgs e){

        if(ladObjectToDrop == 1<<CONN){

            Point p = dynamicTableLayoutPanel.PointToClient(Cursor.Position);

            var cellPos = GetRowColIndex(dynamicTableLayoutPanel, p);
            var mouseCellPos = GetCellLocalPos(dynamicTableLayoutPanel, dynamicTableLayoutPanel.PointToClient(Cursor.Position));
            int[] heights = dynamicTableLayoutPanel.GetRowHeights();

            
            if(cellPos.HasValue && cellPos.Value.X-1>=0)
            {

                if(cellPos.Value.X % 2 == 0){
                    if(mouseCellPos.HasValue && connections[cellPos.Value.Y*COLS+cellPos.Value.X-1] != EMPTY){
                        bool isConnection = false;
                        if(mouseCellPos.Value.Y<(heights[0]/2)){
                            for(int i = cellPos.Value.Y-1; i>=0; i--)
                                if(connections[i*COLS+cellPos.Value.X] != EMPTY){isConnection=true;break;}
                        }else{
                            for(int i = cellPos.Value.Y+1; i<ROWS; i++)
                                if(connections[i*COLS+cellPos.Value.X] != EMPTY){isConnection=true;break;}
                        }

                        if(isConnection)
                            e.Graphics.DrawLine(new Pen(Color.Black, 1), new PointF(p.X+Cursor.Size.Width / 2, p.Y+Cursor.Size.Height / 2), 
                                new PointF(p.X+Cursor.Size.Width / 2,p.Y+Cursor.Size.Height));
                    }
                }else if(cellPos.Value.X % 2 != 0 && connections[cellPos.Value.Y*COLS+cellPos.Value.X-1] != EMPTY){
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


    private void crawlUp(int x, int y, int parent){
        if(x<0 || y<0)return;
        //Console.WriteLine(connections[y*COLS+x]);
         int c = connections[y*COLS+x];
            
                if(isBitSet(c, LEFT)){
                    //Console.WriteLine("A (");
                    crawlLeft(x-1, y, parent);
                    //Console.WriteLine(")");
                }

                if(isBitSet(c, UP)){
                    //Console.WriteLine("O (");
                    crawlUp(x, y-1, parent);
                    //Console.WriteLine(")");
                }
    }

    private void crawlDown(int x, int y, int parent){
        if(y>=ROWS)return;
        //Console.WriteLine(connections[y*COLS+x]);
        int c = connections[y*COLS+x];
            
                if(isBitSet(c, LEFT)){
                    //Console.WriteLine("A (");
                    crawlLeft(x-1, y, parent);
                    //Console.WriteLine(")");
                }

                if(isBitSet(c, DOWN)){
                    //Console.WriteLine("O (");
                    crawlDown(x, y+1, parent);
                    Console.WriteLine("down");
                }
    }

    private void crawlLeft(int x, int y, int p){

        if(y<0 || x<0)return;
        int id = y*COLS+x;

        if(x==0){
            if(p>=0){
                //if(!child[p].Contains(id)){
                    parent[id].Add(p);
                    child[p].Add(0);
                //}
            }
            return;
        }

        if(x%2!=0 && connections[id] != 1<<CONN){
            printConnectionInConsole(connections[y*COLS+x]);

            if(p>=0){
                //if(!child[p].Contains(id)){
                   
                    
                //}
                 parent[id].Add(p);
                 child[p].Add(id);
            }

            p = id;
        }
        //Console.WriteLine(connections[y*COLS+x]);

        int c = connections[y*COLS+x];
            if((isBitSet(c, RIGHT) && (x%2 == 0)) || (isBitSet(c, END) && (x%2 == 0))){
                
                /*if(isBitSet(c, LEFT)){
                    //Console.WriteLine("A (");
                    crawlLeft(x-1, y, parent);
                    //Console.WriteLine(")");
                }*/


                /*if((isBitSet(c, UP) && isBitSet(c, DOWN)) ||//!!!!!
                    (isBitSet(c, UP) && isBitSet(c, LEFT)) ||
                    (isBitSet(c, LEFT) && isBitSet(c, DOWN))){*/
                        //Console.WriteLine("O (");
                        if(isBitSet(c, LEFT)){
                            crawlLeft(x-1, y, p);
                        }
                        if(isBitSet(c, UP)){
                            crawlUp(x, y-1, p);
                        }
                        if(isBitSet(c, DOWN)){
                            crawlDown(x, y+1, p);
                        }
                        
                        //Console.WriteLine(")");
                    /*}*/
                
                
            }else if ((x%2 != 0) && c == 1<<CONN){
                crawlLeft(x - 1, y, p);
            }else if ((x%2 != 0) && c != EMPTY){
                //Console.WriteLine("A (");
                crawlLeft(x - 1, y, p);
                //Console.WriteLine(")");
            }
    }
}
