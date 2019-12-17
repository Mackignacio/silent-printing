﻿using silent_printing.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

class Print
{
    private int verticalSpacing = 0;
    private int logoHeight = 0;
    public Print(string printer, string paper, int width, int height, string[] args)
    {
        verticalSpacing = height;

        using (PrintDocument pd = new PrintDocument())
        {
            pd.PrinterSettings.PrinterName = printer;
            pd.DefaultPageSettings.PaperSize = new PaperSize(paper, width, height);
            pd.PrintPage += PrintPage(args, width);
            pd.Print();
        }
    }

    private PrintPageEventHandler PrintPage(string[] args, int width)
    {
        return (object sender, PrintPageEventArgs e) =>
        {
            Graphics graphics = e.Graphics;
            Logo(args[0], graphics, new PointF(50, 0));
            Headers(args, width, graphics);
        };
    }

    private void Logo(string imgURL, Graphics graphics, PointF point)
    {
        using (WebClient wc = new WebClient())
        {
            byte[] bytes = wc.DownloadData(imgURL);
            MemoryStream ms = new MemoryStream(bytes);
            Image img = Image.FromStream(ms);
            graphics.DrawImage(img, point);
            logoHeight = img.Height;
        }
    }

    private void Headers(string[] args, int width, Graphics graphics)
    {
        string businessName = args[1];
        using (Font headerFont = GetCustomFont(Resources.HelveticaNeueBd, 12, FontStyle.Bold))
            SetBusinessName(LimitTextWidth(businessName, 20), headerFont, width, graphics);
    }

    private void SetBusinessName(string[] text, Font font, int width, Graphics graphics)
    {
        graphics.DrawString(text[0], font, Brushes.Black, Position(width, text[0], font, VerticalSpacing(logoHeight + 25)));
        if (text.Length > 1) graphics.DrawString(text[1], font, Brushes.Black, Position(width, text[1], font, VerticalSpacing(20)));
    }

    private string[] LimitTextWidth(string text, int length)
    {
        string strlist = "";
        string excess = "";
        string[] split = text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        split.ToList().ConvertAll(str =>
        {
            if (strlist.Length < length) strlist += str + " ";
            else excess += str + " ";
            return str;
        });

        return new string[] { strlist, excess };
    }

    private Font GetCustomFont(byte[] fontData, float size, FontStyle style)
    {
        using (PrivateFontCollection pfc = new PrivateFontCollection())
        {
            IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
            Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            pfc.AddMemoryFont(fontPtr, fontData.Length);
            Marshal.FreeCoTaskMem(fontPtr);
            return new Font(pfc.Families[0], size, style);
        }
    }

    private int VerticalSpacing(int spacing)
    {
        verticalSpacing += spacing;
        return verticalSpacing;
    }

    private PointF Position(int width, string text, Font font, int height)
    {
        return new PointF(Center(width, TextWidth(text, font)), height);
    }

    private int TextWidth(string text, Font font)
    {
        return TextRenderer.MeasureText(text, font).Width;
    }

    private float Center(int width, int textLength)
    {
        float margin = (float)(width / 2) - (float)(textLength / 2);
        return margin;
    }
}



