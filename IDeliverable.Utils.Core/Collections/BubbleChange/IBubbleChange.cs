namespace IDeliverable.Utils.Core.Collections.BubbleChange
{
    public delegate void BubbleChangeEventHandler(object sender, BubbleChangeEventArgs e);

    public interface IBubbleChange
    {
        event BubbleChangeEventHandler BubbleChange;
    }
}