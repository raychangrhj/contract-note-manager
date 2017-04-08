using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractNotes
{
    class FluentGrayConverter
    {
        public Bitmap bitmap;
        const int BLOCK_SIZE = 16, COLOR_DOWN_LIMIT = 64, COLOR_UP_LIMIT = 192;
        double LIMIT = 0.4;
        int height, width;
        int verticalBlockCount, horizontalBlockCount;
        double[,] block = new double[400, 400];
        bool[,] canConvertFlag = new bool[400, 400];

        public FluentGrayConverter()
        {
        }

        public void convert(string sourceImagePath, string destinationImagePath)
        {
            try
            {
                bitmap = new Bitmap(sourceImagePath);
                analyzeBitmap();
                convertBitmap();
                removeLines();
                removeVerticalLines();
                insertStamp();
                bitmap.Save(destinationImagePath);
            }
            catch { }
        }

        void analyzeBitmap()
        {
            height = bitmap.Height;
            width = bitmap.Width;
            verticalBlockCount = (height + BLOCK_SIZE - 1) / BLOCK_SIZE;
            horizontalBlockCount = (width + BLOCK_SIZE - 1) / BLOCK_SIZE;
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bitmapData.Stride;
            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                for (int i = 0; i < verticalBlockCount; i++)
                {
                    for (int j = 0; j < horizontalBlockCount; j++)
                    {
                        int h = Math.Min(height - i * BLOCK_SIZE, BLOCK_SIZE);
                        int w = Math.Min(width - j * BLOCK_SIZE, BLOCK_SIZE);
                        block[i, j] = 0;
                        for (int di = 0; di < h; di++)
                        {
                            for (int dj = 0; dj < w; dj++)
                            {
                                int y = i * BLOCK_SIZE + di;
                                int x = j * BLOCK_SIZE + dj;
                                int min = 255, max = 0;
                                for (int k = 0; k < 3; k++)
                                {
                                    min = Math.Min(min, ptr[(x * 3) + y * stride + k]);
                                    max = Math.Max(max, ptr[(x * 3) + y * stride + k]);
                                }
                                if (max - min > 64 || (min > COLOR_DOWN_LIMIT && max < COLOR_UP_LIMIT))
                                {
                                    block[i, j]++;
                                }
                            }
                        }
                        block[i, j] /= BLOCK_SIZE * BLOCK_SIZE;
                        if (block[i, j] > LIMIT)
                        {
                            canConvertFlag[i, j] = true;
                        }
                        else
                        {
                            if (i > 0 && !canConvertFlag[i - 1, j])
                            {
                                canConvertFlag[i, j] = false;
                            }
                            else if (j > 0 && !canConvertFlag[i, j - 1])
                            {
                                canConvertFlag[i, j] = false;
                            }
                            else
                            {
                                canConvertFlag[i, j] = i + j > 0;
                            }
                        }
                    }
                }
            }
            bitmap.UnlockBits(bitmapData);
        }

        void convertBitmap()
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bitmapData.Stride;
            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                for (int i = 0; i < verticalBlockCount; i++)
                {
                    for (int j = 0; j < horizontalBlockCount; j++)
                    {
                        int h = Math.Min(height - i * BLOCK_SIZE, BLOCK_SIZE);
                        int w = Math.Min(width - j * BLOCK_SIZE, BLOCK_SIZE);
                        if (!canConvertFlag[i, j]) continue;
                        for (int di = 0; di < h; di++)
                        {
                            for (int dj = 0; dj < w; dj++)
                            {
                                int y = i * BLOCK_SIZE + di;
                                int x = j * BLOCK_SIZE + dj;
                                int min = 255, max = 0;
                                for (int k = 0; k < 3; k++)
                                {
                                    min = Math.Min(min, ptr[(x * 3) + y * stride + k]);
                                    max = Math.Max(max, ptr[(x * 3) + y * stride + k]);
                                }
                                int color = (max - min > 64 || (min > COLOR_DOWN_LIMIT && max < COLOR_UP_LIMIT)) ? 255 : 0;
                                if (color == 0)
                                {
                                    if (j > 0 && !canConvertFlag[i, j - 1]) color = 255;
                                    if (j < horizontalBlockCount - 1 && !canConvertFlag[i, j + 1]) color = 255;
                                }
                                for (int k = 0; k < 3; k++)
                                {
                                    ptr[(x * 3) + y * stride + k] = (byte)color;
                                }
                            }
                        }
                    }
                }
            }
            bitmap.UnlockBits(bitmapData);
        }

        void removeLines()
        {
            BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int stride = bitmapData.Stride;
            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (ptr[(x * 3) + y * stride] > 200) continue;
                        int w;
                        for (w = 1; x + w < width; w++)
                        {
                            if (ptr[((x + w) * 3) + y * stride] > 200) break;
                            if (w % 20 > 0) continue;
                            bool okUp = false, okDown = false;
                            int heightRange = 8;
                            for (int h = -heightRange; h < 0; h++)
                            {
                                if (y + h >= 0 && y + h < height && ptr[((x + w) * 3) + (y + h) * stride] > 200)
                                {
                                    okUp = true;
                                    break;
                                }
                            }
                            for (int h = 1; h <= heightRange; h++)
                            {
                                if (y + h >= 0 && y + h < height && ptr[((x + w) * 3) + (y + h) * stride] > 200)
                                {
                                    okDown = true;
                                    break;
                                }
                            }
                            if (!okUp || !okDown) break;
                        }
                        if (w > 100)
                        {
                            for (int i = x; i < x + w; i++)
                            {
                                for (int j = 0; j < 3; j++)
                                {
                                    ptr[(i * 3) + y * stride + j] = 255;
                                }
                            }
                        }
                        x += w;
                    }
                }
            }
            bitmap.UnlockBits(bitmapData);
        }

        void removeVerticalLines()
        {
            string[] patterns = new string[] { "010", "0110", "01110", "011110" };
            BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int stride = bitmapData.Stride;
            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int patternNo = 0; patternNo < patterns.Length; patternNo++)
                        {
                            string pattern = patterns[patternNo];
                            if (x + pattern.Length >= width) continue;
                            int matchingPatternCount = 0;
                            while (true)
                            {
                                int comparingY = y + matchingPatternCount;
                                if (comparingY >= height) break;
                                int i;
                                for (i = 0; i < pattern.Length; i++)
                                {
                                    int comparingX = x + i;
                                    int min = 255, max = 0;
                                    for (int k = 0; k < 3; k++)
                                    {
                                        min = Math.Min(min, ptr[(comparingX * 3) + comparingY * stride + k]);
                                        max = Math.Max(max, ptr[(comparingX * 3) + comparingY * stride + k]);
                                    }
                                    char pixelPattern = (max - min > 64 || (min > 25 && max < 220)) ? '1' : '0';
                                    if (pixelPattern != pattern[i]) break;
                                }
                                if (i < pattern.Length) break;
                                matchingPatternCount++;
                            }
                            if (matchingPatternCount > 70)
                            {
                                for (int yy = y; yy < y + matchingPatternCount; yy++)
                                {
                                    for (int xx = x; xx < x + pattern.Length; xx++)
                                    {
                                        for (int k = 0; k < 3; k++)
                                        {
                                            ptr[(xx * 3) + yy * stride + k] = 255;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            bitmap.UnlockBits(bitmapData);
        }

        void insertStamp()
        {
            Graphics graphics = Graphics.FromImage(bitmap);
            try
            {
                Bitmap stampBitmap = new Bitmap("stamp.jpg");
                graphics.DrawImage(stampBitmap, 0, 0, stampBitmap.Width, stampBitmap.Height);
            }
            catch
            {
                graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, 300, 300);
                graphics.DrawString("PDF", new Font("Arial", 70), new SolidBrush(Color.Black), 30, 30);
                graphics.DrawString("OCR", new Font("Arial", 70), new SolidBrush(Color.Black), 30, 150);
            }
        }
    }
}
