using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IES.Common;
using System.Threading;


namespace GenBOE.Tests.DAL.Other
{
    [TestClass]
    public class MemoryCacheTest
    {
        private string _key = "hi";
        private MemoryCache _sut = new MemoryCache();

        [TestCleanup]
        public void Cleanup()
        {
            _sut.Remove(_key);
        }


        [TestMethod]
        public void GetAddTestInfinite()
        {
            Assert.IsNull(_sut.GetData(_key));
            _sut.Add(_key, new object(), -1);
            Assert.IsNotNull(_sut.GetData(_key));
            Thread.Sleep(5 * 1000); // sleep for 5 seconds, cache should expire
            Assert.IsNotNull(_sut.GetData(_key));
        }

        [TestMethod]
        public void GetAddTestSliding()
        {
            Assert.IsNull(_sut.GetData(_key));
            _sut.Add(_key, new object(), 3);
            Assert.IsNotNull(_sut.GetData(_key));
            Thread.Sleep(5 * 1000); // sleep for 5 seconds, cache should expire
            Assert.IsNull(_sut.GetData(_key));
        }

        [TestMethod]
        public void ClearCache()
        {
            _sut.Add(_key, new object(), -1);
            Assert.IsNotNull(_sut.GetData(_key));
            _sut.ClearCache();
            Assert.IsNull(_sut.GetData(_key));
        }
        
        [TestMethod]
        public void GetAddTestAbsolute()
        {
            Assert.IsNull(this._sut.GetData(this._key));
            this._sut.AddAbsolute(this._key, new object(), 3);
            Assert.IsNotNull(this._sut.GetData(this._key));
            Thread.Sleep(5 * 1000); // sleep for 5 seconds, cache should expire
            Assert.IsNull(this._sut.GetData(this._key));
        }
        
        [TestMethod]
        public void Remove()
        {
            this.GetAddTestInfinite();
            _sut.Remove(_key);
            Assert.IsNull(_sut.GetData(_key));
        }

        [TestMethod]
        public void Contains()
        {
            this.GetAddTestInfinite();
            Assert.IsTrue(_sut.Contains(_key));
            Assert.IsFalse(_sut.Contains("key_should_not_be_found"));
        }

        /// <summary>
        /// Tests adding a null value
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNullValueTest()
        {
            this._sut.Add(this._key, null, -1);
        }

        /// <summary>
        /// Tests adding a null value
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddAbsoluteNullValueTest()
        {
            this._sut.AddAbsolute(this._key, null, -1);
        }

        /// <summary>
        /// Tests adding a null key
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddAbsoluteNullKeyTest()
        {
            this._sut.AddAbsolute(null, new object(), -1);
        }
    }

    
}
