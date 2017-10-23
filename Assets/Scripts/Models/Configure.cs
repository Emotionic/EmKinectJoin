public class Configure
{
    /* Calibration */
    public int ch1Lower { get; set; }
    public int ch1Upper { get; set; }
    public int ch2Lower { get; set; }
    public int ch2Upper { get; set; }
    public int ch3Lower { get; set; }
    public int ch3Upper { get; set; }

    /* Local */
    public bool forceLocal { get; set; }

    public Configure()
    {
        ch1Lower = 90;
        ch1Upper = 120;
        ch2Lower = 0;
        ch2Upper = 255;
        ch3Lower = 200;
        ch3Upper = 255;

        forceLocal = false;
    }

}
