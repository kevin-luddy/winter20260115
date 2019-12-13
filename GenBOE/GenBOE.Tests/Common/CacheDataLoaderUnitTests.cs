using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GenBOE.Common;
using GenBOE.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GenBOE.Tests.Common
{
    [TestClass]
    public class CacheDataLoaderUnitTests
    {
        private CacheDataLoader cacheDataLoader = null;
        private Mock<ICache> cacheMock = null;
        private Mock<GenBOEUtilities> utilMock = null;

        private const int INFINITE_SECONDS_TO_CACHE = -1;
        private const int LIMITED_SCONDS_TO_CACHE = 600;

        //keys are all lowercase since ToLower() is used inside the CacheDataLoader class
        private const string TEST_KEY = "testcachekey";
        private const string TEST_NOCACHE_KEY = "missingkey";
        private const string KEY_PREFIX = "testcachekeyprefix";

        private const int EXTRA_PARM_VALUE = 1;
        private const string RETURNED_VALUE = "AWESOME VALUE"; 
        private const string NONCACHE_RETURNED_VALUE = "No Cache Result";
        private const string DICTIONARY_NONCACHE_VALUE = "collection nocache result";

        private Dictionary<string, int> cacheKeyDictionary = new Dictionary<string, int>() 
            { 
                {"testdictcachekey", 1}, 
                {"testdictcachekey2", 2} 
            };

        private Dictionary<string, int> cacheMissKeyDictionary = new Dictionary<string, int>() 
            { 
                {"misseddictcachekey", 3}, 
                {"misseddictcachekey2", 4} 
            };

        IList<string> dictionaryResults = new List<string>() { "collectionValue1", "collectionValue2" };


        private object[] callbackParms = new object[1]{"asdf"};

        private delegate object BasicCallback();
        private delegate object ParameterizedCallback(object parm);
        private delegate ICollection CollectionCallback(ICollection<int> parms);
        private delegate ICollection CollectionWithParmCallback(ICollection<int> parms, int extraParm);

        [TestInitialize]
        public void Init()
        {
            //cacheMock = new Mock<ICache>(MockBehavior.Strict);
            //utilMock = new Mock<GenBOEUtilities>(MockBehavior.Strict);
            cacheMock = new Mock<ICache>();
            utilMock = new Mock<GenBOEUtilities>();

            cacheMock.Setup(s => s.GetData(TEST_KEY)).Returns(RETURNED_VALUE);
            cacheMock.Setup(s => s.GetData(TEST_NOCACHE_KEY)).Returns(null);

            cacheMock.Setup(s => s.GetData("testdictcachekey")).Returns(dictionaryResults[0]);
            cacheMock.Setup(s => s.GetData("testdictcachekey2")).Returns(dictionaryResults[1]);
            cacheMock.Setup(s => s.Contains("testdictcachekey")).Returns(true);
            cacheMock.Setup(s => s.Contains("testdictcachekey2")).Returns(true);


            cacheDataLoader = new CacheDataLoader(cacheMock.Object, INFINITE_SECONDS_TO_CACHE, utilMock.Object);

            
        }

        public object BasicCallbackProcessor()
        {
            return NONCACHE_RETURNED_VALUE;
        }

        public object ParameterizedCallbackProcessor(object parm)
        {
            return NONCACHE_RETURNED_VALUE;
        }

        public ICollection CollectionCallbackProcessor(ICollection<int> parms)
        {
            ArrayList result = new ArrayList();
            if (parms == null)
            {
                return result;
            }

            foreach(int i in parms)
            {
                result.Add(new TestDto(DICTIONARY_NONCACHE_VALUE, i));
            }
            return result;
        }

        public ICollection CollectionWithParmCallbackProcessor(ICollection<int> parms, int extraParm)
        {
            ArrayList result = new ArrayList();
            if (parms == null)
            {
                return result;
            }

            foreach(int i in parms)
            {
                result.Add(new TestDto(DICTIONARY_NONCACHE_VALUE, i));
            }
            return result;
        }
        

        [TestMethod]
        public void CacheDataLoader_GetData_Basic_NominalTest()
        {
            //act
            object result = 
                cacheDataLoader.GetData(new BasicCallback(BasicCallbackProcessor), null, TEST_KEY);

            //assert
            cacheMock.Verify(v => v.Add(TEST_KEY, result, INFINITE_SECONDS_TO_CACHE), Times.Never());
            cacheMock.Verify(v => v.GetData(TEST_KEY), Times.Once());
            Assert.AreEqual(RETURNED_VALUE, result);
        }
        
        //cache miss tests
        [TestMethod]
        public void CacheDataLoader_GetData_MissCacheBasicCallback_NominalTest()
        {
            //act
            object result =
                cacheDataLoader.GetData(new BasicCallback(BasicCallbackProcessor), null, TEST_NOCACHE_KEY);
            //assert
            cacheMock.Verify(v => v.Add(TEST_NOCACHE_KEY, result, INFINITE_SECONDS_TO_CACHE), Times.Once());
            cacheMock.Verify(v => v.GetData(TEST_NOCACHE_KEY), Times.Exactly(2));
            Assert.AreEqual(NONCACHE_RETURNED_VALUE, result);

        }

        [TestMethod]
        public void CacheDataLoader_GetData_MissCacheParameterizedCallback_NominalTest()
        {
            //act
            object result =
                cacheDataLoader.GetData(new ParameterizedCallback(ParameterizedCallbackProcessor), callbackParms, TEST_NOCACHE_KEY);
            //assert
            cacheMock.Verify(v => v.Add(TEST_NOCACHE_KEY, result, INFINITE_SECONDS_TO_CACHE), Times.Once());
            cacheMock.Verify(v => v.GetData(TEST_NOCACHE_KEY), Times.Exactly(2));
            Assert.AreEqual(NONCACHE_RETURNED_VALUE, result);

        }

        [TestMethod]
        public void CacheDataLoader_GetData_MissCacheBasicCallback_LimitedTimeCacheAddTest()
        {
            //act
            object result =
                cacheDataLoader.GetData(new BasicCallback(BasicCallbackProcessor), null, TEST_NOCACHE_KEY, false, LIMITED_SCONDS_TO_CACHE);
            
            //assert
            cacheMock.Verify(v => v.Add(TEST_NOCACHE_KEY, result, LIMITED_SCONDS_TO_CACHE), Times.Once());
            cacheMock.Verify(v => v.GetData(TEST_NOCACHE_KEY), Times.Exactly(2));
            Assert.AreEqual(NONCACHE_RETURNED_VALUE, result);
        }

        [TestMethod]
        public void CacheDataLoader_GetData_BasicDefaultExpiry_NominalTest()
        {
            //act
            object result =
                cacheDataLoader.GetData(new BasicCallback(BasicCallbackProcessor), null, TEST_NOCACHE_KEY, false);

            //assert
            cacheMock.Verify(v => v.Add(TEST_NOCACHE_KEY, result, INFINITE_SECONDS_TO_CACHE), Times.Once());
            cacheMock.Verify(v => v.GetData(TEST_NOCACHE_KEY), Times.Exactly(2));
            Assert.AreEqual(NONCACHE_RETURNED_VALUE, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CacheDataLoader_GetData_NullCallbackThrows_Test()
        {
            //act
            cacheDataLoader.GetData(null, null, TEST_NOCACHE_KEY, false, INFINITE_SECONDS_TO_CACHE);

            //assert
            //nothing to assert, throws exception
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CacheDataLoader_GetData_NullKeyThrows_Test()
        {
            //act
            cacheDataLoader.GetData(new BasicCallback(BasicCallbackProcessor), null, (string)null);

            //assert
            //nothing to assert, throws exception
        }
                

        /******************************/



        //NoCacheResults Tests

        [TestMethod]
        public void CacheDataLoader_GetDataNoCache_Basic_NominalTest()
        {
            //act
            object result = 
                cacheDataLoader.GetDataNoCacheResult(new BasicCallback(BasicCallbackProcessor), null, TEST_KEY);

            //assert
            cacheMock.Verify(v => v.Add(TEST_KEY, It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cacheMock.Verify(v => v.GetData(TEST_KEY), Times.Once());
            Assert.AreEqual(RETURNED_VALUE, result);
        }
        
        [TestMethod]
        public void CacheDataLoader_GetDataNoCache_BasicClone_NominalTest()
        {
            //act
            object result =
                cacheDataLoader.GetDataNoCacheResult(new BasicCallback(BasicCallbackProcessor), null, TEST_KEY, true);

            //assert
            cacheMock.Verify(v => v.Add(TEST_KEY, It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            Assert.AreEqual(RETURNED_VALUE, result);
        }


        //Collection result tests

        [TestMethod]
        public void CacheDataLoader_GetDataCollection_CacheHitClone_NominalTest()
        {
            //act
            ICollection result =
                cacheDataLoader.GetData(new CollectionCallback(CollectionCallbackProcessor), KEY_PREFIX, cacheKeyDictionary);

            //assert 
            foreach(string key in cacheKeyDictionary.Keys)
            {
                cacheMock.Verify(v => v.GetData(key), Times.Once());
                
            }

            foreach (object item in result)
            {
                Assert.IsTrue(dictionaryResults.Contains(item as string));
            }
        }

        [TestMethod]
        public void CacheDataLoader_GetDataCollection_CacheHitNoClone_Test()
        {
            //act
            ICollection result =
                cacheDataLoader.GetData(new CollectionCallback(CollectionCallbackProcessor), KEY_PREFIX, cacheKeyDictionary, EXTRA_PARM_VALUE, false);

            //assert 
            foreach (string key in cacheKeyDictionary.Keys)
            {
                cacheMock.Verify(v => v.GetData(key), Times.Once());

            }

            foreach (object item in result)
            {
                Assert.IsTrue(dictionaryResults.Contains(item as string));
            }
        }

        [TestMethod]
        public void CacheDataLoader_GetDataCollection_MissedCache_NominalTest()
        {
            //act
            ICollection result =
                cacheDataLoader.GetData(new CollectionCallback(CollectionCallbackProcessor), KEY_PREFIX, cacheMissKeyDictionary, true);

            foreach (string key in cacheMissKeyDictionary.Keys)
            {
                string cacheaddkey = KEY_PREFIX + "_" + cacheMissKeyDictionary[key].ToString();
                cacheMock.Verify(v => v.GetData(key), Times.Never());
                cacheMock.Verify(v => v.Contains(key), Times.Once());
                cacheMock.Verify(v => v.Add(cacheaddkey, It.IsAny<object>(), INFINITE_SECONDS_TO_CACHE));
            }

            foreach (TestDto item in result)
            {
                Assert.AreEqual(DICTIONARY_NONCACHE_VALUE, item.Value);
            }
        }

        [TestMethod]
        public void CacheDataLoader_GetDataNoCacheCollection_Basic_NominalTest()
        {
            //act
            ICollection result =
                cacheDataLoader.GetDataNoCacheResult(new CollectionCallback(CollectionCallbackProcessor), KEY_PREFIX, cacheMissKeyDictionary);


            //assert
            foreach (string key in cacheMissKeyDictionary.Keys)
            {
                string cacheaddkey = KEY_PREFIX + "_" + cacheMissKeyDictionary[key].ToString();
                cacheMock.Verify(v => v.GetData(key), Times.Never());
                cacheMock.Verify(v => v.Contains(key), Times.Once());
                cacheMock.Verify(v => v.Add(cacheaddkey, It.IsAny<object>(), INFINITE_SECONDS_TO_CACHE), Times.Never());
            }

            foreach (TestDto item in result)
            {
                Assert.AreEqual(DICTIONARY_NONCACHE_VALUE, item.Value);
            }
        }

        [TestMethod]
        public void CacheDataLoader_GetDataNoCacheCollection_BasicClone_NominalTest()
        {
            //act
            ICollection result =
                cacheDataLoader.GetDataNoCacheResult(new CollectionCallback(CollectionCallbackProcessor), KEY_PREFIX, cacheMissKeyDictionary, true);

            //assert
            foreach (string key in cacheMissKeyDictionary.Keys)
            {
                string cacheaddkey = KEY_PREFIX + "_" + cacheMissKeyDictionary[key].ToString();
                cacheMock.Verify(v => v.GetData(key), Times.Never());
                cacheMock.Verify(v => v.Contains(key), Times.Once());
                cacheMock.Verify(v => v.Add(cacheaddkey, It.IsAny<object>(), INFINITE_SECONDS_TO_CACHE), Times.Never());
            }

            foreach (TestDto item in result)
            {
                Assert.AreEqual(DICTIONARY_NONCACHE_VALUE, item.Value);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CacheDataLoader_GetDataCollection_NullCallbackThrows_Test()
        {
            //act
            cacheDataLoader.GetData(null, KEY_PREFIX, cacheMissKeyDictionary, true);

            //assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CacheDataLoader_GetDataCollection_NullKeysThrows_Test()
        {
            //act
            cacheDataLoader.GetData(new CollectionCallback(CollectionCallbackProcessor), KEY_PREFIX, null);

            //assert
        }

        [TestMethod]
        public void CacheDataLoader_GetDataCollection_NoCacheHitNoClone_Test()
        {
            //act  
            ICollection result =
                cacheDataLoader.GetData(new CollectionWithParmCallback(CollectionWithParmCallbackProcessor), KEY_PREFIX, cacheMissKeyDictionary, EXTRA_PARM_VALUE, false);

            //assert
            foreach (string key in cacheMissKeyDictionary.Keys)
            {
                string cacheaddkey = KEY_PREFIX + "_" + cacheMissKeyDictionary[key].ToString();
                cacheMock.Verify(v => v.GetData(key), Times.Never());
                cacheMock.Verify(v => v.Contains(key), Times.Once());
                cacheMock.Verify(v => v.Add(cacheaddkey, It.IsAny<object>(), INFINITE_SECONDS_TO_CACHE), Times.Once());
            }

            foreach (TestDto item in result)
            {
                Assert.AreEqual(DICTIONARY_NONCACHE_VALUE, item.Value);
            }
        }

        [TestMethod]
        public void CacheDataLoader_GetDataCollection_NoCacheHitClone_Test()
        {
            //act  
            ICollection result =
                cacheDataLoader.GetData(new CollectionWithParmCallback(CollectionWithParmCallbackProcessor), KEY_PREFIX, cacheMissKeyDictionary, EXTRA_PARM_VALUE, true);

            //assert
            foreach (string key in cacheMissKeyDictionary.Keys)
            {
                string cacheaddkey = KEY_PREFIX + "_" + cacheMissKeyDictionary[key].ToString();
                cacheMock.Verify(v => v.GetData(key), Times.Never());
                cacheMock.Verify(v => v.Contains(key), Times.Once());
                cacheMock.Verify(v => v.Add(cacheaddkey, It.IsAny<object>(), INFINITE_SECONDS_TO_CACHE), Times.Once());
            }

            foreach (TestDto item in result)
            {
                Assert.AreEqual(DICTIONARY_NONCACHE_VALUE, item.Value);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CacheDataLoader_GetDataCollection_NoCacheExtraParmNullCallbackThrows_Test()
        {
            cacheDataLoader.GetData(null, KEY_PREFIX, cacheMissKeyDictionary, EXTRA_PARM_VALUE, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CacheDataLoader_GetDataCollection_NoCacheExtraParmNullKeyDicThrows_Test()
        {
            cacheDataLoader.GetData(new CollectionWithParmCallback(CollectionWithParmCallbackProcessor), KEY_PREFIX, null, EXTRA_PARM_VALUE, true);
        }

        [TestMethod]
        public void CacheDataLoader_Remove_NominalTest()
        {
            //act
            cacheDataLoader.Remove(TEST_KEY);

            //assert
            cacheMock.Verify(v => v.Remove(TEST_KEY), Times.Once());
        }
    }

    [Serializable]
    class TestDto : ICachableDTO
    {
        public string Value { get; set; }

        private int id = 0;

        public TestDto(string val, int inID)
        {
            Value = val;
            id = inID;
        }


        public int GetPrimaryKeyID()
        {
            return id;
        }

    }
}

