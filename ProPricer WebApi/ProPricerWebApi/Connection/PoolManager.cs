/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Connection
{
	using System.Collections;

	/// <summary>
	/// A class to manage objects in a pool. 
	///The class is sealed to prevent further inheritance
	/// and is based on the Singleton Design.
	/// </summary>
	public sealed class PoolManager
	{
		/// <summary>
		/// Queue for this Pool
		/// </summary>
		private readonly Queue poolQueue = new();

		/// <summary>
		/// Hashtable for the Pool objects
		/// </summary>
		private readonly Hashtable objPool = new();

		/// <summary>
		/// Maximum Pool size
		/// </summary>
		internal readonly int poolSize;

		/// <summary>
		/// Count of current objects in pool
		/// </summary>
		private int objCount;

		/// <summary>
		/// Whether the objects are in use
		/// </summary>
		private readonly bool[] objInUse;

		/// <summary>
		/// lock object
		/// </summary>
		private readonly object ObjLock = new();

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		public int InstanceId { get; }

		/// <summary>
		/// Gets the name of the friendly.
		/// </summary>
		public string FriendlyName { get; }

		/// <summary>
		/// Public constructor to prevent instantiation
		/// </summary>
		/// <param name="instanceId">The identifier.</param>
		/// <param name="friendlyName">The friendly Name for the connection.</param>
		/// <param name="poolSize">Size of the pool.</param>
		public PoolManager(int instanceId, string friendlyName, int poolSize)
		{
			this.InstanceId = instanceId;
			this.FriendlyName = friendlyName;
			this.poolSize = poolSize;
			this.objInUse = new bool[this.poolSize];
		}

		/// <summary>
		/// Adds an object to the pool
		/// </summary>
		/// <param name="obj">Object to be added</param>
		/// <returns>True if success, false otherwise</returns>
		public bool AddObject(object obj)
		{
			if (this.objCount == this.poolSize)
			{
				return false;
			}

			lock (ObjLock)
			{
				this.objPool.Add(obj.GetHashCode(), obj);
				this.poolQueue.Enqueue(obj);
				this.objInUse[this.objCount] = false;
				this.objCount++;
			}

			return true;
		}

		/// <summary>
		/// Releases an object from the pool (used for testing only)
		/// </summary>
		/// <param name="obj">Object to remove from the pool</param>
		/// <returns>The object if success, null otherwise</returns>
		public object ReleaseObject(object obj)
		{
			// not planning on releasing objects unless app stops 
			// so not going to worry about objInUse flag
			if (this.objCount == 0)
			{
				return null;
			}

			lock (ObjLock)
			{
				this.objPool.Remove(obj.GetHashCode());
				this.objCount--;
				this.RePopulate();
				return obj;
			}
		}

		/// <summary>
		/// Method that repopulates the 
		/// Queue after an object has been removed from the pool.
		/// This is done to make the queue 
		/// objects in sync with the objects in the hash table.
		/// </summary>
		private void RePopulate()
		{
			if (this.poolQueue.Count > 0)
			{
				this.poolQueue.Clear();
			}

			foreach (int key in this.objPool.Keys)
			{
				this.poolQueue.Enqueue(this.objPool[key]);
			}
		}

		/// <summary>
		/// Property that represents the current no of objects in the pool
		/// </summary>
		public int CurrentObjectsInPool
		{
			get
			{
				return this.objCount;
			}
		}

		/// <summary>
		/// Property that represents the maximum no of objects in the pool
		/// </summary>
		public int MaxObjectsInPool
		{
			get
			{
				return this.poolSize;
			}
		}

		/// <summary>
		/// get next available object
		/// </summary>
		public object GetObjectsFromPool()
		{
			object obj = null;
			int j = 0;
			foreach (int key in this.objPool.Keys)
			{
				if (!this.objInUse[j])
				{
					obj = this.objPool[key];
					this.objInUse[j] = true;
					break;
				}

				j++;
			}

			return obj;
		}

		/// <summary>
		/// get next available object
		/// </summary>
		public void GiveObjectBackToPool(object obj)
		{
			int j = 0;
			foreach (int key in this.objPool.Keys)
			{
				if (obj == this.objPool[key])
				{
					this.objInUse[j] = false;
					break;
				}

				j++;
			}
		}

		/// <summary>
		/// Determines whether the indexed number in the pool is in use.
		/// </summary>
		/// <param name="num">The indexed number.</param>
		/// <returns>
		///   <c>true</c> if the indexed number in the pool is in use; otherwise, <c>false</c>.
		/// </returns>
		public bool IsPoolInUse(int num)
		{
			return this.objInUse[num];
		}
	}
}