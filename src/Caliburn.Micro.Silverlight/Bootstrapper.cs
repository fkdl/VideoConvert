﻿namespace Caliburn.Micro
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Instantiate this class in order to configure the framework.
    /// </summary>
    public class Bootstrapper
    {
#if SILVERLIGHT
        /// <summary>
        /// Indicates whether or not the framework is in design-time mode.
        /// </summary>
        public static readonly bool IsInDesignMode = DesignerProperties.GetIsInDesignMode(new UserControl());
#else
        /// <summary>
        /// Indicates whether or not the framework is in design-time mode.
        /// </summary>
        public static readonly bool IsInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
#endif
        /// <summary>
        /// Creates an instance of the bootstrapper.
        /// </summary>
        public Bootstrapper()
        {
            if(!IsInDesignMode)
                Start();
        }

        internal virtual void Start() 
        {
            Execute.InitializeWithDispatcher();
            AssemblySource.Instance.AddRange(SelectAssemblies());
            Configure();

            IoC.GetInstance = GetInstance;
            IoC.GetAllInstances = GetAllInstances;
            IoC.BuildUp = BuildUp;
        }

        /// <summary>
        /// Override to configure the framework and setup your IoC container.
        /// </summary>
        protected virtual void Configure() { }

        /// <summary>
        /// Override to tell the framework where to find assemblies to inspect for views, etc.
        /// </summary>
        /// <returns>A list of assemblies to inspect.</returns>
        protected virtual IEnumerable<Assembly> SelectAssemblies()
        {
#if SILVERLIGHT
            return new[] { Application.Current.GetType().Assembly };
#else
            return new[] { Assembly.GetEntryAssembly() };
#endif
        }

        /// <summary>
        /// Override this to provide an IoC specific implementation.
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <param name="key">The key to locate.</param>
        /// <returns>The located service.</returns>
        protected virtual object GetInstance(Type service, string key)
        {
            return Activator.CreateInstance(service);
        }

        /// <summary>
        /// Override this to provide an IoC specific impelentation
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <returns>The located services.</returns>
        protected virtual IEnumerable<object> GetAllInstances(Type service)
        {
            return new[] { Activator.CreateInstance(service) };
        }

        /// <summary>
        /// Override this to provide an IoC specific implementation.
        /// </summary>
        /// <param name="instance">The instance to perform injection on.</param>
        protected virtual void BuildUp(object instance) {}
    }

    /// <summary>
    /// A strongly-typed version of <see cref="Bootstrapper"/> that specifies the type of rootmodel to create for the application.
    /// </summary>
    /// <typeparam name="TRootModel">The type of root model for the application.</typeparam>
    public class Bootstrapper<TRootModel> : Bootstrapper
    {
        internal override void Start()
        {
            base.Start();
            Application.Current.Startup += OnStartup;
        }

        /// <summary>
        /// Override this to add custom behavior to execute after the application starts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        protected virtual void OnStartup(object sender, StartupEventArgs e)
        {
            var viewModel = IoC.Get<TRootModel>();
            var view = ViewLocator.LocateForModel(viewModel, null);
            ViewModelBinder.Bind(viewModel, view);

            var activator = viewModel as IActivate;
            if (activator != null)
                activator.Activate();

#if SILVERLIGHT
            Application.Current.RootVisual = view;
#else
            ((Window)view).Show();
#endif
        }
    }
}