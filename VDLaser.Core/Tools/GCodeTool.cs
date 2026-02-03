using System.Globalization;
using System.Windows;
using System.Windows.Media;
using VDLaser.Core.Tools.Geometry;

namespace VDLaser.Core.Tools
{
    /// <summary>
    /// G-code class: usefull tool to format, check or parse G-code file.
    /// </summary>
    public class GCodeTool
    {
        #region Fields
        //private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Enum
        public enum GcodeMMode : Int32 { R, L, CW, CCW };
        public enum GcodeDMode : Int32 { A, R };
        public enum MCodeState : Int32 { End, Constant, Dynamic, Stop };
        #endregion

        #region public property
        /// <summary>
        /// Get the GCodeLine property. The Gcode line.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GCodeLine { get; private set; } = string.Empty;

        /// <summary>
        /// Get the FileList property. FileList is populated w/ lines of G-code file.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<string> FileList { get; private set; } = new List<string>();

        /// <summary>
        /// Get the X property. The X position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string X { get; private set; } = "0";

        /// <summary>
        /// Get the Y property. The Y position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Y { get; private set; } = "0";

        /// <summary>
        /// Get the I property. The I position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string I { get; private set; } = string.Empty;

        /// <summary>
        /// Get the J property. The J position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string J { get; private set; } = string.Empty;

        /// <summary>
        /// Get the S property. The laser power.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string S { get; private set; } = string.Empty;

        /// <summary>
        /// Get the F property. The feed rate.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string F { get; private set; } = string.Empty;

        /// <summary>
        /// Get the G property. The G mode.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string G { get; private set; } = string.Empty;

        /// <summary>
        /// Get the X property. The X position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string M { get; private set; } = string.Empty;

        /// <summary>
        /// Get the MMode property. This is the current movement mode (rapid G0, Low G1, CW G2, CCW G3).
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public GcodeMMode MMode { get; private set; }

        /// <summary>
        /// Get the DMode property. This is the current deplacement mode (absolute G90 or relative G91).
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public GcodeDMode DMode { get; private set; }

        /// <summary>
        /// Get the Mcode property. This is the current M-code (end program, stop spindle/laser).
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public MCodeState MCode { get; private set; }

        /// <summary>
        /// Get the IsGcode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsGCode { get; private set; }

        /// <summary>
        /// Get the IsGcode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMetric { get; private set; }

        /// <summary>
        /// Get the IsImperial property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsImperial { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor for methods like TrimGcode...
        /// </summary>
        public GCodeTool()
        {
        }

        /// <summary>
        /// Initialize a new instance of GCodeTool with a G-Code line as parameter which is parsed.
        /// </summary>
        /// <param name="line"></param>
        public GCodeTool(string line)
        {
            if (line != null)
            {
                if (line.Contains('.'))
                {
                    GCodeLine = line.Replace('.', ',');
                }
                else
                {
                    GCodeLine = line;
                }
                if (!line.StartsWith("$", StringComparison.InvariantCulture) || !line.StartsWith("%", StringComparison.InvariantCulture) || !line.StartsWith("(", StringComparison.InvariantCulture) || !line.StartsWith(";", StringComparison.InvariantCulture) || !line.StartsWith(";", StringComparison.InvariantCulture))
                {
                    IsGCode = true; //TODO
                }
            }
            ParseGCode();//TODO In or out of constructor?
        }

        /// <summary>
        /// Initialize a new instance of GCodeTool with a GCode file to calculate the total job time.
        /// </summary>
        /// <param name="fileList"></param>
        public GCodeTool(List<string> fileList)
        {
            FileList = fileList;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Static method to format a G-code line using distance mode G9, motion mode G, positions X, Y, Z, FeedRate and Step.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="fl"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string? FormatGcode(int d, int g, double x, double y, double f, double s)
        {
            int D = d;
            int G = g;
            double X = x * s;
            double Y = y * s;
            double F = f;
            string fLine = string.Format(CultureInfo.CurrentCulture, "G9{0} G{1} X{2} Y{3} F{4}", D, G, X, Y, F);
            if (fLine.Contains(","))
            {
                return fLine.Replace(',', '.');
            }
            else
            {
                return fLine;
            }
        }

        /// <summary>
        /// Change/Remove characters in a G-code line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string? TrimGcode(string line)
        {
            char[] trimArray = new char[] { '\r', '\n' };
            if (!string.IsNullOrEmpty(line))
            {
                return line.ToLower(CultureInfo.CurrentCulture).Replace(" ", string.Empty).TrimEnd(trimArray);
            }
            return null;
        }

        /// <summary>
        /// Remove carriage return at the end of a G-code line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string? TrimEndGcode(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                char[] trimArray = new char[] { '\r', '\n' };
                return line.TrimEnd(trimArray);
            }
            return null;
        }

