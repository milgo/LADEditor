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
    int [] buildTable;
    int [] floodFillTable;
    String [] commTab;
    int ladObjectToDrop = -1;
    Point mouseClickedPos;

    private const int EMPTY = 0;
    private const int UP = 1;
    private const int DOWN = 2;
    private const int LEFT = 3;
    private const int RIGHT = 4;
    private const int CONN = 5;
    private const int NOCON = 6;
    private const int COIL = 7;
    private const int CONN_MASK = 63;
    private const int END = 32;

    private string [] objStr = {"EMPTY", "UP", "DOWN", "LEFT", "RIGHT", "CONN", "NOCON", "COIL"};

    private const int ROWS = 6;
    private const int COLS = 13;//must be odd

    public Form1()
    {
        InitializeComponent();

        /*FileStream fs = new FileStream("out.txt", FileMode.Create);
        var sw = new StreamWriter(fs);
        sw.AutoFlush = true;
        Console.SetOut(sw);
        Console.SetError(sw);*/

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
        commTab = new String[ROWS*COLS];
        buildTable = new int[ROWS*COLS];

        for(int i=0; i<ROWS*COLS; i++){
            connections[i] = 1<<EMPTY;
            commTab[i] = new String("");
        }

        for(int i=0; i<ROWS; i++){
            connections[i*COLS] = 1<<UP | 1<<DOWN;
            connections[i*COLS+COLS-1] = 1<<UP | 1<<DOWN;
        }

        connections[0] |= 1<<0;
        //connections[12] |= 1<<END;
        
        dynamicTableLayoutPanel.CellPaint += tableLayoutPanel_CellPaint;
        dynamicTableLayoutPanel.MouseClick += tableLayout_MouseClick;
        this.Controls.Add(dynamicTableLayoutPanel);
        dynamicTableLayoutPanel.BringToFront();
        dynamicTableLayoutPanel.MouseMove += Form1_MouseMove;

        dynamicTableLayoutPanel.Paint += Form1_Paint;

        this.BackColor = Color.White;
        this.Resize += Form1_Resize;
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

    void compile(int id){

    }

    private void prepareFloodFillFind(){
        floodFillTable = new int[COLS*ROWS*3];
        
        for(int y=0; y<ROWS; y++){
            for(int x=0; x<COLS; x++){
                floodFillTable[y*3*COLS+x] = buildTable[y*COLS+x];
                if(x%2==0 && isBitSet(buildTable[y*COLS+x], DOWN)){
                    floodFillTable[(y*3+1)*COLS+x] = buildTable[y*COLS+x];
                    floodFillTable[(y*3+2)*COLS+x] = buildTable[y*COLS+x];
                }
                else{
                    floodFillTable[(y*3+1)*COLS+x] = 1;
                    floodFillTable[(y*3+2)*COLS+x] = 1;
                }

            }
        }
    }

    private void printFloodFillTable(){
        for(int y=0; y<ROWS*3; y++){
            if(y%3>0)continue;
            StringBuilder sb = new StringBuilder();
            for(int x=0; x<COLS; x++){
                if(floodFillTable[y*COLS+x]>1)
                    sb.Append("X");
                else
                    sb.Append(".");
            }
            Console.WriteLine(sb.ToString());
        }
    }

    private void printBuildTable(){
        for(int y=0; y<ROWS; y++){
            //if(y%3>0)continue;
            StringBuilder sb = new StringBuilder();
            for(int x=0; x<COLS; x++){
                if(buildTable[y*COLS+x]>1)
                    sb.Append(","+buildTable[y*COLS+x]);
                else if(buildTable[y*COLS+x]==1)
                    sb.Append(".");
                else
                    sb.Append(" ");
            }
            Console.WriteLine(sb.ToString());
        }
    }

    private void build_Click(object sender, EventArgs e){
        Console.WriteLine("build");

        for(int y=0; y<ROWS; y++){
            for(int x=0; x<COLS; x++){
                int id = y*COLS+x;
                buildTable[id] = connections[id];
                commTab[id] = new String("");
            }
        }

        bool serialFound = true;
        bool parallelFound = true;
        while(/*serialFound && parallelFound*/true){
            //Console.WriteLine("///");
            int elementCount = 0;
            for(int y=0; y<ROWS; y++){
                for(int x=0; x<COLS; x++){
                    if(buildTable[y*COLS+x] > 0x3F)
                        elementCount++;
                }
            }

//Console.WriteLine("///"+elementCount);

            if(elementCount<=1)
                break;

            serialFound = true;

            while(serialFound){

                serialFound = false;

                for(int y=0; y<ROWS; y++){
                    for(int x=0; x<COLS; x++){
                        int id = y*COLS+x;
                        if(buildTable[y*COLS+x] > 0x3F){
                            //Console.WriteLine(".");
                            Point? p = crawlUntilElement(x+1, y, RIGHT);
                            if(p.HasValue){
                                int fid = p.Value.Y*COLS +p.Value.X;

                                //Console.WriteLine("serial found");

                                if(String.IsNullOrEmpty(commTab[fid])){
                                    if(String.IsNullOrEmpty(commTab[id]))
                                        commTab[fid] = new String(" A " + fid + " A " + id);
                                    else
                                        commTab[fid] = new String(" A " + fid + " A (" + commTab[id] + ")");
                                }else{
                                    if(String.IsNullOrEmpty(commTab[id]))
                                        commTab[fid] = new String(" A " + id + " A (" + commTab[fid] +")");
                                    else
                                        commTab[fid] = new String(" A ( " +commTab[id] + ") A (" + commTab[fid] +")");
                                }

                                //commTab[fid] = new String(" A " + commTab[fid] + " A " + id);
                                Console.WriteLine(id + "found");
                                buildTable[id] = 1<<CONN;
                                serialFound = true;
                            }
                        }

                    }
                }
            }
            //Console.WriteLine("after serial find");
        printBuildTable();

            parallelFound = true;
            while(parallelFound){
                
                parallelFound = false;

                prepareFloodFillFind();
                for(int y=0; y<ROWS*3; y++){
                    for(int x=0; x<COLS; x++){
                        int fftid = y*COLS+x;
                        int id = (y/3)*COLS + x;

                        if(floodFillTable[fftid] > 0x3F){//isElement
                            List<Point> list = new List<Point>();
                            Console.WriteLine("for "+id );
                            floodFillFind(x, y+1, x, y, ref list, false);
                            Console.WriteLine("list.Count="+list.Count);
                            if(list.Count > 1){
                                Console.WriteLine(id + " continue" );
                                continue;
                            }
                            
                            if(list.Count == 1)parallelFound = true;

                            foreach(Point p in list){
                                int fid = p.Y*COLS+p.X;

                                if(String.IsNullOrEmpty(commTab[id])){
                                    

                                    if(String.IsNullOrEmpty(commTab[fid]))
                                        commTab[id] = new String(" O " + id + " O " + fid);
                                    else
                                        commTab[id] = new String(" O " + id + " O (" + commTab[fid] + ") ");
                                }else{
                                        if(String.IsNullOrEmpty(commTab[fid]))
                                            commTab[id] = new String(" O " + commTab[id] + " O " + fid);
                                        else
                                            commTab[id] = new String(" O (" + commTab[id] + ") O (" + commTab[fid] + ") ");
                                }

                                buildTable[fid] = 1<<EMPTY;
                                floodFillTable[p.Y*3*COLS+p.X] = 1<<EMPTY;
                                deattachUntilNode(p.X+1, p.Y, RIGHT);
                                deattachUntilNode(p.X-1, p.Y, LEFT);
                                printFloodFillTable();
                                //prepareFloodFillFind();
                            }

                            //Console.WriteLine("id: "+id +" crawlId:" +crawlId);
                            
                        }
                    }
                }
            }

        printFloodFillTable();

        for(int y=0; y<ROWS; y++){
            for(int x=0; x<COLS; x++){
                int id = y*COLS+x;
                if(buildTable[id] > 0x3F)
                    Console.WriteLine(id + ": " + commTab[id]);
            }
        }

        //Console.WriteLine("after parallel find");
        //printBuildTable();
        }

        //dynamicTableLayoutPanel.Invalidate();


        prepareFloodFillFind();
        /*for(int y=0; y<ROWS*3; y++){
            for(int x=0; x<COLS; x++){
                int id = y*COLS+x;
                
                //int crawlId = crawlUntilNodeOrElement(x+1, y, RIGHT, true);

               if(floodFillTable[y*COLS+x] > 0x3F){//isElement

                    Console.WriteLine("-------------------");
                    Console.WriteLine("checking id: " + id);
                    Console.WriteLine("-------------------");
                    List<Point> list = new List<Point>();
                    
                    floodFillFind(x, y+1, list);

                    foreach(Point p in list){
                        Console.WriteLine("x:"+p.X+", y:"+p.Y);
                    }

                    //Console.WriteLine("id: "+id +" crawlId:" +crawlId);
                }
            }
        }

        for(int y=0; y<ROWS*3; y++){
            if(y%3>0)continue;
            StringBuilder sb = new StringBuilder();
            for(int x=0; x<COLS; x++){
                if(floodFillTable[y*COLS+x]>1)
                    sb.Append("X");
                else if(floodFillTable[y*COLS+x]==1)
                    sb.Append(".");
                else
                    sb.Append(" ");
            }
            Console.WriteLine(sb.ToString());
        }*/

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
            ladObjectToDrop = 1<<EMPTY;
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
                    if(connections[cellPos.Y*COLS+i] == 1<<EMPTY){
                         if(i % 2 == 0){
                             connections[cellPos.Y*COLS+i] |= (1<<RIGHT) | (1<<LEFT);
                         }else{
                             connections[cellPos.Y*COLS+i] = 1<<CONN;
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
                        if(connections[cellPos.Y*COLS+x] != 1<<EMPTY){
                            if(mousePos.Y<(heights[0]/2)){
                                connectAbove(cellPos.X, cellPos.Y, cellPos.Y);
                            }else{
                                connectBelow(cellPos.X, cellPos.Y, cellPos.Y);
                            }
                        }
                        return;
                    }else if(ladObjectToDrop == 1<<EMPTY){
                        disconnectAbove(cellPos.X, cellPos.Y, cellPos.Y);
                        disconnectBelow(cellPos.X, cellPos.Y, cellPos.Y);
                    }
                }
                
                if(cellPos.X % 2 == 0){
                    x -=1;
                }

                if((connections[cellPos.Y*COLS+x] == 1<<EMPTY || connections[cellPos.Y*COLS+x] == 1<<CONN) && ladObjectToDrop != 1<<EMPTY){

                    if(ladObjectToDrop == 1<<CONN && connections[cellPos.Y*COLS+x-1] == 1<<EMPTY && connections[cellPos.Y*COLS+x+1] == 1<<EMPTY){
                        return;
                    }
                    connections[cellPos.Y*COLS+x-1] |= 1<<RIGHT;
                    connections[cellPos.Y*COLS+x+1] |= 1<<LEFT;
                    connections[cellPos.Y*COLS+x] = ladObjectToDrop;
                }

                if(ladObjectToDrop == 1<<EMPTY && cellPos.X % 2 != 0){
                    connections[cellPos.Y*COLS+cellPos.X-1] &= ~(1<<RIGHT);
                    connections[cellPos.Y*COLS+cellPos.X+1] &= ~(1<<LEFT);
                    connections[cellPos.Y*COLS+cellPos.X] = 1<<EMPTY;
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
                    if(mouseCellPos.HasValue && connections[cellPos.Value.Y*COLS+cellPos.Value.X-1] != 1<<EMPTY){
                        bool isConnection = false;
                        if(mouseCellPos.Value.Y<(heights[0]/2)){
                            for(int i = cellPos.Value.Y-1; i>=0; i--)
                                if(connections[i*COLS+cellPos.Value.X] != 1<<EMPTY){isConnection=true;break;}
                        }else{
                            for(int i = cellPos.Value.Y+1; i<ROWS; i++)
                                if(connections[i*COLS+cellPos.Value.X] != 1<<EMPTY){isConnection=true;break;}
                        }

                        if(isConnection)
                            e.Graphics.DrawLine(new Pen(Color.Black, 1), new PointF(p.X+Cursor.Size.Width / 2, p.Y+Cursor.Size.Height / 2), 
                                new PointF(p.X+Cursor.Size.Width / 2,p.Y+Cursor.Size.Height));
                    }
                }else if(cellPos.Value.X % 2 != 0 && connections[cellPos.Value.Y*COLS+cellPos.Value.X-1] != 1<<EMPTY){
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

    private int sb(int bit){
        return 1<<bit;
    }

    private bool isBitSet(int val, int bit){
        return ((val & 1<<bit) == 1<<bit);
    }

    private bool isBitSet(Point p, int bit){
        return ((buildTable[getId(p)] & 1<<bit) == 1<<bit);
    }

    private bool areBitsSet(int val, int mask){
        //Console.WriteLine("mask: " + Convert.ToString(mask,2));
        return ((val & mask) == mask);
    }

    private int getId(Point p){
        return p.Y*COLS + p.X;
    }

    private int getId(int x, int y){
        return y*COLS + x;
    }

    private bool isElement(int v){
        return (v>0x3F);
    }

    private bool isElement(Point p){
        return isElement(buildTable[p.Y*COLS + p.X]);
    }

    private bool isNode(int v){
        return (areBitsSet(v, sb(LEFT)|sb(UP)|sb(RIGHT)) ||
            areBitsSet(v, sb(UP)|sb(RIGHT)|sb(DOWN))||
            areBitsSet(v, sb(RIGHT)|sb(DOWN)|sb(LEFT))||
            areBitsSet(v, sb(DOWN)|sb(LEFT)|sb(UP)));
    }

    private bool isNode(Point p){
        return isNode(buildTable[p.Y*COLS + p.X]);
    }

    private bool isEmpty(int v){
        return (v==1<<EMPTY);
    }

    private void floodFillFind(int x, int y, int xp, int yp, ref List<Point> list, bool up /*prevent from adding elements from same level*/){

        if(y<0 || x<0 || x>=COLS || y>=ROWS*3 || (xp==x && yp==y) )return;

        //Console.WriteLine("checking x:"+x+", y:"+y);

        if(floodFillTable[y*COLS+x] > 0x3F && !up){
            Console.WriteLine("found x:"+x+", y:"+y/3);
            Point p = new Point(x, y/3);
            if(!list.Contains(p))
                list.Add(p);
            //return;
        }        
        else if(floodFillTable[y*COLS+x] == 1){
            floodFillTable[y*COLS+x] = 0;
            floodFillFind(x-1, y, xp, yp, ref list, false);
            floodFillFind(x+1, y, xp, yp, ref list, false);
            floodFillFind(x, y+1, xp, yp, ref list, false);
            floodFillFind(x, y-1, xp, yp, ref list, true);
        }
        
    }

    private Point? crawlUntilElement(int x, int y, int dir){

        if(y<0 || x<0 || x>=COLS || y>=ROWS)return null;
        int id = y*COLS+x;
        int v = buildTable[id];

        //Console.WriteLine("checking "+ x + ", " + y);

        if(isEmpty(v) || isNode(v)){
            Console.WriteLine("node or empty");
            return null;
        }

        if(isElement(v) )
            return new Point(x,y);


        if(isBitSet(v, LEFT) && dir != RIGHT)
            return crawlUntilElement(x-1, y, LEFT);

        if(isBitSet(v, RIGHT) && dir != LEFT)
            return crawlUntilElement(x+1, y, RIGHT);

        if(isBitSet(v, UP) && dir != DOWN)
            return crawlUntilElement(x, y-1, UP);
        
        if(isBitSet(v, DOWN) && dir != UP)
            return crawlUntilElement(x, y+1, DOWN);  

        if(isBitSet(v, CONN) && dir == LEFT)
            return crawlUntilElement(x-1, y, LEFT);  

        if(isBitSet(v, CONN) && dir == RIGHT)
            return crawlUntilElement(x+1, y, RIGHT); 

        return null;    
    }

    private void deattachUntilNode(int x, int y, int dir){

        if(y<0 || x<0 || x>=COLS || y>=ROWS)return;
        int id = y*COLS+x;
        int v = buildTable[id];

        

       if(isElement(v) || isEmpty(v))
            return;

        if( isNode(v) ){
            if(isBitSet(v, RIGHT) && dir == LEFT){
                buildTable[id] &= ~(1<<RIGHT);
                Console.WriteLine("deattaching "+ x + ", " + y);
            }
            else if(isBitSet(v, DOWN) && dir == UP){
                buildTable[id] &= ~(1<<DOWN);
                Console.WriteLine("deattaching "+ x + ", " + y);
            }
            else if(isBitSet(v, LEFT) && dir == RIGHT){
                buildTable[id] &= ~(1<<LEFT);
                Console.WriteLine("deattaching "+ x + ", " + y);
            }
            else if(isBitSet(v, UP) && dir == DOWN){
                buildTable[id] &= ~(1<<UP);
                Console.WriteLine("deattaching "+ x + ", " + y);
            }
            return;
        }

 

        buildTable[id] = 1<<EMPTY;

        if(isBitSet(v, LEFT) && dir != RIGHT)
            deattachUntilNode(x-1, y, LEFT);

        else if(isBitSet(v, RIGHT) && dir != LEFT)
            deattachUntilNode(x+1, y, RIGHT);

        else if(isBitSet(v, UP) && dir != DOWN)
            deattachUntilNode(x, y-1, UP);
        
        else if(isBitSet(v, DOWN) && dir != UP)
            deattachUntilNode(x, y+1, DOWN);  

        else if(isBitSet(v, CONN) && dir == LEFT)
            deattachUntilNode(x-1, y, LEFT);  

        else if(isBitSet(v, CONN) && dir == RIGHT)
            deattachUntilNode(x+1, y, RIGHT); 
    }
}
