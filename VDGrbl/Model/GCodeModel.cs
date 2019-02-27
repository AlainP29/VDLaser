using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace VDGrbl.Model
{
    class GCodeModel : ObservableObject
    {
        #region private Members

        #endregion

        #region public Properties
        public string GCodeLine { get; private set; }
        public string LineNumber { get; private set; }
        public string XGCode { get; private set; }
        public string YGCode { get; private set; }
        public string ZGCode { get; private set; }
        public double IGCode { get; set; }
        public double JGCode { get; set; }
        public double SGCode { get; set; }
        public double FGCode { get; set; }
        public string UnitMode { get; private set; }
        public bool InBuffer { get; private set; }
        public bool IsProssed { get; private set; }
        public bool IsGCode { get; private set; } = true;
        public bool IsMotionMode { get; private set; }
        public bool IsDistanceMode { get; private set; }
        public bool IsRapidMove { get; private set; }
        public bool IsLinearMove { get; private set; }
        public bool IsAbsolutePositionning { get; private set; }
        public bool IsRelativePositionning { get; private set; }
        public bool IsCW { get; private set; }
        public bool IsCCW { get; private set; }
        public bool IsMetric { get; private set; }
        public bool IsImperial { get; private set; }
        public ObservableCollection<GCodeModel> oc = new ObservableCollection<GCodeModel>();
        #endregion
    }
}