        /// <summary>
        /// Convert second in time. Use converter instead! TODO
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string? SecondToTime(double s)
        {
            try
            {
                TimeSpan ts = TimeSpan.FromSeconds(s);
                return ts.ToString(@"hh\:mm\:ss\:fff", CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                //logger.Error(CultureInfo.CurrentCulture, "GCodeTool|Exception SecondToTime raised {0}", ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Parse GCodeline property. Get values of G-Code commands
        /// </summary>
        public void ParseGCode()
        {
            var arr = GCodeLine.ToUpper(CultureInfo.CurrentCulture).Split(' ');

            for (int i = 0; i < arr.Length; i++)
            {
                //logger.Info("GCodeTool1" + arr[i].ToString());

                if (arr[i].Contains("X"))
                {
                    X = arr[i].Remove(0, 1);
                }
                else if (arr[i].Contains("Y"))
                {
                    Y = arr[i].Remove(0, 1);
                }
                else if (arr[i].Contains("I"))
                {
                    I = arr[i].Remove(0, 1);
                }
                else if (arr[i].Contains("J"))
                {
                    J = arr[i].Remove(0, 1);
                }
                else if (arr[i].Contains("F"))
                {
                    F = arr[i].Remove(0, 1);
                }
                else if (arr[i].Contains("S"))
                {
                    S = arr[i].Remove(0, 1);
                }
                else if (arr[i].Contains("G"))
                {
                    G = arr[i].Remove(0, 1);
                    ProcessGCode(G);
                }
                else if (arr[i].Contains("M"))
                {
                    M = arr[i].Remove(0, 1);
                    ProcessMCode(M);
                }
                else
                {
                    arr[i] = string.Empty;
                }
            }
        }
        private string ExtractValue(string line, char code)
        {
            var parts = line.Split(code);
            return parts.Length > 1 ? parts[1].Split(' ')[0] : "0";
        }
        /// <summary>
        /// Parse line. Get values of G-Code commands
        /// </summary>
        public void ParseGCode(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            {
                var arr = line.ToUpper(CultureInfo.CurrentCulture).Split(' ');
                for (int i = 0; i < arr.Length; i++)
                {
                    //logger.Info("GCodeTool|ParseGCode2" + arr[i].ToString());
                    if (line.Contains("X")) X = ExtractValue(line, 'X');
                    if (line.Contains("Y")) Y = ExtractValue(line, 'Y');
                    if (line.Contains("I")) I = ExtractValue(line, 'I');
                    if (line.Contains("J")) J = ExtractValue(line, 'J');
                    if (line.Contains("F")) F = ExtractValue(line, 'F');
                    if (line.Contains("S")) S = ExtractValue(line, 'S');
                    if (line.Contains("G")) G = ExtractValue(line, 'G');
                    if (line.Contains("M")) M = ExtractValue(line, 'M');
                }
            }
        }

        /// <summary>
        /// Process M-Codes
        /// </summary>
        /// <param name="m"></param>
        public void ProcessMCode(string mcode)
        {
            if (!string.IsNullOrEmpty(mcode))
            {
                MCode = mcode switch
                {
                    "2" => MCodeState.End,
                    "3" => MCodeState.Constant,
                    "4" => MCodeState.Dynamic,
                    "5" => MCodeState.Stop,
                    _ => MCodeState.End,
                };
            }
            else MCode = MCodeState.End;
        }

        /// <summary>
        /// Process G-Codes
        /// </summary>
        /// <param name="gcode"></param>
        public void ProcessGCode(string gcode)
        {
            if (!string.IsNullOrEmpty(gcode))
            {
                switch (gcode)
                {
                    case "0":
                        MMode = GcodeMMode.R;
                        break;
                    case "00":
                        MMode = GcodeMMode.R;
                        break;
                    case "1":
                        MMode = GcodeMMode.L;
                        break;
                    case "01":
                        MMode = GcodeMMode.L;
                        break;
                    case "2":
                        MMode = GcodeMMode.CW;
                        break;
                    case "02":
                        MMode = GcodeMMode.CW;
                        break;
                    case "3":
                        MMode = GcodeMMode.CCW;
                        break;
                    case "03":
                        MMode = GcodeMMode.CCW;
                        break;
                    case "90":
                        DMode = GcodeDMode.A;
                        break;
                    case "91":
                        DMode = GcodeDMode.R;
                        break;
                    case "21":
                        IsMetric = true;
                        IsImperial = false;
                        break;
                    case "20":
                        IsImperial = true;
                        IsMetric = false;
                        break;
                    default:
                        DMode = GcodeDMode.A;
                        MMode = GcodeMMode.R;
                        break;
                }
            }
        }
        /// <summary>
        /// Estimate job time in second only in number of lines and transfer delay.
        /// </summary>
        /// <param name="transferDelay"></param>
        /// <returns></returns>
        public double CalculateJobTime(int transferDelay)
        {
            double time = FileList.Count * transferDelay;
            //logger.Info("GCodeTool|CalculateJobTime {0}",time);

            return Math.Round(time / 1000);
        }

        /// <summary>
        /// Estimate the total time in second including transfer delay and max speed
        /// </summary>
        /// <param name="transferDelay"></param>
        /// <param name="maxFeedRate"></param>
        /// <returns></returns>
        public double CalculateJobTime(int transferDelay, double maxFeedRate)
        {
            double time = 0;
            double x0 = 0, y0 = 0;
            for (int i = 0; i < FileList.Count; i++)
            {
                time += transferDelay / 1000;
                if (FileList[i].Contains('.'))
                {
                    GCodeLine = FileList[i].Replace('.', ',');
                }
                else
                {
                    GCodeLine = FileList[i];
                }
                ParseGCode();
                double y1;
                double x1;
                double dist;
                if ((int)DMode == 0)
                {
                    if (X != "0")//utiliser écriture simplifiée ?:
                    {
                        x1 = Convert.ToDouble(X, CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        x1 = 0;
                    }
                    if (Y != "0")
                    {
                        y1 = Convert.ToDouble(Y, CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        y1 = 0;
                    }
                    dist = GeometryEngine.Distance(x0, y0, x1, y1);
                    if ((int)MMode == 0 && maxFeedRate != 0)
                    {
                        time += dist / maxFeedRate;
                    }
                    else if ((int)MMode == 1 || (int)MMode == 2 || (int)MMode == 3)
                    {
                        if (F != "0")
                        {
                            time += dist / Convert.ToDouble(F, CultureInfo.CurrentCulture);
                        }
                        time += 0;
                    }
                    else
                    {
                        //logger.Info("GCodeTool|CalculateJobTime MMode inconnu:" + MMode);
                    }
                    x0 = x1;
                    y0 = y1;
                }
                else if ((int)DMode == 1)
                {
                    if (X != "0")
                    {
                        x1 = Convert.ToDouble(X, CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        x1 = 0;
                    }
                    if (Y != "0")
                    {
                        y1 = Convert.ToDouble(Y, CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        y1 = 0;
                    }
                    dist = GeometryEngine.Distance(x1, y1);
                    if ((int)MMode == 0 && maxFeedRate != 0)
                    {
                        time += dist / maxFeedRate;
                    }
                    else if ((int)MMode == 1 || (int)MMode == 2 || (int)MMode == 3)
                    {
                        if (F != "0")
                        {
                            time += dist / Convert.ToDouble(F, CultureInfo.CurrentCulture);
                        }
                        time += 0;
                    }
                    else
                    {
                        //logger.Info("GCodeTool|CalculateJobTime MMode inconnu:" + MMode);
                    }
                    x0 = x1;
                    y0 = y1;
                }
                else
                {
                    //logger.Info("GCodeTool|CalculateJobTime DMode inconnu:" + DMode);
                }
            }
            //return Math.Round(time * 60);
            return Math.Round(time);
        }

        /// <summary>
        /// Point collection
        /// </summary>
        /// <returns></returns>
        public PointCollection GetGCodePointCollection()
        {
            PointCollection points = new PointCollection();
            if (FileList.Count > 0)
            {
                double xs = 0, xe;
                double ys = 0, ye;
                foreach (string line in FileList)
                {
                    ParseGCode(line);
                    if (line.Contains("X") || line.Contains("Y"))
                    {
                        if (DMode == GcodeDMode.R)//Relatif mode
                        {
                            xe = Convert.ToDouble(X, CultureInfo.CurrentCulture) + xs;
                            ye = Convert.ToDouble(Y, CultureInfo.CurrentCulture) + ys;
                            points.Add(new System.Windows.Point(xe, ye));
                            xs = xe;
                            ys = ye;
                        }
                        else//Absolute mode
                        {
                            xe = Convert.ToDouble(X, CultureInfo.CurrentCulture);
                            ye = Convert.ToDouble(Y, CultureInfo.CurrentCulture);
                            points.Add(new System.Windows.Point(xe, ye));
                        }
                    }
                }
            }
            return points;
        }

        /// <summary>
        /// Add an offset to the point collection: for instance the origin of coordinate plane.
        /// </summary>
        /// <param name="offsetAxisX"></param>
        /// <param name="offsetAxisY"></param>
        /// <returns></returns>
        public PointCollection GetGCodePointCollection(double offsetAxisX, double offsetAxisY)
        {
            PointCollection points = new PointCollection();
            if (FileList.Count > 0)
            {
                double xs = 0, xe;
                double ys = 0, ye;
                points.Add(new System.Windows.Point(offsetAxisX, offsetAxisY));
                foreach (string line in FileList)
                {
                    ParseGCode(line);
                    if (line.Contains("X") || line.Contains("Y"))
                    {
                        if (DMode == GcodeDMode.R)//Relatif mode
                        {
                            xe = Convert.ToDouble(X, CultureInfo.CurrentCulture) + xs;
                            ye = Convert.ToDouble(Y, CultureInfo.CurrentCulture) + ys;
                            points.Add(new System.Windows.Point(xe, ye));
                            xs = xe;
                            ys = ye;
                        }
                        else//Absolute mode
                        {
                            xe = Convert.ToDouble(X, CultureInfo.CurrentCulture);
                            ye = Convert.ToDouble(Y, CultureInfo.CurrentCulture);
                            points.Add(new System.Windows.Point(xe + offsetAxisX, ye + offsetAxisX));
                        }
                    }
                }
            }
            return points;
        }

        public PointCollection GetGCodePointCollection(double offsetAxisX, double offsetAxisY, double scale = 1.0)
        {
            var points = new PointCollection();
            if (scale <= 0 || FileList == null || FileList.Count == 0) return points;

            double xs = 0, ys = 0;
            points.Add(new Point(offsetAxisX, offsetAxisY));

            foreach (string line in FileList)
            {
                ParseGCode(line);
                if (!string.IsNullOrEmpty(X) || !string.IsNullOrEmpty(Y))
                {
                    double xe = double.TryParse(X, NumberStyles.Any, CultureInfo.CurrentCulture, out double xVal) ? xVal * scale : 0;
                    double ye = double.TryParse(Y, NumberStyles.Any, CultureInfo.CurrentCulture, out double yVal) ? yVal * scale : 0;

                    if (DMode == GcodeDMode.R) // Relatif
                    {
                        xe += xs;
                        ye += ys;
                        xs = xe;
                        ys = ye;
                    }
                    else // Absolu
                    {
                        xe += offsetAxisX;
                        ye += offsetAxisY;
                    }

                    points.Add(new Point(xe, ye));
                }
            }
            return points;
        }
        //Ajouter méthode pour obtenir les dimensions du Gcode (minX, minY, maxX, maxY)
        //GetMaxMin, CheckGCode, etc., avec des try-parse pour robustesse
        #endregion
    }
}
