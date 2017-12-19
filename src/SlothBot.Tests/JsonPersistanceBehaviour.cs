using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SlothBot.Bridge;
using SlothBot.Tests.Properties;

namespace SlothBot.Tests
{
    public class StringType
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public static class StringTypeExtensions
    {
        public static KeyValuePair<string, StringType> WithValue(this string key, string value)
        {
            return new KeyValuePair<string, StringType>(key, new StringType()
            {
                key = key,
                value = value
            });
        }
    }

    [Category("Persistance")]
    public class JsonPersistanceBehaviour : BaseTestFixture<IDictionary<string, StringType>>
    {
        private string store;

        public override IDictionary<string, StringType> SetupFixture()
        {
            store = "DictionaryUnitTest.json";
            return ReadOptimisedJsonFilePersistantDictionary<string, StringType>.From(store, v => v.key);
        }
        
        protected override void Cleanup()
        {
            DeleteIf(store);
        }

        [Test]
        public void should_write_to_file_when_operations_performed()
        {
            sut.ContainsKey("aa").Should().BeFalse("the key wasnt added");
            sut.ContainsKey("aa").Should().BeFalse("the key wasnt added");

            sut.Add("aa".WithValue("aaa"));
            sut.Add("bb".WithValue("bbb"));

            sut.ContainsKey("aa").Should().BeTrue("the key was added");
            sut.ContainsKey("bb").Should().BeTrue("the key was added");

            sut.Remove("aa");

            sut.ContainsKey("aa").Should().BeFalse("the key was removed");
            sut.ContainsKey("bb").Should().BeTrue("the key was not removed");

            sut = ReadOptimisedJsonFilePersistantDictionary<string, StringType>.From(store, v => v.key);

            sut.ContainsKey("aa").Should().BeFalse("the key was removed");
            sut.ContainsKey("bb").Should().BeTrue("the key was not removed");
        }

        [Test]
        public void should_load_persisted_data_on_create()
        {
            var filename = Guid.NewGuid().ToString();

            try
            {
                var dict = new Dictionary<string, StringType>();
                sut.Add("a".WithValue("aa"));
                sut.Add("b".WithValue("bb"));
                File.WriteAllText(filename, JsonConvert.SerializeObject(dict));

                sut = ReadOptimisedJsonFilePersistantDictionary<string, StringType>.From(store, v => v.key);
                sut["a"].value.Should().Be("aa", "peristed data should load");
                sut["b"].value.Should().Be("bb", "peristed data should load also");
            }
            finally
            {
                DeleteIf(filename);
            }
        }
        
        public void DeleteIf(string filename)
        {
            try
            {
                File.Delete(filename);
            }
            catch (Exception)
            {
                //dont care
            }
        }
    }

   
}
