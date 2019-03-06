using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace VDGrbl.Model
{
    public class ConsoleModel:ObservableObject//TODO
    {
        public string _consoleInput = string.Empty;
        ObservableCollection<string> _consoleOutput = new ObservableCollection<string>() { "Console Emulation Sample..." };

        #region Properties
        /// <summary>
        /// The <see cref="ConsoleInput" /> property's name.
        /// </summary>
        public const string ConsoleInputPropertyName = "ConsoleInput";
        /// <summary>
        /// Gets the ConsoleInput property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ConsoleInput
        {
            get
            {
                return _consoleInput;
            }
            set
            {
                Set(ref _consoleInput, value);
            }
        }

        /// <summary>
        /// The <see cref="ConsoleOutput" /> property's name.
        /// </summary>
        public const string ConsoleOutputPropertyName = "ConsoleOutput";
        /// <summary>
        /// Gets the ConsoleOutput property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> ConsoleOutput
        {
            get
            {
                return _consoleOutput;
            }
            set
            {
                Set(ref _consoleOutput, value);
            }
        }
        #endregion

        #region Methods
        public void RunCommand()
        {
            ConsoleOutput.Add(ConsoleInput);
            // do your stuff here.
            ConsoleInput = String.Empty;
        }
        #endregion
    }
}
