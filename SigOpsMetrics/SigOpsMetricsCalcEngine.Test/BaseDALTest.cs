using SigOpsMetricsCalcEngine.Core.DataAccess;
using SigOpsMetricsCalcEngine.Core.Models;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;



namespace SigOpsMetricsCalcEngine.Test
{
    [TestClass()]
    public class BaseDALTest : IDisposable
    {
        private DateTime testDay = new DateTime(2024, 6, 1);
        private BaseDataAccessLayer baseDAL;
        private List<long?> eventCodes;
        [TestInitialize()]
        public void TestInitialize()
        {
            // Code that runs before each test
            baseDAL = new BaseDataAccessLayer();
            Console.WriteLine("Test Initialized");
            eventCodes = [101, 102];


        }


        [Fact]
        public void TestStartup()
        {

        }

        [Fact]
        public async Task ParquetTest()
        {
            //var dataAccessLayerMock = new Mock<IDataAccess>();
            //Assign
            var dataMock = new Mock<IDataAccess>();


            if (baseDAL == null) return;
            //Act
            await baseDAL.ProcessEvents(testDay, new List<long?>(), eventCodes);

        //Assert
            Assert.IsNotNull(baseDAL.SignalEvents);
        }

        [Fact]
        public void SQLTest()
        {
            Assert.IsTrue(true);
        }

        [Fact]
        public void GetEventLogTest()
        {
            Assert.IsTrue(true);
        }

        [Fact]
        public void BaseTest()
        {
            //Assign
            //Act
            //Assert
            Assert.IsTrue(true);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Example of a parameterized test using [Theory] and [InlineData].
        /// </summary>
        /// <param name="input">The input value.</param>
        /// <param name="expected">The expected result.</param>
        [Theory]
        //[InlineData(2, 4)]
        //[InlineData(3, 9)]
        //[InlineData(4, 16)]
        public void ParameterizedTest_Should_ReturnExpectedResult(DateTime input, int expected)
        {
            // Act
            var result = baseDAL.GetEventLogsAsync()

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Example of a test that expects an exception.
        /// </summary>
        [Fact]
        public void ExceptionTest_Should_ThrowException_WhenInvalidInput()
        {
            // Arrange
            var invalidInput = -1;

            // Act & Assert
            Assert.ThrowsExceptionAsync<ArgumentException>(() => _testInstance.ValidateInput(invalidInput));
        }

    }
}