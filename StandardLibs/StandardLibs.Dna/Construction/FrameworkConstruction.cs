using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using StandardLibs.Dna.Environment;

namespace StandardLibs.Dna.Construction
{
    /// <summary>
    /// The construction information when starting up and configuring KmsCore.Dna
    /// </summary>
    public class FrameworkConstruction
    {
        #region Protected Members

        /// <summary>
        /// The services that will get used and compiled once the framework is built
        /// </summary>
        protected IServiceCollection mServices;
        protected LightInjectServiceProviderFactory factory;
        #endregion

        #region Public Properties

        /// <summary>
        /// The default dependency injection service provider
        /// </summary>
        public IServiceProvider Provider { get; protected set; }

        /// <summary>
        /// The lightinject dependency injection service provider
        /// </summary>
        public IServiceContainer Container { get; protected set; }

        /// <summary>
        /// The environment used for the KmsCore.Dna.Framework
        /// </summary>
        public IFrameworkEnvironment Environment { get; protected set; }

        /// <summary>
        /// The configuration used for the Dna.Framework
        /// </summary>
        public IConfiguration Configuration { get; protected set; }

        /// <summary>
        /// The services that will get used and compiled once the framework is built
        /// </summary>
        public IServiceCollection Services
        {
            get => mServices;
            set
            {
                // Set services
                mServices = value;

                // If we have some...
                if (mServices != null)
                {
                    // Inject environment into services
                    Services.AddSingleton(Environment);
                }
            }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="createServiceCollection">If true, a new <see cref="ServiceCollection"/> will be created for the Services</param>
        public FrameworkConstruction(bool createServiceCollection = true)
        {
            // Create environment details
            Environment = new DefaultFrameworkEnvironment();

            // If we should create the service collection
            if (createServiceCollection)
            {
                // Create a new list of dependencies
                Services = new ServiceCollection();

                ContainerOptions containerOptions = new ContainerOptions
                {
                    EnablePropertyInjection = false
                  ,
                    DefaultServiceSelector = _services => _services.SingleOrDefault(string.IsNullOrWhiteSpace) ?? _services.Last()
                };

                factory = new LightInjectServiceProviderFactory(containerOptions);

                // create lightinject container also
                Container = factory.CreateBuilder(Services);
            }
        }

        #endregion

        #region Build Methods

        /// <summary>
        /// Builds the service collection into a service provider
        /// </summary>
        public void Build(IServiceProvider provider = null)
        {
            // Use given provider or build it
            // Provider = provider ?? Services.BuildServiceProvider();
            // change to use lightinject... Container.CreateServiceProvider(Services);
            Provider = provider ?? factory.CreateServiceProvider(Container); 
        }

        #endregion

        #region Hosted Environment Methods

        /// <summary>
        /// Uses the given service collection in the framework. 
        /// Typically used in an ASP.Net Core environment where
        /// the ASP.Net server has its own collection.
        /// </summary>
        /// <param name="services">The services to use</param>
        /// <returns></returns>
        public FrameworkConstruction UseHostedServices(IServiceCollection services)
        {
            // Set services
            Services = services;

            // Return self for chaining
            return this;
        }

        /// <summary>
        /// Uses the given configuration in the framework
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        /// <returns></returns>
        public FrameworkConstruction UseConfiguration(IConfiguration configuration)
        {
            // Set configuration
            Configuration = configuration;

            // Return self for chaining
            return this;
        }

        #endregion
    }
}
