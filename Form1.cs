namespace winforms;

public partial class Form1 : System.Windows.Forms.Form
{

    DoubleBufferedTableLayoutPanel dynamicTableLayoutPanel = new DoubleBufferedTableLayoutPanel();
    int [] connections;

    private const int UP = 1;
    private const int DOWN = 2;
    private const int LEFT = 4;
    private const int RIGHT = 8;

    public Form1()
    {
        InitializeComponent();

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

        connections[11*2+2] |= UP | LEFT | DOWN | RIGHT;
        
        dynamicTableLayoutPanel.CellPaint += tableLayoutPanel_CellPaint;
        dynamicTableLayoutPanel.MouseClick += tableLayout_MouseClick;
        this.Controls.Add(dynamicTableLayoutPanel);
        dynamicTableLayoutPanel.MouseMove += Form1_MouseMove;

        dynamicTableLayoutPanel.Paint += Form1_Paint;

        this.BackColor = Color.White;
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
        e.Graphics.DrawLine(Pens.Black, topLeft, topRight);
        e.Graphics.DrawLine(Pens.Black, topRight, bottomRight);

        //if (clickedCellPos.HasValue && clickedCellPos.Value.X > 0 && clickedCellPos.Value.X < 10 &&
        if(cellPos.HasValue){//} && cellPos.Value.X == clickedCellPos.Value.X && cellPos.Value.Y == clickedCellPos.Value.Y) {

            int conn = connections[cellPos.Value.Y*11+cellPos.Value.X];
            
            if((conn & UP) == UP){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[(int)cellPos.Value.X]/2f,e.CellBounds.Top), 
                                    new PointF(e.CellBounds.Left+widths[(int)cellPos.Value.X]/2f,e.CellBounds.Top+heights[(int)cellPos.Value.Y]/2f));
            }
            if((conn & DOWN) == DOWN){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[(int)cellPos.Value.X]/2f,e.CellBounds.Top+heights[(int)cellPos.Value.Y]/2f), 
                                    new PointF(e.CellBounds.Left+widths[(int)cellPos.Value.X]/2f,e.CellBounds.Top+heights[(int)cellPos.Value.Y]));
            }
            if((conn & LEFT) == LEFT){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left,e.CellBounds.Top+heights[(int)cellPos.Value.Y]/2f), 
                                    new PointF(e.CellBounds.Left+widths[(int)cellPos.Value.X]/2f,e.CellBounds.Top+heights[(int)cellPos.Value.Y]/2f));
            }
            if((conn & RIGHT) == RIGHT){
                e.Graphics.DrawLine(Pens.Blue, 
                                    new PointF(e.CellBounds.Left+widths[(int)cellPos.Value.X]/2f,e.CellBounds.Top+heights[(int)cellPos.Value.Y]/2f), 
                                    new PointF(e.CellBounds.Right,e.CellBounds.Top+heights[(int)cellPos.Value.Y]/2f));
            }
        }
    }

    private void tableLayout_MouseClick(object sender, MouseEventArgs e){

        var cellPos = GetRowColIndex(dynamicTableLayoutPanel, dynamicTableLayoutPanel.PointToClient(Cursor.Position));
        
        if(e.Button == MouseButtons.Right)
        {
            if (cellPos.HasValue && cellPos.Value.X > 0 && cellPos.Value.X < 10) 
            {
                if(cellPos.Value.X % 2 == 0){
                    //var localMousePos = dynamicTableLayoutPanel.GetControlFromPosition(cellPos.Value.X, cellPos.Value.Y).PointToClient(Cursor.Position);
                    var mouseCellPos = GetCellLocalPos(dynamicTableLayoutPanel, dynamicTableLayoutPanel.PointToClient(Cursor.Position));
                    ContextMenuStrip m = new ContextMenuStrip();
                    m.Items.Add(new ToolStripMenuItem("Connection "+mouseCellPos.Value.X+","+mouseCellPos.Value.Y));
                    m.Show((Control)(sender), e.Location);
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
                if(cellPos.Value.X % 2 == 0){
                    //var localMousePos = dynamicTableLayoutPanel.GetControlFromPosition(cellPos.Value.X, cellPos.Value.Y).PointToClient(Cursor.Position);
                    connections[cellPos.Value.Y*11+cellPos.Value.X] = LEFT | RIGHT;
                }
                else{
                    if(cellPos.Value.Y == 0 && cellPos.Value.X == 1)
                        connections[cellPos.Value.Y*11+cellPos.Value.X] = LEFT | RIGHT;
                }
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
