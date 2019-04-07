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
        MainViewModel mvm;

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