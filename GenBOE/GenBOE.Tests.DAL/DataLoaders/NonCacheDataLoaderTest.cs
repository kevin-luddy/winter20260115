using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IES.Common;
using IES.Common.Interfaces;
using System.Collections;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class NonCacheDataLoaderTest
    {
        delegate object FakeDelegate();
        delegate object FakeDictionaryDelegate(ICollection<int> dictionary);
        delegate object FakeDictionaryWithExtraParamDelegate(ICollection<int> dictionary, int inExtraParm);

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

        private NonCacheDataLoader CreateSystem()
        {
            return new NonCacheDataLoader();
        }

        [TestMethod]
        public void NonCacheDataLoaderMiss()
        {
            // invoke the core test passing in the delegate 
            // that returns an object if it is called (a cache miss should always call the delegate
            // to load data in)

            // since NonCacheDataLoader is what we're testing, we need a real version of it
            var sut = CreateSystem();
            Object toReturn = sut.GetData(new FakeDelegate(FakeDelegateImpl), new Object[] { }, "hi");

            Assert.IsNotNull(toReturn);

            toReturn = sut.GetDataNoCacheResult(new FakeDelegate(FakeDelegateImpl), new Object[] { }, "hi");

            Assert.IsNotNull(toReturn);
        }

        [TestMethod]
        public void NonCacheDataLoaderRemove()
        {
            var sut = CreateSystem();
            sut.Remove("hi");  // this function does nothing
        }

        [TestMethod]
        public void NonCacheDataLoaderDictionaryGet()
        {
            var sut = CreateSystem();

            string cacheKeyPrefix = "PREFIX";

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

            toReturn = sut.GetDataNoCacheResult(new FakeDictionaryDelegate(FakeDelegateDictionaryImpl), cacheKeyPrefix, cacheDictionary2);
            Assert.AreEqual(5, toReturn.Count);

            toReturn = sut.GetDataNoCacheResult(new FakeDictionaryDelegate(FakeDelegateDictionaryImpl), cacheKeyPrefix, cacheDictionary2, false);
            Assert.AreEqual(5, toReturn.Count);

            toReturn = sut.GetData(new FakeDictionaryDelegate(FakeDelegateDictionaryImpl), cacheKeyPrefix, cacheDictionary3, true);
            Assert.AreEqual(6, toReturn.Count);
        }

        #region Exception Test

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonCacheDataLoader_Expection1()
        {
            var sut = CreateSystem();
            sut.GetData(null, new Object[] { }, "hi");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonCacheDataLoader_Exception2()
        {
            var sut = CreateSystem();
            sut.GetData(null, "", new Dictionary<string, int>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonCacheDataLoader_Exception3()
        {
            var sut = CreateSystem();
            sut.GetData(new FakeDictionaryDelegate(FakeDelegateDictionaryImpl), "", null);
        }

        #endregion Exception Test

        [TestMethod]
        public void NonCacheDataLoaderHit_ResourceCache()
        {

            // resource list ID will be used as the extra parm
            int ResourceListID = 1;

            var sut = CreateSystem();

            string cacheKeyPrefix = "PREFIX".ToLower();


            // not clone
            Dictionary<string, int> cacheDict = new Dictionary<string, int>();

            cacheDict.Add(cacheKeyPrefix + "_" + 5, 5);
            cacheDict.Add(cacheKeyPrefix + "_" + 56, 56);
            cacheDict.Add(cacheKeyPrefix + "_" + 99, 99);

            ICollection toReturn = sut.GetData(new FakeDictionaryWithExtraParamDelegate(FakeDelegateDictionaryWithExtraParamImpl), cacheKeyPrefix, cacheDict, ResourceListID, false);

            Assert.IsNotNull(toReturn);
            Assert.AreEqual(3, toReturn.Count);

            // cloned
            
            cacheDict = new Dictionary<string, int>();

            cacheDict.Add(cacheKeyPrefix + "_" +65, 65);
            cacheDict.Add(cacheKeyPrefix + "_" + 656, 656);
            cacheDict.Add(cacheKeyPrefix + "_" + 199, 199);
         
            toReturn = sut.GetData(new FakeDictionaryWithExtraParamDelegate(FakeDelegateDictionaryWithExtraParamImpl), cacheKeyPrefix, cacheDict, ResourceListID, true);

            Assert.IsNotNull(toReturn);
            Assert.AreEqual(3, toReturn.Count);


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
