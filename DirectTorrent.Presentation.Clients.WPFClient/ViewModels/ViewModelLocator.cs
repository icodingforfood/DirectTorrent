﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;

namespace DirectTorrent.Presentation.Clients.WPFClient.ViewModels
{
    class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<HomeViewModel>();
            SimpleIoc.Default.Register<MovieDetailsViewModel>(true);
            SimpleIoc.Default.Register<MovieVideoViewModel>();
            SimpleIoc.Default.Register<Settings.AppearanceViewModel>();
        }

        public HomeViewModel Home { get { return ServiceLocator.Current.GetInstance<HomeViewModel>(); } }
        public MovieDetailsViewModel MovieDetails { get { return ServiceLocator.Current.GetInstance<MovieDetailsViewModel>(); } }

        public MovieVideoViewModel MovieVideo
        {
            get
            {
                var guid = Guid.NewGuid().ToString();
                var viewModel = ServiceLocator.Current.GetInstance<MovieVideoViewModel>(guid);
                Messenger.Default.Send<string>(guid, "guid");
                return viewModel;
            }
        }

        public Settings.AppearanceViewModel Appearance { get { return ServiceLocator.Current.GetInstance<Settings.AppearanceViewModel>(); } }
    }
}
