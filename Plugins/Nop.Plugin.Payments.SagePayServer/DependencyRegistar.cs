using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Payments.SagePayServer.Data;
using Nop.Plugin.Payments.SagePayServer.Domain;
using Nop.Plugin.Payments.SagePayServer.Services;


namespace Nop.Plugin.Payments.SagePayServer
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_sagepayserver";

        #region Implementation of IDependencyRegistrar

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            //Load custom data settings
            var dataSettingsManager = new DataSettingsManager();
            DataSettings dataSettings = dataSettingsManager.LoadSettings();

            //Register custom object context
            builder.Register<IDbContext>(c => RegisterIDbContext(c, dataSettings)).Named<IDbContext>(CONTEXT_NAME).InstancePerHttpRequest();
            builder.Register(c => RegisterIDbContext(c, dataSettings)).InstancePerHttpRequest();

            //Register services
            builder.RegisterType<SagePayServerTransactionService>().As<ISagePayServerTransactionService>();

            //Override the repository injection
            builder.RegisterType<EfRepository<SagePayServerTransaction>>().As<IRepository<SagePayServerTransaction>>().WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME)).InstancePerHttpRequest();
        }

        #endregion

        #region Implementation of IDependencyRegistrar

        public int Order
        {
            get { return 0; }
        }

        #endregion

        /// <summary>
        /// Registers the I db context.
        /// </summary>
        /// <param name="componentContext">The component context.</param>
        /// <param name="dataSettings">The data settings.</param>
        /// <returns></returns>
        private SagePayServerTransactionObjectContext RegisterIDbContext(IComponentContext componentContext, DataSettings dataSettings)
        {
            string dataConnectionStrings;

            if (dataSettings != null && dataSettings.IsValid())
            {
                dataConnectionStrings = dataSettings.DataConnectionString;
            }
            else
            {
                dataConnectionStrings = componentContext.Resolve<DataSettings>().DataConnectionString;
            }

            return new SagePayServerTransactionObjectContext(dataConnectionStrings);
        }
    }
}