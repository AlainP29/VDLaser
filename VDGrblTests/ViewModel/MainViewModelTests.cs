using VDGrbl.ViewModel;
using GalaSoft.MvvmLight;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using VDGrbl.Model;

namespace VDGrbl.ViewModel.Tests
{
    [TestClass()]
    public class MainViewModelTests : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly string infoBuild = "[0.9i.20150620:]";
        private readonly string infoGrbl = "Grbl 0.9i ['$' for help]";
        //private readonly string currentStatus = "<Run,MPos:5.529,30.860,-7.000,WPos:1.529,-5.440,-0.000>";//For Grbl version 0.9 and lower
        //private readonly string currentStatus = "<Run,MPos:5.529,30.860,-7.000,WPos:1.529,-5.440,-0.000,Buf:5>";//For Grbl version 0.9 and lower w/ buffers
        private readonly string currentStatus = "<Run,MPos:5.529,30.860,-7.000,WPos:1.529,-5.440,-0.000,Buf:5,RX:2>";//For Grbl version 0.9 and lower w/ buffers
        //private readonly string currentStatus = "<Run|MPos:5.529,30.860,-7.000,>";//For Grbl version 1.1 and higher
        private readonly string err11 = "error:17";
        private readonly string err09 = "error: bad number format";
        private readonly string err09ID = "error:Invalid gcode ID:25";
        private readonly string ala = "alarm:6";
        private readonly string set = "$120=10.000 (description)";
        MainViewModel mvm;

        [TestMethod()]
        public void ProcessInfoResponseVersionTest()
        {
            mvm = new MainViewModel(_dataService);
            mvm.ProcessInfoResponse(infoBuild);
            Assert.AreEqual("0.9i", mvm.VersionGrbl);
        }

        [TestMethod()]
        public void ProcessInfoResponseBuildTest()
        {
            mvm = new MainViewModel(_dataService);
            mvm.ProcessInfoResponse(infoBuild);
            Assert.AreEqual("20150620", mvm.BuildInfo);
        }

        [TestMethod()]
        public void ProcessInfoResponseGrblTest()
        {
            mvm = new MainViewModel(_dataService);
            mvm.ProcessInfoResponse(infoGrbl);
            Assert.AreEqual("View startup blocks", mvm.InfoMessage);
        }

        [TestMethod()]
        public void DataGrblSorterMachineStatusTest()
        {
            mvm = new MainViewModel(_dataService);
            mvm.DataGrblSorter(currentStatus);
            Assert.AreEqual("Run", mvm.MachineStatus.ToString());
        }

        [TestMethod()]
        public void ProcessResponseTest()
        {
            mvm = new MainViewModel(_dataService);
            mvm.ProcessResponse("Ok");
            Assert.AreEqual("Ok", mvm.ResponseStatus.ToString());
        }

        [TestMethod()]
        public void ProcessResponseError11Test()
        {
            mvm = new MainViewModel(_dataService)
            {
                VersionGrbl = "1.1"
            };
            mvm.ProcessErrorResponse(err11);
            Assert.AreEqual("Laser mode requires PWM output.", mvm.ErrorMessage);
        }

        [TestMethod()]
        public void ProcessResponseError09Test()
        {
            mvm = new MainViewModel(_dataService)
            {
                VersionGrbl = "0.9"
            };
            mvm.ProcessErrorResponse(err09);
            Assert.AreEqual("The number value suffix of a G-code word is missing in the G-code block, or when configuring a $Nx=line or $x=val Grbl setting and the x is not a number value.", mvm.ErrorMessage);
        }

        [TestMethod()]
        public void ProcessResponseError09IDTest()
        {
            mvm = new MainViewModel(_dataService)
            {
                VersionGrbl = "0.9"
            };
            mvm.ProcessErrorResponse(err09ID);
            Assert.AreEqual("A G-code word was repeated in the block.", mvm.ErrorMessage);
        }

        [TestMethod()]
        public void ProcessResponseAlarmTest()
        {
            mvm = new MainViewModel(_dataService);
            mvm.ProcessAlarmResponse(ala);
            Assert.AreEqual("Homing fail. Reset during active homing cycle.", mvm.AlarmMessage);
        }

        [TestMethod()]
        public void ProcessSettingsResponseTest()
        {
            string[] arr = set.Split(new Char[] { '=', ' ', '\r', '\n' });
            Assert.AreEqual("10.000", arr[1]);
        }

        [TestMethod()]
        public void ProcessCurrentStatusResponseXTest()
        {
            mvm = new MainViewModel(_dataService);
            mvm.ProcessCurrentStatusResponse(currentStatus);
            Assert.AreEqual("5.529", mvm.PosX);
        }

        [TestMethod()]
        public void ProcessCurrentStatusResponseYTest()
        {
            mvm = new MainViewModel(_dataService);
            mvm.ProcessCurrentStatusResponse(currentStatus);
            Assert.AreEqual("30.860", mvm.PosY);
        }

        [TestMethod()]
        public void ProcessCurrentStatusResponseZTest()
        {
            mvm = new MainViewModel(_dataService);
            mvm.ProcessCurrentStatusResponse(currentStatus);
            Assert.AreEqual("-7.000", mvm.PosZ);
        }

        [TestMethod()]
        public void ProcessCurrentStatusResponseBuf()
        {
            mvm = new MainViewModel(_dataService);
            mvm.ProcessCurrentStatusResponse(currentStatus);
            Assert.AreEqual("5", mvm.Buf);
        }

        [TestMethod()]
        public void ProcessCurrentStatusResponseRX()
        {
            mvm = new MainViewModel(_dataService);
            mvm.ProcessCurrentStatusResponse(currentStatus);
            Assert.AreEqual("2", mvm.RX);
        }

        [TestMethod()]
        public void JogWTest()
        {
            mvm = new MainViewModel(_dataService)
            {
                Step = "0.5",
                FeedRate = 200
            };
            mvm.JogW(true);
            Assert.AreEqual("g91g1x-0.5y0z0f200", mvm.TXLine);
        }
    }
}