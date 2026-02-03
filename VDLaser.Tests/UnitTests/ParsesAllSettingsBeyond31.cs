using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Grbl.Commands;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Grbl.Parsers;
using VDLaser.Core.Grbl.Services;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Services;

namespace VDLaser.Tests.UnitTests
{
    public class ParsesAllSettingsBeyond31
    {
        [Fact]
        public void GetSettingsAsync_ParsesAllSettingsBeyond31()
        {
            // ---------- Arrange ----------
            var mockSerial = new Mock<ISerialPortService>();
            mockSerial.Setup(s => s.PortName).Returns("COM3");

            var mockLogger = new Mock<ILogService>();
            var parser = new GrblSettingsParser(mockLogger.Object);
            var mockStatusPolling = Mock.Of<IStatusPollingService>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var service = new GrblCoreService(
                mockSerial.Object,
                mockLogger.Object,
                new[] { parser },
                mockServiceProvider.Object
            );

            IReadOnlyCollection<GrblSetting>? receivedSettings = null;

            service.SettingsUpdated += (_, settings) =>
            {
                receivedSettings = settings;
            };

            // ---------- Act ----------
            // simulation de $$ GRBL
            service.ProcessIncomingLine_ForTests("$0=10");
            service.ProcessIncomingLine_ForTests("$30=1000");
            service.ProcessIncomingLine_ForTests("$31=0");
            service.ProcessIncomingLine_ForTests("$32=1");
            service.ProcessIncomingLine_ForTests("$100=80.000");
            service.ProcessIncomingLine_ForTests("$130=500.000");

            // ---------- Assert ----------
            Assert.NotNull(receivedSettings);

            // vérifie qu'on dépasse bien $31
            Assert.Contains(receivedSettings!, s => s.Id == 32);
            Assert.Contains(receivedSettings!, s => s.Id == 100);
            Assert.Contains(receivedSettings!, s => s.Id == 130);

            // vérifie le nombre total
            Assert.Equal(6, receivedSettings!.Count);
        }

    }
}
