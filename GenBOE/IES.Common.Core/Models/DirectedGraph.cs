// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace IES.Common.Core.Models
{
	/// <summary>
	/// Directed graph algorithms for dependency resolution.
	/// </summary>
	/// <typeparam name="T">Type of object</typeparam>
	public class DirectedGraph<T>
	{
		/// <summary>
		/// Adjacency lists
		/// </summary>
		IDictionary<T, List<T>> adjacencyLists;

		/// <summary>
		/// Table to track which vertices were visited
		/// </summary>
		IDictionary<T, bool> visited;

		/// <summary>
		/// Constructor
		/// </summary>
		public DirectedGraph()
		{
			adjacencyLists = new Dictionary<T, List<T>>();
			visited = new Dictionary<T, bool>();
		}

		/// <summary>
		/// Add an edge to the graph.
		/// </summary>
		/// <param name="v">Starting vertex</param>
		/// <param name="w">Ending vertex</param>
		public void AddEdge(T v, T w)
		{
			if (!adjacencyLists.ContainsKey(v))
			{
				adjacencyLists[v] = new List<T>();
			}

			if (!adjacencyLists.ContainsKey(w))
			{
				adjacencyLists[w] = new List<T>();
			}

			if (!visited.ContainsKey(v))
			{
				visited[v] = false;
			}

			if (!visited.ContainsKey(w))
			{
				visited[w] = false;
			}

			adjacencyLists[v].Add(w);
		}

		/// <summary>
		/// Perform a topological sort of the graph.
		/// </summary>
		/// <returns>A list of vertices, in the order in which they should be updated/evaluated.</returns>
		public IList<T> TopologicalSort()
		{
			IList<T> results = new List<T>();

			Stack<T> stack = new Stack<T>();

			ICollection<T> vertices = new List<T>();
			foreach (T key in visited.Keys)
			{
				vertices.Add(key);
			}

			foreach (T vtx in vertices)
			{
				if (!visited[vtx])
				{
					DoTopologicalSort(vtx, visited, stack);
				}
			}

			while (stack.Count > 0)
			{
				results.Add(stack.Pop());
			}

			return results;
		}

		/// <summary>
		/// Recursive worker method.
		/// </summary>
		/// <param name="v">Vertex being processed</param>
		/// <param name="visits">Table to track which vertices were visited</param>
		/// <param name="stack">Stack to capture ordered results</param>
		private void DoTopologicalSort(T v, IDictionary<T, bool> visits, Stack<T> stack)
		{
			visits[v] = true;

			foreach (T vtx in adjacencyLists[v])
			{
				if (!visits[vtx])
				{
					DoTopologicalSort(vtx, visits, stack);
				}
			}

			stack.Push(v);
		}

	}
}
