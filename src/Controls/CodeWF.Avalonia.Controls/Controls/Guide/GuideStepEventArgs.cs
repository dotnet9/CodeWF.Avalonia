using System;

namespace CodeWF.Avalonia.Controls.Controls;

public class GuideStepEventArgs(int index, IGuideStepOption step) : EventArgs
{
    public int Index { get; } = index;

    public IGuideStepOption Step { get; } = step;

    public bool Cancel { get; set; }
}