using Moq;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Grbl.Errors;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Parsers;
using VDLaser.Core.Grbl.Services;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Services;

namespace VDLaser.Tests.UnitTests
{
    public class GrblCoreServiceTests
    {
        [Fact]
        public async Task ConnectAsync_WhenPortNotDefined_ShouldThrowPortNotDefined()
        {
            var serialMock = new Mock<ISerialPortService>();
            serialMock.SetupGet(s => s.PortName).Returns(string.Empty);

            var logMock = new Mock<ILogService>();
            var providerMock = new Mock<IServiceProvider>();

            var service = new GrblCoreService(
                serialMock.Object,
                logMock.Object,
                Array.Empty<IGrblSubParser>(),
                providerMock.Object
            );

            var ex = await Assert.ThrowsAsync<GrblConnectionException>(
                () => service.ConnectAsync()
            );

            Assert.Equal(
                GrblConnectionError.PortNotDefined,
                ex.Error
            );
        }
        [Fact]
        public void DispatchLine_WhenAlarmReceived_ShouldUpdateStateToAlarm()
        {
            // --- Arrange ---
            var logMock = new Mock<ILogService>();
            var pollingMock = new Mock<IStatusPollingService>();
            var commandQueueMock = new Mock<IGrblCommandQueue>();
            var providerMock = new Mock<IServiceProvider>();

            // On configure le provider pour toutes les dépendances appelées dans DispatchLine
            providerMock
                .Setup(sp => sp.GetService(typeof(IStatusPollingService)))
                .Returns(pollingMock.Object);
            providerMock
                .Setup(sp => sp.GetService(typeof(IGrblCommandQueue)))
                .Returns(commandQueueMock.Object);

            // Initialisation avec une liste vide de parsers (l'alarme est gérée en dur avant les parsers)
            var service = new GrblCoreService(
                Mock.Of<ISerialPortService>(),
                logMock.Object,
                Enumerable.Empty<IGrblSubParser>(),
                providerMock.Object
            );

            bool eventFired = false;
            service.StatusUpdated += (s, e) => eventFired = true;

            // --- Act ---
            // Note : Le code cherche "ALARM:" (avec deux points)
            service.ProcessIncomingLine_ForTests("ALARM:1");

            // --- Assert ---
            // On vérifie l'état de la machine
            Assert.Equal(VDLaser.Core.Grbl.Models.GrblState.MachState.Alarm, service.State.MachineState);
            Assert.True(eventFired, "L'événement StatusUpdated aurait dû être déclenché");

            // On vérifie que le service a bien tenté de forcer un polling pour obtenir plus d'infos
            pollingMock.Verify(p => p.ForcePoll(), Times.Once);
        }
        [Fact]
        public async Task DisconnectAsync_ShouldSendLaserOff()
        {
            var serialMock = new Mock<ISerialConnection>();
            serialMock.SetupGet(s => s.IsOpen).Returns(true);

            var commandQueueMock = new Mock<IGrblCommandQueue>();

            var providerMock = new Mock<IServiceProvider>();
            providerMock
                .Setup(sp => sp.GetService(typeof(IGrblCommandQueue)))
                .Returns(commandQueueMock.Object);

            var service = new GrblCoreService(
                Mock.Of<ISerialPortService>(),
                Mock.Of<ILogService>(),
                new List<IGrblSubParser>(),
                providerMock.Object,
                serialMock.Object
            );

            await service.DisconnectAsync();

            serialMock.Verify(s => s.WriteLine("M5"), Times.Once);
            commandQueueMock.Verify(q => q.Reset(), Times.Once);
        }

    }
}
