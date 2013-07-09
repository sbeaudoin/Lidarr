using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public interface ISeasonService
    {
        void SetMonitored(int seriesId, int seasonNumber, bool monitored);
        List<Season> GetSeasonsBySeries(int seriesId);
    }

    public class SeasonService : ISeasonService,
        IHandle<EpisodeInfoAddedEvent>,
        IHandle<EpisodeInfoUpdatedEvent>,
        IHandleAsync<SeriesDeletedEvent>
    {
        private readonly ISeasonRepository _seasonRepository;
        private readonly Logger _logger;

        public SeasonService(ISeasonRepository seasonRepository, Logger logger)
        {
            _seasonRepository = seasonRepository;
            _logger = logger;
        }


        public void SetMonitored(int seriesId, int seasonNumber, bool monitored)
        {
            var season = _seasonRepository.Get(seriesId, seasonNumber);

            _logger.Trace("Setting monitored flag on Series:{0} Season:{1} to {2}", seriesId, seasonNumber, monitored);

            season.Monitored = monitored;

            //Todo: WTF does this do? Looks like it was from the NOSQL phase
            season.Episodes.ForEach(e => e.Monitored = monitored);

            _seasonRepository.Update(season);

            _logger.Info("Monitored flag for Series:{0} Season:{1} successfully set to {2}", seriesId, seasonNumber, monitored);
        }


        public List<Season> GetSeasonsBySeries(int seriesId)
        {
            return _seasonRepository.GetSeasonBySeries(seriesId);
        }

        public void Handle(EpisodeInfoAddedEvent message)
        {
            EnsureSeasons(message.Episodes);
        }

        public void Handle(EpisodeInfoUpdatedEvent message)
        {
            EnsureSeasons(message.Episodes);
        }

        private void EnsureSeasons(IEnumerable<Episode> episodes)
        {
            var seriesGroup = episodes.GroupBy(c => c.SeriesId);

            foreach (var group in seriesGroup)
            {
                var seriesId = group.Key;

                var existingSeasons = _seasonRepository.GetSeasonNumbers(seriesId);
                var seasonNumbers = group.Select(c => c.SeasonNumber).Distinct();
                var missingSeasons = seasonNumbers.Where(seasonNumber => !existingSeasons.Contains(seasonNumber));

                var seasonToAdd = missingSeasons.Select(c => new Season()
                    {
                        SeriesId = seriesId,
                        SeasonNumber = c,
                        Monitored = true
                    });

                _seasonRepository.InsertMany(seasonToAdd.ToList());
            }
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            var seasons = GetSeasonsBySeries(message.Series.Id);
            _seasonRepository.DeleteMany(seasons);
        }
    }


}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      