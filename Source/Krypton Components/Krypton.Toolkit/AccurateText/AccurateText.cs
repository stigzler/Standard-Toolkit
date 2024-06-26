﻿#region BSD License
/*
 * 
 * Original BSD 3-Clause License (https://github.com/ComponentFactory/Krypton/blob/master/LICENSE)
 *  © Component Factory Pty Ltd, 2006 - 2016, (Version 4.5.0.0) All rights reserved.
 * 
 *  New BSD 3-Clause License (https://github.com/Krypton-Suite/Standard-Toolkit/blob/master/LICENSE)
 *  Modifications by Peter Wagner(aka Wagnerp) & Simon Coghlan(aka Smurf-IV), et al. 2017 - 2023. All rights reserved. 
 *  
 */
#endregion

namespace Krypton.Toolkit
{
    /// <summary>
    /// Provide accurate text measuring and drawing capability.
    /// </summary>
    public class AccurateText : GlobalId
    {
        #region Static Fields

        private const int GLOW_EXTRA_WIDTH = 14;
        private const int GLOW_EXTRA_HEIGHT = 3;

        // DO NOT USE THIS CACHING, because when the Theme Font is changed and there is a global update, 
        // the whole thing gets locked up in resizing......
        //private static TimedCache<(string text, Font font, StringFormatFlags formatFlags, TextRenderingHint hint), AccurateTextMemento> _cache 
        //    = new(TimeSpan.FromMinutes(10));

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Pixel accurate measure of the specified string when drawn with the specified Font object.
        /// </summary>
        /// <param name="g">Graphics instance used to measure text.</param>
        /// <param name="rtl">Right to left setting for control.</param>
        /// <param name="text">String to measure.</param>
        /// <param name="font">Font object that defines the text format of the string.</param>
        /// <param name="trim">How to trim excess text.</param>
        /// <param name="align">How to align multi-line text.</param>
        /// <param name="prefix">How to process prefix characters.</param>
        /// <param name="hint">Rendering hint.</param>
        /// <param name="composition">Should draw on a composition element.</param>
        /// <param name="glowing">When on composition draw with glowing.</param>
        /// <param name="disposeFont">Dispose of font when finished with it.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>A memento used to draw the text.</returns>
        public static AccurateTextMemento MeasureString([DisallowNull] Graphics g,
                                                        RightToLeft rtl,
                                                        [DisallowNull] string text,
                                                        [DisallowNull] Font font,
                                                        PaletteTextTrim trim,
                                                        PaletteRelativeAlign align,
                                                        PaletteTextHotkeyPrefix prefix,
                                                        TextRenderingHint hint,
                                                        bool composition,
                                                        bool glowing,
                                                        bool disposeFont)
        {
            Debug.Assert(g != null);
            Debug.Assert(text != null);
            Debug.Assert(font != null);

            if (g == null)
            {
                throw new ArgumentNullException(nameof(g));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (font == null)
            {
                throw new ArgumentNullException(nameof(font));
            }

            // An empty string cannot be drawn, so uses the empty memento
            if (text.Length == 0)
            {
                return AccurateTextMemento.Empty;
            }

            // Create the format object used when measuring and drawing
            var format = new StringFormat { FormatFlags = StringFormatFlags.NoClip };

            // Ensure that text reflects reversed RTL setting
            if (rtl == RightToLeft.Yes)
            {
                format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
            }

            // How do we position text horizontally?
            switch (align)
            {
                case PaletteRelativeAlign.Near:
                    format.Alignment = (rtl == RightToLeft.Yes) ? StringAlignment.Far : StringAlignment.Near;
                    break;
                case PaletteRelativeAlign.Center:
                    format.Alignment = StringAlignment.Center;
                    break;
                case PaletteRelativeAlign.Far:
                    format.Alignment = (rtl == RightToLeft.Yes) ? StringAlignment.Near : StringAlignment.Far;
                    break;
                default:
                    // Should never happen!
                    Debug.Assert(false);
                    break;
            }

            // Do we need to trim text that is too big?
            switch (trim)
            {
                case PaletteTextTrim.Character:
                    format.Trimming = StringTrimming.Character;
                    break;
                case PaletteTextTrim.EllipsisCharacter:
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    break;
                case PaletteTextTrim.EllipsisPath:
                    format.Trimming = StringTrimming.EllipsisPath;
                    break;
                case PaletteTextTrim.EllipsisWord:
                    format.Trimming = StringTrimming.EllipsisWord;
                    break;
                case PaletteTextTrim.Word:
                    format.Trimming = StringTrimming.Word;
                    break;
                case PaletteTextTrim.Hide:
                    format.Trimming = StringTrimming.None;
                    break;
                default:
                    // Should never happen!
                    Debug.Assert(false);
                    break;
            }

            // Setup the correct prefix processing
            switch (prefix)
            {
                case PaletteTextHotkeyPrefix.None:
                    format.HotkeyPrefix = HotkeyPrefix.None;
                    break;
                case PaletteTextHotkeyPrefix.Hide:
                    format.HotkeyPrefix = HotkeyPrefix.Hide;
                    break;
                case PaletteTextHotkeyPrefix.Show:
                    format.HotkeyPrefix = HotkeyPrefix.Show;
                    break;
                default:
                    // Should never happen!
                    Debug.Assert(false);
                    break;
            }

            // Optimisation: Lookup key before performing expensive / slow GDI functions
            //var key = (text, font, format.FormatFlags, hint);
            var memento = //_cache.GetOrCreate(key,
                () =>
                {

                    // Replace tab characters with a fixed four spaces
                    text = text.Replace("\t", @"    ");

                    // Perform actual measure of the text
                    using var graphicsHint = new GraphicsTextHint(g, hint);
                    var textSize = SizeF.Empty;

                    try
                    {
                        // Declare a proposed size with dimensions set to the maximum integer value.
                        var proposedSize = new Size(int.MaxValue, int.MaxValue);
                        textSize = g.MeasureString(text, font, proposedSize, format);

                        if (composition && glowing) //Seb
                        {
                            textSize.Width += GLOW_EXTRA_WIDTH;
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    return new AccurateTextMemento(text, font, textSize, format, hint, disposeFont);
                };
                //);
            // Return a memento with drawing details
            return memento.Invoke();
        }

        /// <summary>
        /// Pixel accurate drawing of the requested text memento information.
        /// </summary>
        /// <param name="g">Graphics object used for drawing.</param>
        /// <param name="brush">Brush for drawing text with.</param>
        /// <param name="rect">Rectangle to draw text inside.</param>
        /// <param name="rtl">Right to left setting for control.</param>
        /// <param name="orientation">Orientation for drawing text.</param>
        /// <param name="memento">Memento containing text context.</param>
        /// <param name="state">State of the source element.</param>
        /// <param name="composition">Should draw on a composition element.</param>
        /// <param name="glowing">When on composition draw with glowing.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>True if draw succeeded; False is draw produced an error.</returns>
        public static bool DrawString([DisallowNull] Graphics g,
                                      Brush brush,
                                      Rectangle rect,
                                      RightToLeft rtl,
                                      VisualOrientation orientation,
                                      bool composition,
                                      bool glowing,
                                      PaletteState state,
                                      [DisallowNull] AccurateTextMemento memento)
        {
            Debug.Assert(g != null);
            Debug.Assert(memento != null);

            // Cannot draw with a null graphics instance
            if (g == null)
            {
                throw new ArgumentNullException(nameof(g));
            }

            // Cannot draw with a null memento instance
            if (memento == null)
            {
                throw new ArgumentNullException(nameof(memento));
            }

            var ret = true;

            // Is there a valid place to be drawn into
            if (rect is { Width: > 0, Height: > 0 })
            {
                // Does the memento contain something to draw?
                if (!memento.IsEmpty)
                {
                    var translateX = 0;
                    var translateY = 0;
                    var rotation = 0f;

                    // Perform any transformations needed for orientation
                    switch (orientation)
                    {
                        case VisualOrientation.Bottom:
                            // Translate to opposite side of origin, so the rotate can 
                            // then bring it back to original position but mirror image
                            translateX = (rect.X * 2) + rect.Width;
                            translateY = (rect.Y * 2) + rect.Height;
                            rotation = 180f;
                            break;

                        case VisualOrientation.Left:
                            // Invert the dimensions of the rectangle for drawing upwards
                            rect = rect with { Width = rect.Height, Height = rect.Width };
                            // Translate back from a quarter left turn to the original place 
                            translateX = rect.X - rect.Y - 1;
                            translateY = rect.X + rect.Y + rect.Width;
                            rotation = 270;
                            break;

                        case VisualOrientation.Right:
                            // Invert the dimensions of the rectangle for drawing upwards
                            rect = rect with { Width = rect.Height, Height = rect.Width };

                            // Translate back from a quarter right turn to the original place 
                            translateX = rect.X + rect.Y + rect.Height + 1;
                            translateY = -(rect.X - rect.Y);
                            rotation = 90f;
                            break;
                    }

                    // Apply the transforms if we have any to apply
                    if ((translateX != 0) || (translateY != 0))
                    {
                        g.TranslateTransform(translateX, translateY);
                    }

                    if (rotation != 0f)
                    {
                        g.RotateTransform(rotation);
                    }

                    try
                    {
                        switch (Application.RenderWithVisualStyles)
                        {
                            case true when composition && glowing:
                                DrawCompositionGlowingText(g, memento.Text, memento.Font, rect, state,
                                    SystemColors.ActiveCaptionText, true);
                                break;
                            case true when composition:
                            {
                                //Check if correct in all cases
                                var tmpBrush = brush as SolidBrush;
                                Color tmpColor = tmpBrush?.Color ?? SystemColors.ActiveCaptionText;

                                DrawCompositionText(g, memento.Text, memento.Font, rect, state,
                                    tmpColor, true, memento.Format);
                                break;
                            }
                            default:
                                g.DrawString(memento.Text, memento.Font, brush, rect, memento.Format);
                                break;
                        }
                    }
                    catch
                    {
                        // Ignore any error from the DrawString, usually because the display settings
                        // have changed causing Fonts to be invalid. Our controls will notice the change
                        // and refresh the fonts but sometimes the draw happens before the fonts are
                        // regenerated. Just ignore message and everything will sort itself out. Trust me!
                        ret = false;
                    }
                    finally
                    {
                        // Remove the applied transforms
                        if (rotation != 0f)
                        {
                            g.RotateTransform(-rotation);
                        }

                        if ((translateX != 0) || (translateY != 0))
                        {
                            g.TranslateTransform(-translateX, -translateY);
                        }
                    }
                }
            }

            return ret;
        }

        private static Color ContrastColor(Color color)
        {
            //  Counting the perceptive luminance - human eye favours green colour... 
            var a = 1
                     - (((0.299 * color.R)
                         + ((0.587 * color.G) + (0.114 * color.B)))
                        / 255);
            var d = a < 0.5 ? 0 : 255;

            //  dark colours - white font
            return Color.FromArgb(d, d, d);
        }
        #endregion

        #region Implementation
        /// <summary>
        /// Draw text with a glowing background, for use on a composition element.
        /// </summary>
        /// <param name="g">Graphics reference.</param>
        /// <param name="text">Text to be drawn.</param>
        /// <param name="font">Font to use for text.</param>
        /// <param name="bounds">Bounding area for the text.</param>
        /// <param name="state">State of the source element.</param>
        /// <param name="color"><see cref="Color"/> of the text.</param>
        /// <param name="copyBackground">Should existing background be copied into the bitmap.</param>
        public static void DrawCompositionGlowingText(Graphics? g,
                                                      string text,
                                                      Font font,
                                                      Rectangle bounds,
                                                      PaletteState state,
                                                      Color color,
                                                      bool copyBackground)
        {
            // Get the hDC for the graphics instance and create a memory DC
            var gDC = g?.GetHdc() ?? IntPtr.Zero;
            try
            {
                var mDC = PI.CreateCompatibleDC(gDC);

                var bmi = new PI.BITMAPINFO
                {
                    biWidth = bounds.Width,
                    biHeight = -(bounds.Height + (GLOW_EXTRA_HEIGHT * 2)),
                    biCompression = 0,
                    biBitCount = 32,
                    biPlanes = 1
                };
                bmi.biSize = (uint) Marshal.SizeOf(bmi);


                // Create a device independent bitmap and select into the memory DC
                var hDIB = PI.CreateDIBSection(gDC, ref bmi, 0, out _, IntPtr.Zero, 0);
                PI.SelectObject(mDC, hDIB);

                if (copyBackground)
                {
                    // Copy existing background into the bitmap
                    PI.BitBlt(mDC, 0, 0, bounds.Width, bounds.Height + (GLOW_EXTRA_HEIGHT * 2),
                              gDC, bounds.X, bounds.Y - GLOW_EXTRA_HEIGHT, 0x00CC0020);
                }

                // Select the font for use when drawing
                var hFont = font.ToHfont();
                PI.SelectObject(mDC, hFont);

                // Get renderer for the correct state
                var renderer = new VisualStyleRenderer(state == PaletteState.Normal
                    ? VisualStyleElement.Window.Caption.Active
                    : VisualStyleElement.Window.Caption.Inactive);

                // Create structures needed for theme drawing call
                var textBounds = new PI.RECT
                {
                    left = 0,
                    top = 0,
                    right = bounds.Right - bounds.Left,
                    bottom = bounds.Bottom - bounds.Top + (GLOW_EXTRA_HEIGHT * 2)
                };
                var dttOpts = new PI.DTTOPTS
                {
                    dwSize = Marshal.SizeOf(typeof(PI.DTTOPTS)),
                    dwFlags = PI.DTT_COMPOSITED | PI.DTT_GLOWSIZE | PI.DTT_TEXTCOLOR,
                    crText = ColorTranslator.ToWin32(color),
                    iGlowSize = 11
                };

                // Always draw text centered
                const TextFormatFlags TEXT_FORMAT = TextFormatFlags.SingleLine |
                                                   TextFormatFlags.HorizontalCenter |
                                                   TextFormatFlags.VerticalCenter |
                                                   TextFormatFlags.EndEllipsis;

                // Perform actual drawing
                PI.DrawThemeTextEx(renderer.Handle,
                                   mDC, 0, 0,
                                   text, -1, (int)TEXT_FORMAT,
                                   ref textBounds, ref dttOpts);

                // Copy to foreground
                PI.BitBlt(gDC,
                          bounds.Left, bounds.Top - GLOW_EXTRA_HEIGHT,
                          bounds.Width, bounds.Height + (GLOW_EXTRA_HEIGHT * 2),
                          mDC, 0, 0, 0x00CC0020);

                // Dispose of allocated objects
                PI.DeleteObject(hFont);
                PI.DeleteObject(hDIB);
                PI.DeleteDC(mDC);              
            }
            catch
            {
                // ignored
            }
            finally
            {
                // Must remember to release the hDC
                g?.ReleaseHdc(gDC);
            }
        }

        /// <summary>
        /// Draw text without a glowing background, for use on a composition element.
        /// </summary>
        /// <param name="g">Graphics reference.</param>
        /// <param name="text">Text to be drawn.</param>
        /// <param name="font">Font to use for text.</param>
        /// <param name="bounds">Bounding area for the text.</param>
        /// <param name="state">State of the source element.</param>
        /// <param name="color"><see cref="Color"/> of the text.</param>
        /// <param name="copyBackground">Should existing background be copied into the bitmap.</param>
        /// <param name="sf">StringFormat of the memento.</param>
        public static void DrawCompositionText(Graphics? g,
                                                      string text,
                                                      Font font,
                                                      Rectangle bounds,
                                                      PaletteState state,
                                                      Color color,
                                                      bool copyBackground,
                                                      StringFormat sf)
        {
            // Get the hDC for the graphics instance and create a memory DC
            var gDC = g?.GetHdc() ?? IntPtr.Zero;
            try
            {
                var mDC = PI.CreateCompatibleDC(gDC);

                var bmi = new PI.BITMAPINFO
                {
                    biWidth = bounds.Width,
                    biHeight = -bounds.Height,
                    biCompression = 0,
                    biBitCount = 32,
                    biPlanes = 1
                };
                bmi.biSize = (uint)Marshal.SizeOf(bmi);

                // Create a device independent bitmap and select into the memory DC
                var hDIB = PI.CreateDIBSection(gDC, ref bmi, 0, out _, IntPtr.Zero, 0);
                PI.SelectObject(mDC, hDIB);

                if (copyBackground)
                {
                    // Copy existing background into the bitmap
                    PI.BitBlt(mDC, 0, 0, bounds.Width, bounds.Height,
                              gDC, bounds.X, bounds.Y, 0x00CC0020);
                }

                // Select the font for use when drawing
                var hFont = font.ToHfont();
                PI.SelectObject(mDC, hFont);

                // Get renderer for the correct state
                var renderer = new VisualStyleRenderer(state == PaletteState.Normal
                    ? VisualStyleElement.Window.Caption.Active
                    : VisualStyleElement.Window.Caption.Inactive);

                // Create structures needed for theme drawing call
                var textBounds = new PI.RECT
                {
                    left = 0,
                    top = 0,
                    right = bounds.Right - bounds.Left,
                    bottom = bounds.Bottom - bounds.Top
                };
                var dttOpts = new PI.DTTOPTS
                {
                    dwSize = Marshal.SizeOf(typeof(PI.DTTOPTS)),
                    dwFlags = PI.DTT_COMPOSITED | PI.DTT_TEXTCOLOR,
                    crText = ColorTranslator.ToWin32(color)
                };

                // Always draw text centered
                TextFormatFlags textFormat = TextFormatFlags.SingleLine |
                                             TextFormatFlags.HorizontalCenter |
                                             TextFormatFlags.VerticalCenter;
                ////Seb   |  TextFormatFlags.EndEllipsis;


                // Perform actual drawing
                //PI.DrawThemeTextEx(renderer.Handle,
                //                   mDC, 0, 0,
                //                   text, -1, (int)StringFormatToFlags(sf),
                //                   ref textBounds, ref dttOpts);
                PI.DrawThemeTextEx(renderer.Handle,
                                  mDC, 0, 0,
                                  text, -1, (int)textFormat,
                                  ref textBounds, ref dttOpts);

                // Copy to foreground
                PI.BitBlt(gDC,
                          bounds.Left, bounds.Top,
                          bounds.Width, bounds.Height,
                          mDC, 0, 0, 0x00CC0020);

                // Dispose of allocated objects
                PI.DeleteObject(hFont);
                PI.DeleteObject(hDIB);
                PI.DeleteDC(mDC);
            }
            catch
            {
                // ignored
            }
            finally
            {
                // Must remember to release the hDC
                g?.ReleaseHdc(gDC);
            }
        }

        private static StringFormat FlagsToStringFormat(TextFormatFlags flags)
        {
            var sf = new StringFormat();

            // Translation table: http://msdn.microsoft.com/msdnmag/issues/06/03/TextRendering/default.aspx?fig=true#fig4

            // Horizontal Alignment
            if ((flags & TextFormatFlags.HorizontalCenter) == TextFormatFlags.HorizontalCenter)
            {
                sf.Alignment = StringAlignment.Center;
            }
            else if ((flags & TextFormatFlags.Right) == TextFormatFlags.Right)
            {
                sf.Alignment = StringAlignment.Far;
            }
            else
            {
                sf.Alignment = StringAlignment.Near;
            }

            // Vertical Alignment
            if ((flags & TextFormatFlags.Bottom) == TextFormatFlags.Bottom)
            {
                sf.LineAlignment = StringAlignment.Far;
            }
            else if ((flags & TextFormatFlags.VerticalCenter) == TextFormatFlags.VerticalCenter)
            {
                sf.LineAlignment = StringAlignment.Center;
            }
            else
            {
                sf.LineAlignment = StringAlignment.Near;
            }

            // Ellipsis
            if ((flags & TextFormatFlags.EndEllipsis) == TextFormatFlags.EndEllipsis)
            {
                sf.Trimming = StringTrimming.EllipsisCharacter;
            }
            else if ((flags & TextFormatFlags.PathEllipsis) == TextFormatFlags.PathEllipsis)
            {
                sf.Trimming = StringTrimming.EllipsisPath;
            }
            else if ((flags & TextFormatFlags.WordEllipsis) == TextFormatFlags.WordEllipsis)
            {
                sf.Trimming = StringTrimming.EllipsisWord;
            }
            else
            {
                sf.Trimming = StringTrimming.Character;
            }

            // Hotkey Prefix
            if ((flags & TextFormatFlags.NoPrefix) == TextFormatFlags.NoPrefix)
            {
                sf.HotkeyPrefix = HotkeyPrefix.None;
            }
            else if ((flags & TextFormatFlags.HidePrefix) == TextFormatFlags.HidePrefix)
            {
                sf.HotkeyPrefix = HotkeyPrefix.Hide;
            }
            else
            {
                sf.HotkeyPrefix = HotkeyPrefix.Show;
            }

            // Text Padding
            if ((flags & TextFormatFlags.NoPadding) == TextFormatFlags.NoPadding)
            {
                sf.FormatFlags |= StringFormatFlags.FitBlackBox;
            }

            // Text Wrapping
            if ((flags & TextFormatFlags.SingleLine) == TextFormatFlags.SingleLine)
            {
                sf.FormatFlags |= StringFormatFlags.NoWrap;
            }
            else if ((flags & TextFormatFlags.TextBoxControl) == TextFormatFlags.TextBoxControl)
            {
                sf.FormatFlags |= StringFormatFlags.LineLimit;
            }

            // Other Flags
            //if ((flags & TextFormatFlags.RightToLeft) == TextFormatFlags.RightToLeft)
            //        sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
            if ((flags & TextFormatFlags.NoClipping) == TextFormatFlags.NoClipping)
            {
                sf.FormatFlags |= StringFormatFlags.NoClip;
            }

            return sf;
        }

        private static TextFormatFlags StringFormatToFlags(StringFormat sf)
        {
            var flags = new TextFormatFlags();

            // Translation table: http://msdn.microsoft.com/msdnmag/issues/06/03/TextRendering/default.aspx?fig=true#fig4

            flags = sf.Alignment switch
            {
                // Horizontal Alignment
                StringAlignment.Center => flags & TextFormatFlags.HorizontalCenter,
                StringAlignment.Far => flags & TextFormatFlags.Right,
                _ => flags & TextFormatFlags.Left
            };
            flags = sf.LineAlignment switch
            {
                // Vertical Alignment
                StringAlignment.Far => flags & TextFormatFlags.Bottom,
                StringAlignment.Center => flags & TextFormatFlags.VerticalCenter,
                _ => flags & TextFormatFlags.Top
            };
            switch (sf.Trimming)
            {
                // Ellipsis
                case StringTrimming.EllipsisCharacter:
                    flags &= TextFormatFlags.EndEllipsis;
                    break;
                case StringTrimming.EllipsisPath:
                    flags &= TextFormatFlags.PathEllipsis;
                    break;
                case StringTrimming.EllipsisWord:
                    flags &= TextFormatFlags.WordEllipsis;
                    break;
            }

            switch (sf.HotkeyPrefix)
            {
                // Hotkey Prefix
                case HotkeyPrefix.None:
                    flags &= TextFormatFlags.NoPrefix;
                    break;
                case HotkeyPrefix.Hide:
                    flags &= TextFormatFlags.HidePrefix;
                    break;
            }

            switch (sf.FormatFlags)
            {
                // Text Padding
                case StringFormatFlags.FitBlackBox:
                    flags &= TextFormatFlags.NoPadding;
                    break;
                // Text Wrapping
                case StringFormatFlags.NoWrap:
                    flags &= TextFormatFlags.SingleLine;
                    break;
                case StringFormatFlags.LineLimit:
                    flags &= TextFormatFlags.TextBoxControl;
                    break;
                // Other Flags
                case StringFormatFlags.DirectionRightToLeft:
                    flags &= TextFormatFlags.RightToLeft;
                    break;
                case StringFormatFlags.NoClip:
                    flags &= TextFormatFlags.NoClipping;
                    break;
            }

            return flags;
        }
        #endregion
    }
}
