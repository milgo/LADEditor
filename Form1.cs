namespace winforms;

public partial class Form1 : System.Windows.Forms.Form
{

    TableLayoutPanel dynamicTableLayoutPanel = new TableLayoutPanel();
    TextBox textBox = new TextBox();

    public Form1()
    {
        InitializeComponent();
        textBox.Location = new Point(0,0);
        textBox.Size = new Size(200, 30);
        textBox.Text = "hello";
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
        dynamicTableLayoutPanel.Controls.Add(textBox, 4, 4);
        dynamicTableLayoutPanel.CellPaint += tableLayoutPanel_CellPaint;
        this.Controls.Add(dynamicTableLayoutPanel);
    }

    private void tableLayoutPanel_CellPaint(object sender, TableLayoutCellPaintEventArgs e){
        var topLeft = e.CellBounds.Location;
        var topRight = new Point(e.CellBounds.Right, e.CellBounds.Top);
        var bottomRight= new Point(e.CellBounds.Right, e.CellBounds.Bottom);
        e.Graphics.DrawLine(Pens.Black, topLeft, topRight);
        e.Graphics.DrawLine(Pens.Black, topRight, bottomRight);
    }

    
}
