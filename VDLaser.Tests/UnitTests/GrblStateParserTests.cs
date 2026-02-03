using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Grbl.Parsers;
using VDLaser.Core.Interfaces;
using static VDLaser.Core.Grbl.Models.GrblState;

namespace VDLaser.Tests.UnitTests
{
    public class GrblStateParserTests
    {
        /// <summary>
        /// Test unitaire principal (status complet)
        /// </summary>
        [Fact]
        public void Parse_StatusLine_UpdatesMachineAndWorkPositions()
        {
            // Arrange
            var log = new Mock<ILogService>();
            var parser = new GrblStateParser(log.Object);
            var state = new GrblState();

            var line = "<Idle|MPos:1.000,2.000,3.000|WPos:4.000,5.000,6.000|FS:100,200>";

            // Act
            parser.Parse(line, state);

            // Assert
            Assert.Equal(MachState.Idle, state.MachineState);

            Assert.Equal(1.0, state.MachinePosX);
            Assert.Equal(2.0, state.MachinePosY);
            Assert.Equal(3.0, state.MachinePosZ);

            Assert.Equal(4.0, state.WorkPosX);
            Assert.Equal(5.0, state.WorkPosY);
            Assert.Equal(6.0, state.WorkPosZ);

            Assert.Equal("100", state.MachineFeed);
            Assert.Equal("200", state.MachineSpeed);
        }
        /// <summary>
        /// Test : overrides & buffers
        /// </summary>
        [Fact]
        public void Parse_StatusLine_UpdatesOverridesAndBuffers()
        {
            // Arrange
            var log = new Mock<ILogService>();
            var parser = new GrblStateParser(log.Object);
            var state = new GrblState();

            var line = "<Run|Ov:120,80,100|Bf:15,128>";

            // Act
            parser.Parse(line, state);

            // Assert
            Assert.Equal(MachState.Run, state.MachineState);
            Assert.Equal("120", state.OverrideMachineFeed);
            Assert.Equal("80", state.OverrideMachineSpeed);
            Assert.Equal("15", state.PlannerBuffer.ToString());
            Assert.Equal("128", state.RxBuffer.ToString());
        }
        /// <summary>
        /// Test : WCO (offsets)
        /// </summary>
        [Fact]
        public void Parse_StatusLine_UpdatesWorkOffsets()
        {
            // Arrange
            var log = new Mock<ILogService>();
            var parser = new GrblStateParser(log.Object);
            var state = new GrblState();

            var line = "<Idle|WCO:10.5,20.25,0.75>";

            // Act
            parser.Parse(line, state);

            // Assert
            Assert.Equal("10.5", state.OffsetPosX);
            Assert.Equal("20.25", state.OffsetPosY);
            Assert.Equal("0.75", state.OffsetPosZ);
        }
        /// <summary>
        /// Test : ligne invalide (robustesse)
        /// </summary>
        [Fact]
        public void Parse_InvalidLine_DoesNotThrow()
        {
            // Arrange
            var log = new Mock<ILogService>();
            var parser = new GrblStateParser(log.Object);
            var state = new GrblState();

            // Act
            var ex = Record.Exception(() =>
                parser.Parse("<Idle|MPos:abc,def>", state)
            );

            // Assert
            Assert.Null(ex);
        }

    }
}
