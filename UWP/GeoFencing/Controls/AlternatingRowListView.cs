using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GeoFencing.Controls
{
    /// <summary>
    /// Control to fill alternating row in list view
    /// </summary>
    public class AlternatingRowListView : ListView
    {
        /// <summary>
        /// Row background property
        /// </summary>
        public static readonly DependencyProperty RowBackgroundProperty = DependencyProperty.Register("RowBackground", typeof(Brush), typeof(AlternatingRowListView), null);

        /// <summary>
        /// Alternate row background
        /// </summary>
        public static readonly DependencyProperty AlternateRowBackgroundProperty = DependencyProperty.Register("AlternateRowBackground", typeof(Brush), typeof(AlternatingRowListView), null);

        public Brush RowBackground
        {
            get { return (Brush)GetValue(RowBackgroundProperty); }
            set { this.SetValue(RowBackgroundProperty, (Brush)value); }
        }

        public Brush AlternateRowBackground
        {
            get { return (Brush)GetValue(AlternateRowBackgroundProperty); }
            set { this.SetValue(AlternateRowBackgroundProperty, (Brush)value); }
        }

        public void SetListViewItemsBackground()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                var item = this.Items[i];
                SetListItemBackground((ListViewItem)this.ContainerFromItem(item), i);
            }
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var listViewItem = element as ListViewItem;
            this.SetListItemBackground(listViewItem);
        }

        private void SetListItemBackground(ListViewItem listViewItem)
        {
            if (listViewItem != null)
            {
                var index = IndexFromContainer(listViewItem);

                this.SetListItemBackground(listViewItem, index);
            }
        }

        private void SetListItemBackground(ListViewItem listViewItem, int index)
        {
            if (listViewItem != null)
            {
                if ((index + 1) % 2 == 1)
                {
                    listViewItem.Background = this.RowBackground;
                }
                else
                {
                    listViewItem.Background = this.AlternateRowBackground;
                }
            }
        }
    }
}
