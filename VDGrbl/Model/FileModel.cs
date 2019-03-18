using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace VDGrbl.Model
{
    /// <summary>
    /// File model class.
    /// </summary>
    public class FileModel : ObservableObject
    {
        #region Properties
        public string FileHeader { get; private set; }
        public string FileName { get; private set; }
        public string FileFilter = "G-Code files|*.txt;*.gcode;*.ngc;*.nc,*.cnc|Tous les fichiers|*.*";
        public int FileFilterIndex = 1;
        public string FileInitialDirectory;
        public string FileTitle = "Fichier G-code";
        public string FileDefaultExt = ".txt";
        #endregion

        #region Constructor
        public FileModel(string fileHeaderInit)
        {
            FileHeader = fileHeaderInit;
        }
        #endregion
    }
}
