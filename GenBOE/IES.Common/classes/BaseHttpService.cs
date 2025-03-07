// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2021 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Newtonsoft.Json;

	/// <summary>
	/// Helper class for wiring http services
	/// </summary>
	public class BaseHttpService : IDisposable
	{
		/// <summary>
		/// Base URL
		/// </summary>
		private readonly string baseUrl;

		/// <summary>
		/// Service Controller Name
		/// </summary>
		private readonly string serviceController;

		/// <summary>
		/// Has this been disposed
		/// </summary>
		private bool disposedValue;

		/// <summary>
		/// Logger
		/// </summary>
		protected Logger Logger { get; }

		/// <summary>
		/// The Http Client
		/// </summary>
		protected HttpClient HttpClient { get; } = new HttpClient();

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="serviceController">Service controller</param>
		/// <param name="httpClient">Http client</param>
		/// <param name="logger">Logger</param>
		public BaseHttpService(string serviceName, string serviceController, Logger logger)
		{
			if (string.IsNullOrEmpty(serviceName))
			{
				throw new ArgumentNullException(nameof(serviceName));
			}

			if (string.IsNullOrEmpty(serviceController))
			{
				throw new ArgumentNullException(nameof(serviceController));
			}

			this.serviceController = serviceController;
			this.Logger = logger;
			if (!this.serviceController.EndsWith("/", StringComparison.Ordinal))
			{
				this.serviceController += "/";
			}

			baseUrl = ConfigurationUtilities.GetAppSetting(serviceName);
			if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
			{
				baseUrl += "/";
			}

			this.HttpClient.Timeout = Constants.HTTP_TIMEOUT;
			
		}

		/// <summary>
		/// GET method
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <param name="url">Method URL</param>
		/// <returns>Result of method</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public async Task<IESSingleResponse<T>> Get<T>(string url)
		{
			HttpResponseMessage response = await HttpClient.GetAsync($"{baseUrl}{serviceController}{url}");

			if (!response.IsSuccessStatusCode)
			{
				Logger.Error(string.Format("{0} was not successful w/ code {1}", "Get", response.StatusCode));

				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_UNAUTHORIZED } };
				}

				return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_ERROR } };
			}

			return await response.Content.ReadAsAsync<IESSingleResponse<T>>(); // .ReadFromJsonAsync<IESSingleResponse<T>>();
		}

		/// <summary>
		/// synchronous GET method
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <param name="url">Method URL</param>
		/// <returns>Result of method</returns>
		public T GetSync<T>(string url)
		{
			// runing http Get synchronously taken from https://stackoverflow.com/questions/53529061/whats-the-right-way-to-use-httpclient-synchronously
			Task<HttpResponseMessage> getTask = Task.Run(() => HttpClient.GetAsync($"{baseUrl}{serviceController}{url}"));
			getTask.Wait();
			HttpResponseMessage response = getTask.Result;

			if (!response.IsSuccessStatusCode)
			{
				Logger.Error(string.Format("{0} was not successful w/ code {1}", "Get", response.StatusCode));
				return default;
			}

			Task<T> returnTask = response.Content.ReadAsAsync<T>(); // .ReadFromJsonAsync<T>();
			returnTask.Wait();
			return returnTask.Result;
		}

		/// <summary>
		/// DELETE method
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <param name="url">Method URL</param>
		/// <returns>Result of method</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public async Task<IESSingleResponse<T>> Delete<T>(string url)
		{
			HttpResponseMessage response = await HttpClient.DeleteAsync($"{baseUrl}{serviceController}{url}");

			if (!response.IsSuccessStatusCode)
			{
				Logger.Error(string.Format("{0} was not successful w/ code {1}", "Delete", response.StatusCode));
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_UNAUTHORIZED } };
				}

				return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_ERROR } };
			}

			return await response.Content.ReadAsAsync<IESSingleResponse<T>>(); // .ReadFromJsonAsync<IESSingleResponse<T>>();
		}

		/// <summary>
		/// POST method
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <typeparam name="TData">Generic data type</typeparam>
		/// <param name="url">Method URL</param>
		/// <param name="data">Generic parameter data</param>
		/// <returns>Result of method</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public async Task<IESSingleResponse<T>> Post<T, TData>(string url, TData data)
		{
			// Handles self referencing loop with Newtonsoft serializing options
			string serializedResult = JsonConvert.SerializeObject(data, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

			// Get an authetnicated HTTP client
			HttpResponseMessage response = await HttpClient.PostAsync($"{baseUrl}{serviceController}{url}", new StringContent(serializedResult, System.Text.Encoding.UTF8, "application/json"));

			if (!response.IsSuccessStatusCode)
			{
				Logger.Error(string.Format("{0} was not successful w/ code {1}", "Post", response.StatusCode));
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_UNAUTHORIZED } };
				}

				return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_ERROR } };
			}

			return await response.Content.ReadAsAsync<IESSingleResponse<T>>(); // .ReadFromJsonAsync<IESSingleResponse<T>>();
		}

		/// <summary>
		/// POST method for one complex object following the usage of form data content for deserializing complex objects (notably collections of objects containing nested collections, etc.).
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <typeparam name="TData1">Generic data type</typeparam>
		/// <param name="url">Method URL</param>
		/// <param name="dataItem1">Generic parameter data</param>
		/// <returns>Result of method</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public async Task<IESSingleResponse<T>> PostForm<T, TData1>(string url, TData1 dataItem1)
		{
			HttpResponseMessage response;

			using (MultipartFormDataContent formData = new MultipartFormDataContent())
			{
				//add content to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem1, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object1");

				// Get an authetnicated HTTP client
				response = await HttpClient.PostAsync($"{baseUrl}{serviceController}{url}", formData);
			}

			if (!response.IsSuccessStatusCode)
			{
				Logger.Error(string.Format("{0} was not successful w/ code {1}", "MultiPost", response.StatusCode));
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_UNAUTHORIZED } };
				}

				return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_ERROR } };
			}

			return await response.Content.ReadAsAsync<IESSingleResponse<T>>(); // .ReadFromJsonAsync<IESSingleResponse<T>>();
		}

		/// <summary>
		/// POST method for two complex objects
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <typeparam name="TData1">Generic data type</typeparam>
		/// <typeparam name="TData2">Generic data type</typeparam>
		/// <param name="url">Method URL</param>
		/// <param name="dataItem1">Generic parameter data</param>
		/// <param name="dataItem2">Generic parameter data</param>
		/// <returns>Result of method</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public async Task<IESSingleResponse<T>> MultiPost<T, TData1, TData2>(string url, TData1 dataItem1, TData2 dataItem2)
		{
			HttpResponseMessage response;

			using (MultipartFormDataContent formData = new MultipartFormDataContent())
			{
				//add content to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem1, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object1");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem2, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object2");

				// Get an authetnicated HTTP client
				response = await HttpClient.PostAsync($"{baseUrl}{serviceController}{url}", formData);
			}

			if (!response.IsSuccessStatusCode)
			{
				Logger.Error(string.Format("{0} was not successful w/ code {1}", "MultiPost", response.StatusCode));
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_UNAUTHORIZED } };
				}

				return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_ERROR } };
			}

			return await response.Content.ReadAsAsync<IESSingleResponse<T>>(); // .ReadFromJsonAsync<IESSingleResponse<T>>();
		}

		/// <summary>
		/// POST method for two complex objects
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <typeparam name="TData1">Generic data type</typeparam>
		/// <typeparam name="TData2">Generic data type</typeparam>
		/// <typeparam name="TData3">Generic data type</typeparam>
		/// <param name="url">Method URL</param>
		/// <param name="dataItem1">Generic parameter data</param>
		/// <param name="dataItem2">Generic parameter data</param>
		/// <param name="dataItem3">Generic parameter data</param>
		/// <returns>Result of method</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public async Task<IESSingleResponse<T>> MultiPost<T, TData1, TData2, TData3>(string url, TData1 dataItem1, TData2 dataItem2, TData3 dataItem3)
		{
			HttpResponseMessage response;

			using (MultipartFormDataContent formData = new MultipartFormDataContent())
			{
				//add content to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem1, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object1");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem2, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object2");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem3, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object3");

				// Get an authetnicated HTTP client
				response = await HttpClient.PostAsync($"{baseUrl}{serviceController}{url}", formData);
			}

			if (!response.IsSuccessStatusCode)
			{
				Logger.Error(string.Format("{0} was not successful w/ code {1}", "MultiPost", response.StatusCode));
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_UNAUTHORIZED } };
				}

				return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_ERROR } };
			}

			return await response.Content.ReadAsAsync<IESSingleResponse<T>>(); // .ReadFromJsonAsync<IESSingleResponse<T>>();
		}

		/// <summary>
		/// POST method for two complex objects
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <typeparam name="TData1">Generic data type</typeparam>
		/// <typeparam name="TData2">Generic data type</typeparam>
		/// <typeparam name="TData3">Generic data type</typeparam>
		/// <typeparam name="TData4">Generic data type</typeparam>
		/// <param name="url">Method URL</param>
		/// <param name="dataItem1">Generic parameter data</param>
		/// <param name="dataItem2">Generic parameter data</param>
		/// <param name="dataItem3">Generic parameter data</param>
		/// <param name="dataItem4">Generic parameter data</param>
		/// <returns>Result of method</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public async Task<IESSingleResponse<T>> MultiPost<T, TData1, TData2, TData3, TData4>(string url, TData1 dataItem1, TData2 dataItem2, TData3 dataItem3, TData4 dataItem4)
		{
			HttpResponseMessage response;

			using (MultipartFormDataContent formData = new MultipartFormDataContent())
			{
				//add content to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem1, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object1");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem2, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object2");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem3, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object3");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem4, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object4");

				// Get an authetnicated HTTP client
				response = await HttpClient.PostAsync($"{baseUrl}{serviceController}{url}", formData);
			}

			if (!response.IsSuccessStatusCode)
			{
				Logger.Error(string.Format("{0} was not successful w/ code {1}", "MultiPost", response.StatusCode));
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_UNAUTHORIZED } };
				}

				return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_ERROR } };
			}

			return await response.Content.ReadAsAsync<IESSingleResponse<T>>(); // .ReadFromJsonAsync<IESSingleResponse<T>>();
		}

		/// <summary>
		/// POST method for two complex objects
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <typeparam name="TData1">Generic data type</typeparam>
		/// <typeparam name="TData2">Generic data type</typeparam>
		/// <typeparam name="TData3">Generic data type</typeparam>
		/// <typeparam name="TData4">Generic data type</typeparam>
		/// <typeparam name="TData4">Generic data type</typeparam>
		/// <typeparam name="TData5">Generic data type</typeparam>
		/// <param name="url">Method URL</param>
		/// <param name="dataItem1">Generic parameter data</param>
		/// <param name="dataItem2">Generic parameter data</param>
		/// <param name="dataItem3">Generic parameter data</param>
		/// <param name="dataItem4">Generic parameter data</param>
		/// <param name="dataItem5">Generic parameter data</param>
		/// <returns>Result of method</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public async Task<IESSingleResponse<T>> MultiPost<T, TData1, TData2, TData3, TData4, TData5>(string url, TData1 dataItem1, TData2 dataItem2, TData3 dataItem3, TData4 dataItem4, TData5 dataItem5)
		{
			HttpResponseMessage response;

			using (MultipartFormDataContent formData = new MultipartFormDataContent())
			{
				//add content to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem1, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object1");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem2, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object2");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem3, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object3");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem4, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object4");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem5, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object5");

				// Get an authetnicated HTTP client
				response = await HttpClient.PostAsync($"{baseUrl}{serviceController}{url}", formData);
			}

			if (!response.IsSuccessStatusCode)
			{
				Logger.Error(string.Format("{0} was not successful w/ code {1}", "MultiPost", response.StatusCode));
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_UNAUTHORIZED } };
				}

				return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_ERROR } };
			}

			return await response.Content.ReadAsAsync<IESSingleResponse<T>>(); // .ReadFromJsonAsync<IESSingleResponse<T>>();
		}

		/// <summary>
		/// POST method for two complex objects
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <typeparam name="TData1">Generic data type</typeparam>
		/// <typeparam name="TData2">Generic data type</typeparam>
		/// <typeparam name="TData3">Generic data type</typeparam>
		/// <typeparam name="TData4">Generic data type</typeparam>
		/// <typeparam name="TData4">Generic data type</typeparam>
		/// <typeparam name="TData5">Generic data type</typeparam>
		/// <typeparam name="TData6">Generic data type</typeparam>
		/// <param name="url">Method URL</param>
		/// <param name="dataItem1">Generic parameter data</param>
		/// <param name="dataItem2">Generic parameter data</param>
		/// <param name="dataItem3">Generic parameter data</param>
		/// <param name="dataItem4">Generic parameter data</param>
		/// <param name="dataItem5">Generic parameter data</param>
		/// <param name="dataItem6">Generic parameter data</param>
		/// <returns>Result of method</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public async Task<IESSingleResponse<T>> MultiPost<T, TData1, TData2, TData3, TData4, TData5, TData6>(string url, TData1 dataItem1, TData2 dataItem2, TData3 dataItem3, TData4 dataItem4, TData5 dataItem5, TData6 dataItem6)
		{
			HttpResponseMessage response;

			using (MultipartFormDataContent formData = new MultipartFormDataContent())
			{
				//add content to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem1, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object1");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem2, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object2");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem3, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object3");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem4, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object4");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem5, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object5");

				//add config to form data
				formData.Add(new StringContent(JsonConvert.SerializeObject(dataItem6, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore })), "Object6");

				// Get an authenticated HTTP client
				response = await HttpClient.PostAsync($"{baseUrl}{serviceController}{url}", formData);
			}

			if (!response.IsSuccessStatusCode)
			{
				Logger.Error(string.Format("{0} was not successful w/ code {1}", "MultiPost", response.StatusCode));
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_UNAUTHORIZED } };
				}

				return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_ERROR } };
			}

			return await response.Content.ReadAsAsync<IESSingleResponse<T>>(); // .ReadFromJsonAsync<IESSingleResponse<T>>();
		}

		/// <summary>
		/// PUT method
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <typeparam name="TData">Generic data type</typeparam>
		/// <param name="url">Method URL</param>
		/// <param name="data">Generic parameter data</param>
		/// <returns>Result of method</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public async Task<IESSingleResponse<T>> Put<T, TData>(string url, TData data)
		{
			// Handles self referencing loop with Newtonsoft serializing options
			string serializedResult = JsonConvert.SerializeObject(data, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

			// Get an authetnicated HTTP client
			HttpResponseMessage response = await HttpClient.PutAsync($"{baseUrl}{serviceController}{url}", new StringContent(serializedResult, System.Text.Encoding.UTF8, "application/json"));

			if (!response.IsSuccessStatusCode)
			{
				Logger.Error(string.Format("{0} was not successful w/ code {1}", "Put", response.StatusCode));
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_UNAUTHORIZED } };
				}

				return new IESSingleResponse<T>() { IsSuccessful = false, Messages = new List<string>() { Constants.GENERIC_USER_ERROR } };
			}

			return await response.Content.ReadAsAsync<IESSingleResponse<T>>(); // .ReadFromJsonAsync<IESSingleResponse<T>>();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
					this.HttpClient.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
