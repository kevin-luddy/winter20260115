using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using IES.Common;
using IES.Common.Interfaces;
using System.Collections;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class CacheDataLoaderTest
    {
        delegate object FakeDelegate();
        delegate object FakeDictionaryDelegate(ICollection<int> dictionary);
        Mock<ICache> cache = null;
        delegate object FakeDictionaryWithExtraParamDelegate(ICollection<int> dictionary, int inExtraParm);

        /// <summary>
        /// Delegate that will ASSERT if called
        /// </summary>
        /// <returns></returns>
        object FakeDelegateASSERTImpl()
        {
            Assert.Fail("Should not have called this delegate for a cache hit");
            return new object();
        }

        /// <summary>
        /// Delegate that will return an object if called
        /// </summary>
        /// <returns></returns>
        object FakeDelegateImpl()
        {
            return new object();
        }

        ICollection<ICachableDTO> FakeDelegateDictionaryImpl(ICollection<int> inPrimaryKeys)
        {
            List<ICachableDTO> toReturn = new List<ICachableDTO>();
            foreach (int key in inPrimaryKeys)
            {
                toReturn.Add(new FakeObject() { PrimaryKey = key });
            }

            return toReturn;
        }

        ICollection<ICachableDTO> FakeDelegateDictionaryWithExtraParamImpl(ICollection<int> inPrimaryKeys, int inExtraParm)
        {
            List<ICachableDTO> toReturn = new List<ICachableDTO>();
            foreach (int key in inPrimaryKeys)
            {
                toReturn.Add(new FakeObject() { PrimaryKey = key });
            }

            return toReturn;
        }


        private CacheDataLoader CreateSystem()
        {
            // need to handle a nonnull case            
            cache = new Mock<ICache>();

            return new CacheDataLoader(cache.Object, -1);
        }

        [TestMethod]
        public void CacheDataLoaderHit()
        {
            // invoke the core test passing in the delegate 
            // that asserts if it is called (a cache hit should never call the delegate
            // to load data in)

            // since CacheDataLoader is what we're testing, we need a real version of it
            var sut = CreateSystem();

            // need to mock every external call
            // need to setup this mocked cache instance because in CacheDataLoader
            // there is an external call to ICache Get 
            cache.Setup(y => y.GetData("hi")).Returns(new object());

            Object toReturn = sut.GetData(new FakeDelegate(FakeDelegateASSERTImpl), new Object[] { }, "hi");

            Assert.IsNotNull(toReturn);
            cache.Verify(x => x.Add("hi", It.IsAny<object>(), It.IsAny<int>()), Times.Never());

            toReturn = sut.GetData(new FakeDelegate(FakeDelegateASSERTImpl), new Object[] { }, "hi", false);

            Assert.IsNotNull(toReturn);
            cache.Verify(x => x.Add("hi", It.IsAny<object>(), It.IsAny<int>()), Times.Never());

            toReturn = sut.GetDataNoCacheResult(new FakeDelegate(FakeDelegateASSERTImpl), new Object[] { }, "hi", false);
            Assert.IsNotNull(toReturn);
            cache.Verify(x => x.Add("hi", It.IsAny<object>(), It.IsAny<int>()), Times.Never());

            toReturn = sut.GetDataNoCacheResult(new FakeDelegate(FakeDelegateASSERTImpl), new Object[] { }, "hi");
            Assert.IsNotNull(toReturn);
            cache.Verify(x => x.Add("hi", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void CacheDataLoaderMiss()
        {
            // invoke the core test passing in the delegate 
            // that returns an object if it is called (a cache miss should always call the delegate
            // to load data in)

            // since CacheDataLoader is what we're testing, we need a real version of it
            var sut = CreateSystem();

            // need to mock every external call
            // need to setup this mocked cache instance because in CacheDataLoader
            // there is an external call to ICache Get 
            cache.Setup(y => y.GetData("hi")).Returns(null);

            Object toReturn = sut.GetData(new FakeDelegate(FakeDelegateImpl), new Object[] { }, "hi");

            Assert.IsNotNull(toReturn);
            cache.Verify(x => x.Add("hi", It.IsAny<object>(), It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void CacheDataLoaderMissCheckCases()
        {
            // invoke the core test passing in the delegate 
            // that returns an object if it is called (a cache miss should always call the delegate
            // to load data in)

            // since CacheDataLoader is what we're testing, we need a real version of it
            var sut = CreateSystem();

            // need to mock every external call
            // need to setup this mocked cache instance because in CacheDataLoader
            // there is an external call to ICache Get 
            cache.Setup(y => y.GetData("HI")).Returns(null);

            Object toReturn = sut.GetData(new FakeDelegate(FakeDelegateImpl), new Object[] { }, "HI");

            Assert.IsNotNull(toReturn);
            cache.Verify(x => x.Add("hi", It.IsAny<object>(), It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void CacheDataLoaderRemove()
        {
            var sut = CreateSystem();
            sut.Remove("hi");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void CacheDataLoaderDictionaryGet()
        {
            var sut = CreateSystem();

            string cacheKeyPrefix = "PREFIX".ToLower();

            cache.Setup(y => y.Contains(cacheKeyPrefix + "_1")).Returns(true);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_2")).Returns(false);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_3")).Returns(true);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_4")).Returns(false);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_5")).Returns(true);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_6")).Returns(true);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_7")).Returns(false);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_8")).Returns(true);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_9")).Returns(false);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_10")).Returns(true);

            cache.Setup(y => y.GetData(cacheKeyPrefix + "_1")).Returns(new object());
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_2")).Returns(null);
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_3")).Returns(new object());
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_4")).Returns(null);
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_5")).Returns(new object());
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_6")).Returns(new object());
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_7")).Returns(null);
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_8")).Returns(new object());
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_9")).Returns(null);
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_10")).Returns(new object());

            Dictionary<string, int> cacheDictionary1 = new Dictionary<string, int>();
            cacheDictionary1.Add(cacheKeyPrefix + "_1", 1);
            cacheDictionary1.Add(cacheKeyPrefix + "_2", 2);
            cacheDictionary1.Add(cacheKeyPrefix + "_3", 3);
            cacheDictionary1.Add(cacheKeyPrefix + "_4", 4);
            cacheDictionary1.Add(cacheKeyPrefix + "_5", 5);

            Dictionary<string, int> cacheDictionary2 = new Dictionary<string, int>();
            cacheDictionary2.Add(cacheKeyPrefix + "_6", 6);
            cacheDictionary2.Add(cacheKeyPrefix + "_7", 7);
            cacheDictionary2.Add(cacheKeyPrefix + "_8", 8);
            cacheDictionary2.Add(cacheKeyPrefix + "_9", 9);
            cacheDictionary2.Add(cacheKeyPrefix + "_10", 10);

            Dictionary<string, int> cacheDictionary3 = new Dictionary<string, int>();
            cacheDictionary3.Add(cacheKeyPrefix + "_1", 1);
            cacheDictionary3.Add(cacheKeyPrefix + "_3", 3);
            cacheDictionary3.Add(cacheKeyPrefix + "_5", 5);
            cacheDictionary3.Add(cacheKeyPrefix + "_6", 6);
            cacheDictionary3.Add(cacheKeyPrefix + "_8", 8);
            cacheDictionary3.Add(cacheKeyPrefix + "_10", 10);

            ICollection toReturn = sut.GetData(new FakeDictionaryDelegate(FakeDelegateDictionaryImpl), cacheKeyPrefix, cacheDictionary1);
            Assert.AreEqual(5, toReturn.Count);

            cache.Verify(x => x.Add(cacheKeyPrefix + "_1", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_2", It.IsAny<object>(), It.IsAny<int>()), Times.Once());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_3", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_4", It.IsAny<object>(), It.IsAny<int>()), Times.Once());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_5", It.IsAny<object>(), It.IsAny<int>()), Times.Never());

            toReturn = sut.GetDataNoCacheResult(new FakeDictionaryDelegate(FakeDelegateDictionaryImpl), cacheKeyPrefix, cacheDictionary2);
            Assert.AreEqual(5, toReturn.Count);

            cache.Verify(x => x.Add(cacheKeyPrefix + "_6", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_7", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_8", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_9", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_10", It.IsAny<object>(), It.IsAny<int>()), Times.Never());

            toReturn = sut.GetDataNoCacheResult(new FakeDictionaryDelegate(FakeDelegateDictionaryImpl), cacheKeyPrefix, cacheDictionary2, false);
            Assert.AreEqual(5, toReturn.Count);

            cache.Verify(x => x.Add(cacheKeyPrefix + "_6", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_7", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_8", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_9", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_10", It.IsAny<object>(), It.IsAny<int>()), Times.Never());

            toReturn = sut.GetData(new FakeDictionaryDelegate(FakeDelegateDictionaryImpl), cacheKeyPrefix, cacheDictionary3, true);
            Assert.AreEqual(6, toReturn.Count);

            cache.Verify(x => x.Add(cacheKeyPrefix + "_1", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_3", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_5", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_6", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_8", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
            cache.Verify(x => x.Add(cacheKeyPrefix + "_10", It.IsAny<object>(), It.IsAny<int>()), Times.Never());
        }

        #region Exception Test

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CacheDataLoader_Exception1()
        {
            var sut = CreateSystem();
            sut.GetData(null, new Object[] { }, "hi");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CacheDataLoader_Exception2()
        {
            var sut = CreateSystem();
            sut.GetData(null, "", new Dictionary<string, int>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CacheDataLoader_Exception3()
        {
            var sut = CreateSystem();
            sut.GetData(new FakeDictionaryDelegate(FakeDelegateDictionaryImpl), "", null);
        }

        #endregion Exception Test

        [TestMethod]
        public void CacheDataLoaderHit_ResourceCache()
        {

            // resource list ID will be used as the extra parm
            int ResourceListID = 1;

            var sut = CreateSystem();

            string cacheKeyPrefix = "PREFIX".ToLower();
            int ID = 5;

            //not in cache so we are going to add it
            cache.Setup(y => y.Contains(cacheKeyPrefix+"_"+ID)).Returns(false);
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_" + ID)).Returns(new object());

            Dictionary<string, int> cacheDict = new Dictionary<string, int>();

            cacheDict.Add(cacheKeyPrefix+"_"+ID, ID);

            Object toReturn = sut.GetData(new FakeDictionaryWithExtraParamDelegate(FakeDelegateDictionaryWithExtraParamImpl), cacheKeyPrefix, cacheDict, ResourceListID, false);

            Assert.IsNotNull(toReturn);

            cache.Verify(x => x.Add(cacheKeyPrefix+"_"+ID, It.IsAny<object>(), It.IsAny<int>()), Times.Once());


            // in Cache
            ID = 6;
            cacheDict = new Dictionary<string, int>();

            cacheDict.Add(cacheKeyPrefix + "_" + ID, ID);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_" + ID)).Returns(true);
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_" + ID)).Returns(null);

            toReturn = sut.GetData(new FakeDictionaryWithExtraParamDelegate(FakeDelegateDictionaryWithExtraParamImpl), cacheKeyPrefix, cacheDict, ResourceListID, true);

            Assert.IsNotNull(toReturn);

            cache.Verify(x => x.Add(cacheKeyPrefix + "_" + ID, It.IsAny<object>(), It.IsAny<int>()), Times.Never());

            // in Cache and cloned
            ID = 7;
            cacheDict = new Dictionary<string, int>();

            cacheDict.Add(cacheKeyPrefix + "_" + ID, ID);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_" + ID)).Returns(true);
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_" + ID)).Returns(new object());

            toReturn = sut.GetData(new FakeDictionaryWithExtraParamDelegate(FakeDelegateDictionaryWithExtraParamImpl), cacheKeyPrefix, cacheDict, ResourceListID, true);

            Assert.IsNotNull(toReturn);

            cache.Verify(x => x.Add(cacheKeyPrefix + "_" + ID, It.IsAny<object>(), It.IsAny<int>()), Times.Never());

            // in Cache but not cloned
            ID = 8;
            cacheDict = new Dictionary<string, int>();

            cacheDict.Add(cacheKeyPrefix + "_" + ID, ID);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_" + ID)).Returns(true);
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_" + ID)).Returns(new object());

            toReturn = sut.GetData(new FakeDictionaryWithExtraParamDelegate(FakeDelegateDictionaryWithExtraParamImpl), cacheKeyPrefix, cacheDict, ResourceListID, false);

            Assert.IsNotNull(toReturn);

            cache.Verify(x => x.Add(cacheKeyPrefix + "_" + ID, It.IsAny<object>(), It.IsAny<int>()), Times.Never());

            //not in cache and cloned
            ID = 9;
            cacheDict = new Dictionary<string, int>();

            cacheDict.Add(cacheKeyPrefix + "_" + ID, ID);
            cache.Setup(y => y.Contains(cacheKeyPrefix + "_" + ID)).Returns(false);
            cache.Setup(y => y.GetData(cacheKeyPrefix + "_" + ID)).Returns(new object());

            toReturn = sut.GetData(new FakeDictionaryWithExtraParamDelegate(FakeDelegateDictionaryWithExtraParamImpl), cacheKeyPrefix, cacheDict, ResourceListID, true);

            Assert.IsNotNull(toReturn);

            cache.Verify(x => x.Add(cacheKeyPrefix + "_" + ID, It.IsAny<object>(), It.IsAny<int>()), Times.Once());


        }


        [Serializable()]
        class FakeObject : ICachableDTO
        {
            public int PrimaryKey { get; set; }

            public int GetPrimaryKeyID()
            {
                return PrimaryKey;
            }
        }
    }
}
