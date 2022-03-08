using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FastGraphics
{
  /// <summary>
  /// Uses pinned memory and direct array access to manipulate bitmap bits.
  /// Pinned memory is not owned by the CLR, so, it is important to free when done.
  /// </summary>
  public sealed class FastBitmap : IDisposable
  {
    private GCHandle _pinnedMemoryHandle;
    private IntPtr _pinnedMemoryAddress;    

    public int Width { get; private set; }
    public int Height { get; private set; }
    public uint[] Pixels { get; private set; }
    public Bitmap Image { get; private set; }

    public FastBitmap(int width, int height)
    {
      // Pixels is an array that may be used to modify pixel data.
      // Typical formula is ((y*width)+x) = Alpha | Colour;
      // Uses 4 bytes per pixel (Alpha, Red, Green and Blue channels, each 8 bits)
      Pixels = new uint[width * height];

      // Inform OS that pixel allocated array is pinned and not subject to normal garbage collection.
      _pinnedMemoryHandle = GCHandle.Alloc(Pixels, GCHandleType.Pinned);

      // Obtain the pinned memory address, required to create bitmap for use by .NET app.
      _pinnedMemoryAddress = _pinnedMemoryHandle.AddrOfPinnedObject();

      // Set pixel format and calculate scan line stride.
      PixelFormat format = PixelFormat.Format32bppArgb;
      int bitsPerPixel = ((int)format & 0xff00) >> 8;
      int bytesPerPixel = (bitsPerPixel + 7) / 8;
      int stride = 4 * ((width * bytesPerPixel + 3) / 4);
      Image = new Bitmap(width, height, stride, PixelFormat.Format32bppArgb, _pinnedMemoryAddress);
      Width = width;
      Height = height;      
    }

    public void Draw(Graphics target) =>
      target.DrawImage(Image, 0, 0);

    public void Dispose()
    {
      Image.Dispose();

      if (_pinnedMemoryHandle.IsAllocated)
        _pinnedMemoryHandle.Free();

      _pinnedMemoryAddress = IntPtr.Zero;
      Pixels = Array.Empty<uint>();
      Width = 0;
      Height = 0;
    }
  }
}
