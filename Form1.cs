using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FastGraphics
{
  public partial class Form1 : Form
  {
    private FastBitmap _fastBitmap;
    private int _offset = 0;

    private uint _renderTimeInMilliseconds;

    public Form1()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Update on-screen text that displays render time in miliiseconds.
    /// </summary>
    public void UpdateRenderTimeText()
    {
      label1.Text = _renderTimeInMilliseconds.ToString();
    }

    /// <summary>
    /// Dispose of existing fast bitmap (if exists)
    /// Create new fast bitmap based upon client rectangle.
    /// </summary>
    private void CreateFastBitmap()
    {
      _fastBitmap?.Dispose();
      _fastBitmap = new FastBitmap(ClientRectangle.Width, ClientRectangle.Height);
    }

    public void OnIdle()
    {
      Invalidate();
      UpdateRenderTimeText();
    }

    // Resizing form changes drawing area.
    // Resize fast bitmap accordingly.
    protected override void OnResize(EventArgs e)
    {
      CreateFastBitmap();
    }

    /// <summary>
    /// Override OnPaintBackground to do nothing.
    /// The OnPaint will perform all drawing.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    /// <summary>
    /// Magic happens in OnPaint.
    /// 1. Render raw pixel data (Fast bitmap)
    /// 2. Due to the nature of fast bitmap, its image will reflect pixel data.
    /// 3. Draw fast bitmap image to this form's window.
    /// 4. Time all of this so we can determine time taken to render a frame.
    /// Optional - set interpolation mode.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint(PaintEventArgs e)
    {
      DateTime startTime = DateTime.UtcNow;
      e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
      RenderRawPixels();
      _fastBitmap.Draw(e.Graphics);
      this.BackgroundImage = _fastBitmap.Image;
      DateTime endTime = DateTime.UtcNow;
      _renderTimeInMilliseconds = (uint)(endTime - startTime).Milliseconds;
    }

    /// <summary>
    /// RenderRawPixels is called for each onPaint.
    /// I use some maths I found on the Internet years ago,
    /// when programming graphics for the Commodore Amiga.
    /// The maths have been modifed, a lot, to produce,
    /// hopefully, smooth, interesting results.
    /// </summary>
    private void RenderRawPixels()
    {
      int rowAddress = 0;
      int width = _fastBitmap.Width;
      int height = _fastBitmap.Height;
      uint[] pixels = _fastBitmap.Pixels;

      for (int y = 0; y < height; y++)
      {
        for (int x = 0; x < width; x++)
        {
          uint result = (uint)
            ((x * x * y) +
            (y * y * _offset) +
            (x * x * y) +
            ((2 - x) * y) +
            ((y-x) * y) +
            _offset + y) /
            8192;

          result &= 0x00FFFFFF;
          //uint alpha = (uint)(
          //  (x * y) -
          //  (y * _offset)) /
          //  1024;
          //alpha <<= 24;
          //alpha &= 0xFF000000;
          uint alpha = 0xFF000000;

          pixels[rowAddress + x] = alpha | result;
        }
        rowAddress += width;
      }
      BackgroundImage = _fastBitmap.Image;
      _offset += 5;
    }

    /// <summary>
    /// upon closing this form, we must dispose the fast bitmap.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnClosed(EventArgs e)
    {
      _fastBitmap.Dispose();
      base.OnClosed(e);
    }
  }
}
