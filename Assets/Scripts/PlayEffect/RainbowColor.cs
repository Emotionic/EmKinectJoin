using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class RainbowColor
{
    private float H;

    private float Incremental;

    public Color Rainbow
    {
        get
        {
            return Color.HSVToRGB(H, 1, 1);
        }
    }

    public RainbowColor() : this(0, 0.01f) { }

    public RainbowColor(float h) : this(h, 0.01f) { }

    public RainbowColor(float h, float inc)
    {
        H = h - (int)h;
        Incremental = inc;
    }
    
    public void Update()
    {
        H += Incremental;
        if (H > 1f)
            H = 0f;
    }
}
