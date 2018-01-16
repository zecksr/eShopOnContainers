﻿using eShopOnContainers.Services;
using System;
using System.Globalization;
using System.Reflection;
using eShopOnContainers.Core.Services.Catalog;
using eShopOnContainers.Core.Services.OpenUrl;
using eShopOnContainers.Core.Services.RequestProvider;
using eShopOnContainers.Core.Services.Basket;
using eShopOnContainers.Core.Services.Identity;
using eShopOnContainers.Core.Services.Order;
using eShopOnContainers.Core.Services.User;
using eShopOnContainers.Core.Services.Location;
using eShopOnContainers.Core.Services.Marketing;
using eShopOnContainers.Core.Services.Dependency;
using eShopOnContainers.Core.Services.Settings;
using eShopOnContainers.Core.Services.FixUri;
using Xamarin.Forms;
using TinyIoC;

namespace eShopOnContainers.Core.ViewModels.Base
{
    public static class ViewModelLocator
    {
        private static TinyIoCContainer _container;

        public static readonly BindableProperty AutoWireViewModelProperty =
            BindableProperty.CreateAttached("AutoWireViewModel", typeof(bool), typeof(ViewModelLocator), default(bool), propertyChanged: OnAutoWireViewModelChanged);

        public static bool GetAutoWireViewModel(BindableObject bindable)
        {
            return (bool)bindable.GetValue(ViewModelLocator.AutoWireViewModelProperty);
        }

        public static void SetAutoWireViewModel(BindableObject bindable, bool value)
        {
            bindable.SetValue(ViewModelLocator.AutoWireViewModelProperty, value);
        }

        public static bool UseMockService { get; set; }

        static ViewModelLocator()
        {
            _container = new TinyIoCContainer();

            // View models
            _container.Register<BasketViewModel>();
            _container.Register<CatalogViewModel>();
            _container.Register<CheckoutViewModel>();
            _container.Register<LoginViewModel>();
            _container.Register<MainViewModel>();
            _container.Register<OrderDetailViewModel>();
            _container.Register<ProfileViewModel>();
            _container.Register<SettingsViewModel>();
            _container.Register<CampaignViewModel>();
            _container.Register<CampaignDetailsViewModel>();

            // Services
            _container.Register<INavigationService, NavigationService>().AsSingleton();
            _container.Register<IDialogService, DialogService>();
            _container.Register<IOpenUrlService, OpenUrlService>();
            _container.Register<IIdentityService, IdentityService>();
            _container.Register<IRequestProvider, RequestProvider>();
            _container.Register<IDependencyService, Services.Dependency.DependencyService>();
            _container.Register<ISettingsService, SettingsService>().AsSingleton();
            _container.Register<IFixUriService, FixUriService>().AsSingleton();
            _container.Register<ILocationService, LocationService>().AsSingleton();
            _container.Register<ICatalogService, CatalogMockService>().AsSingleton();
            _container.Register<IBasketService, BasketMockService>().AsSingleton();
            _container.Register<IOrderService, OrderMockService>().AsSingleton();
            _container.Register<IUserService, UserMockService>().AsSingleton();
            _container.Register<ICampaignService, CampaignMockService>().AsSingleton();
        }

        public static void UpdateDependencies(bool useMockServices)
        {
            // Change injected dependencies
            if (useMockServices)
            {
                _container.Register<ICatalogService, CatalogMockService>().AsSingleton();
                _container.Register<IBasketService, BasketMockService>().AsSingleton();
                _container.Register<IOrderService, OrderMockService>().AsSingleton();
                _container.Register<IUserService, UserMockService>().AsSingleton();
                _container.Register<ICampaignService, CampaignMockService>().AsSingleton();

                UseMockService = true;
            }
            else
            {
                _container.Register<ICatalogService, CatalogService>().AsSingleton();
                _container.Register<IBasketService, BasketService>().AsSingleton();
                _container.Register<IOrderService, OrderService>().AsSingleton();
                _container.Register<IUserService, UserService>().AsSingleton();
                _container.Register<ICampaignService, CampaignService>().AsSingleton();

                UseMockService = false;
            }
        }

        public static T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        private static void OnAutoWireViewModelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as Element;
            if (view == null)
            {
                return;
            }

            var viewType = view.GetType();
            var viewName = viewType.FullName.Replace(".Views.", ".ViewModels.");
            var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
            var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}Model, {1}", viewName, viewAssemblyName);

            var viewModelType = Type.GetType(viewModelName);
            if (viewModelType == null)
            {
                return;
            }
            var viewModel = _container.Resolve(viewModelType);
            view.BindingContext = viewModel;
        }
    }
}