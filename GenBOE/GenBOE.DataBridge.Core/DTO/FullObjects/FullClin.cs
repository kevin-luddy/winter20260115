//// -----------------------------------------------------------------------
//// <copyright company="Lockheed Martin Corporation">
////     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
//// </copyright>
//// -----------------------------------------------------------------------

//namespace GenBOE.DataBridge.Core.DTO.FullObjects
//{
//	using System;
//	using System.Collections.Generic;
//	using System.Collections.ObjectModel;
//	using System.Linq;
//	using System.Reflection;
//	using IES.Common.Core.Enums;
//	using IES.Common.Core.Interfaces;

//	[Serializable()]
//	public class FullClin : ClinDTO, IDateShiftable
//	{
//		private FullWorkspace workspace;
//		private ReadOnlyCollection<FullBoe> boes;
//		private ReadOnlyCollection<int> workspaceVariableIds;
//		private ReadOnlyCollection<int> boeTaskVariableIds;

//		/// <summary>
//		/// Boe Dto
//		/// </summary>
//		public IReadOnlyCollection<FullBoe> Boes
//		{
//			get
//			{
//				return boes;
//			}
//		}

//		/// <summary>
//		/// Workspace for the BOE
//		/// </summary>
//		public FullWorkspace Workspace
//		{
//			get
//			{
//				return workspace;
//			}
//		}

//		/// <summary>
//		/// Gets Workspace Variables associated w/ the Clin
//		/// </summary>
//		public IReadOnlyCollection<int> WorkspaceVariableIds
//		{
//			get
//			{
//				return workspaceVariableIds;
//			}
//		}

//		/// <summary>
//		/// Gets Boe Task Variables associated w/ the Clin
//		/// </summary>
//		public IReadOnlyCollection<int> BoeTaskVariableIds
//		{
//			get
//			{
//				return boeTaskVariableIds;
//			}
//		}

//		/// <summary>
//		/// Gets the children that can be shifted.
//		/// </summary>
//		public ICollection<IDateShiftable> Children
//		{
//			get
//			{
//				List<IDateShiftable> children = new List<IDateShiftable>(Boes);

//				return children;
//			}
//		}

//		/// <summary>
//		/// Gets a value indicating whether this instance has a spread of values.
//		/// </summary>
//		public bool HasSpread
//		{
//			get
//			{
//				return false;
//			}
//		}

//		/// <summary>
//		/// Gets the date shift level.
//		/// </summary>
//		public Level DateShiftLevel
//		{
//			get
//			{
//				return Level.CLIN;
//			}
//		}

//		/// <summary>
//		/// Filters the input collection to only those BOEs with the same clin ID.
//		/// </summary>
//		/// <param name="inBoes"></param>
//		public void SetBoes(ICollection<FullBoe> inBoes)
//		{
//			boes = inBoes.Where(x => x.CLINID == Id).ToList().AsReadOnly();
//		}
//	}
//}