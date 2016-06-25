using Prism.Commands;
using Prism.Windows.AppModel;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoFencing.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private INavigationService _navigationService;
        private bool _canNavigateToGeoFencingItem = true;
        private bool _canNavigateToGeoFencingList = true;
        private bool _canNavigateToGeoFencingSettings = true;

        public MenuViewModel(INavigationService navigationService, IResourceLoader resourceLoader)
        {
            // TODO: Add ability to indicate which page your on by listening for navigation events once the NuGet package has been updated. Change CanNavigate to use whether or not your on that page to return false.
            // As-is, if navigation occurs via the back button, we won't know and can't update the _canNavigate value
            _navigationService = navigationService;

            Commands = new ObservableCollection<MenuItemViewModel>
            {
                new MenuItemViewModel { DisplayName = resourceLoader.GetString("GeoFencingListDisplayName"), FontIcon = "\ue8bc", Command = new DelegateCommand(NavigateToGeoFencingListPage, CanNavigateToGeoFencingListPage) },
                new MenuItemViewModel { DisplayName = resourceLoader.GetString("GeoFencingItemDisplayName"), FontIcon = "\ue70f", Command = new DelegateCommand(NavigateToGeoFencingItemPage, CanNavigateToGeoFencingItemPage) },
                new MenuItemViewModel { DisplayName = resourceLoader.GetString("GeoFencingSettingsDisplayName"), FontIcon = "\ue713", Command = new DelegateCommand(NavigateToGeoFencingSettingsPage, CanNavigateToGeoFencingSettingsPage) }
            };
        }

        public ObservableCollection<MenuItemViewModel> Commands { get; set; }
        private void NavigateToGeoFencingItemPage()
        {
            if (CanNavigateToGeoFencingItemPage())
            {
                if (_navigationService.Navigate(PageTokens.GeoFencingItemPage, null))
                {
                    _canNavigateToGeoFencingItem = true;
                    _canNavigateToGeoFencingList = true;
                    RaiseCanExecuteChanged();
                }
            }
        }

        private bool CanNavigateToGeoFencingItemPage()
        {
            return _canNavigateToGeoFencingItem;
        }

        private void NavigateToGeoFencingListPage()
        {
            if (CanNavigateToGeoFencingListPage())
            {
                if (_navigationService.Navigate(PageTokens.GeoFencingListPage, null))
                {
                    _canNavigateToGeoFencingItem = true;
                    _canNavigateToGeoFencingList = true;
                    RaiseCanExecuteChanged();
                }
            }
        }

        private bool CanNavigateToGeoFencingListPage()
        {
            return _canNavigateToGeoFencingList;
        }

        public void NavigateToGeoFencingSettingsPage()
        {
            if (CanNavigateToGeoFencingSettingsPage())
            {
                if (_navigationService.Navigate(PageTokens.GeoFencingSettingsPage, null))
                {
                    _canNavigateToGeoFencingItem = true;
                    _canNavigateToGeoFencingList = true;
                    RaiseCanExecuteChanged();
                }
            }
        }

        public bool CanNavigateToGeoFencingSettingsPage()
        {
            return _canNavigateToGeoFencingSettings;
        }

        private void RaiseCanExecuteChanged()
        {
            foreach (var item in Commands)
            {
                (item.Command as DelegateCommand).RaiseCanExecuteChanged();
            }
        }
    }
}