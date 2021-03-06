﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using DirectTorrent.Logic.Models;
using DirectTorrent.Logic.Services;
using DirectTorrent.Presentation.Clients.WPFClient.Models;
using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace DirectTorrent.Presentation.Clients.WPFClient.ViewModels
{
    public class HomeViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        //private IModernNavigationService _modernNavigationService;
        private Visibility _moviesVisibility = Visibility.Collapsed;
        private Visibility _loaderVisibility = Visibility.Visible;
        //private Quality _selectedQuality = Quality.ALL;
        private Sort _selectedSort = Sort.DateAdded;
        private Order _selectedOrder = Order.Descending;
        //private byte _selectedMinimumRating = 0;
        private string _queryString;
        private bool _isLoading = false;
        private uint _currentPage = 1;

        public GalaSoft.MvvmLight.CommandWpf.RelayCommand<ScrollChangedEventArgs> ScrollChangedCommand { get; private set; }
        public GalaSoft.MvvmLight.CommandWpf.RelayCommand TextBoxLostFocus { get; private set; }
        public GalaSoft.MvvmLight.CommandWpf.RelayCommand<int> MovieClicked { get; private set; }

        public ObservableCollection<HomeMovieItem> MovieList { get; private set; }

        //public Quality SelectedQuality
        //{
        //    get { return this._selectedQuality; }
        //    set
        //    {
        //        if (this._selectedQuality != value)
        //        {
        //            this._selectedQuality = value;
        //            OnPropertyChanged("SelectedQuality");
        //            LoadMovies(true);
        //        }
        //    }
        //}
        public Sort SelectedSort
        {
            get { return this._selectedSort; }
            set { this.Set(ref this._selectedSort, value, broadcast: true); }
        }

        public Order SelectedOrder
        {
            get { return this._selectedOrder; }
            set { this.Set(ref this._selectedOrder, value, broadcast: true); }
        }
        //public byte SelectedMinimumRating
        //{
        //    get { return this._selectedMinimumRating; }
        //    set
        //    {
        //        if (this._selectedMinimumRating != value)
        //        {
        //            this._selectedMinimumRating = value;
        //            OnPropertyChanged("SelectedMinimumRating");
        //            QueryChanged();
        //        }
        //    }
        //}

        public string QueryString
        {
            get { return this._queryString; }
            set { this.Set(ref this._queryString, value); }
        }

        public Visibility LoaderVisibility
        {
            get { return this._loaderVisibility; }
            private set { this.Set(ref this._loaderVisibility, value); }
        }

        public Visibility MoviesVisibility
        {
            get { return this._moviesVisibility; }
            private set { this.Set(ref this._moviesVisibility, value); }
        }

        public bool IsLoading
        {
            get { return this._isLoading; }
            private set { this.Set(ref this._isLoading, value); }
        }

        public HomeViewModel(/*IModernNavigationService modernNavigationService*/)
        {
            //try
            //{
            //    _modernNavigationService = modernNavigationService;
            //}
            //catch (Exception e)
            //{

            //    throw new Exception(e.Message);
            //}

            MovieList = new ObservableCollection<HomeMovieItem>();
            this.ScrollChangedCommand = new RelayCommand<ScrollChangedEventArgs>(async (e) =>
            {
                if (e.VerticalOffset == ((ScrollViewer)e.Source).ScrollableHeight && MoviesVisibility == Visibility.Visible)
                    await LoadMovies(false).ConfigureAwait(false);
            });
            this.TextBoxLostFocus = new GalaSoft.MvvmLight.CommandWpf.RelayCommand(async () => await LoadMovies(true).ConfigureAwait(false));
            this.MovieClicked = new RelayCommand<int>(x =>
            {
                //Data.MovieId = x;
                Messenger.Default.Send<int>(x, "movieId");
                (Application.Current.MainWindow as MainWindow).ContentSource = new Uri("/Views/MovieDetails.xaml", UriKind.Relative);
                //this.MovieClickedId = x;
                //Messenger.Default.
                //_modernNavigationService.NavigateTo(ViewModelLocator.MovieDetailsPageKey, x);
            });

            Messenger.Default.Register<PropertyChangedMessage<Sort>>(this, async (sort) => await LoadMovies(true).ConfigureAwait(false));
            Messenger.Default.Register<PropertyChangedMessage<Order>>(this, async (order) => await LoadMovies(true).ConfigureAwait(false));

            LoadMovies(false).ConfigureAwait(false);
        }

        private async Task LoadMovies(bool reset)
        {
            if (reset)
            {
                MoviesVisibility = Visibility.Collapsed;
                LoaderVisibility = Visibility.Visible;
                _currentPage = 1;
                MovieList.Clear();
            }
            else
                IsLoading = true;

            var movies = await MovieRepository.Yify.ListMovies(page: _currentPage/*, quality: _selectedQuality*/, sortBy: _selectedSort, orderBy: _selectedOrder, queryTerm: _queryString);
            if (movies != null)
            {
                foreach (var movie in movies.Select(x => new HomeMovieItem(x)))
                {
                    MovieList.Add(movie);
                }

                _currentPage++;
                IsLoading = false;
                if (MovieList.Count == 0)
                {
                    LoaderVisibility = Visibility.Collapsed;
                    MoviesVisibility = Visibility.Collapsed;
                    ModernDialog.ShowMessage("Query has no results.", "No results!", MessageBoxButton.OK);
                }
                else
                {
                    LoaderVisibility = Visibility.Collapsed;
                    MoviesVisibility = Visibility.Visible;
                }
            }
            else
            {
                LoaderVisibility = Visibility.Collapsed;
                ModernDialog.ShowMessage("No internet connection!" + Environment.NewLine + "Please restart the application with internet access.", "No internet access!", MessageBoxButton.OK);
            }
        }
    }
}