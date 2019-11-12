using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDLaser.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDLaser.Tools.Tests
{
    [TestClass()]
    public class GrblToolTests
    {
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
        GrblTool mvm;

        [TestMethod()]
        public void DataGrblSorterTest()
        {
            mvm = new GrblTool();
            mvm.DataGrblSorter(currentStatus, true);
            Assert.AreEqual("Run", mvm.MachineStatus.ToString());
        }

        [TestMethod()]
        public void ProcessInfoResponseVersionTest()
        {
            mvm = new GrblTool();
            mvm.ProcessInfoResponse(infoBuild);
            Assert.AreEqual("0.9i", mvm.VersionGrbl);
        }

        [TestMethod()]
        public void ProcessInfoResponseBuildTest()
        {
            mvm = new GrblTool();
            mvm.ProcessInfoResponse(infoBuild);
            Assert.AreEqual("20150620", mvm.BuildInfo);
        }

        [TestMethod()]
        public void ProcessStartupBlockResponseGrblTest()
        {
            mvm = new GrblTool();
            mvm.ProcessStartupBlockResponse(infoGrbl);
            Assert.AreEqual("View startup blocks", mvm.InfoMessage);
        }

        [TestMethod()]
        public void ProcessResponseTest()
        {
            mvm = new GrblTool();
            mvm.ProcessResponse("Ok");
            Assert.AreEqual("Ok", mvm.ResponseStatus.ToString());
        }

        [TestMethod()]
        public void ProcessResponseError11Test()
        {
            mvm = new GrblTool
            {
                VersionGrbl = "1.1"
            };
            mvm.ProcessErrorResponse(err11, true);
            Assert.AreEqual("Laser mode requires PWM output.", mvm.ErrorMessage);
        }

        [TestMethod()]
        public void ProcessResponseError09Test()
        {
            mvm = new GrblTool
            {
                VersionGrbl = "0.9"
            };
            mvm.ProcessErrorResponse(err09, false);
            Assert.AreEqual("The number value suffix of a G-code word is missing in the G-code block, or when configuring a $Nx=line or $x=val Grbl setting and the x is not a number value.", mvm.ErrorMessage);
        }

        [TestMethod()]
        public void ProcessResponseError09IDTest()
        {
            mvm = new GrblTool
            {
                VersionGrbl = "0.9"
            };
            mvm.ProcessErrorResponse(err09ID, false);
            Assert.AreEqual("A G-code word was repeated in the block.", mvm.ErrorMessage);
        }

        [TestMethod()]
        public void ProcessResponseAlarmTest()
        {
            mvm = new GrblTool
            {
                VersionGrbl = "1.1"
            };
            mvm.ProcessAlarmResponse(ala, true);
            Assert.AreEqual("Homing fail. Reset during active homing cycle.", mvm.AlarmMessage);
        }

        [TestMethod()]
        public void ProcessCurrentStatusResponseXTest()
        {
            mvm = new GrblTool();
            mvm.ProcessCurrentStatusResponse(currentStatus);
            Assert.AreEqual("5.529", mvm.MachinePositionX);
        }

        [TestMethod()]
        public void ProcessCurrentStatusResponseYTest()
        {
            mvm = new GrblTool();
            mvm.ProcessCurrentStatusResponse(currentStatus);
            Assert.AreEqual("30.860", mvm.MachinePositionY);
        }

        [TestMethod()]
        public void ProcessCurrentStatusResponseBuf()
        {
            mvm = new GrblTool();
            mvm.ProcessCurrentStatusResponse(currentStatus);
            Assert.AreEqual("5", mvm.PlannerBuffer);
        }

        [TestMethod()]
        public void ProcessCurrentStatusResponseRX()
        {
            mvm = new GrblTool();
            mvm.ProcessCurrentStatusResponse(currentStatus);
            Assert.AreEqual("2", mvm.RxBuffer);
        }

        [TestMethod()]
        public void ProcessGrblSettingResponseTest()
        {
            string[] arr = set.Split(new Char[] { '=', ' ', '\r', '\n' });//Use moq for real unit test
            Assert.AreEqual("10.000", arr[1]);
        }
    }
}