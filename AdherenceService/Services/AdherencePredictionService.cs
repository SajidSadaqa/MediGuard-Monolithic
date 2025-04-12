using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdherenceService.Models;
using AdherenceService.Data;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.EntityFrameworkCore;

namespace AdherenceService.Services
{
    public interface IAdherencePredictionService
    {
        Task<List<AdherenceRecord>> GetAdherenceRecordsAsync(string userId);
        Task<bool> TriggerAlertAsync(AdherenceRecord record);
    }

    public class AdherencePredictionService : IAdherencePredictionService
    {
        private readonly ILogger<AdherencePredictionService> _logger;
        private readonly AdherenceDbContext _dbContext;
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<AdherenceInput, AdherencePrediction> _predictionEngine;

        public AdherencePredictionService(
            ILogger<AdherencePredictionService> logger,
            AdherenceDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            _mlContext = new MLContext(seed: 1);

            var trainingData = new List<AdherenceInput>
            {
                new AdherenceInput { HoursLate = 0f, IsMissedDose = false },
                new AdherenceInput { HoursLate = 0.5f, IsMissedDose = false },
                new AdherenceInput { HoursLate = 1f, IsMissedDose = false },
                new AdherenceInput { HoursLate = 2f, IsMissedDose = true },
                new AdherenceInput { HoursLate = 3f, IsMissedDose = true },
                new AdherenceInput { HoursLate = 4f, IsMissedDose = true }
            };

            var trainingDataView = _mlContext.Data.LoadFromEnumerable(trainingData);
            var pipeline = _mlContext.Transforms.Concatenate("Features", nameof(AdherenceInput.HoursLate))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: nameof(AdherenceInput.IsMissedDose), featureColumnName: "Features"));
            var model = pipeline.Fit(trainingDataView);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<AdherenceInput, AdherencePrediction>(model);
        }

        public async Task<List<AdherenceRecord>> GetAdherenceRecordsAsync(string userId)
        {
            var records = await _dbContext.AdherenceRecords
                .Where(r => r.UserId == userId)
                .ToListAsync();

            foreach (var record in records)
            {
                if (!record.IsDoseTaken && record.ActualDoseTime == null && !record.AlertTriggered)
                {
                    float hoursLate = (float)(DateTime.UtcNow - record.ScheduledDoseTime).TotalHours;
                    var input = new AdherenceInput { HoursLate = hoursLate };
                    var prediction = _predictionEngine.Predict(input);

                    _logger.LogInformation($"Predicted missed dose for {record.MedicationId} with hoursLate={hoursLate:F2}: {prediction.IsMissedDose} (Probability: {prediction.Probability:P1})");

                    if (prediction.IsMissedDose && prediction.Probability > 0.5)
                    {
                        record.AlertTriggered = true;
                        record.Note = $"Dose likely missed (predicted probability: {prediction.Probability:P1}).";
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            return records;
        }

        public async Task<bool> TriggerAlertAsync(AdherenceRecord record)
        {
            await Task.Delay(200); // Simulate async operation
            _logger.LogInformation($"Adherence alert triggered for user: {record.UserId}, medication: {record.MedicationId}");
            return true;
        }

        public class AdherenceInput
        {
            public float HoursLate { get; set; }
            public bool IsMissedDose { get; set; }
        }

        public class AdherencePrediction
        {
            [ColumnName("PredictedLabel")]
            public bool IsMissedDose { get; set; }
            public float Probability { get; set; }
            public float Score { get; set; }
        }
    }
}