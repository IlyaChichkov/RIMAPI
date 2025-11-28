using System;
using System.Net;
using System.Threading.Tasks;

namespace RIMAPI.Core
{
    /// <summary>
    /// Router interface for extension mods to register their endpoints
    /// </summary>
    public interface IExtensionRouter
    {
        /// <summary>
        /// Register a GET endpoint under the extension's namespace
        /// </summary>
        /// <param name="path">Endpoint path (e.g., "jobs/active" becomes "/api/v1/{extension-namespace}/jobs/active")</param>
        /// <param name="handler">Request handler function</param>
        void Get(string path, Func<HttpListenerContext, Task> handler);

        /// <summary>
        /// Register a POST endpoint under the extension's namespace
        /// </summary>
        void Post(string path, Func<HttpListenerContext, Task> handler);

        /// <summary>
        /// Register a PUT endpoint under the extension's namespace
        /// </summary>
        void Put(string path, Func<HttpListenerContext, Task> handler);

        /// <summary>
        /// Register a DELETE endpoint under the extension's namespace
        /// </summary>
        void Delete(string path, Func<HttpListenerContext, Task> handler);

        /// <summary>
        /// Register any HTTP method endpoint under the extension's namespace
        /// </summary>
        void AddRoute(string method, string path, Func<HttpListenerContext, Task> handler);

        /// <summary>
        /// Register a controller class with automatic route discovery via attributes
        /// </summary>
        /// <typeparam name="TController">Controller type</typeparam>
        void RegisterController<TController>()
            where TController : class;

        /// <summary>
        /// Register all controllers in an assembly with automatic discovery
        /// </summary>
        /// <param name="assembly">Assembly to scan for controllers</param>
        void RegisterControllers(System.Reflection.Assembly assembly);
    }
}
