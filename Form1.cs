namespace winforms;

public partial class Form1 : System.Windows.Forms.Form
{

    TableLayoutPanel dynamicTableLayoutPanel = new TableLayoutPanel();
    TextBox textBox = new TextBox();

    public Form1()
    {
        InitializeComponent();
        //textBox.Location = new Point(0,0);
        //textBox.Size = new Size(200, 30);
        //textBox.Text = "hello";
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

        //dynamicTableLayoutPanel.Controls.Add(textBox, 4, 4);
        dynamicTableLayoutPanel.CellPaint += tableLayoutPanel_CellPaint;
        dynamicTableLayoutPanel.MouseClick += tableLayout_MouseClick;
        this.Controls.Add(dynamicTableLayoutPanel);

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

    private void tableLayoutPanel_CellPaint(object sender, TableLayoutCellPaintEventArgs e){
        var topLeft = e.CellBounds.Location;
        var topRight = new Point(e.CellBounds.Right, e.CellBounds.Top);
        var bottomRight= new Point(e.CellBounds.Right, e.CellBounds.Bottom);
        e.Graphics.DrawLine(Pens.Black, topLeft, topRight);
        e.Graphics.DrawLine(Pens.Black, topRight, bottomRight);
    }

    private void tableLayout_MouseClick(object sender, MouseEventArgs e){

        var cellPos = GetRowColIndex(dynamicTableLayoutPanel, dynamicTableLayoutPanel.PointToClient(Cursor.Position));

        if(e.Button == MouseButtons.Right)
        {
            if (cellPos.HasValue && cellPos.Value.X > 0 && cellPos.Value.X < 10) 
            {
                if(cellPos.Value.X % 2 == 0){
                    ContextMenuStrip m = new ContextMenuStrip();
                    m.Items.Add(new ToolStripMenuItem("Connection"));
                    m.Show((Control)(sender), e.Location);
                }
                else{
                    ContextMenuStrip m = new ContextMenuStrip();
                    m.Items.Add(new ToolStripMenuItem("Ladder"));
                    m.Show((Control)(sender), e.Location);
                }
            }
        }
    }

    
}
