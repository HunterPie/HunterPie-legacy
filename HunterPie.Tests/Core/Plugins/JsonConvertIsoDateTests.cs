using System;
using Newtonsoft.Json;
using NUnit.Framework;

namespace HunterPie.Tests.Core.Plugins
{
    [TestFixture]
    public class JsonConvertIsoDateTests
    {
        [Test]
        public void DateFormatConverter_Read_WithTime()
        {
            // arrange
            var json = @"{
""DateTime"": ""2021-12-13 23:55""
}";
            // act
            var result = JsonConvert.DeserializeObject<Foo>(json);

            // assert
            Assert.That(result.DateTime, Is.EqualTo(new DateTime(2021, 12, 13, 23, 55, 0)));
        }

        [Test]
        public void DateFormatConverter_Write_WithTime()
        {
            // arrange
            var foo = new Foo {DateTime = new DateTime(2021, 12, 13, 23, 55, 0)};

            // act
            var result = JsonConvert.SerializeObject(foo);

            // assert
            Assert.That(result, Is.EqualTo(@"{""DateTime"":""2021-12-13T23:55:00""}"));
        }

        [Test]
        public void DateFormatConverter_Read_NoTime()
        {
            // arrange
            var json = @"{
""DateTime"": ""2021-12-13""
}";
            // act
            var result = JsonConvert.DeserializeObject<Foo>(json);

            // assert
            Assert.That(result.DateTime, Is.EqualTo(new DateTime(2021, 12, 13, 0, 0, 0)));
        }

        [Test]
        public void DateFormatConverter_Write_NoTime()
        {
            // arrange
            var foo = new Foo {DateTime = new DateTime(2021, 12, 13, 0, 0, 0)};

            // act
            var result = JsonConvert.SerializeObject(foo);

            // assert
            Assert.That(result, Is.EqualTo(@"{""DateTime"":""2021-12-13T00:00:00""}"));
        }

        public class Foo
        {
            public DateTime? DateTime { get; set; }
        }
    }
}
