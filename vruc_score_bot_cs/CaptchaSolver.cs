using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vruc_score_bot_cs
{
    class CaptchaSolver
    {
        private static readonly int NORMALIZE_SIZE = 64;
        private static readonly string GRAYSCALE = " .:-=+*#%@";
        private static Dictionary<string, string> labeled;
        private static Dictionary<string, List<double>> labeled_normalized; 

        static CaptchaSolver()
        {
            labeled = new Dictionary<string, string>
            {
                ["a"] = "iVBORw0KGgoAAAANSUhEUgAAAAsAAAAQCAAAAADl1tqiAAAAuElEQVQIHWNkYA6MtBT9e29j13tGhmOWDGBwx5SRYULKsh0vxUqsGJoZGaT+vmRgYOB9IHSRkQEKtnl+ZGSAghXhDIwMDFpxltJCXOwMDIwMNY1MDBDAGLaS4d/6g08+1jowMB62YQhZy8DAsC6QgfEr1201BiA4bMPA+JvlhCUDAwP3S24Gxgfy72S/MTA01zAwMM5OYdg5iTks5qI+A6PieX4GIFi0eDcDI4N2qz3n7XkTed4zAQBhpjJYiZG3PgAAAABJRU5ErkJggg==",
                ["b"] = "iVBORw0KGgoAAAANSUhEUgAAAA0AAAAXCAAAAAD1zZpdAAAA6klEQVQYGTXBvyvEcRzH8ec7ri67AeUGSrdcrljULX6t7vS5uiyH9ZT/w8JgMUhdIeWjM1iULuoii035tZzEDRIGpLzc91ufx8OEdwQmvCMw4R2BCe8ITHhHYGK/SGBir0RgojpfrmT+7nbWvjGxkVgkcjXxZuK552ir1buUwzsTVMtARz3HiAn6H2mbOmbVxMMgkcRHsmHiZJLYdbplolYg1hj7MXGYJ3Y5+mWiPk7sfuDFRDNFpOu989QEQ7e0FQ5YMUFtVpA8HyZj4qnvbLOZWs6yO2ciP7NA5GL606Tu11Ilq5vt9V/+AYiSVtCojIgMAAAAAElFTkSuQmCC",
                ["c"] = "iVBORw0KGgoAAAANSUhEUgAAAAsAAAAQCAAAAADl1tqiAAAAo0lEQVQIHWNkYGAwzrSTYXl2Zg0jA2NHGQMYHGdkqGpleL/gwl9N3/mMUvfZnlg8ZQACZsaaZoaYpQxgwHjA/pfAdwYwYHwucUOTAQIYf7Aft2KAAMZfrEdsGSCA8Y3wNW0GCGA8bPOL7ycDGDA21jFErGQAA0a5uywPbR8zAIEgI0NDPcOHuZcZFV0+MTIw9RQygMFORgYGBrMMGymm50emAgAO4ixYgrRdAgAAAABJRU5ErkJggg==",
                ["d"] = "iVBORw0KGgoAAAANSUhEUgAAAA0AAAAXCAAAAAD1zZpdAAAA6UlEQVQYGT3BMUtCUQCG4fcbpKZC3cIgCgkiCMGGIHWoaGipTgSB0OYURfQ3IjCopqaGAk9OTmFgcLXIoQgEF2mInGpLCMXIe4fzPMKxBuFYg3CsQTjWIBxrEI41CMcaBMP72fjfy9n1VRYRvk/gu/w5QBS2eDz9HM9l2mMoWecp3QMV10HHR6zeMTDTANUWfkd6+D5i6CvamCVQyaBuqLpIoLSGuiEvReB2E31H3uYIlJfQ83xntI/vfQKdHJLyGJhsgZJ1Kst90M02iOIG3nk7trvSmkJEygl8+eYFgqG9nWm95gvpB/4Bu4pCAaboHh0AAAAASUVORK5CYII=",
                ["e"] = "iVBORw0KGgoAAAANSUhEUgAAAAwAAAAQCAAAAAAHCsHbAAAAsUlEQVQIHWNkYGAIyTbkfLSu6y0DIwPzwmgGEHjudoWRobGOYcvil7ZVnLf1GWXusS6PYmBgiFnMkM5Y2cag+ICBgYH5jcB2xl2uLyUYQOCE+XPGp1IMMPCb8Qc7Axww/mK9lM4ABYyvRa5pM0AB4yHb3wLfGCCAsbaJIWYpAwQwSjxgf2L9iAEIZBgYGUq7GD7Mucwg7eDsw8jA0FbByAAG7owMDAymmQ5SDE+OL9kFANMPL/iRQw3rAAAAAElFTkSuQmCC",
                ["f"] = "iVBORw0KGgoAAAANSUhEUgAAAAsAAAAXCAAAAAD40+oaAAAAk0lEQVQYGRXBvyrAcRQH0PPJaPxZRFltHkNKBt23USZP4Q2ou8pLKIvdzsbiT31d58TYvz49zOtDsD0dGXfBzZX726/jl+D55O3gB8Hn7uOZESxdRtj51WXk4vJ88y/yzub7gz3B0mUES5cRLF1GsHQZwdJlBEuXESxdRrB0GcHSZQRLlxEsXUawdBnB0mUES5fxBy4qMYpZihqtAAAAAElFTkSuQmCC",
                ["g"] = "iVBORw0KGgoAAAANSUhEUgAAAA4AAAAWCAAAAADVpvL7AAABGklEQVQYGS3BOUhCARzA4d8/B/MYbGnKoCGoQJqEDgicDNEgEFoLQUmohjBMaolKySCnDoRoCBeHyIoiFIyGV9FY0NIUdDhIUi+07Hz5fQI637DNjMYu6HY83GEFHs/HhNHVr8EM7t26kS0QlK59D3DgOnaCoBpj08BSqGQBoaqLTwGJibIBhOuOs27gwn5lAyEcJZH8CEwyOw+CPtfLH8VRAQHjZfv7521qpQwIRBbWgtQIPDX2KNQIqMa5+Cv/BNYD/PouKosKgsEXM6Gp9uekOduaTj1DvS1iOe2TjGdvAE04+mYS1ehPovFvlCzyYg4tozly5h2S9haG8kDnjBfXoVhPWig86JvMqOObCOagu62hUrzJbt/DD75pWGiOesCUAAAAAElFTkSuQmCC",
                ["h"] = "iVBORw0KGgoAAAANSUhEUgAAAAwAAAAXCAAAAAAaD/FjAAAAr0lEQVQYGS3BoS4GcBTG4d/rGnSz+YrgAgTNZDunmKIo3IXoDiTNbP5nE4QvEG2u4dsYTRG/JLyc7f88MpVMMpVMMpVMMpVMMpVMMpVMMpVMMvcnZxd7fDxcr2VuNs5pbwcyX1tPtz+7V5tcynB3Cuy/8iLD9if/Votvmfcd2vLoV+b5kDYCmcdj2ghkKmkjkKmkjUCmkjYCmUraCGQqaSOQqaSNQKaSNgKZStoI/gBpNkYaLFsvIAAAAABJRU5ErkJggg==",
                ["k"] = "iVBORw0KGgoAAAANSUhEUgAAAA0AAAAXCAAAAAD1zZpdAAAA3klEQVQYGTXBIUiDYRSG0efKtihY7YJBGaJoGIZhEo0XEZvLA0GLggoTxCYomLUYxrhFDDaDZT9jWNZsazMuyGSO13/hO8dEOImJcBIT4SQmwklMhJOYCCcxEU5iIpzc8wFH9ybCgdMbHuqYCIedl5m33QkmWnssZrO9yhBMPNbmsoXBeh8wcXf8uj2qtsmZaJTO2G8yZeJjEw6fmDLB3/f8cLlPzsTAf7LC+5YAE+FcXXByC5gIp9gp/671wEQ4lDvFz40xJsKBywbX55gIBwrt1UklMxFObqlb+lr5B9jHViMHylzsAAAAAElFTkSuQmCC",
                ["m"] = "iVBORw0KGgoAAAANSUhEUgAAABUAAAAQCAAAAADcsWv5AAAA4klEQVQYGRXBPygEcBjH4c+bwWIzKeuVf2WV4dikmH7vKtPRZbIZDMpms4pFyeD3GmS4lCyUGQtXRimZdErS173PY2Lutr02/v24Xxlsr07o9WyvZ6LVXCFtHl3NkO6bJrqN05Of+a2B3s3SxfHn1M4wGyY4WAd2t+GwBczecW1Co2/A5BN/Ix/0vTTeTTyP0Tf0xcM0qbPwa6KzSBKXy6RaMHFeSCKcVAsmwkkinFQLJsJJIpxUCybCSSKcVAsmwkkinFQLJsJJIpxUCybCSSKcVAsmwkkinFQLJsJJIpxUC/9JdWATZmYXywAAAABJRU5ErkJggg==",
                ["n"] = "iVBORw0KGgoAAAANSUhEUgAAAAwAAAAQCAAAAAAHCsHbAAAAk0lEQVQIHRXBoQ6BYRSA4ffcgxuw+YsgCgLJZDunKcpfuAvRTWiK822CIFBsNtfAmKiIknA4zyPB4DSddXhslh8J6m5NuvQluFW71bu9aDCXgPUE6J05SkDzyd+1eklwb5H2o68EhyHJFQm2Y5IrEhQjuSJBMZIrEhQjuSJBMZIrEhQjuSJBMZIrEhQjuSJBMZIrP0akOBO+Gv1EAAAAAElFTkSuQmCC",
                ["p"] = "iVBORw0KGgoAAAANSUhEUgAAAA0AAAAXCAAAAAD1zZpdAAAA6klEQVQYGTXBzyvDcRzH8ec71NofsIPUioN20VZzUUsR5YY+K6122EEOVv4PFw4cFUW7+IiDksuyWuTitsaWww5+XYQDcnjZ91ufx8PEdKNUzdKpbf9gYjW/RuRu9t3E4+j5wfNwtYB3JtivAAP1AnkTGnmib+6SLRPtDJGhz0TTxMUCsVbm1YR3xJpTvybOFondTn6bqM8Q6469mOiliSQ/Bq9MMP5A39IJmyY4XRYkrrNMmHhLNfZ66Y0ctZKJ3WSFyM38l4nD8sp6TvdHO3+YOC4SmPCOwIR3BCa8IzDhHYEJ7whMeEfwD377VdCo/PAPAAAAAElFTkSuQmCC",
                ["q"] = "iVBORw0KGgoAAAANSUhEUgAAAA0AAAAXCAAAAAD1zZpdAAAA5klEQVQYGT3BwSuDYQDH8e8vLU5krlPSlpLSag4r2w5oBxfzSJRyc1pK/g0t24o/AXm2006amnqNLCW1g5CDOHFU2prsfQ/P5yMY2t2K/d0XK+krxOhlHN/h0zHifI2b0sf4TuY5hhItbtNdUHUFdLBP9oK+6Taomfwd7uJ7j6CvsfYMgUYGdULX8wRqy6gT8lIEKqvoO/w4S6C+gO7mfkZ6+N4mUGGPlEff5Cso0aKx2AOdroOo5vCOPiPbSy9RRLgex1ccyCMYzG9O6aF8drKBcKxBONYgHGsQjjUIxxqEYw3CsYZ/v2JBAfh4IXoAAAAASUVORK5CYII=",
                ["r"] = "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAQCAAAAAAO4WGhAAAAa0lEQVQIHQ3BsQ2BARSF0e+WJvh7W1Cq/4a8xBwWUFlEo3tRqbUWMIIg0dJx3XNkZpf1bnqTWU4OcJXZbr77+1vm85w/QIbxBMi8hh8gc14QMscVIdNFyHQRMl2ETBch00XIdBEyXYRMF/EHjncpL5/cXScAAAAASUVORK5CYII=",
                ["s"] = "iVBORw0KGgoAAAANSUhEUgAAAAsAAAAQCAAAAADl1tqiAAAAsElEQVQIHWNkYBAq9FNlf3Nh0xJGBoVDsgxgsIiRYVUow7y1n+R9Ar0YGT7ynbBkAAKJl4wMP9gP2zGAASPDcQuG3CkMIMDI4LCbheFU2+Z/DAyMDAxOcxQZGG40rGRgZGBgYEssU2JgWB3FyAACzLFdogxNjAwQoHye9yEjAxRs8/zJKMl1lwEImK+p3WO0Obhr+7Vfion2DB2M1kcYIOCgFyNTSLiBOOvbK8sW/QMA4PAuz0VLd+UAAAAASUVORK5CYII=",
                ["t"] = "iVBORw0KGgoAAAANSUhEUgAAAAkAAAASCAAAAACs66uUAAAAg0lEQVQIHRXBIUoDcBgH0PebyFhxabBruBs4MJm+FS8wMBq9x5JHsPllQZPdtCC4aLDIBPtffC9MVh9HhPVzNcL9TTVisV9UI7vtzL+vPLj2+skxDNUIQzXCUI0wVCMM1QhDNcJQjTBsHhF+zm53CC8Xh6t3y7B+OvE7PQ0u787n329/jn4nYyhKTzAAAAAASUVORK5CYII=",
                ["u"] = "iVBORw0KGgoAAAANSUhEUgAAAAwAAAAPCAAAAAD1inGVAAAAj0lEQVQIHRXBoQ7BcRDA8e9NZApZoCveQxUE1RP8g/1uw50gippg8wCyKpgXEGyKoJKk4z4fCdRJxZBAnVQMCdRJxZBAnVQMCdRJxZBAnVQMCdRJxZBAnaRLJFAnrSskmC9I+zHyaWyn/Mmjg1wHz94XmOxAZitO83trVL3bSP3cJ13siNCsht3a7bBpvvgBz3cxAd3P9doAAAAASUVORK5CYII=",
                ["v"] = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAAAAAA6mKC9AAAA6ElEQVQYGSXBvUpCYRzA4d8fkdOBDuGQH4ljQ03R0gWUIJxExIgoMSibWhqdWryC5oywU0PoIHoIRK+gra2hTSReoyQKTFHoffF5JNIPPB4wV89N40J3+2d5jOEou50SitdkmhiHDxzfCSEV9AoYjcwo/Cvgu9/hCeB8WLV9BPIe7hOQ98g2EFgc2LcnQDM9jI0RoJ77ikxZUtZNEQTYq5FqU6iS7IIAtnIqZ/jue2IGgnZ/NIjanwtXF4Cg7bbYXPHZegYELahCpcT52yqaYFROO/H18iWaYOx0/ixZe0UTjEAvxssGxj9SFUAnzuB5lQAAAABJRU5ErkJggg==",
                ["w"] = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAQCAAAAAApT+BJAAABbElEQVQYGRXBQUiTARiA4fdDIREq1nCHBU0X5mEMDaIagbWRzEJGYKTmISEGNmgjCGVCA8uFNIhc5KIUdkmXgSASzIGNCJY7JCUxpDYNF0yT4dpES122/3mkepnX7Sg8/UxdQTFyE52wVJM+huJzPZvqHUq+1X4/Ibywo1sB9Kk/W0esEUCzRsAhtI/TOQbc9UXXrz11Alff0DopaFYl4ABipvvpl8t6wH/7nzonsGD80gDan2JeSWFIwPzJT6cQeOIqqgp0Bwrq3eTx3kcczpb5ehCwTWGNEGmaaOOZ40Mjl99ijSBwKFv+wKP6Vd4RomW6WLXh7fur2kaAuTPhS9df7VTlqcxWdISiF6IWEOChO6MN3phpBsLWUfvvg/cGQADLLNpY9a3ngHPox8Uk52IgQEXuQFdw/2gGqFnCMZxX74FQ8s78/nz8LIrFuo+maRsglPR5wT2I4vEdcPkBoeR0HAwJFJZZMH4F/gNjmHcvVR1rZAAAAABJRU5ErkJggg==",
                ["x"] = "iVBORw0KGgoAAAANSUhEUgAAAA0AAAAQCAAAAADoyKrlAAAA30lEQVQIHQ3BMUgCAQCG0e8nxBINGsNoEDKVApeCqKFBXCQdtKHFRWiwrcVAOLhFSGgscHERGookwcXBwXAIBBUlMShaoqFFCUQirO493ceIl7EkbrmTe7D45v+GheHqyC/Sl2RzYJikikIPexPvx8qzox5C4OvaS8nro+nmKwIM8+/snEweBNjaG9DZmoGw7DZhuwUIy+kFnFwBAtxD52hpvP4JAm4Ov3YeXaUkCMI1sjnDZL+BsPfX3r1Tx8vyIPgjDJNUEY4LZPLyPM33g78w1wtMAqpGOKhiiVao/AMDQECiS2wcwwAAAABJRU5ErkJggg==",
                ["y"] = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAXCAAAAAAnnZAFAAABIklEQVQYGS3BTyiDcRzA4c+XiDE5OFgx+VND0rJykDQOSOZCOZnDLjJlp10UcVhpB0ltl13MRSmFtFxstRxcNrUarYzDrE05rFYrYt73x/NI21v99Tz/Lh2fwunytzmP0p6rPRPsUbZ8KN59ZgQe+7N9VXRPltceAc8BkzE0Y3fs7Am05htPnGhCrp+unADhlYqpBIaCMTKHAONx3AFwHrN4jqBJDSVsELUXO78QNBtHDKe6n8XvBUHTkm/ye7d3sWRA0IVc2d70QHwCEHSj9ziuWA0DgpK0PlhLpgogKGtBCK6jERRjvhlbAo2g1LyYkyPoBGU2gjuATlAuFsodJXSCbvqGQw+KsFQwTG02fAy+owhVNGVHjD9C2lxXvPVl+PcLYlhTAkHDudMAAAAASUVORK5CYII="
            };

            labeled_normalized = labeled.Select(
                pair =>
                {
                    using (var bitmap = OpenBitmapFromBase64(pair.Value))
                    {
                        return new KeyValuePair<string, List<double>>(pair.Key, NormalizeSingleChar(bitmap, 0));
                    }
                }).ToDictionary(x => x.Key, x => x.Value);
        }

        public static List<double> NormalizeSingleChar(Bitmap image, int axis)
        {
            var ret = new List<double>();

            using (var image_resized = ResizeBitmap(image, NORMALIZE_SIZE, NORMALIZE_SIZE))
            {
                for (int r = 0; r < image_resized.Height; r++)
                    for (int c = 0; c < image_resized.Width; c++)
                    {
                        ret.Add(((image_resized.GetPixel(c, r).ToArgb() >> (axis * 8)) & 0xFF) / 256.0);
                    }
            }

            return ret;
        }

        public static Bitmap OpenBitmapFromBase64(string b64image)
        {
            var bytes = Convert.FromBase64String(b64image);
            using (var memory_stream = new System.IO.MemoryStream(bytes))
                return new Bitmap(memory_stream);
        }

        public static string SolveManually(string b64image)
        {
            using (var image = OpenBitmapFromBase64(b64image))
            {
                DrawImageOnConsole(image, 3);

                Console.Write("\n\nCaptcha: ");

                return Console.ReadLine().Trim();
            }
        }

        public static string Solve(string b64image)
        {
            using (var image_ = OpenBitmapFromBase64(b64image))
            using (var image = ResizeBitmap(image_, image_.Width + 2, image_.Height + 2))
            {
                DrawImageOnConsole(image, 3);

                var start_pos = new List<int>();
                var end_pos = new List<int>();

                bool prev_mark = false;
                for (int c = 0; c < image.Width; c++)
                {
                    bool mark = false;

                    for (int r = 0; r < image.Height; r++)
                        mark = mark || (image.GetPixel(c, r).A > 0);
                    
                    if (mark != prev_mark)
                    {
                        if (prev_mark == false)
                        {
                            start_pos.Add(c);
                        }
                        else
                        {
                            end_pos.Add(c);
                        }
                    }

                    prev_mark = mark;
                }

                var result = start_pos.Zip(end_pos, (start, end) =>
                {
                    int spos = 0;
                    int cpos = 0;

                    bool pmark = false;
                    for (int r = 0; r < image.Height; r++)
                    {
                        bool mark = false;

                        for (int c = start; c < end; c++)
                            mark = mark || (image.GetPixel(c, r).A > 0);

                        if (mark != pmark)
                        {
                            if (pmark == false)
                            {
                                spos = r;
                            }
                            else
                            {
                                cpos = r;
                            }
                        }

                        pmark = mark;
                    }

                    using (var chr = image.Clone(new Rectangle(start, spos, end - start, cpos - spos), image.PixelFormat))
                    {
                        string ch = "?";
                        double cost = 1e10;
                        var normalized = NormalizeSingleChar(chr, 3);

                        foreach (var item in labeled_normalized)
                        {
                            var new_cost = normalized
                                            .Zip(item.Value, (x, y) => (x - y) * (x - y))
                                            .Aggregate(0.0, (x, y) => x + y);

                            if (new_cost < cost)
                            {
                                ch = item.Key;
                                cost = new_cost;
                            }
                        }

                        return ch;
                    }
                }).Aggregate("", (x, y) => x + y);

                Console.WriteLine($"Captcha: {result}\n\n");

                return result;
            }
        }

        public static void DrawImageOnConsole(Bitmap bitmap, int axis)
        {
            for (int r = 0; r < bitmap.Height; r += 2)
            {
                for (int c = 0; c < bitmap.Width; c++)
                {
                    var p1 = (bitmap.GetPixel(c, r).ToArgb() >> (axis * 8)) & 0xFF;
                    var p2 = 0;

                    if (r + 1 < bitmap.Height)
                        p2 = (bitmap.GetPixel(c, r + 1).ToArgb() >> (axis * 8)) & 0xFF;

                    var p = (int)((p1 + p2) / 512.0 * 10);

                    Console.Write(GRAYSCALE[p]);
                }
                Console.Write('\n');
            }
        }

        public static Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.High;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(bmp, 1, 1, width - 2, height - 2);
            }

            return result;
        }
    }
}
