// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Standard.OfficeUtilities
{
    /// <summary>
    /// Class to help w/ counters when inserting altChunks, to make sure they are unique
    /// </summary>
    public class ChunkCounter
    {
        /// <summary>
        /// Private counter
        /// </summary>
        private int altChunkCounter;

        /// <summary>
        /// Self incrementing counter
        /// </summary>
        public int AltChunkCounter
        {
            get
            {
                return this.altChunkCounter++;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ChunkCounter()
        {
            this.altChunkCounter = 1;
        }
    }
}
