using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Grbl.Services;
using VDLaser.Core.Interfaces;
using Xunit;
using static VDLaser.Core.Grbl.Models.GrblState;

namespace VDLaser.Tests.UnitTests
{
    public class StatusPollingServiceTests
    {
        /// <summary>
        /// Test unitaire : envoi du ?
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Tick_WhenConnectedAndNotPending_SendsStatusQuery()
        {
            // Arrange
            var core = new Mock<IGrblCoreService>();
            var log = new Mock<ILogService>();

            core.SetupGet(c => c.IsConnected).Returns(true);
            core.SetupGet(c => c.State).Returns(new GrblState
            {
                MachineState = MachState.Idle
            });

            var service = new StatusPollingService(core.Object, log.Object);

            // Act
            service.Tick_ForTests();

            // Assert
            core.Verify(
                c => c.SendRealtimeCommandAsync((byte)'?'),
                Times.Once);
        }
        /// <summary>
        /// Test : pas de double envoi si pending
        /// </summary>
        [Fact]
        public void Tick_WhenRequestPending_DoesNotSendAgain()
        {
            // Arrange
            var core = new Mock<IGrblCoreService>();
            var log = new Mock<ILogService>();

            core.SetupGet(c => c.IsConnected).Returns(true);
            core.SetupGet(c => c.State).Returns(new GrblState
            {
                MachineState = MachState.Idle
            });

            var service = new StatusPollingService(core.Object, log.Object);

            // Act
            service.Tick_ForTests(); // first ?
            service.Tick_ForTests(); // should be ignored

            // Assert
            core.Verify(
                c => c.SendRealtimeCommandAsync((byte)'?'),
                Times.Once);
        }
        /// <summary>
        /// Test : reset du pending sur StatusUpdated
        /// </summary>
        [Fact]
        public void StatusUpdated_ResetsPendingFlag()
        {
            // Arrange
            var core = new Mock<IGrblCoreService>();
            var log = new Mock<ILogService>();

            core.SetupGet(c => c.IsConnected).Returns(true);
            core.SetupGet(c => c.State).Returns(new GrblState
            {
                MachineState = MachState.Idle
            });

            var service = new StatusPollingService(core.Object, log.Object);

            // Act
            service.Tick_ForTests(); // envoie ?
            core.Raise(c => c.StatusUpdated += null, EventArgs.Empty);
            service.Tick_ForTests(); // doit renvoyer ?

            // Assert
            core.Verify(
                c => c.SendRealtimeCommandAsync((byte)'?'),
                Times.Exactly(2));
        }
        /// <summary>
        /// Test : pas de polling en Alarm
        /// </summary>
        [Fact]
        public void Tick_WhenInAlarm_DoesNotPoll()
        {
            // Arrange
            var core = new Mock<IGrblCoreService>();
            var log = new Mock<ILogService>();

            core.SetupGet(c => c.IsConnected).Returns(true);
            core.SetupGet(c => c.State).Returns(new GrblState
            {
                MachineState = MachState.Alarm
            });

            var service = new StatusPollingService(core.Object, log.Object);

            // Act
            service.Tick_ForTests();

            // Assert
            core.Verify(
                c => c.SendRealtimeCommandAsync(It.IsAny<byte>()),
                Times.Never);
        }

    }

}
