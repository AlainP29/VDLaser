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

namespace VDLaser.Tests.UnitTests.Core.Grbl.Services
{
    public class GrblCoreServiceTests
    {
        [Fact]
        public async Task ConnectAsync_WhenPortNotDefined_ShouldThrowPortNotDefined()
        {
            // Arrange
            var serialMock = new Mock<ISerialPortService>();
            serialMock.SetupGet(s => s.PortName).Returns(string.Empty);

            var logMock = new Mock<ILogService>();
            var providerMock = new Mock<IServiceProvider>();
            var parser= new Mock<IConsoleParserService>();

            var service = new GrblCoreService(
                serialMock.Object,
                logMock.Object,
                Array.Empty<IGrblSubParser>(),
                providerMock.Object,
                parser.Object
            );

            // Act
            var ex = await Assert.ThrowsAsync<GrblConnectionException>(
                () => service.ConnectAsync()
            );

            // Assert
            Assert.Equal(
                GrblConnectionError.PortNotDefined,
                ex.Error
            );// We check that the exception thrown has the correct error code indicating that the port was not defined
        }
        [Fact]
        public void DispatchLine_WhenAlarmReceived_ShouldUpdateStateToAlarm()
        {
            // Arrange
            var logMock = new Mock<ILogService>();
            var pollingMock = new Mock<IStatusPollingService>();
            var commandQueueMock = new Mock<IGrblCommandQueue>();
            var providerMock = new Mock<IServiceProvider>();
            var parser = new Mock<IConsoleParserService>();
            providerMock
                .Setup(sp => sp.GetService(typeof(IStatusPollingService)))
                .Returns(pollingMock.Object);
            providerMock
                .Setup(sp => sp.GetService(typeof(IGrblCommandQueue)))
                .Returns(commandQueueMock.Object);

            var service = new GrblCoreService(
                Mock.Of<ISerialPortService>(),
                logMock.Object,
                Enumerable.Empty<IGrblSubParser>(),
                providerMock.Object,
                parser.Object
            );

            bool eventFired = false;
            service.StatusUpdated += (s, e) => eventFired = true;

            // Act
            service.ProcessIncomingLine_ForTests("ALARM:1");

            // Assert
            Assert.Equal(VDLaser.Core.Grbl.Models.GrblState.MachState.Alarm, service.State.MachineState);// We check the machine's status
            Assert.True(eventFired, "The StatusUpdated event should have been triggered");
            pollingMock.Verify(p => p.ForcePoll(), Times.Once);// We verify that the service did indeed attempt to force a poll to obtain more information
        }
        [Fact]
        public async Task DisconnectAsync_ShouldSendLaserOff()
        {
            // Arrange
            var serialMock = new Mock<ISerialConnection>();
            var pollingMock = new Mock<IStatusPollingService>();
            var commandQueueMock = new Mock<IGrblCommandQueue>();
            var parser = new Mock<IConsoleParserService>();
            var providerMock = new Mock<IServiceProvider>();
            providerMock
                .Setup(sp => sp.GetService(typeof(IGrblCommandQueue)))
                .Returns(commandQueueMock.Object);

            bool isOpen = true;
            serialMock.SetupGet(s => s.IsOpen).Returns(() => isOpen);
            serialMock.Setup(s => s.Close()).Callback(() => isOpen = false);

            var service = new GrblCoreService(
                Mock.Of<ISerialPortService>(),
                Mock.Of<ILogService>(),
                new List<IGrblSubParser>(),
                providerMock.Object,
                parser.Object,
                serialMock.Object
            );

            // Act
            await service.DisconnectAsync();

            // Assert
            serialMock.Verify(s => s.WriteLine("M5 S0"), Times.Once);// Verifies that the Laser Off safety command (M5 S0) has been sent to the serial port
            commandQueueMock.Verify(q => q.Reset(), Times.Once);// Verifies that the order queue has been reset
            serialMock.Verify(s => s.Close(), Times.Once);// Check that the serial port has been properly closed
            Assert.False(service.IsConnected);// Checks that the service's connection state has been set to false
        }
        [Fact]
        public async Task ConnectAsync_ShouldSucceed_WhenGrblSignatureReceived()
        {
            // Arrange
            var serialMock = new Mock<ISerialConnection>();
            var configMock = new Mock<ISerialPortService>();
            var logMock = new Mock<ILogService>();
            var commandQueueMock = new Mock<IGrblCommandQueue>();
            var parserMock = new Mock<IConsoleParserService>();
            var pollingMock = new Mock<IStatusPollingService>();
            var providerMock = new Mock<IServiceProvider>();
            providerMock
                .Setup(sp => sp.GetService(typeof(IStatusPollingService)))
                .Returns(pollingMock.Object);
            providerMock
                .Setup(sp => sp.GetService(typeof(IGrblCommandQueue)))
                .Returns(commandQueueMock.Object);

            configMock.SetupGet(c => c.PortName).Returns("COM3");
            configMock.SetupGet(c => c.BaudRate).Returns(115200);

            bool isOpen = false;
            serialMock.SetupGet(s => s.IsOpen).Returns(() => isOpen);
            serialMock.Setup(s => s.Open()).Callback(() => isOpen = true);

            var service = new GrblCoreService(
                configMock.Object,
                logMock.Object,
                Enumerable.Empty<IGrblSubParser>(),
                providerMock.Object,
                parserMock.Object,
                serialMock.Object
            );

            var connectTask = service.ConnectAsync();// Simulation
            await Task.Delay(600);
            service.ProcessIncomingLine_ForTests("Grbl 1.1f ['$' for help]");

            // Act
            await connectTask;

            // Assert
            Assert.True(service.IsConnected);// Checks that the service is considered connected
            serialMock.Verify(s => s.Write(It.Is<byte[]>(b => b[0] == 0x18), 0, 1), Times.AtLeastOnce);// Checks that the initial commands have been sent
            serialMock.Verify(s => s.WriteLine("$I"), Times.AtLeastOnce);
            Assert.True(service.HasLoadedSettings);// Checks that the parameter loading state has passed to true
        }
    }
}
