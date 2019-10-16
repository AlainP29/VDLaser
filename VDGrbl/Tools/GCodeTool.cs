using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.Win32;
using NLog;

namespace VDGrbl.Tools
{
    /// <summary>
    /// G-code class: usefull tool to format, check or parse G-code file.
    /// </summary>
    public class GCodeTool
    {
        #region Fields
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Enum
        public enum GcodeMMode : short { R, L, CW, CCW };
        public enum GcodeDMode : short { A, R };
        public enum MCodeState : short { End, Constant, Dynamic, Stop };
        #endregion

        #region public property
        /// <summary>
        /// Get the GCodeLine property. The Gcode line.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GCodeLine { get; private set; }

        /// <summary>
        /// Get the FileList property. FileList is populated w/ lines of G-code file.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<string> FileList { get; private set; }

        /// <summary>
        /// Get the X property. The X position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string X { get; private set; }

        /// <summary>
        /// Get the Y property. The Y position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Y { get; private set; }

        /// <summary>
        /// Get the I property. The I position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string I { get; private set; }

        /// <summary>
        /// Get the J property. The J position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string J { get; private set; }

        /// <summary>
        /// Get the S property. The laser power.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string S { get; private set; }

        /// <summary>
        /// Get the F property. The feed rate.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string F { get; private set; }

        /// <summary>
        /// Get the G property. The G mode.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string G { get; private set; }

        /// <summary>
        /// Get the X property. The X position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string M { get; private set; }

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
        { }

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
                if (!line.StartsWith("$") || !line.StartsWith("%") || !line.StartsWith("(") || !line.StartsWith(";") || !line.StartsWith(";"))
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
        public string FormatGcode(int d, int g, double x, double y, double z, double f, double s)
        {
            int D = d;
            int G = g;
            double X = x * s;
            double Y = y * s;
            double Z = z * s;
            double F = f;
            string fLine = string.Format("g9{0}g{1}x{2}y{3}z{4}f{5}", D, G, X, Y, Z, F);
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
        public string TrimGcode(string line)
        {
            char[] trimArray = new char[] { '\r', '\n' };
            return line.ToLower().Replace(" ", string.Empty).TrimEnd(trimArray);
        }

        /// <summary>
        /// Remove \n at the end of a G-code line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public string TrimEndGcode(string line)
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
        public string SecondToTime(double s)
        {
            TimeSpan ts = TimeSpan.FromSeconds(s);
            return ts.ToString(@"hh\:mm\:ss\:fff");
        }

        /// <summary>
        /// Parse GCodeline property. Get values of G-Code commands
        /// </summary>
        public void ParseGCode()
        {
            var arr = GCodeLine.ToUpper().Split(' ');
            try
            {
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
            catch(Exception ex)
            {
                logger.Error("Method ParseGCode raised: {0}", ex.ToString());
            }
        }
        /// <summary>
        /// Parse line. Get values of G-Code commands
        /// </summary>
        public void ParseGCode(string line)
        {
            var arr = line.ToUpper().Split(' ');
            try
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    //logger.Info("GCodeTool|ParseGCode2" + arr[i].ToString());
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
            catch (Exception ex)
            {
                logger.Error("Method ParseGCode raised: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Process M-Codes
        /// </summary>
        /// <param name="m"></param>
        public void ProcessMCode(string mcode)
        {
            switch (mcode)
            {
                case "2":
                    MCode = MCodeState.End;
                    break;
                case "3":
                    MCode = MCodeState.Constant;
                    break;
                case "4":
                    MCode = MCodeState.Dynamic;
                    break;
                case "5":
                    MCode = MCodeState.Stop;
                    break;
            }
        }

        /// <summary>
        /// Process G-Codes
        /// </summary>
        /// <param name="gcode"></param>
        public void ProcessGCode(string gcode)
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

        /// <summary>
        /// Estimate the total time in ms
        /// </summary>
        /// <param name="maxFeedRate"></param>
        /// <returns></returns>
        public double CalculateJobTime(double maxFeedRate)
        {
            double dist = 0, time = 0;
            double x0 = 0, y0 = 0, x1 = 0, y1 = 0;
            try
            {
                for (int i = 0; i < FileList.Count; i++)
                {
                    if (FileList[i].Contains('.'))
                    {
                        GCodeLine = FileList[i].Replace('.', ',');
                    }
                    else
                    {
                        GCodeLine = FileList[i];
                    }
                    ParseGCode();
                    if ((int)DMode == 0)
                    {
                        if (X != "0")//utiliser écriture simplifiée ?:
                        {
                            x1 = Convert.ToDouble(X);
                        }
                        else
                        {
                            x1 = 0;
                        }
                        if (Y != "0")
                        {
                            y1 = Convert.ToDouble(Y);
                        }
                        else
                        {
                            y1 = 0;
                        }
                        dist = MathTool.Distance(x0, y0, x1, y1);
                        if ((int)MMode == 0 && maxFeedRate != 0)
                        {
                            time += dist / maxFeedRate;
                        }
                        else if ((int)MMode == 1 || (int)MMode == 2 || (int)MMode == 3)
                        {
                            if (F != "0")
                            {
                                time += dist / Convert.ToDouble(F);
                            }
                            time += 0;
                        }
                        else
                        {
                            logger.Info("GCodeTool|CalculateJobTime MMode inconnu:" + MMode);
                        }
                        x0 = x1;
                        y0 = y1;
                    }
                    else if ((int)DMode == 1)
                    {
                        if (X != "0")
                        {
                            x1 = Convert.ToDouble(X);
                        }
                        else
                        {
                            x1 = 0;
                        }
                        if (Y != "0")
                        {
                            y1 = Convert.ToDouble(Y);
                        }
                        else
                        {
                            y1 = 0;
                        }
                        dist = MathTool.Distance(x1, y1);
                        if ((int)MMode == 0 && maxFeedRate != 0)
                        {
                            time += dist / maxFeedRate;
                        }
                        else if ((int)MMode == 1 || (int)MMode == 2 || (int)MMode == 3)
                        {
                            if (F != "0")
                            {
                                time += dist / Convert.ToDouble(F);
                            }
                            time += 0;
                        }
                        else
                        {
                            logger.Info("GCodeTool|CalculateJobTime MMode inconnu:" + MMode);
                        }
                        x0 = x1;
                        y0 = y1;
                    }
                    else
                    {
                        logger.Info("GCodeTool|CalculateJobTime DMode inconnu:" + DMode);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GCodeTool|Exception CalculateJobTime raised:" + ex);
            }
            return Math.Round(time * 60);
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
                            xe = Convert.ToDouble(X) + xs;
                            ye = Convert.ToDouble(Y) + ys;
                            points.Add(new System.Windows.Point(xe, ye));
                            xs = xe;
                            ys = ye;
                        }
                        else//Absolute mode
                        {
                            xe = Convert.ToDouble(X);
                            ye = Convert.ToDouble(Y);
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
        public PointCollection GetGCodePointCollection(double offsetAxisX,double offsetAxisY)
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
                            xe = Convert.ToDouble(X) + xs;
                            ye = Convert.ToDouble(Y) + ys;
                            points.Add(new System.Windows.Point(xe, ye));
                            xs = xe;
                            ys = ye;
                        }
                        else//Absolute mode
                        {
                            xe = Convert.ToDouble(X);
                            ye = Convert.ToDouble(Y);
                            points.Add(new System.Windows.Point(xe + offsetAxisX, ye + offsetAxisX));
                        }
                    }
                }
            }
            return points;
        }
        #endregion
    }
}
