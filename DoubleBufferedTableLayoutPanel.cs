public class DoubleBufferedTableLayoutPanel : TableLayoutPanel
{
    public DoubleBufferedTableLayoutPanel():base(){
        this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }
}