// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WBS
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.Dtos;

    public class NestedWBSUtilities : INestedWBSUtilities
    {
        // Constructor

        public void AdjustLevels(Collection<WbsDTO> WbsDTOs)
        {
            if (WbsDTOs == null)
            {
                throw new ArgumentNullException(nameof(WbsDTOs));
            }

            // start at 100 since that level is impossible to obtain.
            int level = 100;

            // get the lowest level.
            foreach (WbsDTO wbs in WbsDTOs)
            {
                if (wbs.Level < level)
                {
                    level = wbs.Level;
                }
            }

            // make adjustment
            foreach (WbsDTO wbs in WbsDTOs)
            {
                wbs.Level = wbs.Level - level;
            }
        }

        public void SetParentsAndChildrenInUse(Collection<WbsDTO> WbsDTOs)
        {
            if (WbsDTOs == null)
            {
                throw new ArgumentNullException(nameof(WbsDTOs));
            }

            Collection<WbsDTO> inUseDTOs = this.GetInUseDTOs(WbsDTOs);
            this.SetParentsInUse(WbsDTOs, inUseDTOs);
            this.SetChildrenInUse(WbsDTOs, inUseDTOs);
        }

        public void SetChildrenInUse(Collection<WbsDTO> WbsDTOs)
        {
            if (WbsDTOs == null)
            {
                throw new ArgumentNullException(nameof(WbsDTOs));
            }

            Collection<WbsDTO> inUseDTOs = this.GetInUseDTOs(WbsDTOs);
            this.SetChildrenInUse(WbsDTOs, inUseDTOs);
        }

        public Collection<string> GetParentsWBSNumByChildWBS(string inWBSNumber)
        {
            WbsDTO tempDTO = new WbsDTO();
            tempDTO.WbsNumber = inWBSNumber;

            return this.GetParentsWBSNumByChildWBS(tempDTO);
        }

        public Collection<string> GetParentsWBSNumByChildWBS(WbsDTO inWBS)
        {
            if (inWBS == null)
            {
                throw new ArgumentNullException(nameof(inWBS));
            }

            Collection<string> parents = new Collection<string>();
            string parentToCheck = inWBS.WbsNumber;
            while (parentToCheck.Contains("."))
            {
                parentToCheck = parentToCheck.Substring(0, parentToCheck.LastIndexOf('.'));
                parents.Add(parentToCheck);
            }

            return parents;
        }

        private void SetParentsInUse(Collection<WbsDTO> WbsDTOs, Collection<WbsDTO> inUseWbsDTOs)
        {
            foreach (WbsDTO wbs in inUseWbsDTOs)
            {
                Collection<string> parents = this.GetParentsWBSNumByChildWBS(wbs);

                foreach (string parent in parents)
                {
                    WbsDTO toChange = (from w in WbsDTOs
                                       where w.WbsNumber == parent
                                       select w).FirstOrDefault();
                    if (toChange != null)
                    {
                        toChange.inUse = true;
                    }
                }
            }
        }

        private void SetChildrenInUse(Collection<WbsDTO> WbsDTOs, Collection<WbsDTO> inUseWbsDTOs)
        {
            foreach (WbsDTO wbs in inUseWbsDTOs)
            {
                foreach (WbsDTO wbsToCheck in WbsDTOs)
                {
                    if (wbsToCheck.WbsNumber.StartsWith(wbs.WbsNumber + ".", StringComparison.CurrentCultureIgnoreCase))
                    {
                        wbsToCheck.inUse = true;
                    }
                }
            }
        }

        private Collection<WbsDTO> GetInUseDTOs(Collection<WbsDTO> WbsDTOs)
        {
            Collection<WbsDTO> inUseWbsDTOs = new Collection<WbsDTO>((from w in WbsDTOs
                                                                      where w.inUse == true
                                                                      select w).ToArray());
            return inUseWbsDTOs;
        }

    }
}
