﻿using JMMClient.UserControls;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for RateSeriesForm.xaml
    /// </summary>
    public partial class RateSeriesForm : Window
    {
        public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register("Series",
            typeof(AnimeSeriesVM), typeof(RateSeriesForm), new UIPropertyMetadata(null, null));

        public AnimeSeriesVM Series
        {
            get { return (AnimeSeriesVM)GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value); }
        }

        public static readonly DependencyProperty TraktLinkExistsProperty = DependencyProperty.Register("TraktLinkExists",
            typeof(bool), typeof(RateSeriesForm), new UIPropertyMetadata(false, null));

        public bool TraktLinkExists
        {
            get { return (bool)GetValue(TraktLinkExistsProperty); }
            set { SetValue(TraktLinkExistsProperty, value); }
        }

        public RateSeriesForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cRating.OnRatingValueChangedEvent += new RatingControl.RatingValueChangedHandler(cRating_OnRatingValueChangedEvent);
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(RateSeriesForm_DataContextChanged);

            this.Loaded += new RoutedEventHandler(RateSeriesForm_Loaded);
        }

        void RateSeriesForm_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
            if (ser == null) return;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cboVoteType.Items.Clear();
            cboVoteType.Items.Add(Properties.Resources.VoteTypeAnimeTemporary);
            if (ser.AniDB_Anime.FinishedAiring)
                cboVoteType.Items.Add(Properties.Resources.VoteTypeAnimePermanent);

            if (ser.AniDB_Anime.FinishedAiring && ser.AllFilesWatched)
                cboVoteType.SelectedIndex = 1;
            else
                cboVoteType.SelectedIndex = 0;

            TraktLinkExists = ser.AniDB_Anime != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs.TraktCrossRefExists;
        }

        private void CommandBinding_RevokeVote(object sender, ExecutedRoutedEventArgs e)
        {
            AnimeSeriesVM ser = e.Parameter as AnimeSeriesVM;
            if (ser == null) return;

            try
            {
                JMMServerVM.Instance.RevokeVote(ser.AniDB_ID);
                MainListHelperVM.Instance.UpdateHeirarchy(ser);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void cRating_OnRatingValueChangedEvent(RatingValueEventArgs ev)
        {
            AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
            if (ser == null) return;

            try
            {
                decimal rating = (decimal)ev.RatingValue;

                int voteType = 1;
                if (cboVoteType.SelectedItem.ToString() == Properties.Resources.VoteTypeAnimeTemporary) voteType = 2;
                if (cboVoteType.SelectedItem.ToString() == Properties.Resources.VoteTypeAnimePermanent) voteType = 1;

                JMMServerVM.Instance.VoteAnime(ser.AniDB_ID, rating, voteType);

                // refresh the data
                MainListHelperVM.Instance.UpdateHeirarchy(ser);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void RateSeriesForm_Loaded(object sender, RoutedEventArgs e)
        {
            if (TraktLinkExists)
            {
                //this.Cursor = Cursors.Wait;
                ucTraktComments.RefreshComments();
                //this.Cursor = Cursors.Arrow;
            }
        }

        public void Init(AnimeSeriesVM series)
        {
            Series = series;
            this.DataContext = Series;
            ucTraktComments.DataContext = Series;
        }
    }
}
