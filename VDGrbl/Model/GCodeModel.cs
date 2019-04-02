using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using VDGrbl.Tools;

namespace VDGrbl.Model
{
    public class GCodeModel : ObservableObject
    {
        #region Fields
        private string _gcodeLine;
        private string _n;
        private string _x;
        private string _y;
        private string _s;
        private string _f;
        private string _g;
        private string _m;
        private bool _isGcode;
        private bool _isMetric;
        private bool _isImperial;
        public enum GcodeMMode : short { R, L, CW, CCW };
        private GcodeMMode _mMode = GcodeMMode.R;
        public enum GcodeDMode : short { A, R };
        private GcodeDMode _dMode = GcodeDMode.A;
        public enum MCodeState : short { End, Stop };
        private MCodeState _mcode = MCodeState.End;
        private List<string> _fileList = new List<string>();
        #endregion

        #region public Properties
        /// <summary>
        /// The <see cref="GCodeLine" /> property's name.
        /// </summary>
        public const string GcodeLinePropertyName = "GcodeLine";
        /// <summary>
        /// Get the GcodeLine property. The Gcode line.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GCodeLine
        {
            get { return _gcodeLine; }
            set { Set(ref _gcodeLine, value); }
        }

        /// <summary>
        /// The <see cref="FileList" /> property's name.
        /// </summary>
        public const string FileListPropertyName = "FileList";
        /// <summary>
        /// Gets the FileList property. FileList is populated w/ lines of G-code file.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<string> FileList
        {
            get
            {
                return _fileList;
            }
            set
            {
                Set(ref _fileList, value);
            }
        }

        /// <summary>
        /// The <see cref="N" /> property's name.
        /// </summary>
        public const string NPropertyName = "N";
        /// <summary>
        /// Get the N property. The Gcode line number.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string N
        {
            get { return _n; }
            set { Set(ref _n, value); }
        }

        /// <summary>
        /// The <see cref="X" /> property's name.
        /// </summary>
        public const string XPropertyName = "X";
        /// <summary>
        /// Get the X property. The X position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string X
        {
            get { return _x; }
            set { Set(ref _x, value); }
        }

        /// <summary>
        /// The <see cref="Y" /> property's name.
        /// </summary>
        public const string YPropertyName = "Y";
        /// <summary>
        /// Get the Y property. The Y position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Y
        {
            get { return _y; }
            set { Set(ref _y, value); }
        }

        /// <summary>
        /// The <see cref="S" /> property's name.
        /// </summary>
        public const string sPropertyName = "S";
        /// <summary>
        /// Get the S property. The laser power.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string S
        {
            get { return _s; }
            set { Set(ref _s, value); }
        }

        /// <summary>
        /// The <see cref="F" /> property's name.
        /// </summary>
        public const string FPropertyName = "F";
        /// <summary>
        /// Get the F property. The feed rate.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string F
        {
            get { return _f; }
            set { Set(ref _f, value); }
        }

        /// <summary>
        /// The <see cref="G" /> property's name.
        /// </summary>
        public const string GPropertyName = "G";
        /// <summary>
        /// Get the G property. The G mode.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string G
        {
            get { return _g; }
            set { Set(ref _g, value); }
        }

        /// <summary>
        /// The <see cref="M" /> property's name.
        /// </summary>
        public const string MPropertyName = "M";
        /// <summary>
        /// Get the X property. The X position.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string M
        {
            get { return _m; }
            set { Set(ref _m, value); }
        }

        /// <summary>
        /// The <see cref="IsGcode" /> property's name.
        /// </summary>
        public const string IsGcodePropertyName = "IsGcode";
        /// <summary>
        /// Get the IsGcode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsGCode
        {
            get { return _isGcode; }
            set { Set(ref _isGcode, value); }
        }

        /// <summary>
        /// The <see cref="IsMetric" /> property's name.
        /// </summary>
        public const string IsMetricPropertyName = "IsMetric";
        /// <summary>
        /// Get the IsGcode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMetric
        {
            get { return _isMetric; }
            set { Set(ref _isMetric, value); }
        }

        /// <summary>
        /// The <see cref="IsImperial" /> property's name.
        /// </summary>
        public const string IsImperialPropertyName = "IsImperial";
        /// <summary>
        /// Get the IsImperial property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsImperial
        {
            get { return _isImperial; }
            set { Set(ref _isImperial, value); }
        }

        /// <summary>
        /// The <see cref="MMode" /> property's name.
        /// </summary>
        public const string MModePropertyName = "MMode";
        /// <summary>
        /// Get the MMode property. This is the current movement mode (absolute or relative).
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public GcodeMMode MMode
        {
            get { return _mMode; }
            set { Set(ref _mMode, value); }
        }

        /// <summary>
        /// The <see cref="DMode" /> property's name.
        /// </summary>
        public const string DModePropertyName = "DMode";
        /// <summary>
        /// Get the DMode property. This is the current deplacement mode (absolute or relative).
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public GcodeDMode DMode
        {
            get { return _dMode; }
            set { Set(ref _dMode, value); }
        }

        /// <summary>
        /// The <see cref="MCode" /> property's name.
        /// </summary>
        public const string McodePropertyName = "Mcode";
        /// <summary>
        /// Get the Mcode property. This is the current M-code (end program, stop spindle/laser).
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public MCodeState MCode
        {
            get { return _mcode; }
            set { Set(ref _mcode, value); }
        }

        public ObservableCollection<GCodeModel> oc = new ObservableCollection<GCodeModel>();
        #endregion

        #region Constructor
        public GCodeModel()
        {
            
        }

        public GCodeModel(string line)
        {
            if (line.Contains('.'))
            {
                _gcodeLine = line.Replace('.',',');
            }
            else
            {
                _gcodeLine = line;
            }
            if(!line.StartsWith("$") || !line.StartsWith("%") || !line.StartsWith("(") || !line.StartsWith(";") || !line.StartsWith(";"))
            {
                IsGCode = true; //TODO
            }
            ParseGCode();
        }

        public GCodeModel(List<string> fileListInit)
        {
            _fileList = fileListInit;
        }
        #endregion

        #region methods
        /// <summary>
        /// Parse Gcode line.
        /// </summary>
        public void ParseGCode()
        {
            var arr = GCodeLine.Split(' ');
            for (int i=0 ; i<arr.Length ; i++)
            {
                if(arr[i].Contains("X"))
                {
                    X = arr[i].Remove(0, 1);
                }
                else if(arr[i].Contains("Y"))
                {
                    Y = arr[i].Remove(0, 1);
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
                case "5":
                    MCode=MCodeState.Stop;
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
                    if( FileList[i].Contains('.'))
                    {
                        _gcodeLine = FileList[i].Replace('.', ',');
                    }
                    else
                    {
                        _gcodeLine = FileList[i];
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
                        if ((int)MMode == 0 && maxFeedRate!=0)
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
                            //logger.Info("CalculateJobTime MMode inconnu:" + gm.MMode);
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
                            //logger.Info("CalculateJobTime MMode inconnu:" + gm.MMode);
                        }
                        x0 = x1;
                        y0 = y1;
                    }
                    else
                    {
                        //logger.Info("CalculateJobTime DMode inconnu:" + gm.DMode);
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Error("Exception CalculateJobTime raised:" + ex);
            }
            return Math.Round(time*60);
            //return test;
        }
        #endregion
    }
}


